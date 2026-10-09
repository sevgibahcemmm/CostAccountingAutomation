using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.Application.Presence;
using Cost.Accounting.Automation.Domain.Presence;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// Uygulamanın canlı mesajlaşma motoru: "kim çevrimiçi" ve "bana ne geldi"
/// sorularını arka planda yanıtlar.
/// </summary>
/// <remarks>
/// <para>
/// <b>Neden ayrı bir servis?</b> Bu izleme işi tek bir ekrana ait değildir.
/// Kullanıcı Dashboard'da çalışırken de biri oturum açtığında bilgilendirilmelidir.
/// Ekranlar açılıp kapanırken kaybolan zamanlayıcılara güvenilemez; bu yüzden
/// iş, uygulama ömrü boyunca yaşayan tek bir servis tarafından yürütülür.
/// </para>
/// <para>
/// <b>Nasıl çalışır?</b> Uygulamanın sunucu katmanı olmadığı için anlık iletim
/// kanalı da yoktur. Bunun yerine (uygulama ömrü boyunca) tek bir yoklama
/// tekrarlanır: diğer kullanıcıların durumu ve bize gelen okunmamış mesajlar
/// okunur. <b>Kalp atışı</b> bu servise ait değildir; her oturum için ayrı
/// <see cref="SessionPresenceService"/> çalışır.
/// </para>
/// <para>
/// Yoklama sonucu bir önceki turla karşılaştırılır. Aradaki fark "bu turda yeni
/// oturum açanlar" ve "bu turda yeni mesaj gelenler"dir; işte bildirim üretilecek
/// bilgi budur. <b>İlk tur</b> yalnızca başlangıç noktasıdır ve hiçbir şey
/// duyurulmaz — aksi hâlde uygulamayı açan kullanıcı, ekranda zaten çalışan
/// bütün arkadaşları için "X oturum açtı" duyurusu alırdı.
/// </para>
/// <para>
/// Servis <c>ISender</c>'ı doğrudan tutmaz; her iş için kendi kapsamını açar.
/// Kapsam tek seferlik olacağı için paylaşmak doğru olmazdı.
/// </para>
/// </remarks>
public sealed class LiveMessagingService : IDisposable
{
    /// <summary>
    /// Yoklama aralığı. 10 saniye, bir sohbet uygulaması için hemen hissedilen
    /// ama veritabanına gereksiz yük bindirmeyen değerdir.
    /// </summary>
    private const int PollIntervalSeconds = 10;

    /// <summary>
    /// Yoklama başarısız olduğunda beklenecek süre. Sunucuya ulaşılamadığında
    /// her 10 saniyede bir hata yazmak günlük dosyayı şişirir ve hatayı gizler.
    /// </summary>
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromSeconds(45);

    private CancellationTokenSource? _cts;
    private Task? _loop;

    private int _started;
    private bool _baselineTaken;

    /// <summary>
    /// Son yoklamada görülen durum. Yalnızca döngü iş parçacığı tarafından
    /// yazılır; bu yüzden ayrı kilit gerekmez.
    /// </summary>
    private Dictionary<Guid, UserPresenceStateDto> _lastPresence = new();

    private Dictionary<Guid, int> _lastUnread = new();

    /// <summary>Uygulama açıldığı andaki kullanıcı kimliği; duyurularda elenir.</summary>
    private Guid _selfUserId = Guid.Empty;

    /// <summary>
    /// Arayüz iş parçacığının bağlamı. Servis arka planda çalıştığı için
    /// ekranlara ait olmayan tek doğru yer burada: WinForms denetimlerine
    /// yalnızca arayüz iş parçacığından dokunulabilir.
    /// </summary>
    private SynchronizationContext? _uiContext;

    /// <summary>Şu an çevrimiçi olan <b>diğer</b> kullanıcı sayısı.</summary>
    /// <remarks>
    /// Kendi oturumumuz da çevrimiçidir ama kullanıcıya gösterilmez; mesajlaşma
    /// listesinde ve bu sayıda yer almaz.
    /// </remarks>
    public int OnlineUserCount { get; private set; }

    /// <summary>Toplam okunmamış mesaj sayısı.</summary>
    public int UnreadTotal { get; private set; }

    /// <summary>
    /// Her yoklamadan sonra tetiklenir. Ekranlar bu olayla kendini tazeler.
    /// </summary>
    public event EventHandler? Polled;

    /// <summary>Yeni oturum açan kullanıcılar için tetiklenir (kendimiz hariç).</summary>
    public event EventHandler<UsersChangedEventArgs>? UsersSignedIn;

    /// <summary>Çıkış yapan kullanıcılar için tetiklenir.</summary>
    public event EventHandler<UsersChangedEventArgs>? UsersSignedOut;

    /// <summary>Yeni okunmamış mesajlar için tetiklenir.</summary>
    public event EventHandler<MessageGroupsChangedEventArgs>? UnreadReceived;

