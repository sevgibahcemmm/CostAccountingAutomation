using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.LoginTokens;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;
public sealed record ResetPasswordCommand(
    string ForgotPasswordCode,
    string NewPassword,
    bool LogoutAllDevices) : IRequest<Result<string>>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("Geçerli bir yeni şifre girin")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır");

        RuleFor(p => p.NewPassword)
            .Matches("[A-Za-zÇĞİÖŞÜçğıöşü]").WithMessage("Yeni şifre en az bir harf içermelidir")
            .Matches("[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir");
    }
}

internal sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    ILoginTokenRepository loginTokenRepository) : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    /// <summary>
    /// Kullanıcıya verilen mesaj. Kodun neden geçersiz sayıldığını (süresi
    /// dolmuş, kullanılmış, yanlış) söylemez; hepsi aynı yanıtla döner.
    /// </summary>
    private const string InvalidCodeMessage = "Şifre sıfırlama kodunuz geçersiz veya süresi dolmuş";

    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(request.ForgotPasswordCode, out Guid code) == false)
        {
            return Result<string>.Failure(InvalidCodeMessage);
        }

        // Sorgu yalnızca "kullanılmamış" kodları getirir; süre denetimi uygulama
        // katmanında IsPasswordResetCodeUsable() ile yapılır ve tek kaynaktan
        // (User.PasswordResetCodeValidity) gelir.
        var user = await userRepository.FirstOrDefaultAsync(p =>
            p.ForgotPasswordCode != null
            && p.ForgotPasswordCode.Value == code
            && p.IsForgotPasswordCompleted.Value == false
            , cancellationToken);

        if (user is null || user.IsPasswordResetCodeUsable() == false)
        {
            return Result<string>.Failure(InvalidCodeMessage);
        }

        Password password = new(request.NewPassword);
        user.SetPassword(password);

        // Tek kullanimlilik: basarili bir sifirlamadan sonra kod gecersiz kilinir.
        // Bu cagri atlanzsa kod, gecerlilik suresi dolana kadar defalarca
        // kullanilabilir kalir.
        user.MarkPasswordResetCompleted();

        userRepository.Update(user);

        if (request.LogoutAllDevices)
        {
            var loginTokens = await loginTokenRepository
                .Where(p => p.UserId == user.Id && p.IsActive.Value == true)
                .ToListAsync(cancellationToken);

            foreach (var item in loginTokens)
            {
                item.SetIsActive(new(false));
            }

            loginTokenRepository.UpdateRange(loginTokens);
        }

        return "Şifreniz başarıyla sıfırlandı. Yeni şifrenizle giriş yapabilirsiniz.";
    }
}
