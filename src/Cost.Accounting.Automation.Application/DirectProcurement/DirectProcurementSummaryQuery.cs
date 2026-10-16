using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>
/// Doğrudan temin ana sayfasının üst kısmında gösterilen özet.
/// </summary>
/// <remarks>
/// Tüm sayılar ve tutarlar tek sorguda <c>GroupBy</c> ile okunur. Aşama
/// başına ayrı sorgu atmak dosya sayısı arttıkça ana sayfayı yavaşlatır ve
/// kutular arasındaki sayılar tutarsız görünebilir.
/// </remarks>
public sealed record DirectProcurementSummary(
    int TotalCount,
    int ArchivedCount,
    decimal TotalApproximateCost,
    decimal TotalAppropriation,
    IReadOnlyDictionary<DirectProcurementStage, int> CountByStage);

[Permission("direct_procurement:view")]
public sealed record DirectProcurementSummaryQuery(
    int? Year = null) : IRequest<Result<DirectProcurementSummary>>;

internal sealed class DirectProcurementSummaryQueryHandler(
    IDirectProcurementFileRepository fileRepository) : IRequestHandler<DirectProcurementSummaryQuery, Result<DirectProcurementSummary>>
{
    public async Task<Result<DirectProcurementSummary>> Handle(
        DirectProcurementSummaryQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<DirectProcurementFile> source = fileRepository.GetAllActive();

        if (request.Year is { } year)
        {
            source = source.Where(f => f.WorkingYear == year);
        }

        List<StageRollup> rollups = await source
            .GroupBy(f => f.Stage)
            .Select(g => new StageRollup(
                g.Key,
                g.Count(),
                g.Sum(f => f.ApproximateCost ?? 0m),
                g.Sum(f => f.Appropriation ?? 0m),
                g.Count(f => f.IsArchived)))
            .ToListAsync(cancellationToken);

        Dictionary<DirectProcurementStage, int> countByStage = rollups.ToDictionary(r => r.Stage, r => r.Count);

        return new DirectProcurementSummary(
            rollups.Sum(r => r.Count),
            rollups.Sum(r => r.ArchivedCount),
            rollups.Sum(r => r.ApproximateCost),
            rollups.Sum(r => r.Appropriation),
            countByStage);
    }

    private sealed record StageRollup(
        DirectProcurementStage Stage,
        int Count,
        decimal ApproximateCost,
        decimal Appropriation,
        int ArchivedCount);
}