using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;
[Permission("customer:view")]
public sealed record CustomerGetQuery(
    Guid Id) : IRequest<Result<CustomerDto>>;

internal sealed class CustomerGetQueryHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerGetQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CustomerGetQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(i => i.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Müşteri bulunamadı");
        }

        return customer;
    }
}