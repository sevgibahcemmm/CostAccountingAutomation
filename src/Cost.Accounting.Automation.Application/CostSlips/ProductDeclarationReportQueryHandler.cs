using Cost.Accounting.Automation.Domain.CostSlips;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.CostSlips;

internal sealed class ProductDeclarationReportQueryHandler(
    ICostSlipRepository costSlipRepository) : IRequestHandler<ProductDeclarationReportQuery, List<ProductDeclarationRowDto>>
{
    public async Task<List<ProductDeclarationRowDto>> Handle(
        ProductDeclarationReportQuery request,
        CancellationToken cancellationToken)
    {
        return await costSlipRepository.GetAll()
            .Where(s => s.CostSlipType == CostSlipType.Product)
            .Where(s => s.Status == CostSlipStatus.Approved)
            .Where(s => !s.IsDeleted)
            .Where(s => s.CostDate >= request.StartDate && s.CostDate <= request.EndDate)
            .Where(s => s.ProducedProduct != null)
            .Select(s => new ProductDeclarationRowDto
            {
                WorkshopId = s.Workshop == null ? (Guid?)null : s.Workshop.Id.Value,
                WorkshopName = s.Workshop == null ? string.Empty : s.Workshop.Name.Value,
                ProductName = s.ProducedProduct!.Name.Value,
                ProductUnitTypeName = s.ProducedProduct.ProductUnitType == null
                    ? string.Empty
                    : s.ProducedProduct.ProductUnitType.Name.Value,
                Quantity = s.Quantity,
                UnitCost = s.Quantity > 0 ? s.GrandTotal / s.Quantity : 0m,
                Total = s.GrandTotal,
                CreatedAt = s.CreatedAt
            })
            // Sıra kuralı: önce gruplar (atölye), sonra kayıt tarihleri.
            // Aynı atölye içinde en son kaydedilen pusula üstte; saat çakışması
            // olmayan kayıtlarda istenmeyen bir sıra oluşmasın diye belge
            // numarası son çare anahtarıdır.
            .OrderBy(r => r.WorkshopName)
            .ThenByDescending(r => r.CreatedAt)
            .ThenBy(r => r.ProductName)
            .ToListAsync(cancellationToken);
    }
}