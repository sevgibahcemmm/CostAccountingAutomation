using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>
/// Doğrudan temin dosyasının liste ve detay ekranlarında kullanılan projeksiyon.
/// </summary>
/// <remarks>
/// <para>
/// Enum adları <c>[Column(IsVisible = false)]</c> işaretli ham alanlardan
/// <b>hesaplanan</b> özelliklerle gösterilir (bkz. <c>InvoiceDto</c>).
/// Projeksiyon SQL'e çevrildiği için <c>EnumDisplay.GetDisplayName</c> içine
/// çağrılamaz; çağrılsa sorgu çalışmaz.
/// </para>
/// <para>
/// <see cref="EntityDto.CreatedFullName"/> ve <see cref="EntityDto.UpdatedFullName"/>
/// bilinçli olarak doldurulmaz: kullanıcılar merkezî veritabanında, dosya
/// kayıtları ise yıl veritabanında tutulduğu için SQL birleştirmesi mümkün
/// değildir. Bu iki alan sorgudan sonra <c>IAuditUserNameResolver</c> tarafından,
/// <see cref="CompanyName"/> ise <c>IMasterCompanyNameLookup</c> tarafından doldurulur.
/// </para>
/// </remarks>
public sealed class DirectProcurementFileDto : EntityDto
{
    [Column("Dosya No", Order = 10, Width = 130)] public string FileNumber { get; set; } = default!;
    [Column("İşin Adı", Order = 20, Width = 300)] public string WorkName { get; set; } = default!;
    [Column("Yıl", Order = 30, Width = 60, Alignment = "Center", Format = "0")] public int WorkingYear { get; set; }
    [Column("Birim Id", IsVisible = false)] public Guid? CompanyId { get; set; }
    [Column("Firma", Order = 70, Width = 200)] public string? SupplierName { get; set; }

    // Düzenleme ekranının kaydı değiştirmeden aynı yazabilmesi için kimlikler
    // gizli kolon olarak taşınır; adlar değişse bile ilişki korunur.
    [Column("Firma Id", IsVisible = false)] public Guid? SupplierId { get; set; }
    [Column("Hesap Id", IsVisible = false)] public Guid? ChartOfAccountId { get; set; }
    [Column("İhale Tipi", IsVisible = false)] public ProcurementFileType FileType { get; set; }

    // Düzenleme ekranının geri yazdığı alanlar. Liste grid'inde görünmezler
    // ama formu beslemek için DTO'da bulunmalıdır.
    [Column("Karar Tarihi", IsVisible = false)] public DateOnly? DecisionDate { get; set; }
    [Column("Alımı Yapan Birim", IsVisible = false)] public string? PurchasingUnit { get; set; }
    [Column("İlgili Kişi", IsVisible = false)] public string? RelatedPerson { get; set; }
    [Column("İşin Niteliği", IsVisible = false)] public string? WorkNature { get; set; }
    [Column("İşin Miktarı", IsVisible = false)] public string? WorkQuantity { get; set; }
    [Column("Sözleşme", IsVisible = false)] public bool HasContract { get; set; }
    [Column("Şartname", IsVisible = false)] public bool HasSpecification { get; set; }
    [Column("Komisyon", IsVisible = false)] public bool HasCommission { get; set; }
    [Column("YM Hesap Şekli", IsVisible = false)] public CostEstimationMethodType CostEstimationMethod { get; set; }
    [Column("YM Yapılacak mı?", IsVisible = false)] public bool HasApproximateCost { get; set; }
    [Column("Geçmiş Alım Kullanılsın mı?", IsVisible = false)] public bool UseHistoryPurchases { get; set; }
    [Column("Son Teklif İstensin mi?", IsVisible = false)] public bool RequireFinalOffer { get; set; }
    [Column("Tutar Ondalık", IsVisible = false)] public int DecimalPlaces { get; set; }
    [Column("Birim Fiyat Ondalık", IsVisible = false)] public int UnitPriceDecimalPlaces { get; set; }
    [Column("Yaklaşık Maliyet", Order = 80, Width = 130, Alignment = "Right", Format = "N2")] public decimal? ApproximateCost { get; set; }
    [Column("Ödenek", Order = 90, Width = 130, Alignment = "Right", Format = "N2")] public decimal? Appropriation { get; set; }
    [Column("Tarih", Order = 100, Width = 100, Alignment = "Center", Format = "d")] public DateOnly? Date { get; set; }
    [Column("Proje No", Order = 110, Width = 110, IsVisible = false)] public string? ProjectNumber { get; set; }
    [Column("Karar No", Order = 120, Width = 110, IsVisible = false)] public string? DecisionNumber { get; set; }
    [Column("İhtiyaç Satırı", Order = 130, Width = 100, Alignment = "Right")] public int ItemCount { get; set; }
    [Column("Teklif", Order = 140, Width = 80, Alignment = "Right")] public int QuoteCount { get; set; }
    [Column("Arşiv", Order = 150, Width = 70, Alignment = "Center")] public bool IsArchived { get; set; }

