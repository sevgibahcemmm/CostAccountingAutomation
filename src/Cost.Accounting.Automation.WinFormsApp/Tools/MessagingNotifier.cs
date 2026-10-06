using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Tools;

/// <summary>
/// Canlı mesajlaşma olaylarını kullanıcıya bildirimle taşır.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="LiveMessagingService"/> yalnızca "ne oldu" bilgisini bilir;
/// bunu ekranda nasıl göstereceğine karar vermez. Bu sınıf o kararı tek yerde
/// toplar: metin, ses ve süre burada belirlenir.
/// </para>
/// <para>
/// Ayrı bir sınıf olmasının ikinci nedeni iş parçacığıdır. Servis olayları
/// <b>arka plan</b> iş parçacığından yükseltir; WinForms denetimlerine yalnızca
/// arayüz iş parçacığından dokunulabilir. Bu sınıf olayları
/// <see cref="LiveMessagingService.PostToUi"/> ile arayüz iş parçacığına taşır,
/// böylece çağıran ekranların her birinde <c>Invoke</c> yazmak gerekmez.
/// </para>
/// </remarks>
public sealed class MessagingNotifier : IDisposable
{
    private readonly LiveMessagingService _service;

    public MessagingNotifier(LiveMessagingService service)
    {
        _service = service;

        _service.UsersSignedIn += OnUsersSignedIn;
        _service.UsersSignedOut += OnUsersSignedOut;
        _service.UnreadReceived += OnUnreadReceived;
    }

    /// <summary>Birisi oturum açtığında çalışır.</summary>
    private void OnUsersSignedIn(object? sender, UsersChangedEventArgs e)
    {
        _service.PostToUi(() =>
        {
            // İstenen davranış: yeni oturumu sesli olarak duyur.
            SoundHelper.PlaySignedIn();

            ToastHelper.Show(
                BuildSentence(e.Users, "oturum açtı"),
                ToastType.Success,
                4000,
                // Bildirime basınca Mesajlar sayfası açılır.
                () => ToastHelper.OpenMessages?.Invoke());
        });
    }

    /// <summary>Birisi çıkış yaptığında çalışır.</summary>
    private void OnUsersSignedOut(object? sender, UsersChangedEventArgs e)
    {
        _service.PostToUi(() =>
        {
            // Çıkışta ses çalınmaz: kullanıcı bunu kendi yaptığı için yeni bilgi
            // değildir ve bildirim birikirse rahatsız eder.
            ToastHelper.Show(
                BuildSentence(e.Users, "oturumu kapattı"),
                ToastType.Info,
                3000,
                () => ToastHelper.OpenMessages?.Invoke());
        });
    }

    /// <summary>Bize yeni mesaj geldiğinde çalışır.</summary>
    private void OnUnreadReceived(object? sender, MessageGroupsChangedEventArgs e)
    {
        _service.PostToUi(() =>
        {
            SoundHelper.PlayNewMessage();

            foreach (UnreadMessageGroupDto group in e.Groups)
            {
                Guid senderId = group.SenderId;
                string senderName = group.SenderFullName;

                // Bildirime basınca o kişiyle olan konuşma doğrudan açılır.
                ToastHelper.Show(
                    BuildMessageSentence(group),
                    ToastType.Info,
                    5000,
                    () => ToastHelper.OpenConversation?.Invoke(senderId, senderName));

                // Mesaj bu uygulamaya ulaştı: gönderende gri çift tik görünür.
                // "Okundu" işareti ayrıdır ve yalnızca kullanıcı konuşmayı
                // açtığında (sağ alanda sohbet göründüğünde) yazılır.
                _ = MarkDeliveredAsync(senderId);
            }
        });
    }

    /// <summary>
    /// Bir gönderenin bize gönderdiği mesajları teslim edilmiş olarak işaretler.
    /// </summary>
    /// <remarks>
    /// Arka plan yoklaması mesajın içeriğini önizlemeyle göstermiştir; bu,
    /// WhatsApp'taki "cihaza teslim edildi" anlamıdır. Okunma ise tümüyle
    /// ayrıdır ve hiçbir zaman burada yazılmaz.
    /// </remarks>
    private static async Task MarkDeliveredAsync(Guid senderId)
    {
        try
        {
            using IServiceScope scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            await mediator.Send(
                new MessageMarkConversationAsDeliveredCommand(senderId),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("MessagingNotifier.MarkDelivered", ex);
        }
    }

    /// <summary>
    /// Ad listesini okunabilir tek bir cümleye çevirir: bir kişi, iki kişi ya da
    /// "5 kişi" gibi bir özet.
    /// </summary>
    private static string BuildSentence(IReadOnlyList<string> names, string verb)
    {
        if (names.Count == 1)
        {
            return $"{names[0]} {verb}.";
        }

        if (names.Count <= 3)
        {
            // Türkçede "A, B ve C" biçimi okunur; "A, B, C" değil.
            string joined = string.Join(", ", names.Take(names.Count - 1))
                + " ve " + names[^1];

            return $"{joined} {verb}.";
        }

        return $"{names.Count} kişi {verb}.";
    }

    private static string BuildMessageSentence(UnreadMessageGroupDto group)
    {
        string prefix = group.UnreadCount > 1
            ? $"{group.SenderFullName} size {group.UnreadCount} mesaj gönderdi"
            : $"{group.SenderFullName} size mesaj gönderdi";

        return string.IsNullOrWhiteSpace(group.Preview)
            ? prefix + "."
            : $"{prefix}: {group.Preview}";
    }

    public void Dispose()
    {
        _service.UsersSignedIn -= OnUsersSignedIn;
        _service.UsersSignedOut -= OnUsersSignedOut;
        _service.UnreadReceived -= OnUnreadReceived;
    }
}