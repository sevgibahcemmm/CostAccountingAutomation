using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.CostSlips;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record CostSlipGetNextNumberQuery(CostSlipType CostSlipType) : IRequest<Result<string>>;

internal sealed class CostSlipGetNextNumberQueryHandler(
    ICostSlipRepository costSlipRepository) : IRequestHandler<CostSlipGetNextNumberQuery, Result<string>>
{
    public async Task<Result<string>> Handle(CostSlipGetNextNumberQuery request, CancellationToken cancellationToken)
    {
        string prefix = request.CostSlipType switch
        {
            CostSlipType.Product => "MP",
            CostSlipType.Service => "HS",
            CostSlipType.SemiFinishedProduct => "YP",
            _ => "MP"
        };

        string yearPrefix = $"{prefix}-{DateTime.Now:yyyy}-";

        // Sayım doğrudan veritabanında yapılır. Daha önce liste sorgusu
        // (tüm pusulalar + tüm navigasyonları) belleğe alınıp bellekte
        // sayılıyordu; form açılışındaki bu tek sorgu tüm tabloyu taşıyordu.
        int count = await costSlipRepository
            .GetAllIncludingDeleted()
            .CountAsync(i => i.CostSlipType == request.CostSlipType
                && i.SlipNumber.StartsWith(yearPrefix),
                cancellationToken);

        return Result<string>.Succeed($"{yearPrefix}{count + 1:00000}");
    }
}
