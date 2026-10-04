using Cost.Accounting.Automation.Domain.Abstractions;
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

    /// <summary>
    /// Sicil numarası benzersiz mi diye bakar. Karşılaştırma veritabanında
    /// yapılır; kayıt sayısı çok olsa bile yalnızca varlık kontrolü (EXISTS)
    /// çalıştığı için tüm kayıtlar belleğe alınmaz.
    /// </summary>
    public Task<bool> RegistryNumberExistsAsync(
        string registryNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Employee> query = Context.Set<Employee>()
            .IgnoreQueryFilters()
            .Where(e => e.RegistryNumber == registryNumber);

        // Id bir value-converter alanıdır; filtre koşulunda `.Value`
        // özelliği kullanılamaz (SQL'e çevrilemez), nesnenin kendisi
        // karşılaştırılır.
        if (excludeId is Guid id)
        {
            IdentityId excluded = new(id);
            query = query.Where(e => e.Id != excluded);
        }

        return query.AnyAsync(cancellationToken);
    }
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
