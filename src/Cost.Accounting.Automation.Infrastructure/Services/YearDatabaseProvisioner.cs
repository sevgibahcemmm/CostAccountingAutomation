using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Şirket + mali yıl için ayrı iş veritabanını oluşturur, şemasını günceller ve
/// yıl açılışında gereken temel kayıtları tohumlar.
///
/// Yıl veritabanına DI ile kayıtlı <see cref="ApplicationDbContext"/> üzerinden
/// değil, doğrudan kurulan seçeneklerle bağlanılır: veritabanı adı çalışma
/// anında belirlendiği için seçimden önce açılmış kapsamların seçenek önbelleğine
/// güvenilmemelidir.
/// </summary>
internal sealed class YearDatabaseProvisioner(
    MasterDbContext masterContext,
    IAccountingDbSelector dbSelector,
    IClaimContext claimContext) : IAccountingYearProvisioner
{
    private static readonly string[] UnitTypeNames =
        ["Adet", "Kg", "Lt", "m", "m²", "m³", "Paket", "Koli", "Kutu", "Çuval", "Teneke"];

    private static readonly (string Name, decimal Rate)[] TaxRateSeeds =
        [("KDV % 00", 0m), ("KDV % 01", 0.01m), ("KDV % 10", 0.10m), ("KDV % 20", 0.20m)];

    public async Task<AccountingYearProvisionResult> EnsureDatabaseAsync(
        IdentityId companyId,
        int year,
        string databaseName,
        CancellationToken cancellationToken = default,
        IProgress<DatabaseProvisionProgress>? progress = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(dbSelector.BuildConnectionString(databaseName));
        optionsBuilder.AddInterceptors(new SqlTimingInterceptor());

        await using var yearContext = new ApplicationDbContext(optionsBuilder.Options, claimContext);

        bool exists = await yearContext.Database.CanConnectAsync(cancellationToken);

        // Veritabanı yeni oluşturulduysa tüm migration'lar uygulanacak;
        // mevcutsa yalnız bekleyenler sayılır.
        int appliedMigrationCount = exists
            ? (await yearContext.Database.GetPendingMigrationsAsync(cancellationToken)).Count()
            : yearContext.Database.GetMigrations().Count();

        if (appliedMigrationCount > 0)
        {
            await yearContext.Database.MigrateAsync(cancellationToken);
        }

        AccountingYearProvisionSeedResult seeded =
            await SeedAsync(yearContext, progress, cancellationToken);

        return new AccountingYearProvisionResult
        {
            DatabaseName = databaseName,
            Created = !exists,
            AppliedMigrationCount = appliedMigrationCount,
            SeededChartOfAccountCount = seeded.ChartOfAccountCount,
            SeededReferenceCount = seeded.ReferenceCount,
            SeededSampleRecordCount = seeded.SampleRecordCount
        };
    }

    /// <summary>
    /// Yıl veritabanındaki denetim alanları (CreatedBy) null olamaz. Kullanıcı
    /// kayıtları master'da tutulduğu için tohum kayıtları master'daki admin
    /// kullanıcıya bağlanır.
    ///
    /// Her tohum adımı eklenen satır sayısını döner ve ilerleme bildirimi
    /// verilmişse arayüze iletir; böylece kurulum sihirbazı adımları tek tek
    /// tik işaretiyle gösterebilir.
    /// </summary>
    private async Task<AccountingYearProvisionSeedResult> SeedAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        Guid? adminUserId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminUserId is null)
        {
            return new AccountingYearProvisionSeedResult(0, 0, 0);
        }

        context.SetSeedAdminUserId(adminUserId.Value);
        try
        {
            int chartOfAccountCount = await SeedChartOfAccountsAsync(context, progress, cancellationToken);

            int referenceCount =
                  await SeedProductUnitTypesAsync(context, progress, cancellationToken)
                + await SeedTaxRatesAsync(context, progress, cancellationToken);

            int sampleRecordCount =
                await SeedCustomersAndSuppliersAsync(context, progress, cancellationToken);

            return new AccountingYearProvisionSeedResult(
                chartOfAccountCount,
                referenceCount,
                sampleRecordCount);
        }
        finally
        {
            context.ClearSeedAdminUserId();
        }
    }

    /// <summary>
    /// Standart UFRS hesap planını gömülü Excel kaynağından tohumlar. Tablo
    /// boş değilse dokunulmaz; böylece kullanıcının içe aktardığı veya elle
    /// eklediği hesaplar korunur.
    /// </summary>
    private static async Task<int> SeedChartOfAccountsAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedChartOfAccounts,
            DatabaseProvisionStepState.Running));

        int added = await ChartOfAccountPlanSeeder.SeedAsync(context, cancellationToken);

        if (added > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[Seed] Hesap planı tohumlandı: {added} hesap eklendi.");
        }

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedChartOfAccounts,
            added > 0 ? DatabaseProvisionStepState.Completed : DatabaseProvisionStepState.Skipped,
            added > 0 ? $"{added:N0} hesap" : "hesap planı zaten doluydu"));

        return added;
    }

    private static async Task<int> SeedProductUnitTypesAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedUnitsAndTaxRates,
            DatabaseProvisionStepState.Running));

        if (await context.Set<ProductUnitType>().AnyAsync(cancellationToken))
        {
            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedUnitsAndTaxRates,
                DatabaseProvisionStepState.Running,
                "birim cinsleri zaten tanımlı"));

            return 0;
        }

        foreach (string unitName in UnitTypeNames)
        {
            context.Set<ProductUnitType>().Add(new ProductUnitType(new Name(unitName), true));
        }

        await context.SaveChangesAsync(cancellationToken);

        return UnitTypeNames.Length;
    }

    private static async Task<int> SeedTaxRatesAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (await context.Set<TaxRate>().AnyAsync(cancellationToken))
        {
            return 0;
        }

        foreach ((string name, decimal rate) in TaxRateSeeds)
        {
            context.Set<TaxRate>().Add(new TaxRate(new Name(name), rate, true));
        }

        await context.SaveChangesAsync(cancellationToken);

        return TaxRateSeeds.Length;
    }

    private static async Task<int> SeedCustomersAndSuppliersAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedSampleRecords,
            DatabaseProvisionStepState.Running));

        bool hasCustomers = await context.Set<Customer>().AnyAsync(cancellationToken);
        bool hasSuppliers = await context.Set<Supplier>().AnyAsync(cancellationToken);

        if (hasCustomers && hasSuppliers)
        {
            progress?.Report(new DatabaseProvisionProgress(
                DatabaseProvisionStep.SeedSampleRecords,
                DatabaseProvisionStepState.Skipped,
                "sanal kayıtlar zaten mevcut"));

            return 0;
        }

        int added = 0;

        if (!hasCustomers)
        {
            context.Set<Customer>().AddRange(
                new Customer(
                    new Name("Anadolu Yapı Market A.Ş."),
                    new TaxOffice("Kadıköy"),
                    new TaxNumber("1234567801"),
                    new Contact("02161234501", "02161234502", "info@anadoluyapi.com"),
                    new Address("İstanbul", "Kadıköy", "Caferaşa Mah. Bahariye Cad. No:12"),
                    new Description("İnşaat malzemeleri toptan-perakende"),
                    true),
                new Customer(
                    new Name("Doğuş Tekstil Sanayi"),
                    new TaxOffice("Nilüfer"),
                    new TaxNumber("2345678902"),
                    new Contact("02241234503", "", "satis@dogustekstil.com"),
                    new Address("Bursa", "Nilüfer", "Organize Sanayi Bölgesi 3. Cadde No:45"),
                    new Description("Kumaş ve hazır giyim hammaddesi"),
                    true));

            added += 2;
        }

        if (!hasSuppliers)
        {
            context.Set<Supplier>().AddRange(
                new Supplier(
                    new Name("Anadolu Tedarik A.Ş."),
                    new TaxOffice("Çankaya"),
                    new TaxNumber("5678901235"),
                    new Contact("03121234506", "03121234507", "tedarik@anadolutedarik.com"),
                    new Address("Ankara", "Çankaya", "Kızılay Mah. Atatürk Bulvarı No:56"),
                    new Description("Ofis malzemeleri ve kırtasiye toptan"),
                    true),
                new Supplier(
                    new Name("Ege Alüminyum Sanayi"),
                    new TaxOffice("Aliağa"),
                    new TaxNumber("6789012346"),
                    new Contact("02321234508", "", "satis@egealuminyum.com"),
                    new Address("İzmir", "Aliağa", "Sanayi Mah. 12. Cadde No:321"),
                    new Description("Alüminyum profil ve bileşen tedariki"),
                    true));

            added += 2;
        }

        await context.SaveChangesAsync(cancellationToken);

        progress?.Report(new DatabaseProvisionProgress(
            DatabaseProvisionStep.SeedSampleRecords,
            added > 0 ? DatabaseProvisionStepState.Completed : DatabaseProvisionStepState.Skipped,
            added > 0 ? $"{added} müşteri / tedarikçi" : "sanal kayıtlar zaten mevcut"));

        return added;
    }

    /// <summary>Yıl veritabanına bu çağrıda eklenen tohum satırları.</summary>
    private sealed record AccountingYearProvisionSeedResult(
        int ChartOfAccountCount,
        int ReferenceCount,
        int SampleRecordCount);
}
