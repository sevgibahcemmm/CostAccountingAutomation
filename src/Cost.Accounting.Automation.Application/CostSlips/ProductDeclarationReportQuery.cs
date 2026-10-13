using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record ProductDeclarationReportQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<List<ProductDeclarationRowDto>>;

public sealed class ProductDeclarationRowDto
{
    public Guid? WorkshopId { get; init; }
    public string WorkshopName { get; init; } = string.Empty;

    /// <summary>
    /// Beyan türü: mamül pusulaları için "Mamul", hizmet pusulaları için
    /// "Hizmet". Rapor başlığı bu alana göre dinamik yazılır.
    /// </summary>
    public string DeclarationKind { get; init; } = string.Empty;

    /// <summary>
    /// Tür sıralaması (Mamul = 0, Hizmet = 1). Grup anahtarının başına
    /// eklenerek aynı raporda önce mamül, sonra hizmet sayfalarının
    /// sıralanmasını sağlar.
    /// </summary>
    public int DeclarationOrder { get; init; }

    /// <summary>
    /// Sayfa/grup anahtarı: tür sırası + atölye. Rapor atölye bazında
    /// gruplanıp her grup yeni sayfaya taşındığı için aynı atölyenin
    /// mamül ve hizmet beyanları ayrı sayfalara düşer.
    /// </summary>
    public string DeclarationGroupKey => $"{DeclarationOrder}|{WorkshopName}";

    /// <summary>
    /// Atölyenin şefi. Beyan atölyeye göre gruplandığı için rapor bu alanı
    /// veri alanı olarak okur; parametre grup bazlı değişemez.
    /// </summary>
    public string WorkshopChiefName { get; set; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductUnitTypeName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Total { get; init; }

    /// <summary>
    /// Pusulanın sistemde kaydedildiği zaman. Beyanname satırları atölye
    /// grubundan sonra bu alana göre sıralanır (en son kaydedilen üstte).
    /// Gruba giren pusula adedini etkilediği için rapor satırlarında
    /// gösterilmez; yalnızca sıralama anahtarıdır.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}