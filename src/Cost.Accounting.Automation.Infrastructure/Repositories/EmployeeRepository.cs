using Cost.Accounting.Automation.Domain.Employees;
using Cost.Accounting.Automation.Infrastructure.Abstractions;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Repositories;

internal sealed class EmployeeRepository : AuditableRepository<Employee, ApplicationDbContext>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context, MasterDbContext masterContext)
        : base(context, masterContext)
    {
    }

    /// <summary>
    /// Liste ekranında görev sütunları görüneceği için görevler önceden
    /// yüklenir; <c>GetAllWithAudit</c> sonucu belleğe materyalize edildiği için
    /// Include uygulanmazsa görev listesi boş gelir.
    /// </summary>
    protected override IQueryable<Employee> ApplyDetailIncludes(IQueryable<Employee> query)
        => query.Include(e => e.Duties).ThenInclude(d => d.Workshop);
}

internal sealed class EmployeeDutyRepository : AuditableRepository<EmployeeDuty, ApplicationDbContext>, IEmployeeDutyRepository
{
    public EmployeeDutyRepository(ApplicationDbContext context, MasterDbContext masterContext)
        : base(context, masterContext)
    {
    }
}