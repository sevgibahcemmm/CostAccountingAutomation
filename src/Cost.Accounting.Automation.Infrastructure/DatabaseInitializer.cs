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

public static class DatabaseInitializer
{
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

    public static async Task<DatabaseFirstRunState> GetFirstRunStateAsync(
        IServiceProvider services)
    {
        return (await ProbeAsync(services)).State;
    }

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

                await Task.Delay(ProbeRetryDelay);
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }

        string errorMessage = lastException?.Message ?? "Bilinmeyen hata";

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
            // Log yazma hatası yutulur
        }

        return ProbeResult.Unreachable(errorMessage);
    }

    private sealed record ProbeResult(DatabaseFirstRunState State, string? Error)
    {
        public static ProbeResult Exists => new(DatabaseFirstRunState.Exists, null);

        public static ProbeResult Missing => new(DatabaseFirstRunState.Missing, null);

        public static ProbeResult Unreachable(string error) =>
            new(DatabaseFirstRunState.Unreachable, error);
    }

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

    internal static string BuildLockName(string masterDatabaseName) =>
        $"CAA:Provisioning:{masterDatabaseName}";

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
            System.Diagnostics.Debug.WriteLine(
                $"[DatabaseInitializer] Tablo sayısı okunamadı: {ex.Message}");

            return 0;
        }
    }

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
            var demirciCompany = new Company(
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

            //var focaCompany = new Company(
            //    new Name("Foça Açık Ceza İnfaz Kurumu İşyurdu Müdürlüğü"),
            //    new TaxOffice("İzmir"),
            //    new TaxNumber("4567890123"),
            //    new Description("İzmir"),
            //    new Invoiceinformation("İzmir"),
            //    new Letterhead("İzmir"),
            //    new CompanyPrefix("08691234"),
            //    new Address("İzmir", "Çankaya", "Kızılay"),
            //    new Contact("03124567890", "", "izmir@merkez.com"),
            //    new ExpenditureUnit("ANADOLU HARCAMA BİRİMİ", "2.2.2.2"),
            //    new AccountingUnit("ANADOLU MUHASEBE BİRİMİ", "2002"),
            //    true);

            //var canakkaleCompany = new Company(
            //    new Name("Çanakkale Açık Ceza İnfaz Kurumu İşyurdu Müdürlüğü"),
            //    new TaxOffice("Çanakkale"),
            //    new TaxNumber("7890123456"),
            //    new Description("Çanakkale"),
            //    new Invoiceinformation("Çanakkale"),
            //    new Letterhead("Çanakkale"),
            //    new CompanyPrefix("08691234"),
            //    new Address("Çanakkale", "Konak", "Alsancak"),
            //    new Contact("02327894561", "", "canakkale@merkez.com"),
            //    new ExpenditureUnit("Çanakkale HARCAMA BİRİMİ", "3.3.3.3"),
            //    new AccountingUnit("Çanakkale MUHASEBE BİRİMİ", "3003"),
            //    true);

            var sysAdminRole = new Role(new Name("sys_admin"), true);
            var accountingManagerRole = new Role(new Name("muhasebe_muduru"), true);
            var accountantRole = new Role(new Name("muhasebe_elemani"), true);

            var adminUser = new User(
                firstName: new FirstName("Emrullah"),
                lastName: new LastName("AKPINAR"),
                email: new Email("sevgibahcemm45@gmail.com"),
                userName: new UserName("sevgibahcemm"),
                password: new Password("61785"),
                companyId: demirciCompany.Id,
                roleId: sysAdminRole.Id,
                isActive: true,
                tRIdentityNumber: new TRIdentityNumber("27070677444")
            );

            // Sicil numarasını "AB" ile başlatacak şekilde atıyoruz[cite: 1]
            adminUser.SetRegistryNumber("AB1001");

            masterContext.SetSeedAdminUserId(adminUser.Id.Value);

            try
            {
                masterContext.Companies.AddRange(demirciCompany);
                masterContext.Roles.AddRange(sysAdminRole, accountingManagerRole, accountantRole);
                masterContext.Users.Add(adminUser);

                await masterContext.SaveChangesAsync();

                //(string UserName, string Email, IdentityId CompanyId, IdentityId RoleId, string RegistryNumber, string TCIdentity)[] sampleUsers =
                //[
                //    ("aaa", "aaa@test.com", demirciCompany.Id, accountingManagerRole.Id, "AB1002", "22222222220"),
                //    ("bbb", "bbb@test.com", demirciCompany.Id, accountantRole.Id, "AB1003", "33333333330"),
                //    ("ccc", "ccc@test.com", demirciCompany.Id, accountantRole.Id, "AB1004", "44444444440"),
                //    ("ddd", "ddd@test.com", demirciCompany.Id, accountantRole.Id, "AB1005", "55555555550"),
                //];

                //foreach (var sample in sampleUsers)
                //{
                //    string firstName = char.ToUpperInvariant(sample.UserName[0]) + sample.UserName[1..];

                //    var user = new User(
                //        firstName: new FirstName(firstName),
                //        lastName: new LastName("Soyad"),
                //        email: new Email(sample.Email),
                //        userName: new UserName(sample.UserName),
                //        password: new Password("1"),
                //        companyId: sample.CompanyId,
                //        roleId: sample.RoleId,
                //        isActive: true,
                //        tRIdentityNumber: new TRIdentityNumber(sample.TCIdentity)
                //    );

                    // Sicil numarasını "AB" ile başlatacak şekilde atıyoruz[cite: 1]
                    //user.SetRegistryNumber(sample.RegistryNumber);

                    //masterContext.Users.Add(user);
                //}

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

        Guid? starterAdminId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "sevgibahcemm")
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

            await permissionService.EnsureAnnouncementOnlyForAdminAsync(
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

        Guid? adminId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "sevgibahcemm")
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
            .Where(u => u.UserName.Value == "sevgibahcemm")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminId is null)
        {
            return;
        }

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