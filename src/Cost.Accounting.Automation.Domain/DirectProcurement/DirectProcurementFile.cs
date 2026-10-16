using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.DirectProcurement.ValueObjects;
using Cost.Accounting.Automation.Domain.Suppliers;

namespace Cost.Accounting.Automation.Domain.DirectProcurement;

/// <summary>
/// Doğrudan temin dosyası — modülün kök aggregate'idir (kaynak sistemdeki
/// <c>IHALE</c> tablosu).
/// </summary>
/// <remarks>
/// <para>
/// Kaynak tabloda 70 sütun tek bir "ihale kaydı"nda toplanmıştır. Burada
/// mantıksal olarak birbirinden bağımsız üç grup ayrılmıştır:
/// </para>
/// <list type="bullet">
/// <item><b>Kimlik ve sınıflandırma</b>: dosya no, iş adı, yıl, iş türü, ihale tipi.</item>
/// <item><b>Tutar ve bütçe</b>: yaklaşık maliyet, ödenek, bütçe tertibi seçimleri.</item>
/// <item><b>Süreç bayrakları</b>: sözleşme, şartname, komisyon, piyasa araştırması gibi
/// "bu dosyada şu evrak var mı" bilgileri. Kaynak sistem bunları ayrı sütunlar
/// hâlinde tutuyordu; ölçeklenebilir olmadığı için bayrak alanlarına dönüştürüldü.</item>
/// </list>
/// <para>
/// Firma (<see cref="SupplierId"/>) ve bütçe hesabı (<see cref="ChartOfAccountId"/>)
/// yıl veritabanındaki kataloglardan referans alır; böylece aynı firma iki
/// modülde iki farklı kayıt olarak görünmez.
/// </para>
/// </remarks>
public sealed class DirectProcurementFile : Entity, IHardDeletable
{
    private readonly List<DirectProcurementItem> _items = [];
    private readonly List<ProcurementHistoryPurchase> _historyPurchases = [];
    private readonly List<ProcurementQuotedFirm> _quotedFirms = [];
    private readonly List<ProcurementApprovedFirm> _approvedFirms = [];
    private readonly List<ProcurementAwardedFirm> _awardedFirms = [];
    private readonly List<ProcurementTeamMember> _teamMembers = [];
    private readonly List<DirectProcurementBudgetSelection> _budgetSelections = [];

    private DirectProcurementFile()
    {
    }

    public DirectProcurementFile(
        FileNumber fileNumber,
        WorkName workName,
        ProcurementCategoryType category,
        ProcurementFileType fileType,
        int workingYear,
        DateOnly? date,
        ProcurementFlowType flow,
        IdentityId? companyId,
        IdentityId? supplierId,
        IdentityId? chartOfAccountId,
        bool isActive = true)
    {
        SetFileNumber(fileNumber);
        SetWorkName(workName);
        SetCategory(category);
        SetFileType(fileType);
        SetWorkingYear(workingYear);
        SetDate(date);
        SetFlow(flow);
        SetCompany(companyId);
        SetSupplier(supplierId);
        SetChartOfAccount(chartOfAccountId);
        SetStatus(isActive);
        ResolveDuplicateKey();
        RefreshStage();
    }

    // ---------- Kimlik ----------

    public FileNumber FileNumber { get; private set; } = default!;
    public WorkName WorkName { get; private set; } = default!;
    public ProcurementCategoryType Category { get; private set; }
    public ProcurementFileType FileType { get; private set; }
    public int WorkingYear { get; private set; }
    public DateOnly? Date { get; private set; }
    public ProcurementFlowType Flow { get; private set; }

    // ---------- Bağlantılar ----------

    /// <summary>
    /// Dosyayı yürüten birim / idare (kaynak: <c>IHALE.BIRIMID</c>).
    /// </summary>
    /// <remarks>
    /// <see cref="Companies.Company"/> merkezî (Master) veritabanında tutulduğu
    /// için bu alan yabancı anahtar <b>kısıtı</b> değildir; yalnızca bir kimlik
    /// sütunudur. Adı çözümlemek için
    /// <c>Application.Services.IMasterCompanyNameLookup</c> kullanılır. Bu ayrım
    /// bilinçlidir: kayıt başka bir veritabanında olduğu için EF gezinti özelliği
    /// tanımlanamaz.
    /// </remarks>
    public IdentityId? CompanyId { get; private set; }

    /// <summary>Dosyada belirlenen firma (kaynak: <c>IHALE.FIRMAID</c>).</summary>
    public IdentityId? SupplierId { get; private set; }

    public Supplier? Supplier { get; private set; }

    /// <summary>
    /// Bütçe tertibi yapılacak muhasebe hesabı (kaynak: <c>IHALE.HESAPID</c>).
    /// </summary>
    public IdentityId? ChartOfAccountId { get; private set; }

