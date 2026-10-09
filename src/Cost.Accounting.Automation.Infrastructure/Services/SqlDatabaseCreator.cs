using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Veritabanını, fiziksel dosyaları (<c>.mdf</c> / <c>.ldf</c>) önceden
/// belirlenen <c>Data</c> klasöründe olacak şekilde oluşturur.
/// </summary>
/// <remarks>
/// <para>
/// SQL Server, <c>CREATE DATABASE</c> komutunda dosya yolu verilmezse
/// veritabanını kendi varsayılan veri dizinine açar. Uygulama veritabanlarının
/// proje kökündeki (geliştirme) ya da kurulum klasöründeki <c>Data</c>
/// klasöründe durmasını istediği için dosya yolları açıkça verilir.
/// </para>
/// <para>
/// <c>EF Core Migrate()</c> veritabanı yoksa kendisi de oluşturabilir; bu
/// sınıf yalnızca dosyaların nereye açılacağını belirlemek için migration'dan
/// <b>önce</b> çalıştırılır. Veritabanı zaten varsa hiçbir şey yapılmaz:
/// mevcut dosyaların yeri değiştirilemez (detach/attach bilinçli olarak
/// yapılmaz).
/// </para>
/// <para>
/// Veritabanı sunucusu uzak bir makinedeyse dosya yolları o makinenin dosya
/// sistmini tarif eder. Bu yüzden yolu yalnızca sunucu yerel görünürken ya da
/// klasör <c>DatabaseFiles:DataDirectory</c> ile açıkça verilmişken kullanılır.
/// </para>
/// </remarks>
internal static class SqlDatabaseCreator
{
    /// <summary>Veritabanı başka bir oturum tarafından aynı anda oluşturulmuşsa.</summary>
    private const int AlreadyExistsErrorNumber = 1801;

    /// <summary>Dosya yoluna erişilemediğinde SQL Server'ın döndürdüğü hatalar.</summary>
    private static readonly int[] FileAccessErrorNumbers = [5110, 5120, 5123, 5128, 5137];

    /// <summary>
    /// Veritabanı yoksa dosyaları <c>dataDirectory</c> altında olacak şekilde oluşturur.
    /// </summary>
    /// <returns>
    /// Bu çağrıda veritabanı oluşturulduysa <see langword="true"/>; zaten
    /// varsa (ya da başka bir oturum bu sırada oluşturduysa)
    /// <see langword="false"/>.
    /// </returns>
    public static async Task<bool> EnsureCreatedAsync(
        string connectionString,
        string databaseName,
        DatabaseFilePathResolver filePaths,
        CancellationToken cancellationToken = default)
    {
        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",

            // Bu kısa yönetici bağlantısı havuza girmez: işlem bitince fiziksel
            // bağlantı kapanır ve açık kalan oturum tutulmaz.
            Pooling = false
        };

