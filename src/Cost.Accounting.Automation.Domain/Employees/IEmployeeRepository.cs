using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.Employees;

public interface IEmployeeRepository : IAuditableRepository<Employee>
{
    /// <summary>
    /// Sicil numarası verilmiş başka bir personel var mı diye bakar.
    /// </summary>
    /// <param name="registryNumber">Aranan sicil numarası (kırpılmış hâli).</param>
    /// <param name="excludeId">
    /// Güncellemede kaydın kendisi hariç tutulur; yoksa kayıt kendi sicil
    /// numarasıyla "çakışıyor" görünür.
    /// </param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task<bool> RegistryNumberExistsAsync(
        string registryNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
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
