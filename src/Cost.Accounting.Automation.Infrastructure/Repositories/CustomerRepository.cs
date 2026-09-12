using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;
internal sealed class CustomerRepository : AuditableRepository<Customer, ApplicationDbContext>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }
}