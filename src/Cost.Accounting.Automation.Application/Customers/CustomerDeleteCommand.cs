using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.Invoices;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;

[Permission("customer:delete")]
public sealed record CustomerDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository,
    IInvoiceRepository invoiceRepository,
    ICurrentAccountMovementRepository currentAccountMovementRepository,
    ICostSlipRepository costSlipRepository) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerDeleteCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (customer is null)
        {
            return Result<string>.Failure("Müşteri bulunamadı");
        }

        bool hasMovement = await currentAccountMovementRepository.AnyAsync(
            m => m.CustomerId == new IdentityId(request.Id), cancellationToken);
        if (hasMovement)
        {
            return Result<string>.Failure(
                $"'{customer.Name.Value}' müşterisi cari hareket gördüğü için silinemez.");
        }

        bool hasInvoice = await invoiceRepository.AnyAsync(
            i => i.CustomerId == new IdentityId(request.Id), cancellationToken);
        bool hasCostSlip = await costSlipRepository.AnyAsync(
            c => c.CustomerId == new IdentityId(request.Id), cancellationToken);

        customer.Delete();
        customerRepository.Update(customer);

        if (hasInvoice || hasCostSlip)
        {
            return DeleteWarnings.Compose(
                $"'{customer.Name.Value}' müşterisi silindi. NOT: irsaliye/maliyet pusulası kayıtlarında kullanılıyor; " +
                $"hareket görmediği için silme gerçekleştirildi.");
        }

        return "Müşteri başarıyla silindi";
    }
}