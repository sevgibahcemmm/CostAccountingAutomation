using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Cost.Accounting.Automation.Infrastructure;

/// <summary>
/// Uygulama açılışında master (merkezi) veritabanını hazırlar ve master'ı tohumlar.
///
/// Tek istisna içinde bulunulan yılın açılmasıdır: giriş ekranında seçilebilir bir
/// yıl listesi oluşsun diye, yıl kaydı olmayan şirketler için yılın iş veritabanı
/// <see cref="IAccountingYearProvisioner"/> üzerinden açılır. Sonraki yıllar
/// "Mali Yıl Aç" formundan açılır ve başlangıçta hiçbir yıl veritabanına
/// dokunulmaz.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Açılışta "veritabanı var mı" yoklamasının bağlantı ve komut zaman aşımı.
    /// Kısa tutulur: yoklama yalnızca bir varlık kontrolüdür, veri taşımaz.
    /// </summary>
    private const int ProbeTimeoutSeconds = 5;

    /// <summary>
    /// İlk kurulum sihirbazının ekrana basacağı adımların sırası.
    /// </summary>
    public static IReadOnlyList<DatabaseProvisionStep> Steps { get; } =
    [
        DatabaseProvisionStep.ConnectServer,
        DatabaseProvisionStep.CreateMasterDatabase,
        DatabaseProvisionStep.ApplyMasterSchema,
        DatabaseProvisionStep.SeedCompanies,
        DatabaseProvisionStep.SeedRolesAndUsers,
        DatabaseProvisionStep.SeedPermissions,
        DatabaseProvisionStep.ProvisionYearDatabases,
        DatabaseProvisionStep.SeedChartOfAccounts,
        DatabaseProvisionStep.SeedUnitsAndTaxRates,
        DatabaseProvisionStep.SeedSampleRecords
    ];

    /// <summary>
    /// Ana veritabanının var olup olmadığını sunucuya sorar.
    ///
    /// <para>
    /// Sorgu <c>master</c> kataloğuna bağlanıp <c>sys.databases</c> üzerinde
    /// yapılır; böylece veritabanı yokken bile bağlantı kurulabilir.
    /// </para>
    ///
    /// <para>
    /// <b>Zaman aşımı bilinçli olarak kısadır.</b> Bağlantı dizesindeki
    /// <c>Connect Timeout = 30</c> değeri kullanılmaz; yoklama
    /// <see cref="ProbeTimeoutSeconds"/> saniye ile sınırlıdır. Bu sorgu
    /// uygulama açılışında "veritabanı var mı" sorusuna yanıt arar — sunucu
    /// kapalıysa kullanıcı 30 saniye donmuş ekran beklemek yerine birkaç
    /// saniyede anlaşılır bir hata görmelidir. Asıl veri işlemleri kendi
    /// zaman aşımlarını kendi bağlantı dizelerinde kullanmaya devam eder.
    /// </para>
    /// </summary>
    public static async Task<DatabaseFirstRunState> GetFirstRunStateAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();
        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string? masterConnectionString = configuration.GetConnectionString("Master");

        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            return DatabaseFirstRunState.Unreachable;
        }

        string databaseName = ServiceRegistrar.ReadDatabaseName(masterConnectionString);

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return DatabaseFirstRunState.Unreachable;
        }

        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = ProbeTimeoutSeconds,
            CommandTimeout = ProbeTimeoutSeconds
        };

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);

            await connection.OpenAsync();

            await using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
            command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = databaseName;

            object? result = await command.ExecuteScalarAsync();

            return result is null
                ? DatabaseFirstRunState.Missing
                : DatabaseFirstRunState.Exists;
        }
        catch (Exception ex)
        {
            // Sunucuya ulaşılamadı; kurulum penceresi bu durumu kendi ekranında
            // göstereceği için yalnızca tanılayıcı bilgi düşülür.
            System.Diagnostics.Debug.WriteLine(
                $"[DatabaseInitializer] Sunucuya ulaşılamadı: {ex.Message}");

            return DatabaseFirstRunState.Unreachable;
        }
    }

    /// <param name="progress">
    /// Adım durumlarını arayüze iletir. Opsiyoneldir; verilmezse adımlar
    /// sessizce çalışır (normal açılışta kurulum sihirbazı gösterilmez).
    /// </param>
    public static async Task InitializeAsync(
        IServiceProvider services,
        IProgress<DatabaseProvisionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        var masterContext = sp.GetRequiredService<MasterDbContext>();
        var roleRepository = sp.GetRequiredService<IRoleRepository>();
        var permissionService = sp.GetRequiredService<PermissionService>();
        var databaseNameBuilder = sp.GetRequiredService<IDatabaseNameBuilder>();
        var provisioner = sp.GetRequiredService<IAccountingYearProvisioner>();

        IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
        string databaseName =
            ServiceRegistrar.ReadDatabaseName(configuration.GetConnectionString("Master") ?? string.Empty);

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.ConnectServer,
            DatabaseProvisionStepState.Running,
            "sunucuya bağlanılıyor"));

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.CreateMasterDatabase,
            DatabaseProvisionStepState.Running,
            databaseName));

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.ApplyMasterSchema,
            DatabaseProvisionStepState.Running));

        await masterContext.Database.MigrateAsync(cancellationToken);

        int tableCount = await CountMasterTablesAsync(masterContext, cancellationToken);

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.ConnectServer,
            DatabaseProvisionStepState.Completed));

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.CreateMasterDatabase,
            DatabaseProvisionStepState.Completed,
            databaseName));

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.ApplyMasterSchema,
            DatabaseProvisionStepState.Completed,
            $"{tableCount} tablo"));

        await SeedMasterAsync(masterContext, permissionService, roleRepository, progress, cancellationToken);
        await EnsureCurrentYearAsync(masterContext, databaseNameBuilder, provisioner, progress, cancellationToken);
    }

    /// <summary>Ana veritabanındaki tablo sayısı; kurulum ilerlemesinde raporlanır.</summary>
    private static async Task<int> CountMasterTablesAsync(
        MasterDbContext masterContext,
        CancellationToken cancellationToken)
    {
        try
        {
            return await masterContext.Database
                .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM sys.tables")
                .ToListAsync(cancellationToken) is { Count: > 0 } values
                ? values[0]
                : 0;
        }
        catch (Exception ex)
        {
            // Sayaç yalnızca bilgilendirme amaçlıdır; başarısız olması
            // kurulumu durdurmamalıdır.
            System.Diagnostics.Debug.WriteLine(
                $"[DatabaseInitializer] Tablo sayısı okunamadı: {ex.Message}");

            return 0;
        }
    }

    /// <summary>
    /// Master'da şirket yoksa ilk kurulumdur: örnek şirketler, roller ve
    /// kullanıcılar tohumlanır. Ardından sys_admin rolüne yeni yetkiler eklenir.
    /// </summary>
    private static async Task SeedMasterAsync(
        MasterDbContext masterContext,
        PermissionService permissionService,
        IRoleRepository roleRepository,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedCompanies,
            DatabaseProvisionStepState.Running));

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedRolesAndUsers,
            DatabaseProvisionStepState.Running));

        if (!await masterContext.Companies.AnyAsync(cancellationToken))
        {
            var merkezCompany = new Company(
                new Name("DEMİRCİ AÇIK CEZA İNFAZ KURUMU MÜDÜRLÜĞÜ"),
                new TaxOffice("DEMİRCİ"),
                new TaxNumber("1234567890"),
                new Description("DEMİRCİ"),
                new Invoiceinformation("DACIK"),
                new Letterhead("DEMİRCİ AÇIK CEZA İNFAZ KURUMU MÜDÜRLÜĞÜ"),
                new CompanyPrefix("08691234"),
                new Address("Manisa", "Demirci", "www"),
                new Contact("02161234567", "", "info@merkez.com"),
                new ExpenditureUnit("MERKEZ HARCAMA BİRİMİ", "45.05"),
                new AccountingUnit("DEMİRCİ MAL MÜDÜRLÜĞÜ", "45103"),
                true);

            var anadoluCompany = new Company(
                new Name("Anadolu Şube"),
                new TaxOffice("Ankara"),
                new TaxNumber("4567890123"),
                new Description("Ankara"),
                new Invoiceinformation("Ankara"),
                new Letterhead("Ankara"),
                new CompanyPrefix("08691234"),
                new Address("Ankara", "Çankaya", "Kızılay"),
                new Contact("03124567890", "", "ankara@merkez.com"),
                new ExpenditureUnit("ANADOLU HARCAMA BİRİMİ", "2.2.2.2"),
                new AccountingUnit("ANADOLU MUHASEBE BİRİMİ", "2002"),
                true);

            var egeCompany = new Company(
                new Name("Ege Şube"),
                new TaxOffice("İzmir"),
                new TaxNumber("7890123456"),
                new Description("İzmir"),
                new Invoiceinformation("İzmir"),
                new Letterhead("İzmir"),
                new CompanyPrefix("08691234"),
                new Address("İzmir", "Konak", "Alsancak"),
                new Contact("02327894561", "", "izmir@merkez.com"),
                new ExpenditureUnit("EGE HARCAMA BİRİMİ", "3.3.3.3"),
                new AccountingUnit("EGE MUHASEBE BİRİMİ", "3003"),
                true);

            var sysAdminRole = new Role(new Name("sys_admin"), true);
            var accountingManagerRole = new Role(new Name("muhasebe_muduru"), true);
            var accountantRole = new Role(new Name("muhasebe_elemani"), true);

            var adminUser = new User(
                new FirstName("Emrullah"),
                new LastName("AKPINAR"),
                new Email("admin@test.com"),
                new UserName("admin"),
                new Password("1"),
                merkezCompany.Id,
                sysAdminRole.Id,
                true);

            // CreatedBy alanı NOT NULL ve GetAllWithAudit, CreatedBy üzerinden
            // Users'a inner-join yapar. Tohumlanan tüm kayıtların CreatedBy'si
            // bu yüzden admin kullanıcının gerçek Id'sine bağlanmalı.
            masterContext.SetSeedAdminUserId(adminUser.Id.Value);

            try
            {
                masterContext.Companies.AddRange(merkezCompany, anadoluCompany, egeCompany);
                masterContext.Roles.AddRange(sysAdminRole, accountingManagerRole, accountantRole);
                masterContext.Users.Add(adminUser);

                await masterContext.SaveChangesAsync();

                (string UserName, string Email, IdentityId CompanyId, IdentityId RoleId)[] sampleUsers =
                [
                    ("ahmet.yilmaz", "ahmet@test.com", merkezCompany.Id, accountingManagerRole.Id),
                    ("ayse.kaya", "ayse@test.com", anadoluCompany.Id, accountantRole.Id),
                    ("mehmet.demir", "mehmet@test.com", anadoluCompany.Id, accountantRole.Id),
                    ("fatma.celik", "fatma@test.com", egeCompany.Id, accountantRole.Id),
                ];

                foreach ((string userName, string email, IdentityId companyId, IdentityId roleId) in sampleUsers)
                {
                    string firstName = char.ToUpperInvariant(userName[0])
                        + userName.Substring(1, userName.IndexOf('.') - 1);

                    masterContext.Users.Add(
                        new User(
                            new FirstName(firstName),
                            new LastName("Soyad"),
                            new Email(email),
                            new UserName(userName),
                            new Password("1"),
                            companyId,
                            roleId,
                            true));
                }

                await masterContext.SaveChangesAsync();
            }
            finally
            {
                masterContext.ClearSeedAdminUserId();
            }

            int companyCount = await masterContext.Companies.CountAsync(cancellationToken);
            int roleCount = await masterContext.Roles.CountAsync(cancellationToken);
            int userCount = await masterContext.Users.CountAsync(cancellationToken);

            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedCompanies,
                DatabaseProvisionStepState.Completed,
                $"{companyCount} kurum"));

            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedRolesAndUsers,
                DatabaseProvisionStepState.Completed,
                $"{roleCount} rol, {userCount} kullanıcı"));
        }
        else
        {
            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedCompanies,
                DatabaseProvisionStepState.Skipped,
                "kurum kayıtları zaten mevcut"));

            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedRolesAndUsers,
                DatabaseProvisionStepState.Skipped,
                "rol ve kullanıcı kayıtları zaten mevcut"));
        }

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedPermissions,
            DatabaseProvisionStepState.Running));

        await EnsureAdminRolePermissionsAsync(
            masterContext, permissionService, roleRepository, cancellationToken);

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedPermissions,
            DatabaseProvisionStepState.Completed,
            "yönetici yetkileri tanımlandı"));
    }

    /// <summary>
    /// İlk kurulumda, içinde bulunulan yılın kaydı olmayan her şirket için yılın
    /// iş veritabanını açar. Böylece uygulama ilk açılışta giriş ekranında seçilebilir
    /// bir yıl listesiyle karşılaşır. Kontrol şirket bazında yapılır: bir şirkette
    /// yıl açılmış olması diğer şirketleri etkilemez. Sonraki yıllar
    /// "Mali Yıl Aç" formundan açılır.
    /// </summary>
    private static async Task EnsureCurrentYearAsync(
        MasterDbContext masterContext,
        IDatabaseNameBuilder databaseNameBuilder,
        IAccountingYearProvisioner provisioner,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        int currentYear = DateTime.Now.Year;

        var companies = await masterContext.Companies
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(c => c.Name.Value)
            .Select(c => new { Id = c.Id.Value, Name = c.Name.Value })
            .ToListAsync(cancellationToken);

        if (companies.Count == 0)
        {
            return;
        }

        // Yıl kaydı olmayan şirketler belirlenir; var olan yıllara dokunulmaz.
        // Year ve IdentityId value converter ile eşlendiği için sorgu, alanların
        // .Value özelliklerine değil nesnelerin kendisine karşı yazılmalıdır.
        var companiesWithCurrentYear = await masterContext.CompanyYears
            .AsNoTracking()
            .Where(cy => cy.Year == new Year(currentYear))
            .Select(cy => cy.CompanyId)
            .ToListAsync(cancellationToken);

        var pendingCompanies = companies
            .Where(c => !companiesWithCurrentYear.Contains(new IdentityId(c.Id)))
            .ToList();

        if (pendingCompanies.Count == 0)
        {
            return;
        }

        // CompanyYears kaydının CreatedBy alanı NOT NULL; açılışta oturum olmadığı
        // için admin kullanıcısı tohumlayıcı olarak işaretlenir.
        Guid? adminId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        masterContext.SetSeedAdminUserId(adminId);

        int chartOfAccountTotal = 0;
        int referenceTotal = 0;
        int sampleRecordTotal = 0;

        try
        {
            foreach (var company in pendingCompanies)
            {
                string databaseName = await databaseNameBuilder
                    .SuggestAvailableAsync(company.Name, currentYear, cancellationToken);

                progress?.Report(new DatabaseProvisionProgress(
                    DatabaseProvisionStep.ProvisionYearDatabases,
                    DatabaseProvisionStepState.Running,
                    $"{company.Name} · {databaseName}"));

                AccountingYearProvisionResult result = await provisioner.EnsureDatabaseAsync(
                    new IdentityId(company.Id),
                    currentYear,
                    databaseName,
                    cancellationToken,
                    progress);

                chartOfAccountTotal += result.SeededChartOfAccountCount;
                referenceTotal += result.SeededReferenceCount;
                sampleRecordTotal += result.SeededSampleRecordCount;

                var companyYear = new CompanyYear(
                    new IdentityId(company.Id),
                    new Year(currentYear),
                    new DatabaseName(databaseName));

                companyYear.SetOpeningDate(new DateTimeOffset(currentYear, 1, 1, 0, 0, 0, TimeSpan.Zero));

                masterContext.CompanyYears.Add(companyYear);
                await masterContext.SaveChangesAsync(cancellationToken);
            }

            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.ProvisionYearDatabases,
                DatabaseProvisionStepState.Completed,
                $"{pendingCompanies.Count} kurum · {currentYear} mali yılı"));

            ReportSeedTotal(progress, DatabaseProvisionStep.SeedChartOfAccounts, chartOfAccountTotal, "hesap");
            ReportSeedTotal(progress, DatabaseProvisionStep.SeedUnitsAndTaxRates, referenceTotal, "tanım");
            ReportSeedTotal(progress, DatabaseProvisionStep.SeedSampleRecords, sampleRecordTotal, "müşteri / tedarikçi");
        }
        finally
        {
            masterContext.ClearSeedAdminUserId();
        }
    }

    /// <summary>
    /// Tohum alt adımları yıl veritabanı sağlayıcısı tarafından kurum bazında
    /// bildirilir; burada kurumların toplamı bir kez daha raporlanır. Böylece
    /// ekranda satır başına tek bir tik kalır ve tutarlar tüm kurumları kapsar.
    /// </summary>
    private static void ReportSeedTotal(
        IProgress<DatabaseProvisionProgress>? progress,
        DatabaseProvisionStep step,
        int total,
        string unit)
    {
        if (progress is null)
        {
            return;
        }

        progress.Report(new DatabaseProvisionProgress(
            step,
            total > 0 ? DatabaseProvisionStepState.Completed : DatabaseProvisionStepState.Skipped,
            total > 0 ? $"{total} {unit}" : "kayıtlar zaten mevcut"));
    }

    private static async Task EnsureAdminRolePermissionsAsync(
        MasterDbContext masterContext,
        PermissionService permissionService,
        IRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        Guid? adminId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminId is null)
        {
            return;
        }

        // Yeni yetki kayıtlarının CreatedBy alanı da admin'i işaretlemeli.
        masterContext.SetSeedAdminUserId(adminId.Value);
        try
        {
            await permissionService.EnsureAdminRoleHasAllPermissionsAsync(roleRepository, masterContext);
        }
        finally
        {
            masterContext.ClearSeedAdminUserId();
        }
    }
}