    public ChartOfAccount? ChartOfAccount { get; private set; }

    // ---------- Tutar ----------

    public decimal? ApproximateCost { get; private set; }
    public decimal? Appropriation { get; private set; }
    public decimal? TenderPrice { get; private set; }
    public CostEstimationMethodType CostEstimationMethod { get; private set; } = CostEstimationMethodType.CommonUnitPrice;

    /// <summary>
    /// Tutar sütunlarında gösterilecek ondalık basamak sayısı
    /// (kaynak: <c>IHALE.ONDALIK</c>).
    /// </summary>
    public int DecimalPlaces { get; private set; } = 2;

    /// <summary>
    /// Birim fiyat sütunlarında gösterilecek ondalık basamak sayısı
    /// (kaynak: <c>IHALE.BFONDALIK</c>).
    /// </summary>
    public int UnitPriceDecimalPlaces { get; private set; } = 2;

    /// <summary>
    /// Yaklaşık maliyet bölümünün dosyada açılıp açılmayacağı
    /// (kaynak: <c>IHALE.YMVARMI</c>).
    /// </summary>
    public bool HasApproximateCost { get; private set; }

    /// <summary>Yaklaşık maliyette geçmiş alımların kullanılıp kullanılmayacağı.</summary>
    public bool UseHistoryPurchases { get; private set; }

    /// <summary>Temin aşamasında firmadan son teklif istenecek mi.</summary>
    public bool RequireFinalOffer { get; private set; }

    // ---------- İçerik ----------

    public ProjectNumber? ProjectNumber { get; private set; }
    public DecisionNumber? DecisionNumber { get; private set; }
    public DateOnly? DecisionDate { get; private set; }
    public WorkNature? WorkNature { get; private set; }
    public WorkQuantity? WorkQuantity { get; private set; }
    public string? DecisionNote { get; private set; }
    public string? OtherExplanations { get; private set; }
    public string? SingleSourceExplanation { get; private set; }
    public string? SingleSourceExplanation2 { get; private set; }
    public string? SingleSourceOtherExplanation { get; private set; }
    public string? AdvanceExplanation { get; private set; }
    public string? PriceDifferenceExplanation { get; private set; }
    public string? PurchasingUnit { get; private set; }
    public string? RelatedPerson { get; private set; }
    public DateOnly? DeliveryDate { get; private set; }
    public DateOnly? DemandDate { get; private set; }
    public DateOnly? OrderDate { get; private set; }
    public DateOnly? AcceptanceDate { get; private set; }
    public string? ApprovalOffice { get; private set; }
    public string? ApprovalNumber { get; private set; }
    public DateOnly? ApprovalDate { get; private set; }
    public string? ApprovingPersonnel { get; private set; }
    public string? ApprovingPersonnelTitle { get; private set; }
    public DateOnly? ApprovingPersonnelDate { get; private set; }
    public string? ConformityPersonnel { get; private set; }
    public string? ConformityPersonnelTitle { get; private set; }
    public DateOnly? ConformityPersonnelDate { get; private set; }

    // ---------- Süreç bayrakları ----------

    public bool HasContract { get; private set; }
    public bool HasSpecification { get; private set; }
    public bool HasCommission { get; private set; }
    public bool HasAwardDocumentation { get; private set; }
    public bool HasCostEstimateTeam { get; private set; }
    public bool HasMarketSurveyTeam { get; private set; }
    public bool UseRequirementList { get; private set; }
    public bool RequireApprovedOffer2 { get; private set; }
    public bool IsBudgetApproved { get; private set; }
    public bool IsArchived { get; private set; }
    public bool IsDemo { get; private set; }

    // ---------- Alt kayıtlar ----------

    public IReadOnlyCollection<DirectProcurementItem> Items => _items;
    public IReadOnlyCollection<ProcurementHistoryPurchase> HistoryPurchases => _historyPurchases;
    public IReadOnlyCollection<ProcurementQuotedFirm> QuotedFirms => _quotedFirms;
    public IReadOnlyCollection<ProcurementApprovedFirm> ApprovedFirms => _approvedFirms;
    public IReadOnlyCollection<ProcurementAwardedFirm> AwardedFirms => _awardedFirms;
    public IReadOnlyCollection<ProcurementTeamMember> TeamMembers => _teamMembers;
    public IReadOnlyCollection<DirectProcurementBudgetSelection> BudgetSelections => _budgetSelections;

