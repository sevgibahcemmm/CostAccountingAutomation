using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using FluentValidation;
using GenericRepository;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Kullanıcı adına sıfırlama kodu üretir. <b>Yalnızca yetkili yönetici</b>
/// çağırabilir.
/// </summary>
/// <remarks>
/// <para>
/// Kod burada üretilir ve yalnızca çağıran yöneticiye döner. Kullanıcıya hiçbir
/// otomatik kanal olmadığı için yönetici kodu telefonla ya da yüz yüze iletir.
/// Böylece yönetici parolayı öğrenmez; iletilen şey kısa ömürlü ve tek
/// kullanımlık bir bilgidir.
/// </para>
/// <para>
/// Bu akış, kullanıcının şifresini tamamen kaybetmesi durumunda yönetici müdahale
/// edebildiği yoldur. Daha önce böyle bir yol <b>yoktu</b>: kullanıcı ya
/// sızdıran kendi kendine sıfırlama akışına ya da tohum parolasına düşüyordu.
/// </para>
/// </remarks>
/// <para>
/// Yetki denetimi <c>PermissionBehavior</c> tarafından yapılır ve özniteliği
/// <b>istek kaydının</b> üzerinde arar; işleyici üzerine konulması denetimi
/// sessizce atlar ve herkes kod üretebilir hale gelir. Bu yüzden öznitelik
/// kaydın kendisindedir.
/// </para>
[Permission("user:reset_password")]
public sealed record AdminIssuePasswordResetCommand(
    Guid UserId) : IRequest<Result<AdminIssuePasswordResetResponse>>;

/// <summary>
/// Üretilen sıfırlama kodu ve geçerlilik bilgisi.
/// </summary>
public sealed record AdminIssuePasswordResetResponse
{
    /// <summary>Kullanıcıya iletilecek tek kullanımlık kod.</summary>
    public required string Code { get; init; }

    /// <summary>Yöneticinin ekranında gösterilecek, kullanıcıya verilmeyen bilgi.</summary>
    public required string UserFullName { get; init; }

    /// <summary>Kodun geçerlilik bitişi.</summary>
    public required DateTimeOffset ExpiresAt { get; init; }
}

public sealed class AdminIssuePasswordResetCommandValidator : AbstractValidator<AdminIssuePasswordResetCommand>
{
    public AdminIssuePasswordResetCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir kullanıcı seçin");
    }
}

[Permission("user:reset_password")]
internal sealed class AdminIssuePasswordResetCommandHandler(
    IUserRepository userRepository,
    IClaimContext claimContext
  ) : IRequestHandler<AdminIssuePasswordResetCommand, Result<AdminIssuePasswordResetResponse>>
{
    public async Task<Result<AdminIssuePasswordResetResponse>> Handle(
        AdminIssuePasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(
            p => p.Id == new IdentityId(request.UserId),
            cancellationToken);

        if (user is null)
        {
            return Result<AdminIssuePasswordResetResponse>.Failure("Kullanıcı bulunamadı");
        }

        if (user.IsActive == false)
        {
            return Result<AdminIssuePasswordResetResponse>.Failure(
                "Pasif kullanıcılar için şifre sıfırlama kodu üretilemez");
        }

        // Yönetici kendine kod üretemez. Kendi şifresini "Şifremi Değiştir" ile
        // (eski şifresini vererek) kendisi günceller; kendine kod üretmek hem
        // anlamsızdır hem de "yönetici kendi hesabını kodu üreten kişi olarak
        // kilitleyebilir" riskini taşır.
        if (user.Id == new IdentityId(claimContext.GetUserId()))
        {
            return Result<AdminIssuePasswordResetResponse>.Failure(
                "Kendi hesabınız için sıfırlama kodu üretemezsiniz. "
                + "Kendi şifrenizi 'Şifremi Değiştir' ile güncelleyin.");
        }

        user.CreatePasswordResetRequest();
        userRepository.Update(user);

        // Kodu üreten yönetici kaydın UpdatedBy alanına otomatik yazılır
        // (EntityAuditTracker), yani "kimi sıfırladı" sorusunun cevabı veritabanında
        // hazır bulunur.
        return new AdminIssuePasswordResetResponse
        {
            Code = user.ForgotPasswordCode!.Value.ToString("D"),
            UserFullName = user.FirstName.Value + " " + user.LastName.Value,
            ExpiresAt = DateTimeOffset.Now.Add(User.PasswordResetCodeValidity)
        };
    }
}
