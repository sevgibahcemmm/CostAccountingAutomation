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
    /// Include uygulanmasa görev listesi boş gelir. Görevin adı
    /// <see cref="EmployeeSigningRole"/> kaydından okunduğu için o navigasyon da
    /// yüklenmelidir.
    /// </summary>
    protected override IQueryable<Employee> ApplyDetailIncludes(IQueryable<Employee> query)
        => query.Include(e => e.Duties)
            .ThenInclude(d => d.Workshop)
            .Include(e => e.Duties)
            .ThenInclude(d => d.SigningRole);
}

internal sealed class EmployeeDutyRepository : AuditableRepository<EmployeeDuty, ApplicationDbContext>, IEmployeeDutyRepository
{
    public EmployeeDutyRepository(ApplicationDbContext context, MasterDbContext masterContext)
        : base(context, masterContext)
    {
    }

    /// <summary>
    /// İmza çözümlemesi görevin adını basar; görev kaydının görev tanımı
    /// yüklenmeden sorgu boş görev adı döndürür.
    /// </summary>
    protected override IQueryable<EmployeeDuty> ApplyDetailIncludes(IQueryable<EmployeeDuty> query)
        => query.Include(d => d.SigningRole).Include(d => d.Employee);
}

internal sealed class EmployeeSigningRoleRepository
    : AuditableRepository<EmployeeSigningRole, ApplicationDbContext>, IEmployeeSigningRoleRepository
{
    public EmployeeSigningRoleRepository(ApplicationDbContext context, MasterDbContext masterContext)
        : base(context, masterContext)
    {
    }

    public Task<List<EmployeeSigningRole>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default)
        => Context.Set<EmployeeSigningRole>()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
}