    /// <summary>
    /// Dosyanın süreçteki konumu. Kaynak sistemde tutulmayan, alt kayıtların
    /// varlığından türetilen değerdir.
    /// </summary>
    /// <remarks>
    /// Liste ekranında "sözleşme aşamasındakiler", "teklif bekleyenler" gibi
    /// süzme ve renklendirme yapılabilmesi için <b>saklanan</b> bir sütundur;
    /// aksi hâlde EF Core bu özelliği SQL'e çeviremez ve liste sorgusu
    /// çalışmaz. Değer, alt kayıt koleksiyonları ya da ilgili bayraklar
    /// değiştiğinde yeniden hesaplanır.
    /// </remarks>
    public DirectProcurementStage Stage { get; private set; }

    private DirectProcurementStage CalculateStage()
    {
        if (_awardedFirms.Count > 0)
        {
            return DirectProcurementStage.Ordered;
        }

        if (HasContract)
        {
            return DirectProcurementStage.Contract;
        }

        if (HasAwardDocumentation)
        {
            return DirectProcurementStage.Approved;
        }

        if (HasApproximateCost)
        {
            return DirectProcurementStage.CostEstimated;
        }

        if (_quotedFirms.Count > 0)
        {
            return DirectProcurementStage.OffersReceived;
        }

        return _items.Count > 0
            ? DirectProcurementStage.RequirementList
            : DirectProcurementStage.Draft;
    }

    private void RefreshStage() => Stage = CalculateStage();

    // ---------- Eşzamanlı düzenleme anahtarı ----------

