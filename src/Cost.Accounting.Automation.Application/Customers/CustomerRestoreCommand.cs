using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;
[Permission("customer:delete")]
public sealed record CustomerRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerRestoreCommandHandler(
    ICustomerRepository customerRepository) : IRequestHandler<CustomerRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerRestoreCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdIncludingDeletedAsync(new IdentityId(request.Id), cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure("Müşteri bulunamadı");
        }

        if (!customer.IsDeleted)
        {
            return Result<string>.Failure("Müşteri zaten silinmiş durumda değil");
        }

        customer.Restore();
        customerRepository.Update(customer);

        return "Müşteri başarıyla geri yüklendi";
    }
}