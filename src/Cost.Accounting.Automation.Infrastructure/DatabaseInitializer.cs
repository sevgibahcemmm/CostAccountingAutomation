using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Companies.ValueObjects;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using Cost.Accounting.Automation.Infrastructure.Context;
using GenericRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Cost.Accounting.Automation.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        ApplicationDbContext dbContext = sp.GetRequiredService<ApplicationDbContext>();
        ICompanyRepository companyRepository = sp.GetRequiredService<ICompanyRepository>();
        IRoleRepository roleRepository = sp.GetRequiredService<IRoleRepository>();
        IUserRepository userRepository = sp.GetRequiredService<IUserRepository>();
        ICustomerRepository customerRepository = sp.GetRequiredService<ICustomerRepository>();
        ISupplierRepository supplierRepository = sp.GetRequiredService<ISupplierRepository>();
        IUnitOfWork unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        // 1. Veritabanı yoksa otomatik oluştur; varsa bekleyen migration'ları uygula
        await dbContext.Database.MigrateAsync();

        // 2. Eski seed'lerin CreatedBy alanında, hiçbir kullanıcıya işaret etmeyen
 
        await RepairOrphanAuditReferencesAsync(dbContext);

        // 3. Veritabanı boş mu? (Şirket yoksa seed yapılacak demektir)
        bool isEmpty = !await companyRepository.AnyAsync(i => i.Id != null);

        await SeedProductUnitTypesIfMissingAsync(dbContext, sp);
        await SeedTaxRatesIfMissingAsync(dbContext, sp);
        if (!isEmpty)
        {
            await SeedCustomersAndSuppliersIfMissingAsync(dbContext, sp);
            await EnsureAdminRolePermissionsAsync(dbContext, sp);
            return;
        }

        try
        {
            // ---------- ŞİRKETLER ----------
            Company merkezCompany = new(
                new Name("DEMİRCİ AÇIK CEZA İNFAZ KURUMU İŞYURDU MÜDÜRLÜĞÜ"),
                new TaxOffice("DEMİRCİ"),
                new TaxNumber("1234567890"),
                new Description("DEMİRCİ"),
                new Invoiceinformation("DACİK"),
                new Letterhead("DEMİRCİ AÇIK CEZA İNFAZ KURUMU İŞYURDU MÜDÜRLÜĞÜ"),
                new CompanyPrefix("08691234"),
                new Address("Manisa", "Demirci", "www"),
                new Contact("02161234567", "", "info@merkez.com"),
                new ExpenditureUnit("MERKEZ HARCAMA BİRİMİ", "45.05"),
                new AccountingUnit("MERKEZ MUHASEBE BİRİMİ", "45103"),
                true);

            Company anadoluCompany = new(
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

            Company egeCompany = new(
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

            // ---------- ROLLER ----------
            Role sysAdminRole = new(new Name("sys_admin"), true);
            Role accountingManagerRole = new(new Name("muhasebe_muduru"), true);
            Role accountantRole = new(new Name("muhasebe_elemani"), true);

            // ---------- ADMIN KULLANICI ----------
            User adminUser = new(
                new FirstName("Emrullah"),
                new LastName("AKPINAR"),
                new Email("admin@test.com"),
                new UserName("admin"),
                new Password("1"),
                merkezCompany.Id,
                sysAdminRole.Id,
                true);

            // CreatedBy alanı NOT NULL. Seed edilecek TÜM kayıtların CreatedBy'su admin
            // kullanıcının gerçek Id'sine işaret etsin; aksi halde GetAllWithAudit,
            // CreatedBy üzerinden Users'a inner-join yaptığı için liste boş döner.
            dbContext.SetSeedAdminUserId(adminUser.Id.Value);

            await companyRepository.AddAsync(merkezCompany);
            await companyRepository.AddAsync(anadoluCompany);
            await companyRepository.AddAsync(egeCompany);

            await roleRepository.AddAsync(sysAdminRole);
            await roleRepository.AddAsync(accountingManagerRole);
            await roleRepository.AddAsync(accountantRole);

            await unitOfWork.SaveChangesAsync();

            await userRepository.AddAsync(adminUser);

            await unitOfWork.SaveChangesAsync();

            // ---------- ÖRNEK KULLANICILAR ----------
            (string Name, string Email, IdentityId CompanyId, IdentityId RoleId)[] sampleUsers =
            {
                ("ahmet.yilmaz", "ahmet@test.com", merkezCompany.Id, accountingManagerRole.Id),
                ("ayse.kaya", "ayse@test.com", anadoluCompany.Id, accountantRole.Id),
                ("mehmet.demir", "mehmet@test.com", anadoluCompany.Id, accountantRole.Id),
                ("fatma.celik", "fatma@test.com", egeCompany.Id, accountantRole.Id),
            };

            foreach (var (name, email, companyId, roleId) in sampleUsers)
            {
                string firstName = char.ToUpperInvariant(name[0]) + name.Substring(1, name.IndexOf('.') - 1);

                await userRepository.AddAsync(
                    new User(
                        new FirstName(firstName),
                        new LastName("Soyad"),
                        new Email(email),
                        new UserName(name),
                        new Password("1"),
                        companyId,
                        roleId,
                        true));
            }

            await unitOfWork.SaveChangesAsync();

            // ---------- ÖRNEK MÜŞTERİLER / TEDARİKÇİLER ----------
            await SeedCustomersAndSuppliersAsync(dbContext, customerRepository, supplierRepository, unitOfWork);
            await EnsureAdminRolePermissionsAsync(dbContext, sp);
        }
        finally
        {
            dbContext.ClearSeedAdminUserId();
        }
    }

    private static async Task EnsureAdminRolePermissionsAsync(ApplicationDbContext dbContext, IServiceProvider sp)
    {
        PermissionService permissionService = sp.GetRequiredService<PermissionService>();
        IRoleRepository roleRepository = sp.GetRequiredService<IRoleRepository>();
        IUnitOfWork unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        Guid? adminId = await dbContext.Set<User>()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync();

        if (adminId is null)
        {
            return;
        }

        // Yeni Permission kayıtlarının CreatedBy alanını müşterek admin olarak işaretle
        dbContext.SetSeedAdminUserId(adminId.Value);
        try
        {
            await permissionService.EnsureAdminRoleHasAllPermissionsAsync(roleRepository, unitOfWork);
        }
        finally
        {
            dbContext.ClearSeedAdminUserId();
        }
    }

    private static async Task SeedProductUnitTypesIfMissingAsync(ApplicationDbContext dbContext, IServiceProvider sp)
    {
        IProductUnitTypeRepository unitTypeRepository = sp.GetRequiredService<IProductUnitTypeRepository>();
        IUnitOfWork unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        if (await unitTypeRepository.AnyAsync(i => i.Id != null))
        {
            return;
        }

        Guid? adminId = await dbContext.Set<User>()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync();

        if (adminId is null)
        {
            return;
        }

        dbContext.SetSeedAdminUserId(adminId.Value);
        try
        {
            foreach (string unitName in new[]
            {
                "Adet", "Kg", "Lt", "m", "m²", "m³", "Paket", "Koli", "Kutu", "Çuval", "Teneke"
            })
            {
                await unitTypeRepository.AddAsync(new ProductUnitType(new Name(unitName), true));
            }

            await unitOfWork.SaveChangesAsync();
        }
        finally
        {
            dbContext.ClearSeedAdminUserId();
        }
    }

    private static async Task SeedTaxRatesIfMissingAsync(ApplicationDbContext dbContext, IServiceProvider sp)
    {
        ITaxRateRepository taxRateRepository = sp.GetRequiredService<ITaxRateRepository>();
        IUnitOfWork unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        if (await taxRateRepository.AnyAsync(i => i.Id != null))
        {
            return;
        }

        Guid? adminId = await dbContext.Set<User>()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync();

        if (adminId is null)
        {
            return;
        }

        dbContext.SetSeedAdminUserId(adminId.Value);
        try
        {
            foreach ((string name, decimal rate) in new[]
            {
                ("KDV %0", 0m),
                ("KDV %10", 0.10m),
                ("KDV %20", 0.20m)
            })
            {
                await taxRateRepository.AddAsync(new TaxRate(new Name(name), rate, true));
            }

            await unitOfWork.SaveChangesAsync();
        }
        finally
        {
            dbContext.ClearSeedAdminUserId();
        }
    }

    private static async Task SeedCustomersAndSuppliersIfMissingAsync(ApplicationDbContext dbContext, IServiceProvider sp)
    {
        ICustomerRepository customerRepository = sp.GetRequiredService<ICustomerRepository>();
        ISupplierRepository supplierRepository = sp.GetRequiredService<ISupplierRepository>();
        IUnitOfWork unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        if (await customerRepository.AnyAsync(i => i.Id != null))
        {
            return;
        }

        await SeedCustomersAndSuppliersAsync(dbContext, customerRepository, supplierRepository, unitOfWork);
    }

    private static async Task SeedCustomersAndSuppliersAsync(
        ApplicationDbContext dbContext,
        ICustomerRepository customerRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        // CreatedBy NULL olamaz. Örnek kayıtları admin kullanıcıya bağla;
        // aksi halde GetAllWithAudit join'den dolayı liste boş döner.
        Guid? adminId = await dbContext.Set<User>()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync();

        if (adminId is null)
        {
            return;
        }

        dbContext.SetSeedAdminUserId(adminId.Value);
        try
        {
            Customer[] sampleCustomers =
            [
                new(
                    new Name("Anadolu Yapı Market A.Ş."),
                    new TaxOffice("Kadıköy"),
                    new TaxNumber("1234567801"),
                    new Contact("02161234501", "02161234502", "info@anadoluyapi.com"),
                    new Address("İstanbul", "Kadıköy", "Caferağa Mah. Bahariye Cad. No:12"),
                    new Description("İnşaat malzemeleri toptan-perakende"),
                    true),
                new(
                    new Name("Doğuş Tekstil Sanayi"),
                    new TaxOffice("Nilüfer"),
                    new TaxNumber("2345678902"),
                    new Contact("02241234503", "", "satis@dogustekstil.com"),
                    new Address("Bursa", "Nilüfer", "Organize Sanayi Bölgesi 3. Cadde No:45"),
                    new Description("Kumaş ve hazır giyim hammaddesi"),
                    true),
                new(
                    new Name("Karadeniz Tarım Ürünleri Ltd."),
                    new TaxOffice("Atakum"),
                    new TaxNumber("3456789013"),
                    new Contact("03621234504", "", "info@karadeniztarim.com"),
                    new Address("Samsun", "Atakum", "Yeşilyurt Mah. Sahil Yolu No:78"),
                    new Description("Fındık ve tarım ürünleri alımı"),
                    true),
                new(
                    new Name("Aegean Gayrimenkul Danışmanlık"),
                    new TaxOffice("Bornova"),
                    new TaxNumber("4567890124"),
                    new Contact("02321234505", "05551234505", "ofis@aegeandanismanlik.com"),
                    new Address("İzmir", "Bornova", "Kazımdirik Mah. Üniversite Cad. No:23"),
                    new Description("Gayrimenkul değerleme danışmanlığı"),
                    false),
            ];

            foreach (Customer customer in sampleCustomers)
            {
                await customerRepository.AddAsync(customer);
            }

            await unitOfWork.SaveChangesAsync();

            Supplier[] sampleSuppliers =
            [
                new(
                    new Name("Anadolu Tedarik A.Ş."),
                    new TaxOffice("Çankaya"),
                    new TaxNumber("5678901235"),
                    new Contact("03121234506", "03121234507", "tedarik@anadolutedarik.com"),
                    new Address("Ankara", "Çankaya", "Kızılay Mah. Atatürk Bulvarı No:56"),
                    new Description("Ofis malzemeleri ve kırtasiye toptan"),
                    true),
                new(
                    new Name("Ege Alüminyum Sanayi"),
                    new TaxOffice("Aliağa"),
                    new TaxNumber("6789012346"),
                    new Contact("02321234508", "", "satis@egealuminyum.com"),
                    new Address("İzmir", "Aliağa", "Sanayi Mah. 12. Cadde No:321"),
                    new Description("Alüminyum profil ve bileşen tedariki"),
                    true),
                new(
                    new Name("Marmara Kırtasiye Toptan"),
                    new TaxOffice("Kartal"),
                    new TaxNumber("7890123457"),
                    new Contact("02161234509", "", "siparis@marmarakirtasiye.com"),
                    new Address("İstanbul", "Kartal", "Yukarı Mah. Göksu Cad. No:88"),
                    new Description("Ofis ve kırtasiye ürünleri tedarikçisi"),
                    true),
                new(
                    new Name("Güney Enerji Sistemleri"),
                    new TaxOffice("Muratpaşa"),
                    new TaxNumber("8901234568"),
                    new Contact("02421234510", "05541234510", "info@guneveryenerji.com"),
                    new Address("Antalya", "Muratpaşa", "Kızılsaray Mah. Cumhuriyet Cad. No:34"),
                    new Description("Jeneratör ve enerji sistemleri"),
                    false),
            ];

            foreach (Supplier supplier in sampleSuppliers)
            {
                await supplierRepository.AddAsync(supplier);
            }

            await unitOfWork.SaveChangesAsync();
        }
        finally
        {
            dbContext.ClearSeedAdminUserId();
        }
    }

    private static async Task RepairOrphanAuditReferencesAsync(ApplicationDbContext dbContext)
    {
        Guid? adminId = await dbContext.Set<User>()
            .Where(u => u.UserName.Value == "admin")
            .Select(u => (Guid?)u.Id.Value)
            .FirstOrDefaultAsync();

        if (adminId is null)
        {
            return;
        }

        var parameter = new SqlParameter("@adminId", adminId.Value);

        foreach (IEntityType entityType in dbContext.Model.GetEntityTypes())
        {
            if (entityType.IsOwned() || entityType.BaseType is not null)
            {
                continue;
            }

            string? table = entityType.GetTableName();
            if (string.IsNullOrEmpty(table))
            {
                continue;
            }

            IProperty? createdBy = entityType.FindProperty(nameof(Entity.CreatedBy));
            IProperty? updatedBy = entityType.FindProperty(nameof(Entity.UpdatedBy));
            if (createdBy is null && updatedBy is null)
            {
                continue;
            }

            StoreObjectIdentifier storeId = StoreObjectIdentifier.Table(table, null);

            if (createdBy is not null)
            {
                string column = createdBy.GetColumnName(storeId) ?? nameof(Entity.CreatedBy);
                string sql = "UPDATE [" + table + "] SET [" + column + "] = @adminId WHERE [" + column + "] NOT IN (SELECT [Id] FROM [Users])";
                await dbContext.Database.ExecuteSqlRawAsync(sql, parameter);
            }

            if (updatedBy is not null)
            {
                string column = updatedBy.GetColumnName(storeId) ?? nameof(Entity.UpdatedBy);
                string sql = "UPDATE [" + table + "] SET [" + column + "] = @adminId WHERE [" + column + "] IS NOT NULL AND [" + column + "] NOT IN (SELECT [Id] FROM [Users])";
                await dbContext.Database.ExecuteSqlRawAsync(sql, parameter);
            }
        }
    }
}