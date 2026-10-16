using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Invoices;

/// <summary>
/// Fatura ve fatura satırları için sorgu uçları.
/// </summary>
public interface IInvoiceRepository : IAuditableRepository<Invoice>
{
    /// <summary>
    /// Verilen faturaların satırlarını <b>tek</b> sorguda getirir.
    /// </summary>
    /// <remarks>
    /// Liste ekranı satırları master-detail alt ızgarasında gösterir. Satırları
    /// ana liste projeksiyonunun içine almak kartesian çarpım üretir (her
    /// fatura için satırları çarpıp tek satıra indirgemek), bu da liste
    /// sorgusunu üstel olarak şişirir. Bu yüzden ana liste satırsız gelir ve
    /// satırlar ikinci bir sorguda yalnızca seçili faturalar için alınır.
    /// </remarks>
    Task<List<InvoiceLine>> GetLinesByInvoiceIdsAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tek bir faturayı satırları ve ürünleriyle birlikte getirir.
    /// </summary>
    Task<Invoice?> GetByIdWithLinesAsync(
        IdentityId id,
        CancellationToken cancellationToken = default);
}
