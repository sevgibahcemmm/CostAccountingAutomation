using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Employees;
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
    IClaimContext claimContext,
    DatabaseFilePathResolver filePaths) : IAccountingYearProvisioner
{
    private static readonly string[] UnitTypeNames =
        ["Adet", "Kg", "Lt", "Metre (m)", "Metre Kare (m²)", "Metre Küp (m³)", "Paket", "Takım", "Koli", "Kutu", "Çuval", "Teneke"];

    private static readonly (string Name, decimal Rate)[] TaxRateSeeds =
        [("KDV % 00", 0m), ("KDV % 01", 0.01m), ("KDV % 10", 0.10m), ("KDV % 20", 0.20m)];

    /// <summary>
    /// Rapor imza bloklarında kullanılan standart yetkili görevler. Daha önce
    /// enum olan görevler artık veritabanı kaydıdır; bu liste yeni yıl
    /// veritabanlarında başlangıç değerlerini sağlar. Kullanıcı sonradan
    /// istediği görevi "Yetkili Görevler" ekranından ekleyebilir.
    ///
    /// <para>
    /// Sıra numarası, imza bloklarının okunması içindir: kurum müdürü en üstte,
    /// sayım/kontrol görevleri en altta görünür.
    /// </para>
    /// </summary>
    private static readonly SigningRoleSeed[] SigningRoleSeeds =
    [
        new("İşyurdu Müdürü", "Kurumun en üst yetkilisi; maliyet pusulası onay imzası", false, 10),
        new("Atölye Şefi", "Üretimin yapıldığı atölyenin şefi; mamül beyanı ve maliyet pusulası imzası", true, 20),
        new("Taşınır Kayıt Yetkilisi", "Taşınır işlem fişi giriş/çıkış kaydını yapan yetkili", false, 30),
        new("Muhasebe Yetkilisi", "Mali kayıtları tutan ve onaylayan muhasebe yetkilisi", false, 40),
        new("Muhasebe Memuru", "Tüketim fişinde malzemeyi teslim alan muhasebe memuru", false, 45),
        new("Harcama Yetkilisi", "Harcama onaylayan yetkili", false, 50),
        new("Gerçekleştirme Görevlisi", "Belgeyi düzenleyen ve teslim eden sabit kadro görevlisi", false, 60),
        new("Katip", "Stok sayımını fiilen gerçekleştiren personel", false, 70),
        new("Satınalma Memuru", "Satınalma işini yapan ve sonucunu kontrol edip onaylayan personel", false, 80)
    ];

    /// <summary>Yetkili görev tohum satırı.</summary>
    private sealed record SigningRoleSeed(
        string Name,
        string Description,
        bool RequiresWorkshop,
        int SortOrder);

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

        // Veritabanının varlığı doğrudan bağlanarak sorulmaz: veritabanı henüz
        // yokken yapılan başarısız bağlanma, bağlantı havuzunda bayat durum
        // bırakır. Veritabanı oluşturulduktan sonra EF'in Exists kontrolü bu
        // bayat oturumdan 'veritabanı yok' (4060) alıp yanlışlıkla CREATE
        // DATABASE çalıştırır (1801). Varlık master katalog - DB_ID ile sorulur.
        bool exists = await SqlDatabaseCreator.ExistsAsync(
            dbSelector.BuildConnectionString(databaseName),
            databaseName,
            cancellationToken);

        if (!exists)
        {
            // Veritabanı yokken dosyaların Data klasörüne açılması için önce
            // CREATE DATABASE çalıştırılır; MigrateAsync yalnız şemayı uygular.
            await SqlDatabaseCreator.EnsureCreatedAsync(
                dbSelector.BuildConnectionString(databaseName),
                databaseName,
                filePaths,
                cancellationToken);
        }
        else
        {
            // Ad sunucuda kayıtlı ama durum bozuksa (örneğin dosyalar kayıp →
            // RECOVERY_PENDING) EF veritabanını 'yok' sanıp CREATE DATABASE
            // çalıştırarak 1801 yanlış hatası verir; burada açıkça raporlanır.
            await SqlDatabaseCreator.EnsureAccessibleOrThrowAsync(
                dbSelector.BuildConnectionString(databaseName),
                databaseName,
                cancellationToken);
        }

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
            .Where(u => u.UserName.Value == "sevgibahcemm")
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
                + await SeedTaxRatesAsync(context, progress, cancellationToken)
                + await SeedSigningRolesAsync(context, progress, cancellationToken);

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

    /// <summary>
    /// Rapor imza bloklarının ihtiyaç duyduğu standart yetkili görevleri tohumlar.
    ///
    /// <para>
    /// Tabloda hiç kayıt yoksa hepsi eklenir. Kayıt varsa dokunulmaz; sadece
    /// <c>DuplicateKey</c> boş kalan görevler (enum'dan taşınan kayıtlar)
    /// uygulama kuralıyla tamamlanır. Kullanıcının sildiği görev satırı silik
    /// hâlde durduğu için yeniden eklenmez.
    /// </para>
    /// </summary>
    private static async Task<int> SeedSigningRolesAsync(
        ApplicationDbContext context,
        IProgress<DatabaseProvisionProgress>? progress,
        CancellationToken cancellationToken)
    {
        List<EmployeeSigningRole> existing = await context.Set<EmployeeSigningRole>()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        // Aynı adı taşıyan kayıtlar teorik olarak olamaz (DuplicateKey kuralı);
        // yine de ilk kayıt seçilerek ToDictionary hatası önlenir.
        var byName = existing
            .GroupBy(r => r.Name.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        int added = 0;
        bool updated = false;

        foreach (SigningRoleSeed seed in SigningRoleSeeds)
        {
            if (byName.TryGetValue(seed.Name, out EmployeeSigningRole? role))
            {
                // DuplicateKey uygulama tarafından üretilir; migration bu alanı
                // boş bırakır çünkü .NET'in büyük harf dönüşümü ile SQL
                // UPPER() sonuçları Türkçe collation'da ayrışabilir.
                if (string.IsNullOrWhiteSpace(role.DuplicateKey))
                {
                    role.SetName(new Name(role.Name.Value));
                    context.Set<EmployeeSigningRole>().Update(role);
                    updated = true;
                }

                continue;
            }

            context.Set<EmployeeSigningRole>().Add(
                new EmployeeSigningRole(
                    new Name(seed.Name),
                    seed.Description,
                    seed.RequiresWorkshop,
                    seed.SortOrder));

            added++;
        }

        if (added > 0 || updated)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return added;
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
