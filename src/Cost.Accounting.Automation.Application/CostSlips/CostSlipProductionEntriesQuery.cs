using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

/// <summary>
/// Taşınır işlem fişine dönüştürülecek mamül üretim girişlerini getirir.
///
/// <para>
/// Stok girişini yazan maliyet pusulası ŞU koşulların tümünü sağlar:
/// onaylı (onay işlemi girişi yazar), mamül tipinde ve üretilen ürünü
/// ile miktarı dolu. Sorgu aynı filtreleri kullanır; böylece fiş, stok
/// defterinde karşılığı OLMAYAN bir girişi basmaz.
/// </para>
/// </summary>
[Permission("costslip:view")]
public sealed record CostSlipProductionEntriesQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<List<CostSlipProductionEntryDto>>;

/// <summary>Bir mamül üretim girişinin fiş satırına dönüşen hali.</summary>
public sealed class CostSlipProductionEntryDto
{
    public Guid Id { get; init; }
    public string SlipNumber { get; init; } = string.Empty;
    public DateOnly CostDate { get; init; }

    public Guid WorkshopId { get; init; }
    public string WorkshopName { get; init; } = string.Empty;

    public Guid ProducedProductId { get; init; }
    public string ProducedProductName { get; init; } = string.Empty;

    /// <summary>
    /// Ürünün kendi stok hesabı (ör. "152.10.01.00001"). Fişin atölyeye
    /// ait 152 hesabını buradan türetiriz: atölye hesabı 152 alt hesabıyla
    /// bağlı değilse (hesap planında bağ kurulmamışsa) ürün hesabının üst
    /// düzeyi atölyenin hesabını verir.
    /// </summary>
    public string ProducedProductAccountCode { get; init; } = string.Empty;

    public int Quantity { get; init; }
    public decimal GrandTotal { get; init; }

    /// <summary>
    /// Stok girişine yazılan birim maliyet. CostSlipStockHelper ile aynı
    /// hesap: Genel Toplam / Miktar, 2 haneye yuvarlanmış.
    /// </summary>
    public decimal UnitCost => Quantity > 0 ? Math.Round(GrandTotal / Quantity, 2) : 0m;
}

internal sealed class CostSlipProductionEntriesQueryHandler(
    ICostSlipRepository costSlipRepository)
    : IRequestHandler<CostSlipProductionEntriesQuery, List<CostSlipProductionEntryDto>>
{
    public async Task<List<CostSlipProductionEntryDto>> Handle(
        CostSlipProductionEntriesQuery request,
        CancellationToken cancellationToken)
    {
        return await costSlipRepository.GetAll()
            .Where(s => !s.IsDeleted)
            .Where(s => s.Status == CostSlipStatus.Approved)
            .Where(s => s.CostSlipType == CostSlipType.Product)
            .Where(s => s.CostDate >= request.StartDate && s.CostDate <= request.EndDate)
            .Where(s => s.ProducedProduct != null && s.Quantity > 0)
            .Select(s => new CostSlipProductionEntryDto
            {
                Id = s.Id.Value,
                SlipNumber = s.SlipNumber,
                CostDate = s.CostDate,
                WorkshopId = s.WorkshopId.Value,
                WorkshopName = s.Workshop == null ? string.Empty : s.Workshop.Name.Value,
                ProducedProductId = s.ProducedProductId!.Value,
                ProducedProductName = s.ProducedProduct!.Name.Value,
                ProducedProductAccountCode = s.ProducedProduct.ChartOfAccount == null
                    ? string.Empty
                    : s.ProducedProduct.ChartOfAccount.Code.Value,
                Quantity = s.Quantity,
                GrandTotal = s.GrandTotal
            })
            .OrderBy(r => r.WorkshopName)
            .ThenBy(r => r.CostDate)
            .ThenBy(r => r.SlipNumber)
            .ToListAsync(cancellationToken);
    }
}