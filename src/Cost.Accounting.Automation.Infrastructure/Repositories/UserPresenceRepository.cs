using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Presence;
using Cost.Accounting.Automation.Infrastructure.Context;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

/// <summary>
/// Çevrimiçi kullanıcı bilgisinin master veritabanındaki karşılığı.
/// </summary>
/// <remarks>
/// <para>
/// Bu tablo bir iş kaydı değil, uygulama açısından bir <b>önbellek</b>tir:
/// "kim şu anda uygulamada" sorusunun gecikmeli cevabı. Bu yüzden denetim
/// alanları ve satır sürümü belirteci yoktur — bilgi zaten "son yazan istemci"
/// tarafından üstüne yazılır.
/// </para>
/// <para>
/// Güvenlik kararları buradan değil, <c>Users</c> tablosundan verilir. Bu depo
/// hiçbir "kim görebilir" sorusuna yanıt vermez; yalnızca kullanıcı adını
/// çevrimiçi bir noktaya bağlar.
/// </para>
/// <para>
/// Yazma işlemi "varsa güncelle, yoksa ekle" biçimindedir. Aynı kullanıcı iki
/// istemcide aynı anda kalp attığında satır zaten vardır; yalnızca ilk girişte
/// ekleme gerekir.
/// </para>
/// </remarks>
internal sealed class UserPresenceRepository
    : Repository<UserPresence, MasterDbContext>, IUserPresenceRepository
{
    private readonly MasterDbContext _masterContext;

    public UserPresenceRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public Task<UserPresence?> FindByUserIdAsync(
        IdentityId userId,
        CancellationToken cancellationToken = default)
        => _masterContext.Set<UserPresence>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public Task<List<UserPresence>> ListSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => _masterContext.Set<UserPresence>()
            .AsNoTracking()
            .Where(p => p.LastHeartbeatAt >= since)
            .OrderBy(p => p.LastHeartbeatAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountOnlineAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
        => _masterContext.Set<UserPresence>()
            .AsNoTracking()
            .CountAsync(
                p => p.LoggedOutAt == null && p.LastHeartbeatAt >= now - UserPresence.OnlineWindow,
                cancellationToken);

    public async Task<int> PurgeClosedSessionsAsync(
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default)
    {
        // Yalnızca kapanmış satırlar silinir. Kalp atışı devam eden bir oturum
        // "eski" olsa bile yanlışlıkla silinmemelidir.
        UserPresence[] expired = await _masterContext.Set<UserPresence>()
            .Where(p => p.LoggedOutAt != null && p.LastHeartbeatAt < olderThan)
            .ToArrayAsync(cancellationToken);

        if (expired.Length == 0)
        {
            return 0;
        }

        _masterContext.Set<UserPresence>().RemoveRange(expired);

        await _masterContext.SaveChangesAsync(cancellationToken);

        return expired.Length;
    }
}