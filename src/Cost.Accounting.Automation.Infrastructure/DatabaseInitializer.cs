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
using Cost.Accounting.Automation.Infrastructure.Options;
using Cost.Accounting.Automation.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.IO;
using System.Threading.Tasks;

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
    /// </summary>
    /// <remarks>
    /// 15 saniye: LocalDB'nin soğuk başlatması için yeterli, hata durumunda kullanıcı
    /// yine de kabul edilebilir sürede geri bildirim alır.
    /// </remarks>
    private const int ProbeTimeoutSeconds = 15;

    /// <summary>
    /// Yoklama başarısız olursa bir kez yeniden denemeden önce beklenecek süre.
    /// </summary>
    private static readonly TimeSpan ProbeRetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Maksimum yoklama deneme sayısı.
    /// </summary>
    private const int MaxProbeAttempts = 2;

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
        return (await ProbeAsync(services)).State;
    }

    /// <summary>
    /// Ana veritabanının varlığını yoklar ve sonucu ayrıntısıyla döndürür.
    /// </summary>
    /// <remarks>
    /// <see cref="GetFirstRunStateAsync"/> yalnızca durumu döndürür. Açılış
    /// ekranı ise kullanıcıya <b>neden</b> ulaşılamadığını göstermek zorunda
    /// olduğu için hata metni de taşınır.
    /// </remarks>
    private static async Task<ProbeResult> ProbeAsync(IServiceProvider services)
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxProbeAttempts; attempt++)
        {
            using var scope = services.CreateScope();
            IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            string? masterConnectionString = configuration.GetConnectionString("Master");

            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                return ProbeResult.Unreachable(
                    "appsettings.json içinde ConnectionStrings:Master tanımlı değil.");
            }

            string databaseName = ServiceRegistrar.ReadDatabaseName(masterConnectionString);

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return ProbeResult.Unreachable("Bağlantı dizesinde Initial Catalog boş.");
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

                if (result is not null)
                {
                    return ProbeResult.Exists;
                }

                return ProbeResult.Missing;
            }
            catch (Exception ex) when (attempt < MaxProbeAttempts)
            {
                lastException = ex;
                
                // Kısa bir gecikme sonra tekrar dene
                await Task.Delay(ProbeRetryDelay);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        // Tüm denemeler başarısız oldu
        string errorMessage = lastException?.Message ?? "Bilinmeyen hata";
        
        // Hata log dosyasına da yaz (debugger olmadan da görülebilir)
        string logMessage = $"[DatabaseInitializer] Sunucuya ulaşılamadı ({MaxProbeAttempts} deneme sonrası): {errorMessage}";
        System.Diagnostics.Debug.WriteLine(logMessage);
        
        try
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "crash.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTimeOffset.Now:O}] {logMessage}{Environment.NewLine}");
        }
        catch
        {
            // Log yazma hatası yutulur; ana hata yine de kullanıcıya gösterilir
        }

        return ProbeResult.Unreachable(errorMessage);
    }

    /// <summary>Yoklama sonucu; hata durumunda ayrıntıyı da taşır.</summary>
    private sealed record ProbeResult(DatabaseFirstRunState State, string? Error)
    {
        public static ProbeResult Exists => new(DatabaseFirstRunState.Exists, null);

        public static ProbeResult Missing => new(DatabaseFirstRunState.Missing, null);

        public static ProbeResult Unreachable(string error) =>
            new(DatabaseFirstRunState.Unreachable, error);
    }

    /// <summary>
    /// Bağlantılı olunan veritabanının kullanıma hazır olup olmadığını
    /// <b>hiçbir şey yazmadan</b> doğrular.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bu metot merkezi sunucuya bağlanan istemcilerin açılış yoludur. Uygulama
    /// açılırken veritabanını <em>değiştirmez</em>; yalnızca bekleyen migration
    /// varsa bunu öğrenir ve ekranda yöneticiye ne yapılması gerektiğini söyler.
    /// </para>
    /// <para>
    /// Kontrol iki aşamalıdır. Önce veritabanının varlığı <c>sys.databases</c>
    /// üzerinden yoklanır (veritabanı yokken de bağlantı kurulabilmesi için
    /// <c>master</c> kataloğuna bağlanılır). Sonra bekleyen migration'lar
    /// okunur. İkinci adım yalnızca <c>__EFMigrationsHistory</c> tablosunu okur;
    /// EF'nin <c>MigrateAsync</c>'i aksine hiçbir şema değişikliği yapmaz ve
    /// geçmiş tablosuna satır eklemez. Bu yüzden kontrol güvenle her istemci
    /// açılışında, hatta aynı anda N istemci açılışında çalıştırılabilir.
    /// </para>
    /// </remarks>
    public static async Task<DatabaseSchemaCheckResult> CheckSchemaAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
        string? masterConnectionString = configuration.GetConnectionString("Master");

        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            return DatabaseSchemaCheckResult.Unreachable(
                "appsettings.json içinde ConnectionStrings:Master tanımlı değil.");
        }

        string databaseName = ServiceRegistrar.ReadDatabaseName(masterConnectionString);

        ProbeResult probe = await ProbeAsync(services);

        switch (probe.State)
        {
            case DatabaseFirstRunState.Missing:
                return DatabaseSchemaCheckResult.DatabaseMissing(databaseName);

            case DatabaseFirstRunState.Unreachable:
                return DatabaseSchemaCheckResult.Unreachable(
                    probe.Error ?? "Sunucuya ulaşılamadı.");
        }

        var options = sp.GetRequiredService<IOptions<DatabaseProvisioningOptions>>().Value;
        var masterContext = sp.GetRequiredService<MasterDbContext>();

        try
        {
            // Bekleyen migration sorgusu yalnızca okur; yine de kısa bir komut
            // zaman aşımı uygulanır ki yavaş sunucuda giriş ekranı asılı kalmasın.
            masterContext.Database.SetCommandTimeout(
                Math.Max(1, options.SchemaCheckTimeoutSeconds));

            var pendingMigrations = (await masterContext.Database
                .GetPendingMigrationsAsync(cancellationToken))
                .ToArray();

            return pendingMigrations.Length == 0
                ? DatabaseSchemaCheckResult.Ready()
                : DatabaseSchemaCheckResult.SchemaOutdated(pendingMigrations);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DatabaseInitializer] Şema kontrolü başarısız: {ex.Message}");

            return DatabaseSchemaCheckResult.Unreachable(ex.Message);
        }
    }

    /// <summary>
    /// Veritabanı hazırlık kilidinin kaynak adı.
    /// </summary>
    /// <remarks>
    /// Master veritabanı adı kaynağa dâhil edilir: aynı SQL Server üzerinde
    /// farklı ortamların (geliştirme / test / canlı) master veritabanları
    /// farklıysa birbirlerini bekletmemeleri gerekir.
    /// </remarks>
    internal static string BuildLockName(string masterDatabaseName) =>
        $"CAA:Provisioning:{masterDatabaseName}";

    /// <summary>
    /// Veritabanını idempotent biçimde hazırlar: master migration'ları,
    /// tohumlama ve içinde bulunulan yılın iş veritabanları. Veriler zaten
    /// hazırsa hiçbir şey yazmaz.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bu metot şema değiştirir ve yalnızca yönetici tarafından çağrılmalıdır.</b>
    /// Uygulama açılışında <see cref="CheckSchemaAsync"/> kullanılır.
    /// </para>
    /// <para>
    /// Çağrılar arasında sunucu çapında bir kilit tutulur ve kilit alındıktan
    /// <em>sonra</em> durum yeniden okunur. İkisi birden gereklidir:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Kilit olmadan iki yönetici aynı anda migration çalıştırırsa ikisi de
    /// <c>__EFMigrationsHistory</c>'ye yazar ve biri diğerinin DDL'ini kilitli
    /// bulup beklerken hata alır.
    /// </item>
    /// <item>
    /// Kilit olmadan "şirket var mı?" kontrolü iki yöneticiye de "yok" der ve
    /// ikisi de aynı şirketi, aynı rolü ve aynı kullanıcıyı ekler.
    /// </item>
    /// </list>
    /// <para>
    /// Kilitten sonraki yeniden kontrol, ikinci çağrının hiçbir şey yapmadan
    /// çıkmasını sağlar; yalnızca kilitlemek yarışı engeller, tekrarı
    /// engellemez.
    /// </para>
    /// </remarks>
    /// <param name="progress">
    /// Adım durumlarını arayüze iletir. Opsiyoneldir; verilmezse adımlar
    /// sessizce çalışır.
    /// </param>
    public static async Task InitializeAsync(
        IServiceProvider services,
        IProgress<DatabaseProvisionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
        string? masterConnectionString = configuration.GetConnectionString("Master");

        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            throw new InvalidOperationException(
                "appsettings.json içinde ConnectionStrings:Master tanımlı değil.");
        }

        string databaseName = ServiceRegistrar.ReadDatabaseName(masterConnectionString);

        var provisioningOptions =
            sp.GetRequiredService<IOptions<DatabaseProvisioningOptions>>().Value;

        TimeSpan lockTimeout =
            TimeSpan.FromSeconds(Math.Max(1, provisioningOptions.LockTimeoutSeconds));

        await using var provisioningLock = await ProvisioningLock.AcquireAsync(
            masterConnectionString,
            BuildLockName(databaseName),
            lockTimeout,
            cancellationToken);

        // Kilit artık elimizde: başka bir oturum bu noktadan sonra hazırlığa
        // başlayamaz. Yapılacak işin kaldığını burada yeniden okuyarak
        // "kontrol et ve sonra yaz" aralığını kapatıyoruz.
        DatabaseSchemaCheckResult schemaCheck = await CheckSchemaAsync(services, cancellationToken);

        if (schemaCheck.State is DatabaseSchemaState.Unreachable)
        {
            throw new InvalidOperationException(
                $"Veritabanı sunucusuna ulaşılamadı: {schemaCheck.Detail}");
        }

        var masterContext = sp.GetRequiredService<MasterDbContext>();
        var roleRepository = sp.GetRequiredService<IRoleRepository>();
        var permissionService = sp.GetRequiredService<PermissionService>();
        var databaseNameBuilder = sp.GetRequiredService<IDatabaseNameBuilder>();
        var provisioner = sp.GetRequiredService<IAccountingYearProvisioner>();

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
                new Name("Demirci Açık Ceza İnfaz Kurumu İşyurdu Müdürlüğü"),
                new TaxOffice("DEMİRCİ"),
                new TaxNumber("1234567890"),
                new Description("DEMİRCİ"),
                new Invoiceinformation("DACIK"),
                new Letterhead("T.C.\nADALET BAKANLIĞI\nCeza İnfaz Kurumları İle Tutukevleri İşyurtları Kurumu\nDemirci Açık Ceza İnfaz Kurumu İşyurdu Müdürlüğü"),
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

        // Standart rollerin baslangic yetkileri (or. mesajlasma) tamamlanir.
        // Yalnizca EKLER; yoneticinin rol ekranindaki tercihlerini degistirmez.
        Guid? starterAdminId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (starterAdminId is not null)
        {
            masterContext.SetSeedAdminUserId(starterAdminId.Value);
        }

        try
        {
            await permissionService.EnsureStarterRolePermissionsAsync(
                roleRepository, masterContext, cancellationToken);
        }
        finally
        {
            masterContext.ClearSeedAdminUserId();
        }

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedPermissions,
            DatabaseProvisionStepState.Completed,
            "yönetici yetkileri tanımlandı"));
    }

    /// <summary>
    /// İçinde bulunulan mali yılın iş veritabanlarını tüm kurumlar için güncel
    /// hale getirir.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yıl kaydı olmayan kurum</b> için veritabanı açılır, tohumlanır ve
    /// <c>CompanyYears</c> kaydı yazılır. <b>Yıl kaydı olan kurum</b> için yalnızca
    /// bekleyen migration'lar uygulanır; tohumlama zaten dolu tablolarda
    /// "atla" davranışı gösterir.
    /// </para>
    /// <para>
    /// Kayıt olan kurumları da kapsamak bilinçlidir. Şema ilerlemesi
    /// (örneğin eşzamanlılık belirteci eklenmesi) yalnızca yeni açılan
    /// veritabanlarına uygulansaydı, <b>mevcut</b> kurumların veritabanları
    /// geride kalır ve o kurumlar uygulamayı hiç açamazdı. Hazırlık komutunun
    /// anlamı "her şeyi güncel tut" olduğu için var olan yıl veritabanları da
    /// taranır.
    /// </para>
    /// </remarks>
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

        // Year ve IdentityId value converter ile eşlendiği için sorgu, alanların
        // .Value özelliklerine değil nesnelerin kendisine karşı yazılmalıdır.
        var existingYears = await masterContext.CompanyYears
            .AsNoTracking()
            .Where(cy => cy.Year == new Year(currentYear))
            .Select(cy => new { cy.CompanyId, cy.DatabaseName })
            .ToListAsync(cancellationToken);

        var existingByCompany = existingYears
            .GroupBy(y => y.CompanyId.Value)
            .ToDictionary(g => g.Key, g => g.First());

        int newDatabaseCount = companies.Count(c => !existingByCompany.ContainsKey(c.Id));

        if (newDatabaseCount == 0 && existingByCompany.Count == 0)
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
        int appliedMigrationTotal = 0;

        try
        {
            foreach (var company in companies)
            {
                bool isNew = !existingByCompany.TryGetValue(company.Id, out var existingYear);

                // Yeni yıl veritabanının adı şirkete göre üretilir; mevcut kaydın
                // adı ise bellidir ve asla değiştirilmez (master'daki kayıtla
                // eşleşmek zorunda).
                string databaseName = isNew
                    ? await databaseNameBuilder.SuggestAvailableAsync(company.Name, currentYear, cancellationToken)
                    : existingYear!.DatabaseName.Value;

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
                appliedMigrationTotal += result.AppliedMigrationCount;

                if (!isNew)
                {
                    continue;
                }

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
                newDatabaseCount > 0
                    ? $"{newDatabaseCount} yeni veritabanı · {existingByCompany.Count} güncellendi · {appliedMigrationTotal} migration"
                    : $"{existingByCompany.Count} veritabanı · {appliedMigrationTotal} migration"));

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
