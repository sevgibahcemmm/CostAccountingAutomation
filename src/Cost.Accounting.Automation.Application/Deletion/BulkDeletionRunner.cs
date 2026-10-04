using Cost.Accounting.Automation.Domain.Abstractions;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Deletion;

/// <summary>
/// <see cref="BulkDeletionRunner{TEntity}"/> çalıştıktan sonra elde edilen sonuç.
/// </summary>
/// <param name="DeletedCount">Yumuşak silmeye gönderilen kayıt sayısı.</param>
/// <param name="RelatedCount">
/// Hareket görmediği için silinmiş, ancak ilişkili referansı bulunduğu için
/// kullanıcıya not üretilecek kayıt sayısı.
/// </param>
public sealed record BulkDeletionOutcome(
    int DeletedCount,
    int RelatedCount)
{
    /// <summary>Not üretecek ilişkili kayıt var mı?</summary>
    public bool HasRelated => RelatedCount > 0;
}

/// <summary>
/// Program genelindeki toplu silme davranışının tek uygulaması.
///
/// <para>
/// Tüm toplu silme komutları bu jenerik sınıfı çağırır; böylece
/// "hareket gördüğü için silinemez" kuralı, seçim büyüklüğünden bağımsız
/// olarak iki adımda uygulanır:
/// </para>
/// <list type="number">
///   <item>
///     <description>Denetim seçimin tamamını TEK sorguda yapar
///     (<c>GetDeletionCheckAsync</c>).</description>
///   </item>
///   <item>
///     <description>Engelleyici kayıt yoksa hepsi tek çağrıda yumuşak
///     silinir; <c>SaveChangesAsync</c> handler sonunda bir kez çalıştığı
///     için ya hep silinir ya hiçbiri silinmez.</description>
///   </item>
/// </list>
///
/// <para>
/// Kayıt başına ayrı komut göndermek hem sorgu sayısını seçimle birlikte
/// büyütür hem de yarıda kalmış silme durumu üretir. Bu sınıf her ikisini de
/// ortadan kaldırır.
/// </para>
/// </summary>
public sealed class BulkDeletionRunner<TEntity>(
    IAuditableRepository<TEntity> repository,
    Func<IReadOnlyCollection<Guid>, CancellationToken, Task<DeletionCheck>>? checkDeletion = null,
    Func<IReadOnlyCollection<Guid>, CancellationToken, Task>? beforeDelete = null)
    where TEntity : Entity
{
    /// <summary>
    /// Seçilen kayıtları denetleyip tek transaction'da yumuşak siler.
    /// </summary>
    /// <param name="ids">Silinecek kayıt kimlikleri.</param>
    /// <param name="entityLabel">
    /// Kullanıcıya gösterilen kayıt adı (örn. "müşteri", "hesap").
    /// </param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    public async Task<Result<BulkDeletionOutcome>> RunAsync(
        IReadOnlyCollection<Guid> ids,
        string entityLabel,
        CancellationToken cancellationToken = default)
    {
        List<Guid> distinctIds = ids.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return Result<BulkDeletionOutcome>.Failure(DeletionMessages.NoSelection(entityLabel));
        }

        // Denetim ve silme aynı handler içinde, yani aynı DbContext örneğinde
        // çalışır. Böylece kontrol sonucu ile silme arasında başka bir
        // işlemin kayıt değiştirmesi mümkün olmaz.
        DeletionCheck check = checkDeletion is null
            ? DeletionCheck.Empty
            : await checkDeletion(distinctIds, cancellationToken);

        if (check.HasMovement)
        {
            return Result<BulkDeletionOutcome>.Failure(
                DeletionMessages.MovementBlocked(entityLabel, check.MovementIds.Count));
        }

        if (beforeDelete is not null)
        {
            await beforeDelete(distinctIds, cancellationToken);
        }

        await repository.SoftDeleteRangeAsync(
            distinctIds.Select(id => new IdentityId(id)).ToList(),
            cancellationToken);

        return Result<BulkDeletionOutcome>.Succeed(
            new BulkDeletionOutcome(distinctIds.Count, check.RelatedIds.Count));
    }
}
