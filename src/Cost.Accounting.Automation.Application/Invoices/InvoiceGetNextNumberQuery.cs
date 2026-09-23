using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Invoices;

[Permission("invoice:view")]
public sealed record InvoiceGetNextNumberQuery(InvoiceType InvoiceType) : IRequest<Result<string>>;

internal sealed class InvoiceGetNextNumberQueryHandler(
    IInvoiceRepository invoiceRepository,
    ICompanyRepository companyRepository,
    IClaimContext claimContext) : IRequestHandler<InvoiceGetNextNumberQuery, Result<string>>
{
    public async Task<Result<string>> Handle(InvoiceGetNextNumberQuery request, CancellationToken cancellationToken)
    {
        if (request.InvoiceType != InvoiceType.Sales && request.InvoiceType != InvoiceType.SalesReturn)
        {
            return Result<string>.Failure("DACİK numaralama yalnızca satış ve satışlardan iade faturaları için geçerlidir.");
        }

        Guid companyId = claimContext.GetCompanyId();

        Company? company = await companyRepository
            .GetByIdIncludingDeletedAsync(new IdentityId(companyId), cancellationToken);

        string prefix = company is not null && !string.IsNullOrWhiteSpace(company.Invoiceinformation.Value)
            ? company.Invoiceinformation.Value.Trim()
            : "FAT";

        string year = DateTime.Today.Year.ToString();
        string beginsWith = $"{prefix}{year}";

        List<string> numbers = await invoiceRepository
            .GetAllWithAuditIncludingDeleted()
            .Where(i => i.Entity.InvoiceType == InvoiceType.Sales || i.Entity.InvoiceType == InvoiceType.SalesReturn)
            .Select(i => i.Entity.InvoiceNumber)
            .ToListAsync(cancellationToken);

        long maxSequence = 0;
        foreach (string number in numbers)
        {
            if (!number.StartsWith(beginsWith, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string tail = number[beginsWith.Length..];
            if (tail.Length > 0 && tail.All(char.IsDigit) && long.TryParse(tail, out long sequence))
            {
                maxSequence = Math.Max(maxSequence, sequence);
            }
        }

        return Result<string>.Succeed($"{beginsWith}{maxSequence + 1:0000000}");
    }
}