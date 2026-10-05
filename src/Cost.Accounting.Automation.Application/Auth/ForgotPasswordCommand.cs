using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Şifre sıfırlama talebi.
/// </summary>
/// <remarks>
/// <b>Bu istek kodu döndürmez.</b> Kod, talebi alan kişiye iletilmez; onu
/// sistem yöneticisi üretir ve kullanıcıya sözlü olarak iletir. Daha önce kod
/// doğrudan yanıtta dönüyordu; e-posta adresini bilen herkes bu bilgiyi alıp
/// hesabı ele geçirebiliyordu.
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
    IUserRepository userRepository
  ) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordCommandResponse>>
{
    /// <summary>
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
            // Yalnızca kaydın varlığı doğrulanır ve eski kod (varsa) geçersiz
            // kılınır. Yeni kod burada ÜRETİLMEZ: üretimi yönetici yapar.
            user.MarkPasswordResetCompleted();
            userRepository.Update(user);
        }

        return new ForgotPasswordCommandResponse(NeutralMessage);
    }
}
