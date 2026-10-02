using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Employees;

public interface IEmployeeRepository : IAuditableRepository<Employee>
{
}

public interface IEmployeeDutyRepository : IAuditableRepository<EmployeeDuty>
{
}

/// <summary>
/// Yetkili görev tanımları (rapor imza bloklarındaki görevler).
/// </summary>
public interface IEmployeeSigningRoleRepository : IAuditableRepository<EmployeeSigningRole>
{
    /// <summary>
    /// Silinmiş kayıtlar dâhil tüm görev tanımlarını döner. Görevlendirme
    /// doğrulamasında silinmiş bir görevi "bulunamadı" olarak ayırt etmek için
    /// gerekir; global sorgu filtresi bunu gizlerdi.
    /// </summary>
    Task<List<EmployeeSigningRole>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
}
