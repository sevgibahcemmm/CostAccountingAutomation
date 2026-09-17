using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public sealed class ChartOfAccountLookUpDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ChartOfAccountType Type { get; set; }
    public Guid? ParentId { get; set; }

    public string Display => $"{Code} - {Name}";
}

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountLookUpQuery : IRequest<Result<List<ChartOfAccountLookUpDto>>>;

internal sealed class ChartOfAccountLookUpQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountLookUpQuery, Result<List<ChartOfAccountLookUpDto>>>
{
    public async Task<Result<List<ChartOfAccountLookUpDto>>> Handle(ChartOfAccountLookUpQuery request, CancellationToken cancellationToken)
    {
        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);

        List<ChartOfAccountLookUpDto> result = accounts
            .Where(a => !a.IsDeleted)
            .Select(a => new ChartOfAccountLookUpDto
            {
                Id = a.Id,
                Code = a.Code.Value,
                Name = a.Name.Value,
                Type = a.Type,
                ParentId = a.ParentId == null ? null : a.ParentId.Value
            })
            .ToList();

        result.Sort(Comparer<ChartOfAccountLookUpDto>.Create(
            (a, b) => AccountCodeComparer.Instance.Compare(a.Code, b.Code)));

        return result;
    }
}