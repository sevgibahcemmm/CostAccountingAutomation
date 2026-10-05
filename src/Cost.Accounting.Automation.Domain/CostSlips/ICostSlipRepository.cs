using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips.CostSlipItems;

namespace Cost.Accounting.Automation.Domain.CostSlips;

public interface ICostSlipRepository : IAuditableRepository<CostSlip>
{
    Task<CostSlip?> GetWithDetailsAsync(IdentityId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen pusulalara ait satırları ürün ve birim bilgileriyle birlikte
    /// döner.
    /// </summary>
    /// <remarks>
    /// Liste ekranı satırları ihtiyaç duyduğu halde <c>CostSlipItems</c>
    /// koleksiyonunu <c>CostSlip</c> üzerinden <c>Include</c> etmemelidir: tek
    /// birleşimli (split olmayan) projeksiyon, ana kayıt satırlarıyla satır
    /// listesinin kartesian çarpımını üretir. Koleksiyon projeksiyonda
    /// kullanıldığında EF ayrı bir "split" komutu üretir ve liste 25 saniyeye
    /// çıkmıştır. Bu metot satırları bağımsız ve <c>IN</c> filtreli basit bir
    /// sorguyla getirir.
    /// </remarks>
    Task<IReadOnlyList<CostSlipItem>> GetItemsAsync(
        IReadOnlyCollection<IdentityId> costSlipIds,
        CancellationToken cancellationToken = default);
}