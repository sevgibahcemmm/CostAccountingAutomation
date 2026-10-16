using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>Doğrudan temin dosyalarının tümünü getiren liste sorgusu.</summary>
[Permission("direct_procurement:view")]
public sealed record DirectProcurementFileGetAllQuery(
    bool OnlyDeleted = false,
    int? Year = null) : IRequest<IQueryable<DirectProcurementFileDto>>
{
    public DirectProcurementFileGetAllQuery() : this(false, null)
    {
    }
}

internal sealed class DirectProcurementFileGetAllQueryHandler(
    IDirectProcurementFileRepository fileRepository,
    IAuditUserNameResolver auditUserNames,
    IMasterCompanyNameLookup companyNames) : IRequestHandler<DirectProcurementFileGetAllQuery, IQueryable<DirectProcurementFileDto>>
{
    public async Task<IQueryable<DirectProcurementFileDto>> Handle(
        DirectProcurementFileGetAllQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<DirectProcurementFile> source = request.OnlyDeleted
            ? fileRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : fileRepository.GetAllActive();

        if (request.Year is { } year)
        {
            source = source.Where(f => f.WorkingYear == year);
        }

        IQueryable<DirectProcurementFileDto> projected = await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);

        // Birim adları merkezî (Master) veritabanında olduğu için SQL'e
        // katılamaz; tek toplu sorguda çözüp DTO'ya yazıyoruz.
        List<DirectProcurementFileDto> items = projected.ToList();
        await FillCompanyNamesAsync(items, cancellationToken);

        return items.AsQueryable();
    }

    private async Task FillCompanyNamesAsync(List<DirectProcurementFileDto> items, CancellationToken cancellationToken)
    {
        List<Guid> ids = [.. items
            .Where(i => i.CompanyId.HasValue)
            .Select(i => i.CompanyId!.Value)
            .Distinct()];

        if (ids.Count == 0)
        {
            return;
        }

        Dictionary<Guid, string> names = await companyNames.GetNamesAsync(ids, cancellationToken);

        foreach (DirectProcurementFileDto item in items)
        {
            if (item.CompanyId is { } id && names.TryGetValue(id, out string? name))
            {
                item.CompanyName = name;
            }
        }
    }
}

/// <summary>Tek bir doğrudan temin dosyasının detayını getiren sorgu.</summary>
[Permission("direct_procurement:view")]
public sealed record DirectProcurementFileGetQuery(
    Guid Id) : IRequest<Result<DirectProcurementFileDto>>;

internal sealed class DirectProcurementFileGetQueryHandler(
    IDirectProcurementFileRepository fileRepository,
    IAuditUserNameResolver auditUserNames,
    IMasterCompanyNameLookup companyNames) : IRequestHandler<DirectProcurementFileGetQuery, Result<DirectProcurementFileDto>>
{
    public async Task<Result<DirectProcurementFileDto>> Handle(
        DirectProcurementFileGetQuery request,
        CancellationToken cancellationToken)
    {
        DirectProcurementFile? file = await fileRepository.GetByIdWithDetailsAsync(
            new IdentityId(request.Id),
            cancellationToken);

        if (file is null)
        {
            return Result<DirectProcurementFileDto>.Failure("Doğrudan temin dosyası bulunamadı");
        }

        DirectProcurementFileDto dto = new()
        {
            Id = file.Id,
            FileNumber = file.FileNumber.Value,
            WorkName = file.WorkName.Value,
            WorkingYear = file.WorkingYear,
            Category = file.Category,
            Flow = file.Flow,
            Stage = file.Stage,
            CompanyId = file.CompanyId?.Value,
            SupplierId = file.SupplierId?.Value,
            ChartOfAccountId = file.ChartOfAccountId?.Value,
            FileType = file.FileType,
            SupplierName = file.Supplier?.Name.Value,
            ApproximateCost = file.ApproximateCost,
            Appropriation = file.Appropriation,
            Date = file.Date,
            ProjectNumber = file.ProjectNumber?.Value,
            DecisionNumber = file.DecisionNumber?.Value,
            DecisionDate = file.DecisionDate,
            PurchasingUnit = file.PurchasingUnit,
            RelatedPerson = file.RelatedPerson,
            WorkNature = file.WorkNature?.Value,
            WorkQuantity = file.WorkQuantity?.Value,
            HasContract = file.HasContract,
            HasSpecification = file.HasSpecification,
            HasCommission = file.HasCommission,
            ItemCount = file.Items.Count,
            QuoteCount = file.QuotedFirms.Count,
            IsArchived = file.IsArchived,
            IsActive = file.IsActive,
            CreatedAt = file.CreatedAt,
            CreatedBy = file.CreatedBy,
            UpdatedAt = file.UpdatedAt,
            UpdatedBy = file.UpdatedBy?.Value
        };

        await auditUserNames.FillAsync([dto], cancellationToken);

        if (file.CompanyId is { } companyId)
        {
            Dictionary<Guid, string> names = await companyNames.GetNamesAsync([companyId], cancellationToken);
            if (names.TryGetValue(companyId, out string? name))
            {
                dto.CompanyName = name;
            }
        }

        return dto;
    }
}