using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;
public sealed record ForgotPasswordCommand(
    string Email) : IRequest<Result<ForgotPasswordCommandResponse>>;

public sealed record ForgotPasswordCommandResponse(string Message, Guid ResetCode);

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Geçerli bir mail adresi girin")
            .EmailAddress().WithMessage("Geçerli bir mail adresi girin");
    }
}

internal sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository
  ) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordCommandResponse>>
{
    public async Task<Result<ForgotPasswordCommandResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository
            .FirstOrDefaultAsync(p => p.Email.Value == request.Email, cancellationToken);

        if (user is null)
        {
            return Result<ForgotPasswordCommandResponse>.Failure("Kullanıcı bulunamadı");
        }

        user.CreateForgotPasswordId();
        userRepository.Update(user);

        var response = new ForgotPasswordCommandResponse(
            "Şifre sıfırlama kodu oluşturuldu. Lütfen kodu kullanarak yeni şifrenizi belirleyin.",
            user.ForgotPasswordCode!.Value);

        return response;
    }
}