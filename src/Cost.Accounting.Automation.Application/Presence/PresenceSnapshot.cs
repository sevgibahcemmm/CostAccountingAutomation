using Cost.Accounting.Automation.Domain.Presence;

namespace Cost.Accounting.Automation.Application.Presence;

/// <summary>
/// Varlık satırlarından okunabilir durum sözlüğü üretir.
/// </summary>
/// <remarks>
/// <para>
/// "Çevrimiçi mi" sorusunun tek bir kuralı olmalıdır. Kural hem
/// <c>UserPresenceStateQuery</c> hem kullanıcı listesini besleyen sorgu
/// tarafından aynı biçimde uygulanmalıdır; iki yerde ayrı ayrı yazılırsa
/// biri <c>LoggedOutAt</c>'ı unutur ve liste ile rozet birbirini yalanlar.
/// </para>
/// <para>
/// Bu sınıf yalnızca saf eşleme yapar; veritabanına sorgu atmaz.
/// </para>
/// </remarks>
internal static class PresenceSnapshot
{
    /// <summary>
    /// Satırları kullanıcı kimliğine göre sözlüğe çevirir.
    /// </summary>
    /// <param name="rows">Depodan gelen çevrimiçi kayıtları.</param>
    /// <param name="now">Hesaplama anı.</param>
    public static Dictionary<Guid, UserPresenceStateDto> ToStatesByUserId(
        IEnumerable<UserPresence> rows,
        DateTimeOffset now)
    {
        List<UserPresence> list = rows as List<UserPresence> ?? rows.ToList();

        Dictionary<Guid, UserPresenceStateDto> states = new(list.Count);

        foreach (UserPresence row in list)
        {
            states[row.UserId.Value] = ToState(row, now);
        }

        return states;
    }

    /// <summary>Tek bir satırı okunabilir duruma çevirir.</summary>
    public static UserPresenceStateDto ToState(UserPresence row, DateTimeOffset now) => new()
    {
        UserId = row.UserId.Value,
        IsOnline = row.IsOnlineAt(now),
        LastSeenAt = row.LastHeartbeatAt,
        SessionStartedAt = row.SessionStartedAt,
        SignedOut = row.LoggedOutAt is not null,
        MachineName = row.MachineName
    };

    /// <summary>
    /// Sözlükteki çevrimiçi kullanıcı kimliklerini döndürür.
    /// </summary>
    public static HashSet<Guid> OnlineUserIds(Dictionary<Guid, UserPresenceStateDto> states)
    {
        HashSet<Guid> online = new();

        foreach ((Guid userId, UserPresenceStateDto state) in states)
        {
            if (state.IsOnline)
            {
                online.Add(userId);
            }
        }

        return online;
    }
}