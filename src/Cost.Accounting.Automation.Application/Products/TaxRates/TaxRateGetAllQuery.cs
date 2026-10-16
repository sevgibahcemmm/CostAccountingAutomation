using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Products.TaxRates;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products.TaxRates;

[Permission("product:view")]
public sealed record TaxRateGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<TaxRateDto>>
{
    public TaxRateGetAllQuery() : this(false) { }
}

internal sealed class TaxRateGetAllQueryHandler(
    ITaxRateRepository taxRateRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<TaxRateGetAllQuery, IQueryable<TaxRateDto>>
{
    public async Task<IQueryable<TaxRateDto>> Handle(TaxRateGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<TaxRate> source = request.OnlyDeleted
            ? taxRateRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : taxRateRepository.GetAllActive();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
