using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Bir konuşmanın çağıran kullanıcıya özel görünüm durumu.
/// </summary>
/// <remarks>
/// Dönen değer o kullanıcının temizleme (görünüm gizleme) anıdır; temizleme
/// yoksa <c>null</c>. Yalnızca kendi satırı okunur — bir başkasının temizleme
/// durumu bu sorguyla görülemez. Konuşma sorgusu aynı bilgiyi filtre için
/// zaten kullanır; bu sorgu arayüzün "Geçmişi Göster" düğmesini hangi kipte
/// göstereceğini bilmesi için vardır.
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record MessageConversationStateQuery(
    Guid CounterpartId,
    bool AnnouncementChannel = false) : IRequest<Result<DateTimeOffset?>>;

internal sealed class MessageConversationStateQueryHandler(
    IMessageRepository messageRepository,
    IClaimContext claimContext) : IRequestHandler<MessageConversationStateQuery, Result<DateTimeOffset?>>
{
    public async Task<Result<DateTimeOffset?>> Handle(
        MessageConversationStateQuery request,
        CancellationToken cancellationToken)
    {
        var me = new IdentityId(claimContext.GetUserId());
        var counterpart = new IdentityId(request.CounterpartId);

        DateTimeOffset? clearedAt = await messageRepository.GetClearedAtAsync(
            me, counterpart, request.AnnouncementChannel, cancellationToken);

        return clearedAt;
    }
}
