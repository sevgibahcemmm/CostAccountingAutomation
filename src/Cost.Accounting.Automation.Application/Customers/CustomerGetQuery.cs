using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Customers;
[Permission("customer:view")]
public sealed record CustomerGetQuery(
    Guid Id) : IRequest<Result<CustomerDto>>;

internal sealed class CustomerGetQueryHandler(
    ICustomerRepository customerRepository,
    IAuditUserNameResolver auditUserNames) : IRequestHandler<CustomerGetQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CustomerGetQuery request, CancellationToken cancellationToken)
    {
        // Kimlik doğrudan veritabanında filtrelenir. Daha önce liste sorgusu
        // üzerinden FirstOrDefaultAsync çağrılıyordu; liste sorgusu belleğe
        // alındığı için "tek müşteri getir" isteği tüm müşteri tablosunu
        // belleğe alıp filtreliyordu.
        CustomerDto? customer = await customerRepository
            .GetAllActive()
            .Where(i => i.Id == new IdentityId(request.Id))
            .MapTo()
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Müşteri bulunamadı");
        }

        await auditUserNames.FillAsync([customer], cancellationToken);

        return customer;
    }
}
