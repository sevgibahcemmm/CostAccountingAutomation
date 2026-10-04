namespace Cost.Accounting.Automation.Domain.Abstractions;

/// <summary>
/// Silme öncesi denetim sonucu. İlgili denetimi yapan repository bu kaydı
/// döndürür; silme komutları ve liste ekranları aynı sonucu kullanır.
/// </summary>
/// <param name="MovementIds">
/// Hareket/işlem gördüğü için silinmesi engellenen kayıtlar.
/// </param>
/// <param name="RelatedIds">
/// Hareket görmemiş, ancak başka kayıtlarda referans verildiği için silme
/// sonrasında not üretilen kayıtlar.
/// </param>
public sealed record DeletionCheck(
    IReadOnlyCollection<Guid> MovementIds,
    IReadOnlyCollection<Guid> RelatedIds)
{
    /// <summary>Engelleyici kayıt bulunmayan sonuç.</summary>
    public static DeletionCheck Empty { get; } = new([], []);

    /// <summary>Hareket görmüş (silinmesi engellenen) kayıt var mı?</summary>
    public bool HasMovement => MovementIds.Count > 0;

    /// <summary>Yalnızca ilişkili referansı olan (not üretilecek) kayıt var mı?</summary>
    public bool HasRelated => RelatedIds.Count > 0;
}
