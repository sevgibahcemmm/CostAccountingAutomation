using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Messages;
using Cost.Accounting.Automation.Domain.Users;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Messages;

/// <summary>Bir kullanıcıya tek bir mesaj gönderir.</summary>
[Permission(MessagePermissions.Send)]
public sealed record MessageSendCommand(
    Guid RecipientId,
    string Body,
    string? Subject) : IRequest<Result<string>>;

public sealed class MessageSendCommandValidator : AbstractValidator<MessageSendCommand>
{
    public MessageSendCommandValidator()
    {
        RuleFor(p => p.RecipientId)
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir alıcı seçin");

        RuleFor(p => p.Body)
            .NotEmpty().WithMessage("Mesaj boş olamaz")
            .MaximumLength(MessageBody.MaxLength)
            .WithMessage($"Mesaj en fazla {MessageBody.MaxLength} karakter olabilir");

        RuleFor(p => p.Subject)
            .MaximumLength(200)
            .WithMessage("Konu en fazla 200 karakter olabilir");
    }
}

/// <summary>Tek bir alıcıya mesaj gönderir.</summary>
internal sealed class MessageSendCommandHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    IClaimContext claimContext) : IRequestHandler<MessageSendCommand, Result<string>>
{
    public async Task<Result<string>> Handle(MessageSendCommand request, CancellationToken cancellationToken)
    {
        Guid senderId = claimContext.GetUserId();
        var recipientId = new IdentityId(request.RecipientId);

        if (recipientId.Value == senderId)
        {
            return Result<string>.Failure("Kendinize mesaj gönderemezsiniz");
        }

        var recipient = await userRepository.FirstOrDefaultAsync(
            p => p.Id == recipientId, cancellationToken);

        if (recipient is null)
        {
            return Result<string>.Failure("Alıcı bulunamadı");
        }

        if (recipient.IsActive == false)
        {
            return Result<string>.Failure("Alıcı pasif durumda, mesaj gönderilemiyor");
        }

        var message = new UserMessage(
            new IdentityId(senderId),
            recipientId,
            new MessageBody(request.Body),
            string.IsNullOrWhiteSpace(request.Subject) ? null : new MessageSubject(request.Subject));

        await messageRepository.AddAsync(message, cancellationToken);

        return "Mesaj gönderildi";
    }
}
