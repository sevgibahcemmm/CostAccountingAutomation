using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>
/// Birden çok kullanıcıya aynı duyuruyu gönderir.
/// </summary>
/// <remarks>
/// Her alıcı için ayrı bir satır yazılır; böylece okunmamış sayacı kullanıcı
/// başına doğru çalışır ve gelen kutusu tek sorguyla yeterli olur.
/// Yetki <c>message:announce</c> ile ayrıdır: duyuru göndermek, tekil mesaj
/// göndermekten daha güçlü bir yetkidir ve varsayılan olarak yalnızca yöneticilere
/// verilmelidir.
/// <para>
/// <c>SendToAll</c> <see langword="true"/> iken <c>RecipientIds</c> yok sayılır
/// ve duyuru pasif olmayan (çevrimdışı olanlar dâhil) tüm kullanıcılara gider;
/// bu, alıcı listesine bağlı kalmadan şirket geneline duyuru yapılmasını sağlar.
/// </para>
/// </remarks>
[Permission(MessagePermissions.Announce)]
public sealed record MessageBroadcastCommand(
    IReadOnlyList<Guid> RecipientIds,
    string Body,
    string? Subject,
    bool SendToAll = false) : IRequest<Result<string>>;

public sealed class MessageBroadcastCommandValidator : AbstractValidator<MessageBroadcastCommand>
{
    public MessageBroadcastCommandValidator()
    {
        // Alıcı kuralları yalnızca seçili-alıcı kipinde anlamlıdır;
        // "Tüm Kullanıcılara" kipinde hedef kitle sunucu tarafında çözülür.
        When(p => !p.SendToAll, () =>
        {
            RuleFor(p => p.RecipientIds)
                .NotNull().WithMessage("En az bir alıcı seçin");

            RuleFor(p => p.RecipientIds)
                .Must(ids => ids is not null && ids.Count > 0).WithMessage("En az bir alıcı seçin");

            // Yinelenen kimlikler aynı kişiye iki kez duyuru gitmesine yol açar ve
            // okunmamış sayacını bozar.
            RuleFor(p => p.RecipientIds)
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
                .WithMessage("Aynı alıcı birden çok kez seçilemez");
        });

        RuleFor(p => p.Body)
            .NotEmpty().WithMessage("Duyuru metni boş olamaz")
            .MaximumLength(MessageBody.MaxLength)
            .WithMessage($"Duyuru en fazla {MessageBody.MaxLength} karakter olabilir");

        RuleFor(p => p.Subject)
            .NotEmpty().WithMessage("Duyuru konusu zorunludur")
            .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir");
    }
}

internal sealed class MessageBroadcastCommandHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IClaimContext claimContext) : IRequestHandler<MessageBroadcastCommand, Result<string>>
{
    /// <summary>Tek bir seferde işlenecek en fazla alıcı sayısı.</summary>
    private const int MaxRecipients = 500;

    public async Task<Result<string>> Handle(MessageBroadcastCommand request, CancellationToken cancellationToken)
    {
        Guid senderId = claimContext.GetUserId();
        var sender = new IdentityId(senderId);

        IdentityId[]? requestedIds = null;

        if (!request.SendToAll)
        {
            var distinctIds = request.RecipientIds.Distinct().ToArray();

            if (distinctIds.Length == 0)
            {
                return Result<string>.Failure("En az bir alıcı seçin");
            }

            if (distinctIds.Length > MaxRecipients)
            {
                return Result<string>.Failure($"En fazla {MaxRecipients} alıcıya duyuru gönderebilirsiniz");
            }

            // Alıcılar önceden tek sorguda çözülür; her alıcı için ayrı sorgu yapmak
            // yüzlerce tur veritabanına gidiş-dönüş demektir.
            requestedIds = distinctIds.Select(id => new IdentityId(id)).ToArray();
        }

        var recipients = request.SendToAll
            ? await userRepository
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken)
            : await userRepository
                .Where(u => requestedIds!.Contains(u.Id) && u.IsActive)
                .ToListAsync(cancellationToken);

        if (recipients.Count == 0)
        {
            return Result<string>.Failure(
                request.SendToAll
                    ? "Duyuru gönderilecek aktif kullanıcı bulunamadı"
                    : "Seçilen alıcılardan hiçbiri bulunamadı");
        }

        int delivered = 0;

        foreach (var recipient in recipients)
        {
            if (recipient.Id.Value == senderId)
            {
                // Gönderen kendi listesinde olabilir (tüm-kullanıcılar kipinde
                // kesinlikle vardır); kendine duyuru gönderilmez.
                continue;
            }

            // Değer nesneleri her mesaj için YENİDEN oluşturulur.
            //
            // Aynı MessageBody/MessageSubject örneğini birden çok mesajda
            // paylaşmak EF'i bozar: bir CLR örneği yalnızca tek bir sahne
            // bağlanabilir. İkinci ve sonraki mesajlarda gövde "değişmemiş"
            // sayılır ve INSERT listesine hiç girmez; kolon NOT NULL olduğu için
            // "Cannot insert the value NULL into column 'Body_Value'" hatası
            // ve tüm duyurunun kaydedilmemesiyle sonuçlanır.
            await messageRepository.AddAsync(
                new UserMessage(
                    sender,
                    recipient.Id,
                    new MessageBody(request.Body),
                    new MessageSubject(request.Subject!),
                    isAnnouncement: true),
                cancellationToken);

            delivered++;
        }

        if (delivered == 0)
        {
            return Result<string>.Failure("Duyuru gönderilecek alıcı kalmadı");
        }

        return delivered == 1
            ? "Duyuru gönderildi"
            : $"{delivered} kullanıcıya duyuru gönderildi";
    }
}
