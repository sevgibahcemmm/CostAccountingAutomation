using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Customers;
public interface ICustomerRepository : IAuditableRepository<Customer>
{
}