        await using var connection = new SqlConnection(masterBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        try
        {
            if (await DatabaseExistsAsync(connection, databaseName, cancellationToken))
            {
                await EnsureAccessibleOrThrowCoreAsync(connection, databaseName, cancellationToken);

                return false;
            }

            string plainSql = $"CREATE DATABASE {Quote(databaseName)}";
            string? dataDirectory = ResolveDataDirectory(
                masterBuilder.DataSource, databaseName, filePaths);

            if (dataDirectory is null)
            {
                return await TryCreateAsync(connection, plainSql, cancellationToken);
            }

            string placedSql = BuildCreateWithFilesSql(databaseName, dataDirectory);

            try
            {
                return await TryCreateAsync(connection, placedSql, cancellationToken);
            }
            catch (SqlException ex) when (!filePaths.IsDirectoryConfigured
                && !IsLocalServer(masterBuilder.DataSource)
                && IsFileAccessError(ex))
            {
                // Yalnızca uzak sunucuda otomatik klasör kullanılamadıysa (dosya
                // yolu yanlış makineyi tarif etti) sunucunun kendi varsayılan
                // dizinine düşülür. Yerel sunucuda aynı davranış bilinçli olarak
                // YOKTUR: sessizce varsayılan dizine geçilmesi veritabanının
                // tek sabit klasör dışında ikinci bir yerde oluşmasına yol açar.
                Debug.WriteLine(
                    $"[SqlDatabaseCreator] Dosya yolu ile oluşturulamadı ({ex.Number}); " +
                    $"sunucu varsayılan veri dizinine düşülüyor: {ex.Message}");

                return await TryCreateAsync(connection, plainSql, cancellationToken);
            }
            catch (SqlException ex)
            {
                // Klasör kullanıcı tarafından açıkça seçilmiş: sessizce sunucu
                // varsayılan dizinine düşmek seçimin sessizce yok sayılması olur.
                throw new InvalidOperationException(
                    $"\"{databaseName}\" veritabanının dosyaları şu klasöre oluşturulamadı:"
                    + $"{Environment.NewLine}{dataDirectory}"
                    + $"{Environment.NewLine}{Environment.NewLine}{ex.Message}"
                    + $"{Environment.NewLine}{Environment.NewLine}"
                    + "SQL Server hizmetinin bu klasöre yazma yetkisi olmalıdır. Örnek:"
                    + $"{Environment.NewLine}icacls \"{dataDirectory}\" /grant \"{DescribeSqlServerService(masterBuilder.DataSource)}:(OI)(CI)F\"",
                    ex);
            }
        }
        finally
        {
            // EF'in Exists kontrolü, veritabanının kendisine bağlanıp SELECT 1
            // çalıştırır (sanatsal DB_ID kontrolü değil). Daha önce henüz var
            // olmayan bir veritabanına yapılan başarısız bağlanma denemesi
            // (CanConnectAsync / ilk kurulum yoklaması) bağlantı havuzuna bayat
            // bir oturum bırakır; bu oturumda SELECT 1 'veritabanı yok' (4060)
            // döndürür ve EF yanlışlıkla veritabanını oluşturmaya kalkar.
            // Önceden oluşturulmuş ya da yok sayılan her durumda havuzu
            // temizleyerek EF'in temiz bağlantıya açılmasını sağlarız.
            ClearPool(connectionString);
        }
    }

    /// <summary>
    /// Konuyla ilgili bağlantı havuzunu temizler; veritabanı oluşturulduktan
    /// (ya da zaten varken) bayat oturumlar yüzünden hatalı dönüş olmaması için.
    /// </summary>
    private static void ClearPool(string connectionString)
    {
        try
        {
            SqlConnection.ClearPool(new SqlConnection(connectionString));
        }
        catch
        {
            // Havuz temizliği kritik değil; hata burada işlem akışını bozmasın.
        }
    }

    /// <summary>
    /// Veritabanı var mı diye sorar; hedef veritabanına bağlanmaz (master
    /// katalog üzerinden <c>DB_ID</c>). Bağlantı havuzuna bayat oturum
    /// bırakmadığı için varlık yoklamasında güvenle kullanılır.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Doğrudan hedef veritabanına bağlanıp varlık sormak (örneğin EF'in
    /// <c>CanConnectAsync</c>) yanlış beklenti yaratır: veritabanı henüz
    /// yokken yapılan başarısız bağlanma, bağlantı havuzunda (ve bağlantı
    /// örneğinde) bayat durum bırakır. O veritabanı oluşturulduktan sonra
    /// EF'in kendi <c>Exists</c> kontrolü bu bayat oturumdan
    /// 'veritabanı yok' (4060) yanıtı alır, eksik sanır ve <c>CREATE
    /// DATABASE</c> çalıştırıp 'already exists' (1801) hatası alır.
    /// </para>
    /// </remarks>
    public static async Task<bool> ExistsAsync(
        string connectionString,
        string databaseName,
        CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            Pooling = false
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        return await DatabaseExistsAsync(connection, databaseName, cancellationToken);
    }

