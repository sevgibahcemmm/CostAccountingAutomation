using Microsoft.Data.SqlClient;
using System.Data;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// İki istisna türü: kilidi alamadık (biri önce başladı) ya da kilit çağrısının
/// kendisi hata verdi.
/// </summary>
public enum ProvisioningLockOutcome
{
    /// <summary>Kilit alındı; kurulum yapılabilir.</summary>
    Acquired,

    /// <summary>
    /// Başka bir oturum kurulumu yapıyor ve <see cref="ProvisioningLockTimeoutException.TimeSpan"/>
    /// boyunca kilidi bırakmadı. Bu bir hata değil, normal bir sıralamadır.
    /// </summary>
    TimedOut,

    /// <summary>Sunucuya ulaşılamadı veya kilit çağrısı başarısız oldu.</summary>
    Failed
}

/// <summary>
/// Kilit alınamadığında fırlatılan özel durum.
/// </summary>
public sealed class ProvisioningLockTimeoutException(TimeSpan timeSpan)
    : Exception($"Veritabanı hazırlığı başka bir oturum tarafından yapılıyor. {timeSpan.TotalSeconds:F0} saniye beklenildi.")
{
    public TimeSpan TimeSpan { get; } = timeSpan;
}

/// <summary>
/// Kilit alma denemesinin sonucu.
/// </summary>
/// <param name="Outcome">Alma denemesinin sonucu.</param>
/// <param name="Lock">
/// Alındıysa kilidi tutan nesne; çağıran <c>DisposeAsync</c> çağırmakla
/// sorumludur. Alınamadıysa <see langword="null"/>'dur.
/// </param>
public sealed record ProvisioningLockAttempt(
    ProvisioningLockOutcome Outcome,
    ProvisioningLock? Lock);

/// <summary>
/// Veritabanı hazırlığını sunucu çapında tekilleştirir.
/// </summary>
/// <remarks>
/// <para>
/// Sorun şudur: <c>MigrateAsync</c>, "şirket var mı?" kontrolü ve
/// <c>CREATE DATABASE</c> gibi adımlar <b>kontrol et ve sonra yaz</b>
/// biçimindedir. Aynı anda iki istemci çalıştığında ikisi de "yok" görür ve
/// ikisi de yazar; sonuç çift kayıt, migration geçmişi tablosunda çakışma ve
/// <c>CREATE DATABASE</c> hatasıdır. Uygulama içi bir kilit bunu çözmez çünkü
/// istemciler farklı makinelerdedir.
/// </para>
/// <para>
/// Çözüm SQL Server'ın <c>sp_getapplock</c> uygulama kilididir: kilit sunucu
/// tarafında tutulur, tüm bağlantıları kapsar ve bağlantı kapanınca (hatta
/// süreç çökse bile) otomatik olarak serbest kalır.
/// </para>
/// <para>
/// Kilit <b>oturum</b> kapsamlıdır (<c>@LockOwner = 'Session'</c>). Bu
/// bilinçli bir seçimdir: kilidi tutan bağlantı ayrı ve uzun ömürlüdür, iş
/// bittikten sonra kapatılır. Böylece işlem çökse bile "ölmüş" kilit kalmaz.
/// </para>
/// </remarks>
public sealed class ProvisioningLock : IAsyncDisposable
{
    /// <summary>Zaman aşımı hâlinde <c>sp_getapplock</c>'in döndürdüğü değer.</summary>
    private const int SqlTimeoutReturnCode = -1;

    private readonly SqlConnection _connection;
    private bool _disposed;

    private ProvisioningLock(SqlConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Kurulum kilidini dener.
    /// </summary>
    /// <param name="connectionString">
    /// Herhangi bir katalogu işaret edebilir; kilit <c>master</c> üzerinde
    /// alınır. Böylece uygulamanın kendi master veritabanı henüz yokken de
    /// kilit çalışır.
    /// </param>
    /// <param name="resourceName">
    /// Kilit kaynağı. Farklı uygulama veritabanları aynı sunucuda
    /// birbirini beklemesin diye master veritabanı adı kaynağa dâhil edilir.
    /// </param>
    /// <param name="timeout">Bekleme süresi.</param>
    public static async Task<ProvisioningLockAttempt> TryAcquireAsync(
        string connectionString,
        string resourceName,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        // Kilit alma işleminin kendisi uzun sürebilir (bekleme). Bu yüzden
        // komut zaman aşımı, bekleme süresinden uzun olmalı; aksi hâlde SQL
        // bağlantı zaman aşımı sp_getapplock'i keser ve yanlış hata alınır.
        builder.ConnectTimeout = Math.Max(15, (int)timeout.TotalSeconds + 15);

        SqlConnection connection = new(builder.ConnectionString);

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = (int)timeout.TotalSeconds + 30;
            command.CommandText = """
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock
                     @Resource          = @resource,
                     @LockMode          = 'Exclusive',
                     @LockOwner         = 'Session',
                     @LockTimeout       = @timeoutMs,
                     @DbPrincipal       = 'public';
                SELECT @result AS ReturnCode;
                """;

            command.Parameters.Add("@resource", SqlDbType.NVarChar, 255).Value = resourceName;
            command.Parameters.Add("@timeoutMs", SqlDbType.Int).Value = (int)timeout.TotalMilliseconds;

            object? value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            int returnCode = value is null or DBNull ? SqlTimeoutReturnCode : Convert.ToInt32(value);

            if (returnCode >= 0)
            {
                return new ProvisioningLockAttempt(
                    ProvisioningLockOutcome.Acquired,
                    Acquired(connection));
            }

            await connection.DisposeAsync().ConfigureAwait(false);

            return new ProvisioningLockAttempt(
                returnCode == SqlTimeoutReturnCode
                    ? ProvisioningLockOutcome.TimedOut
                    : ProvisioningLockOutcome.Failed,
                null);
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);

            throw;
        }
    }

    private static ProvisioningLock Acquired(SqlConnection connection) =>
        new(connection);

    /// <summary>
    /// Kurulum kilidini alır; alınamazsa açıklayıcı bir hata fırlatır.
    /// </summary>
    /// <remarks>
    /// Çağıran taraf <c>await using</c> ile kullanır. Kilit, çağıranın işi
    /// bitene kadar tutulur.
    /// </remarks>
    public static async Task<ProvisioningLock> AcquireAsync(
        string connectionString,
        string resourceName,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ProvisioningLockAttempt attempt = await TryAcquireAsync(
            connectionString,
            resourceName,
            timeout,
            cancellationToken).ConfigureAwait(false);

        ThrowIfNotAcquired(attempt.Outcome, timeout);

        return attempt.Lock!;
    }

    private static void ThrowIfNotAcquired(ProvisioningLockOutcome outcome, TimeSpan timeout)
    {
        switch (outcome)
        {
            case ProvisioningLockOutcome.TimedOut:
                throw new ProvisioningLockTimeoutException(timeout);

            case ProvisioningLockOutcome.Failed:
                throw new InvalidOperationException(
                    "Veritabanı hazırlığı kilidi alınamadı (sp_getapplock başarısız).");

            default:
                return;
        }
    }

    /// <summary>
    /// Kilit bağlantısını kapatır ve sunucudaki kilidi serbest bırakır.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
