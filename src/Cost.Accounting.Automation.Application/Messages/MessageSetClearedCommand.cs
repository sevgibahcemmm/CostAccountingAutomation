using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Bir konuşmayı yalnızca çağıran kullanıcının görünümünden temizler ya da
/// temizliği geri alır.
/// </summary>
/// <remarks>
/// <para>
/// Temizleme <b>tek taraflı ve geri alınabilirdir</b>: mesaj satırlarına ve
/// karşı tarafın görünümüne dokunulmaz. Arayüzdeki karşılığı "Temizle" ve
/// "Geçmişi Göster" düğmeleridir.
/// </para>
/// <para>
/// Hangi kullanıcının temizlediği kimlik bağlamından (claim) okunur; istemci
/// karşı tarafın kimliğini verse bile başkasının görünümünü değiştiremez.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageSetClearedCommand(
    Guid CounterpartId,
    bool Cleared,
    bool AnnouncementChannel = false) : IRequest<Result<DateTimeOffset?>>;

internal sealed class MessageSetClearedCommandHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageSetClearedCommand, Result<DateTimeOffset?>>
{
    public async Task<Result<DateTimeOffset?>> Handle(
        MessageSetClearedCommand request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());
        var counterpart = new IdentityId(request.CounterpartId);

        if (counterpart.Value == me.Value)
        {
            return Result<DateTimeOffset?>.Failure("Kendi konuşmanız için bu işlem yapılamaz");
        }

        DateTimeOffset? clearedAt = request.Cleared ? DateTimeOffset.Now : null;

        await messageRepository.SetClearedAsync(
            me, counterpart, request.AnnouncementChannel, clearedAt, cancellationToken);

        return clearedAt;
    }
}
