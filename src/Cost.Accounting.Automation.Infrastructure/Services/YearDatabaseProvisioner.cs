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
        CancellationToken cancellationToken = default)
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

        await SeedAsync(yearContext, cancellationToken);

        return new AccountingYearProvisionResult
        {
            DatabaseName = databaseName,
            Created = !exists,
            AppliedMigrationCount = appliedMigrationCount
        };
    }

    /// <summary>
    /// Yıl veritabanındaki denetim alanları (CreatedBy) null olamaz. Kullanıcı
    /// kayıtları master'da tutulduğu için tohum kayıtları master'daki admin
    /// kullanıcıya bağlanır.
    /// </summary>
    private async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        Guid? adminUserId = await masterContext.Users
            .AsNoTracking()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminUserId is null)
        {
            return;
        }

        context.SetSeedAdminUserId(adminUserId.Value);
        try
        {
            await SeedChartOfAccountsAsync(context, cancellationToken);
            await SeedProductUnitTypesAsync(context, cancellationToken);
            await SeedTaxRatesAsync(context, cancellationToken);
            await SeedCustomersAndSuppliersAsync(context, cancellationToken);
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
    private static async Task SeedChartOfAccountsAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        int added = await ChartOfAccountPlanSeeder.SeedAsync(context, cancellationToken);
        if (added > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[Seed] Hesap planı tohumlandı: {added} hesap eklendi.");
        }
    }

    private static async Task SeedProductUnitTypesAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        if (await context.Set<ProductUnitType>().AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (string unitName in UnitTypeNames)
        {
            context.Set<ProductUnitType>().Add(new ProductUnitType(new Name(unitName), true));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTaxRatesAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        if (await context.Set<TaxRate>().AnyAsync(cancellationToken))
        {
            return;
        }

        foreach ((string name, decimal rate) in TaxRateSeeds)
        {
            context.Set<TaxRate>().Add(new TaxRate(new Name(name), rate, true));
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCustomersAndSuppliersAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        bool hasCustomers = await context.Set<Customer>().AnyAsync(cancellationToken);
        bool hasSuppliers = await context.Set<Supplier>().AnyAsync(cancellationToken);

        if (hasCustomers && hasSuppliers)
        {
            return;
        }

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
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
