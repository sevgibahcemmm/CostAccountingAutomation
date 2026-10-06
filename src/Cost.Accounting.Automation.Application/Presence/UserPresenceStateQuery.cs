using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Presence;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Presence;

/// <summary>
/// Kullanıcıların anlık çevrimiçi durumunu okur.
/// </summary>
/// <remarks>
/// <para>
/// Bu sorgu "oturum açanlar" listesinin ve rozetlerin veri kaynağıdır. İstemci
/// düzenli aralıklarla çağırır ve <b>önceki</b> sonuçla karşılaştırarak yeni
/// oturum açanları bulur; farkı kendisi hesaplamak yerine burada almak, aynı
/// iş mantığının her ekranda ayrı ayrı yazılmasını önler.
/// </para>
/// <para>
/// Sonuç yalnızca yakın zamanda hareket etmiş satırları içerir; bu yüzden
/// "kim ne zaman çıktı" bilgisi bir haftaya kadar korunur, tabloda biriken
/// eski kayıtlar okunmaz.
/// </para>
/// </remarks>
[Permission(MessagePermissions.View)]
public sealed record UserPresenceStateQuery : IRequest<Result<List<UserPresenceStateDto>>>;

/// <summary>Bir kullanıcının çevrimiçi durumu.</summary>
public sealed class UserPresenceStateDto
{
    public Guid UserId { get; set; }

    /// <summary>Kullanıcının adı; bildirimlerde okunabilir metin olarak kullanılır.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Şu an uygulamada mı?</summary>
    public bool IsOnline { get; set; }

    /// <summary>Son kalp atışının zamanı; "son görülme" bilgisi.</summary>
    public DateTimeOffset LastSeenAt { get; set; }

    /// <summary>Mevcut oturumun açıldığı an.</summary>
    public DateTimeOffset SessionStartedAt { get; set; }

    /// <summary>
    /// Kullanıcı düğmeye basarak mı çıktı? <c>true</c> ise kapanış bilinçlidir;
    /// <c>false</c> ise kalp atışı susmuştur (kapanma ya da bağlantı sorunu).
    /// </summary>
    public bool SignedOut { get; set; }

    public string? MachineName { get; set; }
}

internal sealed class UserPresenceStateQueryHandler(
    IUserPresenceRepository presenceRepository,
    IUserRepository userRepository) : IRequestHandler<UserPresenceStateQuery, Result<List<UserPresenceStateDto>>>
{
    public async Task<Result<List<UserPresenceStateDto>>> Handle(
        UserPresenceStateQuery request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.Now;

        List<UserPresence> rows = await presenceRepository.ListSinceAsync(
            now - UserPresence.ClosedSessionRetention, cancellationToken);

        List<UserPresenceStateDto> states = rows
            .Select(p => PresenceSnapshot.ToState(p, now))
            .ToList();

        await FillFullNamesAsync(states, cancellationToken);

        return Result<List<UserPresenceStateDto>>.Succeed(states);
    }

    /// <summary>
    /// Adlar sonradan çözülür: bildirim metni kullanıcının adını söylemelidir,
    /// "oturum açan biri" demek kullanıcıya hiçbir şey anlatmaz.
    /// </summary>
    private async Task FillFullNamesAsync(
        List<UserPresenceStateDto> states,
        CancellationToken cancellationToken)
    {
        if (states.Count == 0)
        {
            return;
        }

        var ids = states.Select(s => new IdentityId(s.UserId)).ToArray();

        var names = await userRepository
            .Where(u => ids.Contains(u.Id))
            .Select(u => new
            {
                Id = u.Id.Value,
                FullName = u.FirstName.Value + " " + u.LastName.Value
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> byId = names.ToDictionary(u => u.Id, u => u.FullName);

        foreach (UserPresenceStateDto state in states)
        {
            state.FullName = byId.GetValueOrDefault(state.UserId, "Kullanıcı");
        }
    }
}