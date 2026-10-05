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