    /// <summary>
    /// Servisi başlatır. Oturum açıldıktan hemen sonra çağrılır.
    /// </summary>
    public void Start(Guid selfUserId)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return;
        }

        _selfUserId = selfUserId;

        // Start her zaman arayüz iş parçacığından çağrılır; bağlam burada
        // yakalanır. Olay dinleyicileri kendi BeginInvoke'larını yazmak
        // zorunda kalmasın diye PostToUi sunulur.
        _uiContext = SynchronizationContext.Current;

        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => RunAsync(_cts.Token));
    }

    /// <summary>
    /// Verilen işi arayüz iş parçacığında çalıştırır.
    /// </summary>
    /// <remarks>
    /// Servis henüz başlamadıysa (örneğin mesajlaşma yetkisi yoksa) iş
    /// doğrudan çağrılır. Bu yol nadir olduğu için pencereler zaten açık
    /// değildir.
    /// </remarks>
    public void PostToUi(Action action)
    {
        SynchronizationContext? context = _uiContext;

        if (context is null)
        {
            action();
            return;
        }

        context.Post(_ =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("LiveMessagingService.PostToUi", ex);
            }
        }, null);
    }

    /// <summary>
    /// Servisi durdurur.
    /// </summary>
    /// <remarks>
    /// Oturumun çevrimiçi izini kapatma (kalp atışı ve çıkış bildirimi)
    /// <see cref="SessionPresenceService"/> tarafından yapılır; bu servis
    /// yalnızca mesajlaşma yoklamasını bitirir.
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
            TimeSpan wait = TimeSpan.FromSeconds(PollIntervalSeconds);

            try
            {
                bool succeeded = await TickAsync(cancellationToken);

                if (!succeeded)
                {
                    wait = FailureBackoff;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("LiveMessagingService.Tick", ex);

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

    /// <summary>
    /// Bir tur: kalp atışı, yoklama, karşılaştırma ve olayların tetiklenmesi.
    /// </summary>
    /// <returns>Tur başarıyla tamamlandıysa <c>true</c>.</returns>
    private async Task<bool> TickAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = Program.Services.CreateScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            Result<List<UserPresenceStateDto>> presence = await mediator.Send(
                new UserPresenceStateQuery(), cancellationToken);

            if (!presence.IsSuccessful || presence.Data is null)
            {
                return false;
            }

            Result<List<UnreadMessageGroupDto>> unread = await mediator.Send(
                new MessageUnreadSummaryQuery(), cancellationToken);

            List<UnreadMessageGroupDto> unreadGroups = unread.Data ?? [];

            // Kendimiz listede görünmediği için sayaca da girmeyiz. Aksi hâlde
            // durum çubuğundaki sayı, kullanıcının listede gördüğü sayıdan bir
            // fazla olur ve ikisi tutarsız görünür.
            OnlineUserCount = presence.Data.Count(s => s.IsOnline && s.UserId != _selfUserId);
            UnreadTotal = unreadGroups.Sum(g => g.UnreadCount);

            if (_baselineTaken)
            {
                RaiseTransitions(presence.Data);
                RaiseUnreadChanges(unreadGroups);
            }
            else
            {
                // İlk tur yalnızca başlangıç noktasını alır; mevcut
                // kullanıcılar için "oturum açtı" duyurusu yapılmaz.
                _baselineTaken = true;
            }

            _lastPresence = presence.Data.ToDictionary(s => s.UserId, s => s);
            _lastUnread = unreadGroups.ToDictionary(g => g.SenderId, g => g.UnreadCount);

            Polled?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("LiveMessagingService.Query", ex);

            return false;
        }
    }

    /// <summary>Çevrimiçi olan ve pasif olan kullanıcı farkını çıkarır.</summary>
    private void RaiseTransitions(List<UserPresenceStateDto> current)
    {
        List<string> signedIn = [];
        List<string> signedOut = [];

        foreach (UserPresenceStateDto state in current)
        {
            if (state.UserId == _selfUserId)
            {
                continue;
            }

            bool wasOnline = _lastPresence.TryGetValue(state.UserId, out UserPresenceStateDto? previous)
                && previous.IsOnline;

            if (state.IsOnline && !wasOnline)
            {
                signedIn.Add(state.FullName);
            }
            else if (!state.IsOnline && wasOnline)
            {
                signedOut.Add(state.FullName);
            }
        }

        if (signedIn.Count > 0)
        {
            UsersSignedIn?.Invoke(this, new UsersChangedEventArgs(signedIn));
        }

        if (signedOut.Count > 0)
        {
            UsersSignedOut?.Invoke(this, new UsersChangedEventArgs(signedOut));
        }
    }

    /// <summary>Yeni okunmamış mesaj gruplarını bildirir.</summary>
    private void RaiseUnreadChanges(List<UnreadMessageGroupDto> current)
    {
        List<UnreadMessageGroupDto> arrivals = current
            .Where(g => g.SenderId != _selfUserId
                && (!_lastUnread.TryGetValue(g.SenderId, out int before) || g.UnreadCount > before))
            .ToList();

        if (arrivals.Count > 0)
        {
            UnreadReceived?.Invoke(this, new MessageGroupsChangedEventArgs(arrivals));
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

/// <summary>Kullanıcı listesi taşıyan olay.</summary>
public sealed class UsersChangedEventArgs : EventArgs
{
    public UsersChangedEventArgs(IReadOnlyList<string> users) => Users = users;

    public IReadOnlyList<string> Users { get; }
}

/// <summary>Yeni mesaj grupları taşıyan olay.</summary>
public sealed class MessageGroupsChangedEventArgs : EventArgs
{
    public MessageGroupsChangedEventArgs(IReadOnlyList<UnreadMessageGroupDto> groups) => Groups = groups;

    public IReadOnlyList<UnreadMessageGroupDto> Groups { get; }
}