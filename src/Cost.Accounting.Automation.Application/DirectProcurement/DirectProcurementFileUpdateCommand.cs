using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.Domain.DirectProcurement.ValueObjects;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>Doğrudan temin dosyası güncelleme komutu.</summary>
[Permission("direct_procurement:update")]
public sealed record DirectProcurementFileUpdateCommand(
    Guid Id,
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

public sealed class DirectProcurementFileUpdateCommandValidator : AbstractValidator<DirectProcurementFileUpdateCommand>
{
    public DirectProcurementFileUpdateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Geçerli bir ID girin");

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

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 10).WithMessage("Tutar ondalık hassasiyeti 0 ile 10 arasında olmalıdır");

        RuleFor(x => x.UnitPriceDecimalPlaces)
            .InclusiveBetween(0, 10).WithMessage("Birim fiyat ondalık hassasiyeti 0 ile 10 arasında olmalıdır");
    }
}

internal sealed class DirectProcurementFileUpdateCommandHandler(
    IDirectProcurementFileRepository fileRepository,
    IDuplicateCheckService duplicateCheckService) : IRequestHandler<DirectProcurementFileUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DirectProcurementFileUpdateCommand request, CancellationToken cancellationToken)
    {
        IdentityId fileId = new(request.Id);

        DirectProcurementFile? file = await fileRepository.GetByExpressionWithTrackingAsync(
            p => p.Id == fileId,
            cancellationToken);

        if (file is null)
        {
            return Result<string>.Failure("Doğrudan temin dosyası bulunamadı");
        }

        if (!file.IsEditable)
        {
            return Result<string>.Failure("Arşivlenmiş dosyalar düzenlenemez");
        }

        string? duplicateKey = DirectProcurementFile.BuildDuplicateKey(request.FileNumber, request.WorkingYear);

        DirectProcurementFile? duplicate = await duplicateCheckService.FindDuplicateAsync<DirectProcurementFile>(
            duplicateKey,
            excludeId: request.Id,
            cancellationToken: cancellationToken);

        if (duplicate is not null)
        {
            return Result<string>.Failure(
                $"'{request.WorkingYear}' yılında '{request.FileNumber}' numaralı başka bir dosya var");
        }

        file.SetFileNumber(new FileNumber(request.FileNumber.Trim()));
        file.SetWorkName(new WorkName(request.WorkName.Trim()));
        file.SetCategory(request.Category);
        file.SetFileType(request.FileType);
        file.SetWorkingYear(request.WorkingYear);
        file.SetDate(request.Date);
        file.SetFlow(request.Flow);
        file.SetCompany(request.CompanyId == null ? null : new IdentityId(request.CompanyId.Value));
        file.SetSupplier(request.SupplierId == null ? null : new IdentityId(request.SupplierId.Value));
        file.SetChartOfAccount(request.ChartOfAccountId == null ? null : new IdentityId(request.ChartOfAccountId.Value));
        file.SetProjectNumber(Normalize(request.ProjectNumber) is { } project ? new ProjectNumber(project) : null);
        file.SetDecisionNumber(Normalize(request.DecisionNumber) is { } decision ? new DecisionNumber(decision) : null);
        file.SetDecisionDate(request.DecisionDate);
        file.SetApproximateCost(request.ApproximateCost);
        file.SetAppropriation(request.Appropriation);
        file.SetWorkNature(Normalize(request.WorkNature) is { } nature ? new WorkNature(nature) : null);
        file.SetWorkQuantity(Normalize(request.WorkQuantity) is { } quantity ? new WorkQuantity(quantity) : null);
        file.SetPurchasingUnit(Normalize(request.PurchasingUnit));
        file.SetRelatedPerson(Normalize(request.RelatedPerson));
        file.SetHasContract(request.HasContract);
        file.SetHasSpecification(request.HasSpecification);
        file.SetHasCommission(request.HasCommission);
        file.SetCostEstimationMethod(request.CostEstimationMethod);
        file.SetHasApproximateCost(request.HasApproximateCost);
        file.SetUseHistoryPurchases(request.UseHistoryPurchases);
        file.SetRequireFinalOffer(request.RequireFinalOffer);
        file.SetDecimalPlaces(request.DecimalPlaces);
        file.SetUnitPriceDecimalPlaces(request.UnitPriceDecimalPlaces);
        file.SetStatus(request.IsActive);

        fileRepository.Update(file);

        return "Doğrudan temin dosyası başarıyla güncellendi";
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}