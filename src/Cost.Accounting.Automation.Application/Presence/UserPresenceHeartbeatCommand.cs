using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Presence;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Presence;

/// <summary>
/// Oturum açan kullanıcının "ben buradayım" bildirimini tazeler.
/// </summary>
/// <remarks>
/// <para>
/// Uygulamada sunucu katmanı olmadığı için çevrimiçi bilgisi bu komutla
/// veritabanına yazılır. İstemci komutu girişte bir kez, sonra kalp atışı
/// aralığı boyunca periyodik olarak gönderir.
/// </para>
/// <para>
/// Komut <b>oturum kimliğini</b> taşır ve mesajlaşma yetkisinden bağımsız
/// olarak her oturum açan kullanıcı için çalışır: tek-oturum kuralı tüm
/// kullanıcılar için geçerlidir. Aynı kullanıcı iki makineden çalışırsa son
/// kalp atışının oturumu geçerli sayılır.
/// </para>
/// </remarks>
public sealed record UserPresenceHeartbeatCommand(
    Guid SessionId,
    string? MachineName = null) : IRequest<Result<bool>>;

internal sealed class UserPresenceHeartbeatCommandHandler(
    IUserPresenceRepository presenceRepository,
    IClaimContext claimContext) : IRequestHandler<UserPresenceHeartbeatCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UserPresenceHeartbeatCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new IdentityId(claimContext.GetUserId());
        var companyId = new IdentityId(claimContext.GetCompanyId());

        DateTimeOffset now = DateTimeOffset.Now;

        UserPresence? presence = await presenceRepository.FindByUserIdAsync(
            userId, cancellationToken);

        // Satır yoksa ya da başka bir oturumun sahibiyse yeni oturum açılır.
        if (presence is null)
        {
            presence = new UserPresence(userId, companyId, request.SessionId, now, request.MachineName);

            await presenceRepository.AddAsync(presence, cancellationToken);

            // Biriken kapalı oturum kayıtları giriş anında temizlenir. Kalp atışı
            // sırasında yapılsaydı her istemci her seferinde silme sorgusu
            // çalıştırırdı; oysa giriş zaten seyrek bir işlemdir.
            await presenceRepository.PurgeClosedSessionsAsync(
                now - UserPresence.ClosedSessionRetention, cancellationToken);
        }
        else if (presence.SessionId != request.SessionId)
        {
            presence.BeginSession(request.SessionId, now, request.MachineName);
        }
        else
        {
            presence.Touch(now);
            presence.SetMachineName(request.MachineName);
        }

        return Result<bool>.Succeed(true);
    }
}

/// <summary>
/// Oturumu kapatır: kullanıcının satırı "pasif" olarak işaretlenir.
/// </summary>
/// <remarks>
/// Kapanış bildirimi olmasa da kullanıcı bir süre sonra kendiliğinden pasife
/// düşer (<see cref="UserPresence.OnlineWindow"/>). Ancak o süre boyunca diğer
/// istemciler onu hâlâ çevrimiçi görür; çıkış bildirimi bunu bekletmeyi
/// önlemek içindir.
/// </remarks>
public sealed record UserPresenceSignOutCommand(
    Guid SessionId) : IRequest<Result<bool>>;

internal sealed class UserPresenceSignOutCommandHandler(
    IUserPresenceRepository presenceRepository,
    IClaimContext claimContext) : IRequestHandler<UserPresenceSignOutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UserPresenceSignOutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = new IdentityId(claimContext.GetUserId());

        UserPresence? presence = await presenceRepository.FindByUserIdAsync(
            userId, cancellationToken);

        if (presence is null)
        {
            return Result<bool>.Succeed(false);
        }

        // Başka bir oturumun sahipliğindeyse bu çıkış ona ait değildir.
        bool closed = presence.SignOut(request.SessionId, DateTimeOffset.Now);

        return Result<bool>.Succeed(closed);
    }
}