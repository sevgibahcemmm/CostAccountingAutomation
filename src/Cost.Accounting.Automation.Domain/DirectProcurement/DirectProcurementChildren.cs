using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.DirectProcurement;

/// <summary>
/// Doğrudan temin dosyasının ihtiyaç listesi satırı (kaynak: <c>IHMALZEME</c>).
/// </summary>
/// <remarks>
/// Kaynak sistemde satırın malzemesi yalnızca serbest metin olarak tutuluyordu
/// (<c>KODU</c>/<c>TANIMI</c>). <see cref="ProductId"/> alanı, malzeme kataloğu
/// ile ilişki kurulabilmesi için eklenmiştir; ilişki kurulmamış satırlarda
/// serbest metin geçerli kalır.
/// </remarks>
public sealed class DirectProcurementItem : Entity, IHardDeletable
{
    private DirectProcurementItem()
    {
    }

    public DirectProcurementItem(
        int lineNo,
        string code,
        string definition,
        string description,
        string unit,
        decimal? quantity,
        int? taxRatePercent,
        decimal? approximateCost,
        IdentityId? productId,
        IdentityId? materialGroupId,
        IdentityId? bookId)
    {
        SetLineNo(lineNo);
        SetCode(code);
        SetDefinition(definition);
        SetDescription(description);
        SetUnit(unit);
        SetQuantity(quantity);
        SetTaxRate(taxRatePercent);
        SetApproximateCost(approximateCost);
        SetProduct(productId);
        SetMaterialGroup(materialGroupId);
        SetBook(bookId);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public int LineNo { get; private set; }
    public string Code { get; private set; } = default!;
    public string Definition { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public decimal? Quantity { get; private set; }
    public int? TaxRatePercent { get; private set; }
    public decimal? ApproximateCost { get; private set; }

    /// <summary>Bağlı malzeme kataloğu kaydı (kaynak: <c>IHMALZEME.MALZEMEID</c>).</summary>
    public IdentityId? ProductId { get; private set; }

    /// <summary>Malzeme grubu (kaynak: <c>IHMALZEME.MGRUPID</c>).</summary>
    public IdentityId? MaterialGroupId { get; private set; }

    /// <summary>Malzeme kitabı (kaynak: <c>IHMALZEME.KITAPID</c>).</summary>
    public IdentityId? BookId { get; private set; }

    public DirectProcurementFile? File { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetLineNo(int lineNo) => LineNo = lineNo;
    public void SetCode(string code) => Code = code;
    public void SetDefinition(string definition) => Definition = definition;
    public void SetDescription(string description) => Description = description;
    public void SetUnit(string unit) => Unit = unit;
    public void SetQuantity(decimal? quantity) => Quantity = quantity;
    public void SetTaxRate(int? taxRatePercent) => TaxRatePercent = taxRatePercent;
    public void SetApproximateCost(decimal? approximateCost) => ApproximateCost = approximateCost;
    public void SetProduct(IdentityId? productId) => ProductId = productId;
    public void SetMaterialGroup(IdentityId? materialGroupId) => MaterialGroupId = materialGroupId;
    public void SetBook(IdentityId? bookId) => BookId = bookId;
}

/// <summary>
/// Geçmiş alım satırı (kaynak: <c>IHGECMISALIMLAR</c>). Yaklaşık maliyet
/// hesabında referans fiyat olarak kullanılır.
/// </summary>
public sealed class ProcurementHistoryPurchase : Entity, IHardDeletable
{
    private ProcurementHistoryPurchase()
    {
    }

    public ProcurementHistoryPurchase(
        string code,
        string definition,
        string description,
        string unit,
        decimal? quantity,
        int? taxRatePercent,
        decimal? price,
        decimal? currentPrice,
        DateOnly? date,
        IdentityId? productId,
        DateOnly? indexDate,
        decimal? currentIndex,
        decimal? coefficient,
        decimal? baseIndex,
        string? supplierName)
    {
        SetCode(code);
        SetDefinition(definition);
        SetDescription(description);
        SetUnit(unit);
        SetQuantity(quantity);
        SetTaxRate(taxRatePercent);
        SetPrice(price);
        SetCurrentPrice(currentPrice);
        SetDate(date);
        SetProduct(productId);
        SetIndexDate(indexDate);
        SetCurrentIndex(currentIndex);
        SetCoefficient(coefficient);
        SetBaseIndex(baseIndex);
        SetSupplierName(supplierName);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public IdentityId DirectProcurementItemId { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string Definition { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public decimal? Quantity { get; private set; }
    public int? TaxRatePercent { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? CurrentPrice { get; private set; }
    public DateOnly? Date { get; private set; }
    public IdentityId? ProductId { get; private set; }
    public DateOnly? IndexDate { get; private set; }
    public decimal? CurrentIndex { get; private set; }
    public decimal? Coefficient { get; private set; }
    public decimal? BaseIndex { get; private set; }

    /// <summary>
    /// Geçmiş alımın yapıldığı firma adı. Firma kaydı silinmiş olabileceğinden
    /// ilişki yerine ad metni tutulur; kaynak sistemde de aynı şekilde
    /// (<c>IHGECMISALIMLAR.FIRMA</c>) saklanmaktadır.
    /// </summary>
    public string? SupplierName { get; private set; }

    public DirectProcurementFile? File { get; private set; }
    public DirectProcurementItem? Item { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetItem(IdentityId itemId) => DirectProcurementItemId = itemId;
    public void SetCode(string code) => Code = code;
    public void SetDefinition(string definition) => Definition = definition;
    public void SetDescription(string description) => Description = description;
    public void SetUnit(string unit) => Unit = unit;
    public void SetQuantity(decimal? quantity) => Quantity = quantity;
    public void SetTaxRate(int? taxRatePercent) => TaxRatePercent = taxRatePercent;
    public void SetPrice(decimal? price) => Price = price;
    public void SetCurrentPrice(decimal? currentPrice) => CurrentPrice = currentPrice;
    public void SetDate(DateOnly? date) => Date = date;
    public void SetProduct(IdentityId? productId) => ProductId = productId;
    public void SetIndexDate(DateOnly? indexDate) => IndexDate = indexDate;
    public void SetCurrentIndex(decimal? currentIndex) => CurrentIndex = currentIndex;
    public void SetCoefficient(decimal? coefficient) => Coefficient = coefficient;
    public void SetBaseIndex(decimal? baseIndex) => BaseIndex = baseIndex;
    public void SetSupplierName(string? supplierName) => SupplierName = supplierName;
}

/// <summary>
/// Teklif veren firma (kaynak: <c>IHTEKLIFFIRMA</c>). Firma ihtiyaç satırı
/// bazında teklif verdiği için satırın hem dosya hem firma bağlantısı vardır.
/// </summary>
public sealed class ProcurementQuotedFirm : Entity, IHardDeletable
{
    private ProcurementQuotedFirm()
    {
    }

    public ProcurementQuotedFirm(
        IdentityId? supplierId,
        IdentityId itemId,
        decimal? price,
        decimal? finalPrice,
        bool isSelected,
        bool isEligible,
        DateOnly? letterDate,
        decimal? totalOffer)
    {
        SetSupplier(supplierId);
        SetItem(itemId);
        SetPrice(price);
        SetFinalPrice(finalPrice);
        SetSelected(isSelected);
        SetEligible(isEligible);
        SetLetterDate(letterDate);
        SetTotalOffer(totalOffer);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public IdentityId DirectProcurementItemId { get; private set; } = default!;

    /// <summary>Teklifi veren tedarikçi (kaynak: <c>IHTEKLIFFIRMA.FIRMAID</c>).</summary>
    public IdentityId? SupplierId { get; private set; }

    public decimal? Price { get; private set; }
    public decimal? FinalPrice { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsEligible { get; private set; }
    public DateOnly? LetterDate { get; private set; }
    public decimal? TotalOffer { get; private set; }

    public DirectProcurementFile? File { get; private set; }
    public DirectProcurementItem? Item { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetItem(IdentityId itemId) => DirectProcurementItemId = itemId;
    public void SetSupplier(IdentityId? supplierId) => SupplierId = supplierId;
    public void SetPrice(decimal? price) => Price = price;
    public void SetFinalPrice(decimal? finalPrice) => FinalPrice = finalPrice;
    public void SetSelected(bool isSelected) => IsSelected = isSelected;
    public void SetEligible(bool isEligible) => IsEligible = isEligible;
    public void SetLetterDate(DateOnly? letterDate) => LetterDate = letterDate;
    public void SetTotalOffer(decimal? totalOffer) => TotalOffer = totalOffer;
}

/// <summary>
/// Uygun görülen / onaylanan firma (kaynak: <c>IHONAYFIRMA</c>). Teklif
/// aşamasından sonra, yaklaşık maliyet ve onay belgesi ekranlarında kullanılır.
/// </summary>
public sealed class ProcurementApprovedFirm : Entity, IHardDeletable
{
    private ProcurementApprovedFirm()
    {
    }

    public ProcurementApprovedFirm(
        IdentityId? supplierId,
        IdentityId itemId,
        decimal? price,
        bool isSelected,
        bool isEligible,
        DateOnly? letterDate)
    {
        SetSupplier(supplierId);
        SetItem(itemId);
        SetPrice(price);
        SetSelected(isSelected);
        SetEligible(isEligible);
        SetLetterDate(letterDate);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public IdentityId DirectProcurementItemId { get; private set; } = default!;
    public IdentityId? SupplierId { get; private set; }
    public decimal? Price { get; private set; }
    public bool IsSelected { get; private set; }
    public bool IsEligible { get; private set; }
    public DateOnly? LetterDate { get; private set; }

    public DirectProcurementFile? File { get; private set; }
    public DirectProcurementItem? Item { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetItem(IdentityId itemId) => DirectProcurementItemId = itemId;
    public void SetSupplier(IdentityId? supplierId) => SupplierId = supplierId;
    public void SetPrice(decimal? price) => Price = price;
    public void SetSelected(bool isSelected) => IsSelected = isSelected;
    public void SetEligible(bool isEligible) => IsEligible = isEligible;
    public void SetLetterDate(DateOnly? letterDate) => LetterDate = letterDate;
}

/// <summary>
/// İş kalemi bazında hesap satırı (kaynak: <c>IHUYGUNFIRMADETAY</c>). Sözleşme
/// ve ödeme aşamasında hangi muhasebe hesabına ne kadar borç/alacak yazılacağını
/// tutar.
/// </summary>
public sealed class ProcurementAwardedFirmAccount : Entity, IHardDeletable
{
    private ProcurementAwardedFirmAccount()
    {
    }

    public ProcurementAwardedFirmAccount(
        string accountName,
        string? accountNo,
        decimal? debit,
        decimal? credit,
        bool isPaid)
    {
        SetAccountName(accountName);
        SetAccountNo(accountNo);
        SetDebit(debit);
        SetCredit(credit);
        SetPaid(isPaid);
    }

    public IdentityId ProcurementAwardedFirmId { get; private set; } = default!;
    public string AccountName { get; private set; } = default!;
    public string? AccountNo { get; private set; }
    public decimal? Debit { get; private set; }
    public decimal? Credit { get; private set; }

    /// <summary>Ödendi mi bilgisi. Kaynak sistemde <c>bit</c> olarak tutuluyordu.</summary>
    public bool IsPaid { get; private set; }

    public ProcurementAwardedFirm? AwardedFirm { get; private set; }

    public void SetAwardedFirm(IdentityId awardedFirmId) => ProcurementAwardedFirmId = awardedFirmId;
    public void SetAccountName(string accountName) => AccountName = accountName;
    public void SetAccountNo(string? accountNo) => AccountNo = accountNo;
    public void SetDebit(decimal? debit) => Debit = debit;
    public void SetCredit(decimal? credit) => Credit = credit;
    public void SetPaid(bool isPaid) => IsPaid = isPaid;
}

/// <summary>
/// Kazanılan firma ve sözleşme/ödeme bilgileri (kaynak: <c>IHUYGUNFIRMA</c>).
/// </summary>
/// <remarks>
/// Kaynak sistemde bu tablo sipariş, sözleşme, fatura, teslim, kabul ve
/// ödeme evraklarının tek satırında toplanmıştır. Burada da tek aggregate
/// olarak korunur; bu sayede sözleşme ve sipariş ekranları aynı kaydı okur.
/// </remarks>
public sealed class ProcurementAwardedFirm : Entity, IHardDeletable
{
    private readonly List<ProcurementAwardedFirmAccount> _accounts = [];

    private ProcurementAwardedFirm()
    {
    }

    public ProcurementAwardedFirm(
        IdentityId? supplierId,
        decimal? amount,
        DateOnly? contractDate,
        DateOnly? invoiceDate,
        string? invoiceNo,
        DateOnly? deliveryDate,
        DateOnly? acceptanceDate,
        decimal? vatAmount,
        DateOnly? paymentDate,
        DateOnly? paymentOrderDate,
        string? paymentOrderNo,
        string? paymentOrderNo2,
        string? chequeNo,
        string? note,
        string? memorandum,
        string? attachment,
        decimal? budgetExpense)
    {
        SetSupplier(supplierId);
        SetAmount(amount);
        SetContractDate(contractDate);
        SetInvoiceDate(invoiceDate);
        SetInvoiceNo(invoiceNo);
        SetDeliveryDate(deliveryDate);
        SetAcceptanceDate(acceptanceDate);
        SetVatAmount(vatAmount);
        SetPaymentDate(paymentDate);
        SetPaymentOrderDate(paymentOrderDate);
        SetPaymentOrderNo(paymentOrderNo);
        SetPaymentOrderNo2(paymentOrderNo2);
        SetChequeNo(chequeNo);
        SetNote(note);
        SetMemorandum(memorandum);
        SetAttachment(attachment);
        SetBudgetExpense(budgetExpense);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public IdentityId? SupplierId { get; private set; }
    public decimal? Amount { get; private set; }
    public DateOnly? ContractDate { get; private set; }
    public DateOnly? InvoiceDate { get; private set; }
    public string? InvoiceNo { get; private set; }
    public DateOnly? DeliveryDate { get; private set; }
    public DateOnly? AcceptanceDate { get; private set; }
    public decimal? VatAmount { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public DateOnly? PaymentOrderDate { get; private set; }
    public string? PaymentOrderNo { get; private set; }
    public string? PaymentOrderNo2 { get; private set; }
    public string? ChequeNo { get; private set; }
    public string? Note { get; private set; }
    public string? Memorandum { get; private set; }
    public string? Attachment { get; private set; }
    public decimal? BudgetExpense { get; private set; }

    /// <summary>Özel gider indirimi (kaynak: <c>OEOZELGIDERINDIRIMI</c>).</summary>
    public decimal? SpecialExpenseReduction { get; private set; }

    /// <summary>Ödenecek tutar (kaynak: <c>OEODENECEKTUTAR</c>).</summary>
    public decimal? PayableAmount { get; private set; }

    /// <summary>Kesinti toplamı (kaynak: <c>OEKESINTITOPLAMI</c>).</summary>
    public decimal? DeductionTotal { get; private set; }

    /// <summary>Pazarlık notu (kaynak: <c>PAZARLIKNOT</c>).</summary>
    public string? NegotiationNote { get; private set; }

    public DirectProcurementFile? File { get; private set; }
    public IReadOnlyCollection<ProcurementAwardedFirmAccount> Accounts => _accounts;

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetSupplier(IdentityId? supplierId) => SupplierId = supplierId;
    public void SetAmount(decimal? amount) => Amount = amount;
    public void SetContractDate(DateOnly? contractDate) => ContractDate = contractDate;
    public void SetInvoiceDate(DateOnly? invoiceDate) => InvoiceDate = invoiceDate;
    public void SetInvoiceNo(string? invoiceNo) => InvoiceNo = invoiceNo;
    public void SetDeliveryDate(DateOnly? deliveryDate) => DeliveryDate = deliveryDate;
    public void SetAcceptanceDate(DateOnly? acceptanceDate) => AcceptanceDate = acceptanceDate;
    public void SetVatAmount(decimal? vatAmount) => VatAmount = vatAmount;
    public void SetPaymentDate(DateOnly? paymentDate) => PaymentDate = paymentDate;
    public void SetPaymentOrderDate(DateOnly? paymentOrderDate) => PaymentOrderDate = paymentOrderDate;
    public void SetPaymentOrderNo(string? paymentOrderNo) => PaymentOrderNo = paymentOrderNo;
    public void SetPaymentOrderNo2(string? paymentOrderNo2) => PaymentOrderNo2 = paymentOrderNo2;
    public void SetChequeNo(string? chequeNo) => ChequeNo = chequeNo;
    public void SetNote(string? note) => Note = note;
    public void SetMemorandum(string? memorandum) => Memorandum = memorandum;
    public void SetAttachment(string? attachment) => Attachment = attachment;
    public void SetBudgetExpense(decimal? budgetExpense) => BudgetExpense = budgetExpense;
    public void SetSpecialExpenseReduction(decimal? value) => SpecialExpenseReduction = value;
    public void SetPayableAmount(decimal? value) => PayableAmount = value;
    public void SetDeductionTotal(decimal? value) => DeductionTotal = value;
    public void SetNegotiationNote(string? value) => NegotiationNote = value;

    public void AddAccount(ProcurementAwardedFirmAccount account)
    {
        account.SetAwardedFirm(Id);
        _accounts.Add(account);
    }

    public void ReplaceAccounts(IEnumerable<ProcurementAwardedFirmAccount> accounts)
    {
        _accounts.Clear();
        foreach (ProcurementAwardedFirmAccount account in accounts)
        {
            account.SetAwardedFirm(Id);
            _accounts.Add(account);
        }
    }
}

/// <summary>
/// Dosyanın görevli havuzundaki bir kişi (kaynak: <c>IHYMGOREVLILERI</c>,
/// <c>IHPIYASAGOREVLILERI</c>, <c>IHKOMISYONGOREVLILERI</c>).
/// </summary>
/// <remarks>
/// Kaynak sistemde aynı "sıra numarası + personel + unvan" yapısı üç ayrı
/// tabloda tekrarlanmıştır. Tek aggregate'te birleştirildiğinden ayrım
/// <see cref="Role"/> alanı ile yapılır; böylece görevli listeleri tek sorguyla
/// ve rol süzgeciyle okunabilir.
/// </remarks>
public sealed class ProcurementTeamMember : Entity, IHardDeletable
{
    private ProcurementTeamMember()
    {
    }

    public ProcurementTeamMember(ProcurementTeamRole role, int sequence, string personnelName, string? title, DateOnly? date)
    {
        SetRole(role);
        SetSequence(sequence);
        SetPersonnelName(personnelName);
        SetTitle(title);
        SetDate(date);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;
    public ProcurementTeamRole Role { get; private set; }
    public int Sequence { get; private set; }
    public string PersonnelName { get; private set; } = default!;
    public string? Title { get; private set; }
    public DateOnly? Date { get; private set; }

    public DirectProcurementFile? File { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetRole(ProcurementTeamRole role) => Role = role;
    public void SetSequence(int sequence) => Sequence = sequence;
    public void SetPersonnelName(string personnelName) => PersonnelName = personnelName;
    public void SetTitle(string? title) => Title = title;
    public void SetDate(DateOnly? date) => Date = date;
}

/// <summary>
/// Dosyaya seçilmiş bütçe hesabı (kaynak: <c>IHBUTCE</c>). Bütçe tertibi
/// ekranından dosyaya eklenir ve yıl bazında tutar taşır.
/// </summary>
public sealed class DirectProcurementBudgetSelection : Entity, IHardDeletable
{
    private DirectProcurementBudgetSelection()
    {
    }

    public DirectProcurementBudgetSelection(
        IdentityId? chartOfAccountId,
        int year,
        BudgetVoucherType voucherType,
        decimal? allocatedAmount,
        decimal? spentAmount,
        bool isApproved)
    {
        SetChartOfAccount(chartOfAccountId);
        SetYear(year);
        SetVoucherType(voucherType);
        SetAllocatedAmount(allocatedAmount);
        SetSpentAmount(spentAmount);
        SetApproved(isApproved);
    }

    public IdentityId DirectProcurementFileId { get; private set; } = default!;

    /// <summary>Seçilen bütçe hesabı (kaynak: <c>IHBUTCE.HESAPID</c>).</summary>
    public IdentityId? ChartOfAccountId { get; private set; }

    public int Year { get; private set; }
    public BudgetVoucherType VoucherType { get; private set; }
    public decimal? AllocatedAmount { get; private set; }
    public decimal? SpentAmount { get; private set; }
    public bool IsApproved { get; private set; }

    public DirectProcurementFile? File { get; private set; }

    public void SetFile(IdentityId fileId) => DirectProcurementFileId = fileId;
    public void SetChartOfAccount(IdentityId? chartOfAccountId) => ChartOfAccountId = chartOfAccountId;
    public void SetYear(int year) => Year = year;
    public void SetVoucherType(BudgetVoucherType voucherType) => VoucherType = voucherType;
    public void SetAllocatedAmount(decimal? allocatedAmount) => AllocatedAmount = allocatedAmount;
    public void SetSpentAmount(decimal? spentAmount) => SpentAmount = spentAmount;
    public void SetApproved(bool isApproved) => IsApproved = isApproved;
}