    [Column("Birim", IsVisible = false)] public string? CompanyName { get; set; }

    [Column("İş Türü", IsVisible = false)] public ProcurementCategoryType Category { get; set; }
    [Column("Temin Şekli", IsVisible = false)] public ProcurementFlowType Flow { get; set; }
    [Column("Aşama", IsVisible = false)] public DirectProcurementStage Stage { get; set; }

    [Column("İş Türü", Order = 40, Width = 110)] public string CategoryName => EnumDisplay.GetDisplayName(Category);
    [Column("Temin Şekli", Order = 45, Width = 180)] public string FlowName => EnumDisplay.GetDisplayName(Flow);
    [Column("Aşama", Order = 50, Width = 130)] public string StageName => EnumDisplay.GetDisplayName(Stage);
}

public static class DirectProcurementFileExtensions
{
    /// <summary>
    /// Liste projeksiyonu. Alt kayıt sayıları koleksiyon üzerinden
    /// <c>Count</c> ile okunur; EF Core bunu ilgili satırla sınırlı bir alt
    /// sorguya çevirir. Koleksiyonları ana sorguya <c>SelectMany</c> ile
    /// katmak kartesian çarpım üretir ve liste ekranını üstel olarak şişirir
    /// (bkz. <c>InvoiceExtensions.MapTo</c> notu).
    /// </summary>
    public static IQueryable<DirectProcurementFileDto> MapTo(this IQueryable<DirectProcurementFile> entity)
    {
        return entity
            .Select(f => new DirectProcurementFileDto
            {
                Id = f.Id,
                FileNumber = f.FileNumber.Value,
                WorkName = f.WorkName.Value,
                WorkingYear = f.WorkingYear,
                Category = f.Category,
                Flow = f.Flow,
                Stage = f.Stage,
                CompanyId = f.CompanyId == null ? null : f.CompanyId.Value,
                SupplierId = f.SupplierId == null ? null : f.SupplierId.Value,
                ChartOfAccountId = f.ChartOfAccountId == null ? null : f.ChartOfAccountId.Value,
                FileType = f.FileType,
                SupplierName = f.Supplier == null ? null : f.Supplier.Name.Value,
                ApproximateCost = f.ApproximateCost,
                Appropriation = f.Appropriation,
                Date = f.Date,
                ProjectNumber = f.ProjectNumber == null ? null : f.ProjectNumber.Value,
                DecisionNumber = f.DecisionNumber == null ? null : f.DecisionNumber.Value,
                DecisionDate = f.DecisionDate,
                PurchasingUnit = f.PurchasingUnit,
                RelatedPerson = f.RelatedPerson,
                WorkNature = f.WorkNature == null ? null : f.WorkNature.Value,
                WorkQuantity = f.WorkQuantity == null ? null : f.WorkQuantity.Value,
                HasContract = f.HasContract,
                HasSpecification = f.HasSpecification,
                HasCommission = f.HasCommission,
                CostEstimationMethod = f.CostEstimationMethod,
                HasApproximateCost = f.HasApproximateCost,
                UseHistoryPurchases = f.UseHistoryPurchases,
                RequireFinalOffer = f.RequireFinalOffer,
                DecimalPlaces = f.DecimalPlaces,
                UnitPriceDecimalPlaces = f.UnitPriceDecimalPlaces,
                ItemCount = f.Items.Count,
                QuoteCount = f.QuotedFirms.Count,
                IsArchived = f.IsArchived,

                CreatedAt = f.CreatedAt,
                CreatedBy = f.CreatedBy,
                IsActive = f.IsActive,
                UpdatedAt = f.UpdatedAt,
                UpdatedBy = f.UpdatedBy == null ? null : f.UpdatedBy.Value
            });
    }
}