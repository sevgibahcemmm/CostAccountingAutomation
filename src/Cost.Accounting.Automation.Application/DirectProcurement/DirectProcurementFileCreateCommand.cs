using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.Domain.DirectProcurement.ValueObjects;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>
/// Arayüzün doğrulama hatalarını alanlara bağlamak için kullanılan alan adları.
/// FluentValidation hata adı ile <c>[Column]</c> başlığı aynı olmadığı için tek
/// kaynak burada tanımlanır.
/// </summary>
public static class DirectProcurementCommandFields
{
    public const string FileNumber = nameof(FileNumber);
    public const string WorkName = nameof(WorkName);
    public const string WorkingYear = nameof(WorkingYear);
    public const string Date = nameof(Date);
    public const string ApproximateCost = nameof(ApproximateCost);
    public const string Appropriation = nameof(Appropriation);
    public const string CostEstimationMethod = nameof(CostEstimationMethod);
    public const string DecimalPlaces = nameof(DecimalPlaces);
    public const string UnitPriceDecimalPlaces = nameof(UnitPriceDecimalPlaces);
    public const string HasApproximateCost = nameof(HasApproximateCost);
    public const string UseHistoryPurchases = nameof(UseHistoryPurchases);
    public const string RequireFinalOffer = nameof(RequireFinalOffer);
}

/// <summary>
/// Doğrudan temin dosyası oluşturma komutu. Kaynak sistemdeki "Yeni Doğrudan
/// Temin Girişi" ekranının ana kaydını yaratır.
/// </summary>
[Permission("direct_procurement:create")]
public sealed record DirectProcurementFileCreateCommand(
    string FileNumber,
    string WorkName,
    ProcurementCategoryType Category,
    ProcurementFileType FileType,
    int WorkingYear,
    DateOnly? Date,
    ProcurementFlowType Flow,
    Guid? CompanyId,
    Guid? SupplierId,
    Guid? ChartOfAccountId,
    string? ProjectNumber,
    string? DecisionNumber,
    DateOnly? DecisionDate,
    decimal? ApproximateCost,
    decimal? Appropriation,
    string? WorkNature,
    string? WorkQuantity,
    string? PurchasingUnit,
    string? RelatedPerson,
    bool HasContract,
    bool HasSpecification,
    bool HasCommission,
    CostEstimationMethodType CostEstimationMethod,
    bool HasApproximateCost,
    bool UseHistoryPurchases,
    bool RequireFinalOffer,
    int DecimalPlaces,
    int UnitPriceDecimalPlaces,
    bool IsActive) : IRequest<Result<string>>;

public sealed class DirectProcurementFileCreateCommandValidator : AbstractValidator<DirectProcurementFileCreateCommand>
{
    public DirectProcurementFileCreateCommandValidator()
    {
        RuleFor(x => x.FileNumber)
            .NotEmpty().WithMessage("Geçerli bir dosya numarası girin")
            .MaximumLength(50).WithMessage("Dosya numarası en fazla 50 karakter olabilir");

        RuleFor(x => x.WorkName)
            .NotEmpty().WithMessage("Geçerli bir iş adı girin")
            .MaximumLength(500).WithMessage("İş adı en fazla 500 karakter olabilir");

        RuleFor(x => x.WorkingYear)
            .InclusiveBetween(1900, 2999).WithMessage("Çalışma yılı 1900 ile 2999 arasında olmalıdır");

        RuleFor(x => x.ApproximateCost)
            .GreaterThanOrEqualTo(0).WithMessage("Yaklaşık maliyet negatif olamaz");

        RuleFor(x => x.Appropriation)
            .GreaterThanOrEqualTo(0).WithMessage("Ödenek negatif olamaz");

        RuleFor(x => x.ProjectNumber)
            .MaximumLength(50).WithMessage("Proje numarası en fazla 50 karakter olabilir");

        // Kaynak ekranda bu iki alan cxDBSpinEdit ile giriliyor; sınır dışı
        // değer SQL'de yuvarlama hatasına yol açtığı için burada da sınırlandı.
        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 10).WithMessage("Tutar ondalık hassasiyeti 0 ile 10 arasında olmalıdır");

        RuleFor(x => x.UnitPriceDecimalPlaces)
            .InclusiveBetween(0, 10).WithMessage("Birim fiyat ondalık hassasiyeti 0 ile 10 arasında olmalıdır");
    }
}

internal sealed class DirectProcurementFileCreateCommandHandler(
    IDirectProcurementFileRepository fileRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<DirectProcurementFileCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DirectProcurementFileCreateCommand request, CancellationToken cancellationToken)
    {
        string? duplicateKey = DirectProcurementFile.BuildDuplicateKey(request.FileNumber, request.WorkingYear);

        DirectProcurementFile? duplicate = await duplicateCheckService.FindDuplicateAsync<DirectProcurementFile>(
            duplicateKey,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<string>.Failure(
                $"'{request.WorkingYear}' yılında '{request.FileNumber}' numaralı dosya zaten var");
        }

        DirectProcurementFile file = new(
            new FileNumber(request.FileNumber.Trim()),
            new WorkName(request.WorkName.Trim()),
            request.Category,
            request.FileType,
            request.WorkingYear,
            request.Date,
            request.Flow,
            request.CompanyId == null ? null : new Domain.Abstractions.IdentityId(request.CompanyId.Value),
            request.SupplierId == null ? null : new Domain.Abstractions.IdentityId(request.SupplierId.Value),
            request.ChartOfAccountId == null ? null : new Domain.Abstractions.IdentityId(request.ChartOfAccountId.Value),
            request.IsActive);

        file.SetProjectNumber(string.IsNullOrWhiteSpace(request.ProjectNumber)
            ? null
            : new ProjectNumber(request.ProjectNumber.Trim()));

        file.SetDecisionNumber(string.IsNullOrWhiteSpace(request.DecisionNumber)
            ? null
            : new DecisionNumber(request.DecisionNumber.Trim()));

        file.SetDecisionDate(request.DecisionDate);
        file.SetApproximateCost(request.ApproximateCost);
        file.SetAppropriation(request.Appropriation);
        file.SetWorkNature(string.IsNullOrWhiteSpace(request.WorkNature) ? null : new WorkNature(request.WorkNature.Trim()));
        file.SetWorkQuantity(string.IsNullOrWhiteSpace(request.WorkQuantity) ? null : new WorkQuantity(request.WorkQuantity.Trim()));
        file.SetPurchasingUnit(Normalize(request.PurchasingUnit));
        file.SetRelatedPerson(Normalize(request.RelatedPerson));
        file.SetHasContract(request.HasContract);
        file.SetHasSpecification(request.HasSpecification);
        file.SetHasCommission(request.HasCommission);

        // "Dosya Ayarları" grup kutusundaki alanlar
        file.SetCostEstimationMethod(request.CostEstimationMethod);
        file.SetHasApproximateCost(request.HasApproximateCost);
        file.SetUseHistoryPurchases(request.UseHistoryPurchases);
        file.SetRequireFinalOffer(request.RequireFinalOffer);
        file.SetDecimalPlaces(request.DecimalPlaces);
        file.SetUnitPriceDecimalPlaces(request.UnitPriceDecimalPlaces);

        await fileRepository.AddAsync(file, cancellationToken);

        return "Doğrudan temin dosyası başarıyla oluşturuldu";
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}