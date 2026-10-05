using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Sıfırlama kodunun geçerli olup olmadığını kontrol eder.
/// </summary>
/// <remarks>
/// Yalnızca arayüzde "yeni şifre" adımına geçmeden önce bilgi vermek içindir;
/// asıl doğrulama <see cref="ResetPasswordCommand"/> içinde yapılır.
/// </remarks>
public sealed record CheckForgotPasswordCodeCommand(
    string ForgotPasswordCode) : IRequest<Result<bool>>;

internal sealed class CheckForgotPasswordCodeCommandHandler(
    IUserRepository userRepository) : IRequestHandler<CheckForgotPasswordCodeCommand, Result<bool>>
{
    private const string InvalidCodeMessage = "Şifre sıfırlama kodunuz geçersiz veya süresi dolmuş";

    public async Task<Result<bool>> Handle(CheckForgotPasswordCodeCommand request, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(request.ForgotPasswordCode, out Guid code) == false)
        {
            return Result<bool>.Failure(InvalidCodeMessage);
        }

        var user = await userRepository.FirstOrDefaultAsync(p =>
            p.ForgotPasswordCode != null
            && p.ForgotPasswordCode.Value == code
            && p.IsForgotPasswordCompleted.Value == false
            , cancellationToken);

        if (user is null || user.IsPasswordResetCodeUsable() == false)
        {
            return Result<bool>.Failure(InvalidCodeMessage);
        }

        return true;
    }
}
