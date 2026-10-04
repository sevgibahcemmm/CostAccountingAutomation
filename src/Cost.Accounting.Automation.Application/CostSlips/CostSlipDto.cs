using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;

namespace Cost.Accounting.Automation.Application.CostSlips;

public sealed class CostSlipItemDto
{
    [Column("Id", IsVisible = false)]
    public Guid Id { get; set; }

    [Column("Ürün / Masraf", Order = 10, Width = 220)]
    public string ProductName { get; set; } = string.Empty;

    [Column("Birim", Order = 20, Width = 80, Alignment = "Center")]
    public string ProductUnitTypeName { get; set; } = string.Empty;

    [Column("Hesap", Order = 30, Width = 260)]
    public string ExpenseAccountTypeName => CostSlipDto.GetDisplayName(ExpenseAccountType);

    [Column("Miktar", Order = 40, Width = 90, Format = "n2", Alignment = "Right")]
    public decimal Quantity { get; set; }

    [Column("Birim Fiyat", Order = 50, Width = 100, Format = "n2", Alignment = "Right")]
    public decimal UnitPrice { get; set; }

    [Column("Tutar", Order = 60, Width = 110, Format = "n2", Alignment = "Right")]
    public decimal TotalAmount { get; set; }

    [Column("Açıklama", IsVisible = false)]
    public string Description { get; set; } = string.Empty;

    public Guid? ProductId { get; set; }

    public Guid? ProductUnitTypeId { get; set; }

    public ExpenseAccountType ExpenseAccountType { get; set; }
}

public sealed class CostSlipListDto : EntityDto, IApprovalStatusDto
{
    [Column("Pusula No", Order = 10, Width = 130)]
    public string SlipNumber { get; set; } = default!;

    [Column("Tür", Order = 20, Width = 90, Alignment = "Center")]
    public string CostSlipTypeName => CostSlipDto.GetDisplayName(CostSlipType);

    [Column("Durum", Order = 25, Width = 75, Alignment = "Center")]
    public string StatusName => Status == CostSlipStatus.Approved ? "Onaylı" : "Taslak";

    [Column("Tarih", Order = 30, Width = 90, Format = "dd.MM.yyyy", Alignment = "Center")]
    public DateOnly CostDate { get; set; }

    [Column("Atölye", Order = 40, Width = 170)]
    public string WorkshopName { get; set; } = string.Empty;

    [Column("Üretilen Ürün", Order = 45, Width = 190)]
    public string ProducedProductName { get; set; } = string.Empty;

    [Column("Müşteri", Order = 50, Width = 170, IsVisible = false)]
    public string CustomerName { get; set; } = string.Empty;

    [Column("Miktar", Order = 55, Width = 70, Alignment = "Center")]
    public int Quantity { get; set; }

    [Column("Genel Toplam", Order = 60, Width = 120, Format = "n2", Alignment = "Right")]
    public decimal GrandTotal { get; set; }

    [Column("Açıklama", Order = 70, Width = 200)]
    public string Description { get; set; } = string.Empty;

    [Column("Pusula Tipi", IsVisible = false)]
    public CostSlipType CostSlipType { get; set; }

    [Column("Durum Kodu", IsVisible = false)]
    public CostSlipStatus Status { get; set; }

    /// <summary>
    /// Ortak durum sözleşmesi: liste ekranı taslak/onaylı ayrımını renk olarak
    /// yansıtır. <c>[Column]</c> taşımadığı için gridde ayrı kolon olusmaz.
    /// </summary>
    public bool IsApproved => Status == CostSlipStatus.Approved;

    [Column("Atölye Id", IsVisible = false)]
    public Guid WorkshopId { get; set; }

    [Column("Üretilen Ürün Id", IsVisible = false)]
    public Guid? ProducedProductId { get; set; }

    [Column("Müşteri Id", IsVisible = false)]
    public Guid? CustomerId { get; set; }

    public List<CostSlipItemDto> CostSlipItems { get; set; } = [];
}

public sealed class CostSlipDto : EntityDto
{
    public string SlipNumber { get; set; } = default!;
    public CostSlipType CostSlipType { get; set; }
    public CostSlipStatus Status { get; set; }
    public DateOnly CostDate { get; set; }

    public Guid WorkshopId { get; set; }
    public string WorkshopName { get; set; } = string.Empty;

    public Guid? ProducedProductId { get; set; }
    public string ProducedProductName { get; set; } = string.Empty;
    public string ProducedProductUnitTypeName { get; set; } = string.Empty;
    public decimal ProducedProductUnitCost { get; set; }

    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }

    public List<CostSlipItemDto> CostSlipItems { get; set; } = [];

    public static string GetDisplayName<T>(T value) where T : struct, Enum
        => Cost.Accounting.Automation.Application.Helpers.EnumDisplay.GetDisplayName(value);
}

