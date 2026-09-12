using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Customers;
[Permission("customer:view")]
public sealed record CustomerGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<CustomerDto>>
{
    public CustomerGetAllQuery() : this(false) { }
}

internal sealed class CustomerGetAllQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerGetAllQuery, IQueryable<CustomerDto>>
{
    public Task<IQueryable<CustomerDto>> Handle(CustomerGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Customer>> source = request.OnlyDeleted
            ? customerRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : customerRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}