    public static string? BuildDuplicateKey(string fileNumber, int workingYear)
        => DuplicateKeyRule.From(fileNumber, workingYear);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(FileNumber.Value, WorkingYear));

    // ---------- Davranışlar ----------

    public void SetFileNumber(FileNumber fileNumber)
    {
        FileNumber = fileNumber;
        ResolveDuplicateKey();
    }

    public void SetWorkName(WorkName workName)
    {
        WorkName = workName;
        ResolveDuplicateKey();
    }

    public void SetCategory(ProcurementCategoryType category) => Category = category;
    public void SetFileType(ProcurementFileType fileType) => FileType = fileType;

    public void SetWorkingYear(int workingYear)
    {
        WorkingYear = workingYear;
        ResolveDuplicateKey();
    }

    public void SetDate(DateOnly? date) => Date = date;
    public void SetFlow(ProcurementFlowType flow) => Flow = flow;
    public void SetCompany(IdentityId? companyId) => CompanyId = companyId;
    public void SetSupplier(IdentityId? supplierId) => SupplierId = supplierId;
    public void SetChartOfAccount(IdentityId? chartOfAccountId) => ChartOfAccountId = chartOfAccountId;
    public void SetApproximateCost(decimal? approximateCost) => ApproximateCost = approximateCost;
    public void SetAppropriation(decimal? appropriation) => Appropriation = appropriation;
    public void SetTenderPrice(decimal? tenderPrice) => TenderPrice = tenderPrice;
    public void SetCostEstimationMethod(CostEstimationMethodType method) => CostEstimationMethod = method;
    public void SetProjectNumber(ProjectNumber? projectNumber) => ProjectNumber = projectNumber;
    public void SetDecisionNumber(DecisionNumber? decisionNumber) => DecisionNumber = decisionNumber;
    public void SetDecisionDate(DateOnly? decisionDate) => DecisionDate = decisionDate;
    public void SetWorkNature(WorkNature? workNature) => WorkNature = workNature;
    public void SetWorkQuantity(WorkQuantity? workQuantity) => WorkQuantity = workQuantity;
    public void SetDecisionNote(string? note) => DecisionNote = note;
    public void SetOtherExplanations(string? value) => OtherExplanations = value;
    public void SetSingleSourceExplanation(string? value) => SingleSourceExplanation = value;
    public void SetSingleSourceExplanation2(string? value) => SingleSourceExplanation2 = value;
    public void SetSingleSourceOtherExplanation(string? value) => SingleSourceOtherExplanation = value;
    public void SetAdvanceExplanation(string? value) => AdvanceExplanation = value;
    public void SetPriceDifferenceExplanation(string? value) => PriceDifferenceExplanation = value;
    public void SetPurchasingUnit(string? value) => PurchasingUnit = value;
    public void SetRelatedPerson(string? value) => RelatedPerson = value;
    public void SetDeliveryDate(DateOnly? value) => DeliveryDate = value;
    public void SetDemandDate(DateOnly? value) => DemandDate = value;
    public void SetOrderDate(DateOnly? value) => OrderDate = value;
    public void SetAcceptanceDate(DateOnly? value) => AcceptanceDate = value;
    public void SetApprovalOffice(string? value) => ApprovalOffice = value;
    public void SetApprovalNumber(string? value) => ApprovalNumber = value;
    public void SetApprovalDate(DateOnly? value) => ApprovalDate = value;
    public void SetApprovingPersonnel(string? value) => ApprovingPersonnel = value;
    public void SetApprovingPersonnelTitle(string? value) => ApprovingPersonnelTitle = value;
    public void SetApprovingPersonnelDate(DateOnly? value) => ApprovingPersonnelDate = value;
    public void SetConformityPersonnel(string? value) => ConformityPersonnel = value;
    public void SetConformityPersonnelTitle(string? value) => ConformityPersonnelTitle = value;
    public void SetConformityPersonnelDate(DateOnly? value) => ConformityPersonnelDate = value;

    public void SetHasSpecification(bool value) => HasSpecification = value;
    public void SetHasCommission(bool value) => HasCommission = value;
    public void SetHasCostEstimateTeam(bool value) => HasCostEstimateTeam = value;
    public void SetHasMarketSurveyTeam(bool value) => HasMarketSurveyTeam = value;
    public void SetUseHistoryPurchases(bool value) => UseHistoryPurchases = value;
    public void SetUseRequirementList(bool value) => UseRequirementList = value;
    public void SetRequireApprovedOffer2(bool value) => RequireApprovedOffer2 = value;
    public void SetIsBudgetApproved(bool value) => IsBudgetApproved = value;
    public void SetIsDemo(bool value) => IsDemo = value;

    // Aşamayı etkileyen üç bayrak ayrı yazıldı: hepsinde yeniden hesaplama
    // çağrısı unutulması kolaydır ve unutulduğunda liste ekranı yanlış aşama
    // gösterir.
    public void SetHasContract(bool value)
    {
        HasContract = value;
        RefreshStage();
    }

    public void SetHasAwardDocumentation(bool value)
    {
        HasAwardDocumentation = value;
        RefreshStage();
    }

    public void SetHasApproximateCost(bool value)
    {
        HasApproximateCost = value;
        RefreshStage();
    }

    public void SetDecimalPlaces(int decimalPlaces) => DecimalPlaces = decimalPlaces;
    public void SetUnitPriceDecimalPlaces(int value) => UnitPriceDecimalPlaces = value;
    public void SetRequireFinalOffer(bool value) => RequireFinalOffer = value;

    public void SetIsArchived(bool value) => IsArchived = value;

    // ---------- Alt kayıt yönetimi ----------

    public void ReplaceItems(IEnumerable<DirectProcurementItem> items)
    {
        _items.Clear();
        foreach (DirectProcurementItem item in items)
        {
            item.SetFile(Id);
            _items.Add(item);
        }

        RefreshStage();
    }

    public void AddItem(DirectProcurementItem item)
    {
        item.SetFile(Id);
        item.SetLineNo(_items.Count + 1);
        _items.Add(item);
        RefreshStage();
    }

    public void ReplaceHistoryPurchases(IEnumerable<ProcurementHistoryPurchase> purchases)
    {
        _historyPurchases.Clear();
        foreach (ProcurementHistoryPurchase purchase in purchases)
        {
            purchase.SetFile(Id);
            _historyPurchases.Add(purchase);
        }
    }

    public void ReplaceQuotedFirms(IEnumerable<ProcurementQuotedFirm> firms)
    {
        _quotedFirms.Clear();
        foreach (ProcurementQuotedFirm firm in firms)
        {
            firm.SetFile(Id);
            _quotedFirms.Add(firm);
        }

        RefreshStage();
    }

    public void ReplaceApprovedFirms(IEnumerable<ProcurementApprovedFirm> firms)
    {
        _approvedFirms.Clear();
        foreach (ProcurementApprovedFirm firm in firms)
        {
            firm.SetFile(Id);
            _approvedFirms.Add(firm);
        }
    }

    public void ReplaceAwardedFirms(IEnumerable<ProcurementAwardedFirm> firms)
    {
        _awardedFirms.Clear();
        foreach (ProcurementAwardedFirm firm in firms)
        {
            firm.SetFile(Id);
            _awardedFirms.Add(firm);
        }

        RefreshStage();
    }

    public void ReplaceTeamMembers(IEnumerable<ProcurementTeamMember> members)
    {
        _teamMembers.Clear();
        foreach (ProcurementTeamMember member in members)
        {
            member.SetFile(Id);
            _teamMembers.Add(member);
        }
    }

    public void ReplaceBudgetSelections(IEnumerable<DirectProcurementBudgetSelection> selections)
    {
        _budgetSelections.Clear();
        foreach (DirectProcurementBudgetSelection selection in selections)
        {
            selection.SetFile(Id);
            _budgetSelections.Add(selection);
        }
    }

    /// <summary>
    /// Dosyayı arşivler. Arşivlenen dosya listede kalır ancak düzenlenemez.
    /// </summary>
    public void Archive() => SetIsArchived(true);

    public bool IsEditable => !IsArchived;
}