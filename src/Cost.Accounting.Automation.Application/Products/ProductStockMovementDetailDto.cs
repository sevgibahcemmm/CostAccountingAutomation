using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.Products;

/// <summary>
/// Ürün detaylarında (liste satırı altı ve detay formu) gösterilen, belge bilgisi
/// çözülmüş stok hareketi satırı.
/// </summary>
public sealed class ProductStockMovementDetailDto
{
    [Column("Hareket Tarihi", Order = 10, Width = 95, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly Date { get; set; }

    [Column("Belge Türü", Order = 20, Width = 140)]
    public string DocumentTypeName { get; set; } = string.Empty;

    [Column("Belge No", Order = 30, Width = 130)]
    public string? DocumentNumber { get; set; }

    [Column("Belge Tarihi", Order = 40, Width = 95, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly? DocumentDate { get; set; }

    [Column("Hareket", Order = 50, Width = 80, Alignment = "Center")]
    public string MovementTypeName => MovementType == ProductMovementType.Input ? "Giriş" : "Çıkış";

    [Column("Neden", Order = 60, Width = 110)]
    public string ReasonName => EnumDisplay.GetDisplayName(Reason);

    [Column("Miktar", Order = 70, Width = 85, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 80, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal? UnitPrice { get; set; }

    [Column("Tutar", Order = 90, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal? TotalPrice => UnitPrice.HasValue ? Quantity * UnitPrice.Value : null;

    [Column("Açıklama", Order = 100, Width = 260)]
    public string Description { get; set; } = string.Empty;

    [Column("Hareket Türü", IsVisible = false)]
    public ProductMovementType MovementType { get; set; }

    [Column("Neden", IsVisible = false)]
    public ProductMovementReason Reason { get; set; }

    [Column("Belge / Ref No", IsVisible = false)]
    public string? ReferenceNo { get; set; }
}
