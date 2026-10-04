namespace Cost.Accounting.Automation.Domain.Abstractions;

/// <summary>
/// Onay akışı olan belgelerin liste DTO'ları için ortak durum sözleşmesi.
///
/// <para>
/// Maliyet pusulası, irsaliye ve stok belgesi farklı durum enum'ları kullanır
/// (<c>CostSlipStatus</c>, <c>InvoiceStatus</c>, <c>StockIssueStatus</c>) ama
/// anlamları aynıdır: <b>Taslak</b> henüz stok/yevmiye hareketi üretmemiş,
/// <b>Onaylı</b> kalıcılaşmış kayıttır. Liste ekranlarının bu ayrımı ortak bir
/// arayüzle tanıması, renklendirmenin tek bir yerde yapılmasını sağlar.
/// </para>
///
/// <para>
/// Uygulayan DTO, <c>IsApproved</c> üyesini kendi enum'üne göre eşler.
/// Sözleşme yalnızca <b>okuma</b> amaçlıdır; durumu yalnızca sunucu değiştirir.
/// </para>
/// </summary>
public interface IApprovalStatusDto
{
    /// <summary>
    /// Kayıt onaylandı mı? <c>false</c> ise kayıt TASLAKtır ve stok/yevmiye
    /// hareketi henüz oluşmamıştır.
    /// </summary>
    bool IsApproved { get; }
}