using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Mesajı okundu olarak işaretler.</summary>
[Permission(MessagePermissions.View)]
public sealed record MessageMarkAsReadCommand(
    Guid MessageId) : IRequest<Result<bool>>;

internal sealed class MessageMarkAsReadCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageMarkAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        MessageMarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        // Alıcı denetimi depoda yapılır: başkasının mesajını okundu yapılamaz.
        bool marked = await messageRepository.MarkAsReadAsync(
            request.MessageId, me, cancellationToken);

        return marked
            ? Result<bool>.Succeed(true)
            : Result<bool>.Failure("Mesaj bulunamadı");
    }
}

/// <summary>
/// Karşı tarafın bana gönderdiği mesajları teslim edilmiş olarak işaretler.
/// </summary>
/// <remarks>
/// <para>
/// "Teslim" ile "okundu" farklıdır. Teslim, mesajın alıcının ekranında
/// <b>göründüğü</b> andır; okundu ise konuşmanın <b>açıldığı</b> andır. Gönderen
/// kullanıcı için "tek tik / çift tik / yeşil tik" ayrımı bu iki bilgiden
/// üretilir.
/// </para>
/// <para>
/// Bu komut yalnızca <b>alınan</b> mesajları işaretler. Gönderdiğimiz mesajların
/// teslim durumunu yalnızca alıcının istemcisi yazabilir; kendi tarafımızdan
/// yazmak, gönderene "alıcı gördü" yalanını söylemek olurdu.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageMarkConversationAsDeliveredCommand(
    Guid CounterpartId,
    bool AnnouncementChannel = false) : IRequest<Result<int>>;

internal sealed class MessageMarkConversationAsDeliveredCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageMarkConversationAsDeliveredCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        MessageMarkConversationAsDeliveredCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());
        var counterpart = new IdentityId(request.CounterpartId);

        // Alıcı denetimi depoda: yalnızca bana gelen mesajlar işaretlenir.
        int marked = await messageRepository.MarkConversationAsDeliveredAsync(
            me, counterpart, request.AnnouncementChannel, cancellationToken);

        return Result<int>.Succeed(marked);
    }
}

/// <summary>Bir karşı tarafın bana gönderdiği tüm mesajları okundu sayar.</summary>
/// <remarks>
/// <para>
/// Konuşma penceresi açıldığında <see cref="MessageMarkAsReadCommand"/> tek tek
/// mesaj işaretlemek yerine bu komut gönderilir: kullanıcı ekranda geçmişi
/// görüyor demektir, dolayısıyla hepsi okunmuş sayılmalıdır.
/// </para>
/// <para>
/// Kapsam, pencerenin kendi kanalıyla sınırlıdır. Duyuru kanalı açıksa yalnızca
/// o kanaldaki mesajlar işaretlenir; kişisel konuşmadaki okunmamışlar yerinde
/// kalır.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageMarkConversationAsReadCommand(
    Guid CounterpartId,
    bool AnnouncementChannel = false) : IRequest<Result<int>>;

internal sealed class MessageMarkConversationAsReadCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageMarkConversationAsReadCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        MessageMarkConversationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());
        var counterpart = new IdentityId(request.CounterpartId);

        // Alıcı denetimi depoda yapılır: yalnızca bana gelen mesajlar işaretlenir.
        int marked = await messageRepository.MarkConversationAsReadAsync(
            me, counterpart, request.AnnouncementChannel, cancellationToken);

        return Result<int>.Succeed(marked);
    }
}

/// <summary>Kullanıcının okunmamış mesaj sayısı.</summary>
[Permission(MessagePermissions.View)]
public sealed record MessageUnreadCountQuery : IRequest<Result<int>>;

internal sealed class MessageUnreadCountQueryHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(
        MessageUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());

        int count = await messageRepository.CountUnreadAsync(me, cancellationToken);

        return Result<int>.Succeed(count);
    }
}
