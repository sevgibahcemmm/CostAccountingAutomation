using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Denetim kullanıcı adlarını master katalogdan okuyup DTO'lara yazar.
///
/// <para>
/// Kullanıcı sayısı az (bir kurumda onlarca), buna karşılık denetim sütunları
/// <b>her</b> liste ekranında görünür. Bu yüzden çözümlenen adlar kısa ömürlü
/// belleğe alınır: liste ekranı her açıldığında master'a ikinci bir gidiş
/// yapılmaz.
/// </para>
/// </summary>
internal sealed class AuditUserNameResolver(
    MasterDbContext masterContext,
    IMemoryCache cache) : IAuditUserNameResolver
{
    /// <summary>
    /// Önbellek anahtarı ve ömrü. Süre kısa tutulur: kullanıcı adı değişikliği
    /// (veya yeni kullanıcı eklenmesi) listelerde gecikmeden görünsün.
    /// </summary>
    public const string CacheKey = "audit:username:v1";

    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Bir kaydın denetleyeni master'da yoksa (silinmiş kullanıcı) listelerde
    /// görünecek metin.
    /// </summary>
    private const string MissingUserLabel = "(silinmis kullanici)";

    public async Task FillAsync(
        IEnumerable<EntityDto> items,
        CancellationToken cancellationToken = default)
    {
        List<EntityDto> list = [.. items];
        if (list.Count == 0)
        {
            return;
        }

        HashSet<Guid> needed = [];
        foreach (EntityDto item in list)
        {
            needed.Add(item.CreatedBy);
            if (item.UpdatedBy is Guid updatedBy)
            {
                needed.Add(updatedBy);
            }
        }

        Dictionary<Guid, string> names = await GetNamesAsync(needed, cancellationToken);

        foreach (EntityDto item in list)
        {
            item.CreatedFullName = names.GetValueOrDefault(item.CreatedBy, MissingUserLabel);
            item.UpdatedFullName = item.UpdatedBy is Guid id
                ? names.GetValueOrDefault(id, MissingUserLabel)
                : null;
        }
    }

    private async Task<Dictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> neededIds,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, string>? cached = cache.Get<Dictionary<Guid, string>>(CacheKey);
        if (cached is not null && neededIds.All(cached.ContainsKey))
        {
            return cached;
        }

        List<IdentityId> keys = [.. neededIds.Select(id => new IdentityId(id))];

        Dictionary<Guid, string> names = await masterContext.Set<User>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => keys.Contains(u.Id))
            .Select(u => new { Id = u.Id.Value, FullName = u.FullName.Value })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

        // Kullanıcı adları neredeyse hiç değişmediği için tüm kullanıcıları
        // kısa ömürlü önbelleğe alırız. Sonraki listelerde veritabanına hiç
        // gidilmez.
        cache.Set(CacheKey, names, CacheTtl);

        return names;
    }
}
