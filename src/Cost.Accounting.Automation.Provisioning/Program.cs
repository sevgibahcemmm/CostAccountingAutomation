using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cost.Accounting.Automation.Provisioning;

/// <summary>
/// Veritabanı hazırlık aracı.
/// </summary>
/// <remarks>
/// <para>
/// Bu araç, <b>tek bir kez</b> sunucuda veritabanını hazırlamak için
/// kullanılır: master veritabanı oluşturulur, migration'lar uygulanır, roller
/// ve kullanıcılar tohumlanır, şirketler için mali yıl iş veritabanları açılır.
/// </para>
/// <para>
/// WinForms uygulaması bu işi <b>yapmaz</b>. Uygulama yalnızca şemanın güncel
/// olduğunu salt okunur doğrular. Bunun nedeni eşzamanlılıktır: merkezi bir
/// SQL Server'a bağlanan on istemci aynı anda açıldığında her biri migration
/// çalıştırmaya ve tohumlama yapmaya çalışırsa migration geçmişi tablosunda
/// çakışma, çift kayıt ve "veritabanı zaten var" hataları oluşur.
/// </para>
/// <para>
/// Kurulum işleminin kendisi yine de <c>sp_getapplock</c> ile korunur; iki
/// yönetici aynı anda komutu çalıştırırsa ikincisi bekler, işlem bitince
/// "bir şey yapılacak iş yok" mesajıyla çıkar.
/// </para>
/// </remarks>
internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitFailure = 1;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();

            return args.Length == 0 ? ExitFailure : ExitSuccess;
        }

        string command = args[0].ToLowerInvariant();
        string? settingsPath = ReadOption(args, "--settings");

        IConfiguration configuration = BuildConfiguration(settingsPath);

        try
        {
            return command switch
            {
                "provision" => await ProvisionAsync(configuration),
                "status" => await StatusAsync(configuration),
                "can-connect" => await CanConnectAsync(configuration),
                "set-version" => await SetVersionAsync(configuration, args),
                _ => UnknownCommand(command)
            };
        }
        catch (ProvisioningLockTimeoutException ex)
        {
            Console.Error.WriteLine($"HATA: {ex.Message}");
            Console.Error.WriteLine("      Başka bir kurulum oturumu sürüyor. Lütfen onun bitmesini bekleyip yeniden deneyin.");

            return ExitFailure;
        }
        catch (Exception ex)
        {
            // Yönetici aracı için ayrıntı kritiktir: ekranda "HATA: <mesaj>"
            // görmek teşhis için yeterli değildir. Yığın izi de basılır.
            Console.Error.WriteLine($"HATA: {ex.Message}");

            if (ex.InnerException is { } inner)
            {
                Console.Error.WriteLine($"      iç hata: {inner.Message}");
            }

            Console.Error.WriteLine();
            Console.Error.WriteLine("Ayrıntılı izlem:");

            foreach (string line in (ex.ToString() ?? string.Empty).Split(Environment.NewLine))
            {
                Console.Error.WriteLine($"  {line}");
            }

            return ExitFailure;
        }
    }

    private static async Task<int> ProvisionAsync(IConfiguration configuration)
    {
        using var services = BuildServiceProvider(configuration);
        var progress = new ConsoleProgressReporter();

        Console.WriteLine("Veritabanı hazırlanıyor...");
        Console.WriteLine();

        await DatabaseInitializer.InitializeAsync(services, progress);

        Console.WriteLine();
        Console.WriteLine("Hazırlık tamamlandı.");

        // Kurulumdan sonra şemanın gerçekten güncel olduğunu doğrula; böylece
        // çağıran taraf yalnızca "başarılı" mesajına değil, doğrulanmış duruma
        // bakar.
        return await PrintStatusAsync(services) ? ExitSuccess : ExitFailure;
    }

    private static async Task<int> StatusAsync(IConfiguration configuration)
    {
        using var services = BuildServiceProvider(configuration);

        return await PrintStatusAsync(services) ? ExitSuccess : ExitFailure;
    }

    private static async Task<bool> PrintStatusAsync(IServiceProvider services)
    {
        DatabaseSchemaCheckResult result = await DatabaseInitializer.CheckSchemaAsync(services);

        switch (result.State)
        {
            case DatabaseSchemaState.Ready:
                Console.WriteLine("Durum: HAZIR - şema güncel.");
                return true;

            case DatabaseSchemaState.SchemaOutdated:
                Console.WriteLine($"Durum: GÜNCEL DEĞİL - {result.PendingMigrationCount} bekleyen migration.");
                Console.WriteLine();
                Console.WriteLine("Bekleyen migration'lar:");
                foreach (string migration in result.PendingMigrations)
                {
                    Console.WriteLine($"  - {migration}");
                }

                Console.WriteLine();
                Console.WriteLine("Çözüm: yönetici olarak 'caa-provision provision' komutunu çalıştırın.");

                return false;

            case DatabaseSchemaState.DatabaseMissing:
                Console.WriteLine($"Durum: VERİTABANI YOK - {result.Detail}");
                Console.WriteLine();
                Console.WriteLine("Çözüm: yönetici olarak 'caa-provision provision' komutunu çalıştırın.");

                return false;

            default:
                Console.WriteLine($"Durum: ERİŞİLEMEDİ - {result.Detail}");
                Console.WriteLine();
                Console.WriteLine("Çözüm: bağlantı dizesini ve sunucuya erişimi kontrol edin.");

                return false;
        }
    }

    private static async Task<int> SetVersionAsync(IConfiguration configuration, string[] args)
    {
        string version = RequireOption(args, "--version", "set-version");
        string setupPath = RequireOption(args, "--setup-path", "set-version");
        string notes = ReadOption(args, "--notes") ?? $"Surum {version}";
        bool mandatory = args.Contains("--mandatory", StringComparer.OrdinalIgnoreCase);

        string? masterConnection = configuration.GetConnectionString("Master")
            ?? throw new InvalidOperationException("Master baglanti dizesi yapilandirilmemis.");

        var builder = new SqlConnectionStringBuilder(masterConnection)
        {
            ConnectTimeout = 15
        };

        const string sql = """
            MERGE dbo.AppReleases AS target
            USING (VALUES (1)) AS source(Dummy) ON target.[Version] = @version
            WHEN MATCHED THEN
                UPDATE SET
                    SetupPath   = @setupPath,
                    Notes       = @notes,
                    Mandatory   = @mandatory,
                    PublishedAt = SYSUTCDATETIME(),
                    IsActive    = 1
            WHEN NOT MATCHED THEN
                INSERT (Id, [Version], SetupPath, Notes, Mandatory, PublishedAt, IsActive)
                VALUES (NEWID(), @version, @setupPath, @notes, @mandatory, SYSUTCDATETIME(), 1);
            """;

        await using (var connection = new SqlConnection(builder.ConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 15;

            command.Parameters.AddWithValue("@version", version);
            command.Parameters.AddWithValue("@setupPath", setupPath);
            command.Parameters.AddWithValue("@notes", (object?)notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@mandatory", mandatory);

            await command.ExecuteNonQueryAsync();
        }

        Console.WriteLine($"Surum kaydedildi: {version}");
        Console.WriteLine($"  Setup: {setupPath}");
        Console.WriteLine($"  Not  : {notes}   Mandatory: {mandatory}");

        return ExitSuccess;
    }

    private static string RequireOption(string[] args, string name, string command)
    {
        return ReadOption(args, name)
            ?? throw new InvalidOperationException($"{command} icin '{name}' parametresi zorunludur.");
    }

    private static async Task<int> CanConnectAsync(IConfiguration configuration)
    {
        var builder = new SqlConnectionStringBuilder(
            configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer baglanti dizesi yok."));

        // Kurulum sihirbazi erisim denetimini kisa tutmak ister; uzun baglanti
        // zamani kurulumu dakikalarca durdurur.
        if (builder.ConnectTimeout <= 0 || builder.ConnectTimeout > 10)
        {
            builder.ConnectTimeout = 10;
        }

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            await command.ExecuteScalarAsync();

            Console.WriteLine("SQL Server erisilebilir.");
            return ExitSuccess;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERISILEMEDI: {ex.Message}");
            return ExitFailure;
        }
    }

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        ServiceCollection services = new();

        services.AddSingleton(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddLogging();

        // Kurulum oturumunda giriş yapan kullanıcı yoktur. Tohumlama kayıtlarının
        // CreatedBy alanı NOT NULL olduğu için SessionClaimContext tekil kayıt
        // olarak kaydedilir; yönetici kullanıcısı kimliği ayrıca
        // SetSeedAdminUserId ile atanır.
        services.AddSingleton<SessionClaimContext>();
        services.AddSingleton<IClaimContext>(sp => sp.GetRequiredService<SessionClaimContext>());

        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(string? settingsPath)
    {
        string basePath = settingsPath is null
            ? AppContext.BaseDirectory
            : Path.GetDirectoryName(Path.GetFullPath(settingsPath)) ?? AppContext.BaseDirectory;

        string fileName = settingsPath is null ? "appsettings.json" : Path.GetFileName(settingsPath);

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            // WinForms uygulamasiyla ayni oncelik sirasi. Ayar dosyasiyla ayni
            // klasorde bulunur, yoksa sunucu ayarlari iki arac arasinda
            // ayrisir ve yonetici yanlis veritabanini hazirlar.
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string? ReadOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static bool IsHelp(string argument) =>
        argument is "-h" or "--help" or "-?" or "/?" or "help";

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Bilinmeyen komut: '{command}'");
        Console.WriteLine();
        PrintUsage();

        return ExitFailure;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Cost Accounting Automation - veritabanı hazırlık aracı

            Kullanım:
              caa-provision provision   Sunucudaki veritabanını hazırlar (idempotent).
                                       Master veritabanını oluşturur, migration'ları
                                       uygular, rol/kullanıcıları tohumlar ve şirketler
                                       için içinde bulunulan mali yılın iş veritabanlarını açar.
                                       Yalnızca sunucu yöneticisi tarafından çalıştırılmalıdır.

              caa-provision status      Sunucudaki şemanın güncel olup olmadığını
                                       salt okunur olarak raporlar. Hiçbir şey yazmaz.

              caa-provision can-connect   SQL Server'a erişilip erişilemediğini
                                       denetler (yalnızca bağlanır, hiçbir şey yazmaz).
                                       0 = erişilebilir, 1 = erişilemedi. Kurulum
                                       sihirbazı sunucuda SQL yoksa otomatik kurulum
                                       kararını bu komutun çıkış koduyla verir.

              caa-provision set-version   AppReleases tablosuna yeni sürüm kaydı
                                       yazar (idempotent MERGE). İstemci açılışta
                                       bu kaydı okuyup güncelleme bildirir.
                                       Zorunlu parametreler:
                                         --version 1.0.1
                                         --setup-path \\192.168.1.5\Paylasim\Setup-1.0.1.exe
                                       İsteğe bağlı: --notes "Sürüm notu" --mandatory

            Seçenekler:
              --settings <yol>          appsettings.json dosyasının yolu.
                                       Verilmezse araç klasöründeki appsettings.json okunur.

            Yapılandırma önceliği (düşükten yükseğe):
              1. appsettings.json            izlenen, gizli olmayan varsayılanlar
              2. appsettings.Local.json      makineye özgü, git tarafından izlenmez
              3. Ortam değişkenleri          ConnectionStrings__Master, Jwt__SecretKey

            appsettings.Local.json dosyası verilirse ayar dosyasıyla AYNI klasörde
            aranır. Böylece yönetici aracı ile uygulama aynı sunucuya bağlanır.
            """);
    }

    /// <summary>
    /// Kurulum adımlarını konsola yazar.
    /// </summary>
    /// <remarks>
    /// İlerleme yalnızca adım sonuçlarını basar; ara adımlarda tekrarlanan
    /// <c>Running</c> bildirimleri (birden çok kurum için aynı adım) gürültü
    /// yaratmasın diye <c>Running</c> durumu yalnızca adımın ilk kez
    /// göründüğünde basılır.
    /// </remarks>
    private sealed class ConsoleProgressReporter : IProgress<DatabaseProvisionProgress>
    {
        private readonly Dictionary<DatabaseProvisionStep, bool> _started = [];

        public void Report(DatabaseProvisionProgress value)
        {
            switch (value.State)
            {
                case DatabaseProvisionStepState.Running
                    when !_started.TryAdd(value.Step, true):
                    return;

                case DatabaseProvisionStepState.Completed:
                case DatabaseProvisionStepState.Failed:
                case DatabaseProvisionStepState.Skipped:
                    _started.Remove(value.Step);
                    break;
            }

            string marker = value.State switch
            {
                DatabaseProvisionStepState.Completed => "[+]",
                DatabaseProvisionStepState.Skipped => "[=]",
                DatabaseProvisionStepState.Failed => "[!]",
                _ => "[-]"
            };

            string detail = value.Detail is null ? string.Empty : $"  {value.Detail}";

            string line = value.State == DatabaseProvisionStepState.Failed && value.Error is { } error
                ? $"{marker} {value.Step} - HATA: {error}"
                : $"{marker} {value.Step}{detail}";

            if (value.State == DatabaseProvisionStepState.Failed)
            {
                Console.Error.WriteLine(line);
            }
            else
            {
                Console.WriteLine(line);
            }
        }
    }
}
