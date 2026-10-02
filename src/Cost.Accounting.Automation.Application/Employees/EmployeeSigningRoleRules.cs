using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Görev tanımlarının iş kuralları. Raporlar bu kurallara göre imza yuvasını
/// doldurur; aynı kurallar düzenleme formundaki doğrulamada da kullanılır.
/// </summary>
public static class EmployeeSigningRoleRules
{
    /// <summary>
    /// Bu görev için atölye seçimi zorunlu mudur? Yalnızca "Atölye Şefi"
    /// atölyeye bağlıdır; imzası çıkan diğer görevler kurum genelindedir.
    /// </summary>
    public static bool RequiresWorkshop(EmployeeSigningRole role)
        => role == EmployeeSigningRole.WorkshopChief;

    /// <summary>
    /// Bu görev bir atölyeye bağlı olabilir mi? Atölye seçilenebilen tüm
    /// görevler için true; kurum geneli görevler için false.
    /// </summary>
    public static bool AllowsWorkshop(EmployeeSigningRole role)
        => role == EmployeeSigningRole.WorkshopChief;

    /// <summary>Rapor imza bloklarında kullanılan görevlerin sırası.</summary>
    public static IReadOnlyList<EmployeeSigningRole> ReportRoles { get; } =
    [
        EmployeeSigningRole.InstitutionDirector,
        EmployeeSigningRole.WorkshopChief,
        EmployeeSigningRole.MovableAssetOfficer,
        EmployeeSigningRole.AccountingOfficer,
        EmployeeSigningRole.SpendingOfficer,
        EmployeeSigningRole.PermanentOfficer,
        EmployeeSigningRole.Counter,
        EmployeeSigningRole.Controller
    ];
}