    /// <summary>
    /// Veritabanı sunucuda kayıtlıysa kullanılabilir (<c>ONLINE</c>) durumda
    /// olduğunu doğrular; değilse net bir açıklamayla hata verir.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fiziksel dosyalar (<c>.mdf</c>/<c>.ldf</c>) kaybolduğunda ya da SQL
    /// Server hizmeti bunlara erişemediğinde veritabanı <c>RECOVERY_PENDING</c>,
    /// <c>OFFLINE</c>, <c>SUSPECT</c> gibi durumlarda kalır. Böyle bir
    /// veritabanına bağlanılamadığı için EF onu 'yok' sanıp <c>CREATE DATABASE</c>
    /// çalıştırır ve 'already exists' (1801) yanlış hatasına neden olur.
    /// Burada o acı duruma düşmeden sorun açıkça raporlanır.
    /// </para>
    /// </remarks>
    public static async Task EnsureAccessibleOrThrowAsync(
        string connectionString,
        string databaseName,
        CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            Pooling = false
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureAccessibleOrThrowCoreAsync(connection, databaseName, cancellationToken);
    }

    /// <summary>Veritabanı durumu <c>ONLINE</c> değilse açıklayıcı hata verir.</summary>
    private static async Task EnsureAccessibleOrThrowCoreAsync(
        SqlConnection connection,
        string databaseName,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT state_desc FROM sys.databases WHERE name = @name";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = databaseName;

        string? state = await command.ExecuteScalarAsync(cancellationToken) as string;

        if (string.Equals(state, "ONLINE", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string stateText = string.IsNullOrWhiteSpace(state) ? "bilinmiyor" : state;

        throw new InvalidOperationException(
            $"\"{databaseName}\" veritabanı sunucuda kayıtlı ancak kullanılamıyor "
            + $"(durum: {stateText})."
            + Environment.NewLine
            + "Bunun en sık nedeni veritabanı dosyalarının (Data klasöründeki "
            + ".mdf/.ldf) kaybolması ya da SQL Server hizmetinin bu dosyalara "
            + "erişememesidir."
            + Environment.NewLine + Environment.NewLine
            + "Dosyalar kurtarılamıyorsa veritabanını yönetici olarak silip "
            + "kurulumu yeniden çalıştırın:"
            + Environment.NewLine
            + $"    DROP DATABASE {Quote(databaseName)};");
    }

    /// <summary>Veritabanı var mı diye <c>master</c> üzerinden sorar.</summary>
    private static async Task<bool> DatabaseExistsAsync(
        SqlConnection connection,
        string databaseName,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT DB_ID(@name)";
        command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = databaseName;

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is not null and not DBNull;
    }

    /// <summary>
    /// Veritabanını oluşturur; aynı anda başka bir oturum oluşturmuşsa (1801)
    /// hata yerine <see langword="false"/> döner.
    /// </summary>
    private static async Task<bool> TryCreateAsync(
        SqlConnection connection,
        string createSql,
        CancellationToken cancellationToken)
    {
        try
        {
            await using SqlCommand command = connection.CreateCommand();
            command.CommandText = createSql;
            command.CommandTimeout = 60;
            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        catch (SqlException ex) when (ex.Number == AlreadyExistsErrorNumber)
        {
            return false;
        }
    }

    /// <summary>
    /// Dosyaların açılacağı klasörü belirler; yolu verilmeyecekse
    /// <see langword="null"/> döner.
    /// </summary>
    private static string? ResolveDataDirectory(
        string dataSource,
        string databaseName,
        DatabaseFilePathResolver filePaths)
    {
        if (!filePaths.UseExplicitFileNames || !IsValidFileBaseName(databaseName))
        {
            return null;
        }

        // Açıkça verilen klasör her zaman kullanılır: kurulum sihirbazının
        // seçimi sunucu yerel görünmese de geçerlidir (yönetici bilinçli
        // seçmiştir) ve hata durumunda sorun görünür kalır.
        if (filePaths.IsDirectoryConfigured)
        {
            return filePaths.GetDataDirectory();
        }

        return IsLocalServer(dataSource) ? filePaths.GetDataDirectory() : null;
    }

    /// <summary>
    /// <c>CREATE DATABASE ... ON PRIMARY (...) LOG ON (...)</c> komutunu üretir.
    /// </summary>
    private static string BuildCreateWithFilesSql(string databaseName, string dataDirectory)
    {
        string dataPath = Path.Combine(dataDirectory, databaseName + ".mdf");
        string logPath = Path.Combine(dataDirectory, databaseName + "_log.ldf");

        return
            $"CREATE DATABASE {Quote(databaseName)}"
            + $" ON PRIMARY (NAME = {Quote(databaseName)}, FILENAME = N'{SqlLiteral(dataPath)}')"
            + $" LOG ON (NAME = {Quote(databaseName + "_log")}, FILENAME = N'{SqlLiteral(logPath)}')";
    }

    /// <summary>
    /// Dosya adı olarak kullanılamayacak adlarda yolu vermez (dosya adı
    /// kuralları veritabanı adı kurallarından daha daridir).
    /// </summary>
    private static bool IsValidFileBaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName)
            || databaseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || databaseName.EndsWith(' ')
            || databaseName.EndsWith('.'))
        {
            return false;
        }

        // Uzun yol sınırı: klasör + ad + uzantı 260 karakteri aşarsa Windows
        // eski API'lerle dosyayı açamaz; böyle durumda sunucu varsayılan
        // dizini daha güvenlidir.
        return databaseName.Length + "_log.ldf".Length < 180;
    }

