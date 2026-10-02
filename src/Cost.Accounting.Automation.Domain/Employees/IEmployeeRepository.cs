using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Employees;

public interface IEmployeeRepository : IAuditableRepository<Employee>
{
}

public interface IEmployeeDutyRepository : IAuditableRepository<EmployeeDuty>
{
}