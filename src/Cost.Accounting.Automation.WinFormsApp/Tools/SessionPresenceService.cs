using Cost.Accounting.Automation.Application.Presence;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// Oturumun canlılık izini (kalp atışı) yönetir: "şu an bu kullanıcının
/// uygulaması açık" bilgisi.
/// </summary>
/// <remarks>
/// <para>
/// Mesajlaşma motorunun (<see cref="LiveMessagingService"/>) aksine bu servis
/// <b>her</b> oturum açan kullanıcı için çalışır; mesajlaşma yetkisi gerekmez.
/// Tek-oturum kuralı (aynı kullanıcı aynı anda tek yerde giriş yapabilir)
/// tüm kullanıcılar için geçerli olduğundan, kalp atışı ve çıkış bildirimi
/// herkese aittir.
/// </para>
/// <para>
/// Servis singleton'dır ve oturum bağlamı girişten sonra dolu olduğu için
/// ana form açıldığında başlatılır. Oturum kimliği uygulama (pencere) başına
/// bir kez üretilir; çıkışta <see cref="StopAsync"/> ile kapanış bildirimi
/// gönderilir.
/// </para>
/// </remarks>
public sealed class SessionPresenceService : IDisposable
{
    /// <summary>
    /// Kalp atışı aralığı. <see cref="UserPresence.OnlineWindow"/> üç atlama
    /// sonrası kullanıcıyı pasife çevirir; kısa bir ağ kesintisi yanlışlıkla
    /// "pasif" göstermemelidir.
    /// </summary>
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Kalp atışı başarısız olduğunda beklenecek süre. Sunucuya ulaşılamadığında
    /// her 10 saniyede bir hata yazmak günlük dosyayı şişirir.
    /// </summary>
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromSeconds(45);

    /// <summary>
    /// Bu oturumun kimliği. Her kalp atışında gönderilir ve aynı kullanıcının
    /// başka bir penceredeki oturumundan ayırt edilmesini sağlar.
    /// </summary>
    private readonly Guid _sessionId = Guid.CreateVersion7();

    private CancellationTokenSource? _cts;
    private Task? _loop;
    private int _started;

    /// <summary>Bu oturumun kimliği; çıkış bildiriminde kullanılır.</summary>
    public Guid SessionId => _sessionId;

    /// <summary>Servisi başlatır. Oturum açıldıktan hemen sonra çağrılır.</summary>
    public void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => RunAsync(_cts.Token));
    }

    /// <summary>
    /// Servisi durdurur ve çıkış bildirimini gönderir.
    /// </summary>
    /// <remarks>
    /// Kapanış bildirimi "best effort"tir: uygulama kapanırken bağlantı zaten
    /// kopmuş olabilir. Başarısız olması sorun değildir; diğer istemciler
    /// kullanıcıyı <see cref="UserPresence.OnlineWindow"/> sonunda yine pasife
    /// çevirir.
    /// </remarks>
    public async Task StopAsync()
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        CancellationTokenSource? cts = _cts;
        _cts = null;

        if (cts is not null)
        {
            try
            {
                await cts.CancelAsync();
            }
            catch
            {
            }
        }

        // Çıkış bildirimi oturum bağlamı hâlâ doluyken gönderilir; bu yüzden
        // çağıran taraf session.Clear() öncesinde StopAsync'i çağırmalıdır.
        await SendSignOutAsync(CancellationToken.None);

        try
        {
            if (_loop is not null)
            {
                await _loop;
            }
        }
        catch
        {
        }

        cts?.Dispose();
        _loop = null;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan wait = HeartbeatInterval;

            try
            {
                await HeartbeatAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("SessionPresenceService.Heartbeat", ex);

                wait = FailureBackoff;
            }

            try
            {
                await Task.Delay(wait, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task HeartbeatAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = Program.Services.CreateScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(
            new UserPresenceHeartbeatCommand(_sessionId, Environment.MachineName),
            cancellationToken);
    }

    private async Task SendSignOutAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            await mediator.Send(new UserPresenceSignOutCommand(_sessionId), cancellationToken);
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("SessionPresenceService.SignOut", ex);
        }
    }

    /// <summary>
    /// Servis singleton olarak kayıtlıdır ve uygulama kapanana kadar yaşar;
    /// bu nedenle <see cref="Dispose"/> yalnızca sınıf sözleşmesini yerine
    /// getirir ve tutulan kaynak yoktur.
    /// </summary>
    public void Dispose()
    {
    }
}