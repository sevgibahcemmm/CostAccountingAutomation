using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Şifre sıfırlama talebi.
/// </summary>
/// <remarks>
/// <para>
/// Bu istek kodu <b>yanıt içinde</b> döndürmez. E-posta gönderimi açıksa
/// (<c>Email.Enabled</c>) kod üretilir ve kayıtlı e-posta adresine gönderilir;
/// adresin sistemde olup olmadığı yanıttan anlaşılamaz (nötr yanıt). E-posta
/// gönderimi kapalıysa kod üretimi sistem yöneticisine aittir
/// (<see cref="AdminIssuePasswordResetCommand"/>).
/// </para>
/// <para>
/// Daha önce kod doğrudan yanıtta dönüyordu; e-posta adresini bilen herkes bu
/// bilgiyi alıp hesabı ele geçirebiliyordu. Bu yüzden yanıt her koşulda nötr
/// tutulur; kod yalnızca kullanıcının kendi e-posta adresine gönderilir.
/// </para>
/// </remarks>
public sealed record ForgotPasswordCommand(
    string Email) : IRequest<Result<ForgotPasswordCommandResponse>>;

public sealed record ForgotPasswordCommandResponse(string Message);

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
    IUserRepository userRepository,
    IEmailSender emailSender
  ) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordCommandResponse>>
{
    /// <summary>
    /// E-posta gönderimi açıkken kullanılan yanıt.
    /// </summary>
    private const string EmailSentMessage =
        "Eğer bu adres kayıtlıysa, şifre sıfırlama kodu e-posta adresinize gönderildi. "
        + "Posta kutunuzu (gerekiyorsa önemsiz/spam klasörünü) kontrol edin.";

    /// <summary>
    /// E-posta gönderimi kapalıyken kullanılan yanıt; kod üretimini yönetici yapar.
    /// Bilinçli olarak nötr. Hesabın var olup olmadığını ayırmak, e-posta
    /// adreslerinin sisteme kayıtlı kullanıcılara ait olup olmadığını ölçmeye
    /// yarar. Gerekirse "kullanıcı yok" bilgisi ayrı bir denetim kaydına yazılır.
    /// </summary>
    private const string NeutralMessage =
        "Eğer bu adres kayıtlıysa, sıfırlama talebiniz alındı. Devam etmek için "
        + "sistem yöneticinizle görüşün.";

    public async Task<Result<ForgotPasswordCommandResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // E-posta eşleşmesi veritabanı collation'ına değil koda bağlıdır; bkz. LoginNameMatcher.
        string email = LoginNameMatcher.Clean(request.Email);

        var user = await userRepository
            .FirstOrDefaultAsync(p =>
                EF.Functions.Collate(p.Email.Value, LoginNameMatcher.Collation) == email,
                cancellationToken);

        if (user is not null)
        {
            if (emailSender.IsEnabled)
            {
                // E-posta kanalı açık: kod üretilir ve kullanıcının adresine
                // gönderilir. Kod yanıtta döndürülmez; gönderim hatası sızdırmaya
                // neden olmasın diye yanıt yine nötrdür (gönderim aracı hatayı
                // günlüğe yazar).
                user.CreatePasswordResetRequest();
                userRepository.Update(user);

                await emailSender.SendPasswordResetCodeAsync(
                    user.Email.Value,
                    user.FirstName.Value,
                    user.ForgotPasswordCode!.Value.ToString("D"),
                    DateTimeOffset.Now.Add(User.PasswordResetCodeValidity),
                    cancellationToken);
            }
            else
            {
                // E-posta kanalı yok: eski kod (varsa) geçersiz kılınır. Yeni kod
                // burada ÜRETİLMEZ; üretimi yönetici yapar.
                user.MarkPasswordResetCompleted();
                userRepository.Update(user);
            }
        }

        return new ForgotPasswordCommandResponse(
            emailSender.IsEnabled ? EmailSentMessage : NeutralMessage);
    }
}