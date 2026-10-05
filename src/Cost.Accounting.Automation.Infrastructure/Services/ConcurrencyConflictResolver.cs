using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Eşzamanlılık çakışmalarını okunabilir bir mesaja dönüştürür.
/// </summary>
/// <remarks>
/// <para>
/// EF çakışmayı <c>DbUpdateConcurrencyException</c> ile bildirir ve istisnaya
/// eklediği kayıtların <b>güncel</b> veritabanı değerlerini değiş izleyicisine
/// yükler. Bu, önemlidir: kaydın <c>UpdatedBy</c> alanı artık <em>kaydı bizim
/// değil, başka birinin</em> değiştirdiğini gösterir. Dolayısıyla çakışan
/// kullanıcıyı öğrenmek için yıl veritabanında ek sorgu yapmaya gerek yoktur.
/// </para>
/// <para>
/// Geriye yalnızca o kullanıcının <b>adını</b> çözmek kalır. Kullanıcı kayıtları
/// master veritabanında tutulduğu için bu çözümleme master üzerinden yapılır.
/// </para>
/// </remarks>
internal sealed class ConcurrencyConflictResolver(MasterDbContext masterContext)
    : IConcurrencyConflictResolver
{
    private const string GenericMessage =
        "Kayıt sizin yaptığınız değişiklikten sonra başka biri tarafından güncellendi.";

    private const string Guidance =
        "Yaptığınız değişiklikler kaydedilmedi. Listeyi yenileyip değişikliklerinizi "
        + "yeniden uygulayın.";

    public async Task<string> BuildMessageAsync(
        DbUpdateConcurrencyException exception,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ConcurrencyConflict> conflicts =
            await CollectAsync(exception, cancellationToken);

        if (conflicts.Count == 0)
        {
            return $"{GenericMessage}{Environment.NewLine}{Guidance}";
        }

        string detail = string.Join(
            Environment.NewLine,
            conflicts.Select(Describe).Distinct());

        string headline = conflicts.Count == 1
            ? GenericMessage
            : $"{conflicts.Count} kayıt eşzamanlı olarak değiştirilmiş.";

        return $"{headline}{Environment.NewLine}{Environment.NewLine}{detail}"
               + $"{Environment.NewLine}{Environment.NewLine}{Guidance}";
    }

    /// <summary>
    /// İstisnadaki çakışan kayıtların denetim alanlarını okur, kullanıcı
    /// adlarını tek sorguda çözer ve sonuçları birleştirir.
    /// </summary>
    private async Task<IReadOnlyList<ConcurrencyConflict>> CollectAsync(
        DbUpdateConcurrencyException exception,
        CancellationToken cancellationToken)
    {
        // EF, istisnayı oluştururken kayıtların güncel veritabanı değerlerini
        // zaten değiş izleyicisine yüklemiştir. UpdatedBy/UpdatedAt buradan okunur.
        var pending = new List<(string EntityName, string Key, Guid? ChangedBy, DateTimeOffset? ChangedAt)>();

        foreach (var entry in exception.Entries)
        {
            if (entry.Entity is not Entity entity)
            {
                continue;
            }

            pending.Add((
                entry.Metadata.ClrType.Name,
                entity.Id.Value.ToString(),
                entity.UpdatedBy?.Value,
                entity.UpdatedAt));
        }

        if (pending.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, string> userNames = await ResolveUserNamesAsync(
            pending.Where(p => p.ChangedBy is not null).Select(p => p.ChangedBy!.Value),
            cancellationToken);

        return pending
            .Select(p => new ConcurrencyConflict(
                p.EntityName,
                p.Key,
                p.ChangedBy is { } id && userNames.TryGetValue(id, out string? name) ? name : null,
                p.ChangedAt))
            .ToList();
    }

    /// <summary>
    /// Kullanıcı kimliklerini görünen adlara çözer.
    /// </summary>
    /// <remarks>
    /// Kullanıcı silinmiş ya da master'a erişilemiyorsa boş sözlük döner;
    /// mesaj "başka bir kullanıcı" diye devam eder. Çakışma bildirimi asla
    /// işlemi düşürmemelidir.
    /// </remarks>
    private async Task<Dictionary<Guid, string>> ResolveUserNamesAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        // Kimlikler model tipine (IdentityId) çevrilerek verilir. Bu önemlidir:
        // Guid listesi doğrudan verilirse EF değer dönüştürücüsünü atlar ve
        // LINQ ifadesi SQL'e çevrilemez.
        var distinctIds = userIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Select(id => new IdentityId(id))
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return [];
        }

        try
        {
            var users = await masterContext.Users
                .AsNoTracking()
                .Where(u => distinctIds.Contains(u.Id))
                .Select(u => new { Id = u.Id.Value, FullName = u.FirstName.Value + " " + u.LastName.Value })
                .ToListAsync(cancellationToken);

            return users.ToDictionary(u => u.Id, u => u.FullName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ConcurrencyConflict] Kullanıcı adları çözülemedi: {ex.Message}");

            return [];
        }
    }

    private static string Describe(ConcurrencyConflict conflict)
    {
        string actor = conflict.ChangedByUserName is { } name
            ? $"\"{name}\""
            : "başka bir kullanıcı";

        string when = conflict.ChangedAt is { } at
            ? $" ({at.ToLocalTime():dd.MM.yyyy HH:mm})"
            : string.Empty;

        return $"• {conflict.EntityName} ({conflict.PrimaryKey}) kaydı {actor} tarafından değiştirildi{when}.";
    }
}
