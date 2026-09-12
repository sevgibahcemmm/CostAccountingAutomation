using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:delete")]
public sealed record CustomerDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure("Müşteri bulunamadı");
        }

        customer.Delete();
        customerRepository.Update(customer);

        return "Müşteri başarıyla silindi";
    }
}