    /// <summary>
    /// Bağlantı dizesindeki sunucu adı bu makineyi mi tarif ediyor?
    /// </summary>
    /// <remarks>
    /// Örnekler: <c>.</c>, <c>.\SQLEXPRESS</c>, <c>(local)</c>,
    /// <c>localhost\SQLEXPRESS</c>, <c>BILGISAYARADI</c>, <c>127.0.0.1</c>.
    /// </remarks>
    private static bool IsLocalServer(string dataSource)
    {
        string value = dataSource.Trim().Trim('"');

        int slash = value.IndexOf('\\');
        if (slash >= 0)
        {
            value = value[..slash];
        }

        value = value.Trim().Trim('(', ')').Trim().ToLowerInvariant();

        return value.Length == 0
            || value is "." or "local" or "localhost" or "127.0.0.1"
            || value == Environment.MachineName.ToLowerInvariant();
    }

    /// <summary>
    /// Hata mesajında örnek verilecek SQL Server hizmet hesabını üretir
    /// (<c>NT SERVICE\MSSQLSERVER</c> ya da <c>NT SERVICE\MSSQL$EXAMPLE</c>).
    /// </summary>
    private static string DescribeSqlServerService(string dataSource)
    {
        string value = dataSource.Trim().Trim('"');
        int slash = value.IndexOf('\\');
        string instance = slash >= 0 ? value[(slash + 1)..] : string.Empty;

        return string.IsNullOrWhiteSpace(instance)
            || instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
            ? "NT SERVICE\\MSSQLSERVER"
            : $"NT SERVICE\\MSSQL${instance}";
    }

    private static bool IsFileAccessError(SqlException exception) =>
        Array.IndexOf(FileAccessErrorNumbers, exception.Number) >= 0;

    /// <summary>SQL tanımlayıcısını köşeli ayraçlarla tırnaklar: <c>[ad]</c>.</summary>
    private static string Quote(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";

    /// <summary>SQL metin sabitindeki tek tırnağı kaçırır: <c>'</c> → <c>''</c>.</summary>
    private static string SqlLiteral(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);
}
