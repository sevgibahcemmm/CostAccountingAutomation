using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:view")]
public sealed record CustomerGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<CustomerDto>>
{
    public CustomerGetAllQuery() : this(false) { }
}

internal sealed class CustomerGetAllQueryHandler(
    ICustomerRepository customerRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CustomerGetAllQuery, IQueryable<CustomerDto>>
{
    public async Task<IQueryable<CustomerDto>> Handle(CustomerGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Customer> source = request.OnlyDeleted
            ? customerRepository.GetAllIncludingDeleted().Where(i => i.IsDeleted)
            : customerRepository.GetAll();

        return await source
            .MapTo()
            .ToAuditedListAsync(auditUserNames, cancellationToken);
    }
}