public static class CostSlipExtensions
{
    public static IQueryable<CostSlipListDto> MapTo(this IQueryable<EntityWithAuditDto<CostSlip>> entity)
    {
        return entity
            .Select(s => new CostSlipListDto
            {
                Id = s.Entity.Id,
                SlipNumber = s.Entity.SlipNumber,
                CostSlipType = s.Entity.CostSlipType,
                Status = s.Entity.Status,
                CostDate = s.Entity.CostDate,
                WorkshopId = s.Entity.WorkshopId,
                WorkshopName = s.Entity.Workshop == null ? string.Empty : s.Entity.Workshop.Name.Value,
                ProducedProductId = s.Entity.ProducedProductId == null ? null : s.Entity.ProducedProductId.Value,
                ProducedProductName = s.Entity.ProducedProduct == null ? string.Empty : s.Entity.ProducedProduct.Name.Value,
                CustomerId = s.Entity.CustomerId == null ? null : s.Entity.CustomerId.Value,
                CustomerName = s.Entity.Customer == null ? string.Empty : s.Entity.Customer.Name.Value,
                Quantity = s.Entity.Quantity,
                GrandTotal = s.Entity.GrandTotal,
                Description = s.Entity.Description.Value,
                CostSlipItems = s.Entity.CostSlipItems
                    .Select(i => new CostSlipItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId == null ? null : i.ProductId.Value,
                        ProductName = i.Product == null ? string.Empty : i.Product.Name.Value,
                        ProductUnitTypeId = i.ProductUnitTypeId == null ? null : i.ProductUnitTypeId.Value,
                        ProductUnitTypeName = i.ProductUnitType == null ? string.Empty : i.ProductUnitType.Name.Value,
                        ExpenseAccountType = i.ExpenseAccountType,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalAmount = i.TotalAmount,
                        Description = i.Description.Value
                    })
                    .ToList(),

                CreatedAt = s.Entity.CreatedAt,
                CreatedBy = s.Entity.CreatedBy,
                IsActive = s.Entity.IsActive,
                UpdatedAt = s.Entity.UpdatedAt,
                UpdatedBy = s.Entity.UpdatedBy == null ? null : s.Entity.UpdatedBy.Value,
                CreatedFullName = s.CreatedUser.FullName.Value,
                UpdatedFullName = s.UpdatedUser == null ? null : s.UpdatedUser.FullName.Value
            })
            .AsQueryable();
    }

    public static CostSlipDto ToDto(this CostSlip slip)
    {
        return new CostSlipDto
        {
            Id = slip.Id,
            SlipNumber = slip.SlipNumber,
            CostSlipType = slip.CostSlipType,
            Status = slip.Status,
            CostDate = slip.CostDate,
            WorkshopId = slip.WorkshopId,
            WorkshopName = slip.Workshop?.Name.Value ?? string.Empty,
            ProducedProductId = slip.ProducedProductId == null ? null : slip.ProducedProductId.Value,
            ProducedProductName = slip.ProducedProduct?.Name.Value ?? string.Empty,
            ProducedProductUnitTypeName = slip.ProducedProduct?.ProductUnitType?.Name.Value ?? string.Empty,
            CustomerId = slip.CustomerId == null ? null : slip.CustomerId.Value,
            CustomerName = slip.Customer?.Name.Value ?? string.Empty,
            Quantity = slip.Quantity,
            Description = slip.Description.Value,
            GrandTotal = slip.GrandTotal,
            IsActive = slip.IsActive,
            CreatedAt = slip.CreatedAt,
            CreatedBy = slip.CreatedBy,
            UpdatedAt = slip.UpdatedAt,
            UpdatedBy = slip.UpdatedBy == null ? null : slip.UpdatedBy.Value,
            CostSlipItems = slip.CostSlipItems
                .Select(l => new CostSlipItemDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId == null ? null : l.ProductId.Value,
                    ProductName = l.Product?.Name.Value ?? string.Empty,
                    ProductUnitTypeId = l.ProductUnitTypeId == null ? null : l.ProductUnitTypeId.Value,
                    ProductUnitTypeName = l.ProductUnitType?.Name.Value ?? string.Empty,
                    ExpenseAccountType = l.ExpenseAccountType,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    TotalAmount = l.TotalAmount,
                    Description = l.Description.Value
                })
                .OrderBy(x => (int)x.ExpenseAccountType)
                .ToList()
        };
    }
}
