using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Options;
using Cost.Accounting.Automation.Infrastructure.Services;
using GenericRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Cost.Accounting.Automation.Infrastructure;

public static class ServiceRegistrar
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DatabaseNamingOptions>(configuration.GetSection(DatabaseNamingOptions.SectionName));
        services.Configure<DatabaseProvisioningOptions>(configuration.GetSection(DatabaseProvisioningOptions.SectionName));
        services.Configure<UpdateOptions>(configuration.GetSection(UpdateOptions.SectionName));
        services.Configure<DatabaseFilesOptions>(configuration.GetSection(DatabaseFilesOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        // Veritabanı dosyalarının (mdf/ldf) konacağı Data klasörü. Veritabanı
        // oluşturma adımlarında (master ve yıl veritabanları) dosya yolları bu
        // klasöre göre üretilir; ayrıca uygulama açılışında klasör oluşturulur.
        services.AddSingleton<DatabaseFilePathResolver>();

        string masterConnectionString = RequireConnectionString(configuration, "Master");

        // Yıl veritabanlarının bağlantı dizesi şablonundan üretilir. Şablonun
        // "Initial Catalog" değeri master'a bırakılır: yıl seçimi yapılmadan
        // önce yanlışlıkla ikinci bir veritabanı oluşturulmasın.
        string yearTemplateConnectionString =
            configuration.GetConnectionString("SqlServer") ?? masterConnectionString;

        string masterDatabaseName = ReadDatabaseName(masterConnectionString);

        services.AddSingleton<AccountingDbSelector>(
            new AccountingDbSelector(yearTemplateConnectionString, masterDatabaseName));
        services.AddSingleton<IAccountingDbSelector>(
            sp => sp.GetRequiredService<AccountingDbSelector>());

        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            var selector = sp.GetRequiredService<IAccountingDbSelector>();
            UseSqlServerWithRetry(opt, selector.GetConnectionString());
            opt.AddInterceptors(new Diagnostics.SqlTimingInterceptor());
        });

        services.AddDbContextFactory<ApplicationDbContext>((sp, opt) =>
        {
            var selector = sp.GetRequiredService<IAccountingDbSelector>();
            UseSqlServerWithRetry(opt, selector.GetConnectionString());
        });

        services.AddDbContext<MasterDbContext>(opt =>
        {
            UseSqlServerWithRetry(opt, masterConnectionString);
            opt.AddInterceptors(new Diagnostics.SqlTimingInterceptor());
        });

        services.AddMemoryCache();

        services.AddScoped<IBarcodeGeneratorService, BarcodeGeneratorService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDuplicateCheckService, DuplicateCheckService>();

        services.Scan(action => action
            .FromAssemblies(typeof(ServiceRegistrar).Assembly)
            .AddClasses(publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IMasterUnitOfWork>(sp => sp.GetRequiredService<MasterDbContext>());

        return services;
    }

    private static void UseSqlServerWithRetry(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(
            connectionString,
            sql => sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null));
    }

    private static string RequireConnectionString(IConfiguration configuration, string name)
    {
        string? value = configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"appsettings.json içinde ConnectionStrings:{name} tanımlı değil.");
        }

        return value;
    }

    internal static string ReadDatabaseName(string connectionString)
    {
        try
        {
            return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException("Bağlantı dizesi çözümlenemedi.", ex);
        }
    }
}
