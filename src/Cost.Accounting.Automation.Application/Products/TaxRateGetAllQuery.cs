using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:view")]
public sealed record TaxRateGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<TaxRateDto>>
{
    public TaxRateGetAllQuery() : this(false) { }
}

internal sealed class TaxRateGetAllQueryHandler(
    ITaxRateRepository taxRateRepository) : IRequestHandler<TaxRateGetAllQuery, IQueryable<TaxRateDto>>
{
    public Task<IQueryable<TaxRateDto>> Handle(TaxRateGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<TaxRate>> source = request.OnlyDeleted
            ? taxRateRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : taxRateRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}