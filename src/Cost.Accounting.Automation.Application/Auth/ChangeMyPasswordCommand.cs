using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Domain.Users.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Oturumu açan kullanıcının, mevcut şifresini vererek kendi şifresini
/// değiştirme isteği.
/// </summary>
/// <remarks>
/// <para>
/// Bu komutta <c>[Permission]</c> yoktur: <b>her kimliği doğrulanmış kullanıcı</b>
/// kendi şifresini değiştirebilir. Güvenlik iki noktada durur: kullanıcıdi
/// bir tanımlama yapılmaz (kimlik claim'den gelir) ve eski şifre doğrulanmadan
/// değişiklik yapılmaz.
/// </para>
/// <para>
/// Yöneticinin başkası için şifre işlemi <see cref="AdminIssuePasswordResetCommand"/>
/// ile sınırlıdır: yönetici yalnızca tek kullanımlık bir kod üretir ve parolayı
/// öğrenmez. Bu komut kişinin yalnızca kendi hesabına dokunur; yönetici kendisi
/// için de bu yolu kullanır (yönetici kendi hesabına kod üretemez, çünkü kendine
/// sıfırlama kodu üretme anlamsız ve tehlikelidir).
/// </para>
/// </remarks>
public sealed record ChangeMyPasswordCommand(
    string OldPassword,
    string NewPassword) : IRequest<Result<string>>;

public sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(p => p.OldPassword)
            .NotEmpty().WithMessage("Mevcut şifrenizi girin");

        RuleFor(p => p.NewPassword)
            .NotEmpty().WithMessage("Geçerli bir yeni şifre girin")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır");

        RuleFor(p => p.NewPassword)
            .Matches("[A-Za-zÇĞİÖŞÜçğıöşü]").WithMessage("Yeni şifre en az bir harf içermelidir")
            .Matches("[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir");
    }
}

internal sealed class ChangeMyPasswordCommandHandler(
    IUserRepository userRepository,
    IClaimContext claimContext) : IRequestHandler<ChangeMyPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChangeMyPasswordCommand request, CancellationToken cancellationToken)
    {
        // Kimlik kullanıcıdan gelmez; oturum bilgisinden okunur. Böylece başka
        // birinin hesabı için "kendi şifresini değiştir" çağrısı yapılamaz.
        var me = new IdentityId(claimContext.GetUserId());

        var user = await userRepository.FirstOrDefaultAsync(
            p => p.Id == me,
            cancellationToken);

        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı");
        }

        var verification = user.VerifyPassword(request.OldPassword);

        if (verification == PasswordVerification.Failed)
        {
            // Yanlış eski şifre üzerinde deneme yapılıp yapılmadığı denetim
            // kaydına düşmez; ancak arayüz bu hatayı yöneticiye gösterir.
            return Result<string>.Failure("Mevcut şifreniz hatalı");
        }

        user.SetPassword(new Password(request.NewPassword));

        // Daha önce üretilmiş sıfırlama kodları geçersiz kılınır; kullanıcının
        // şifresini değiştirdikten sonra eski kodla giriş yapılamasın.
        user.MarkPasswordResetCompleted();

        userRepository.Update(user);

        return "Şifreniz başarıyla güncellendi";
    }
}