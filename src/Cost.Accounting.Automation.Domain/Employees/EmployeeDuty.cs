using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;

namespace Cost.Accounting.Automation.Domain.Employees;

/// <summary>
/// Bir personelin yetkili olduğu görev ve bu görevin bağlı olduğu atölye.
///
/// Görev ile atölye ayrı tutulur çünkü bazı görevler atölyeye bağlı değildir:
/// "Kurum Müdürü", "Muhasebe Yetkilisi" ve "Taşınır Kayıt Yetkilisi" kurum
/// genelinde geçerlidir (<see cref="WorkshopId"/> boştur), "Atölye Şefi" ise
/// yalnızca belirli bir atölye için tanımlanabilir.
/// </summary>
public sealed class EmployeeDuty : Entity
{
    private EmployeeDuty()
    {
    }

    public EmployeeDuty(
        IdentityId employeeId,
        EmployeeSigningRole signingRole,
        IdentityId? workshopId,
        bool isActive)
    {
        SetEmployee(employeeId);
        SetSigningRole(signingRole);
        SetWorkshop(workshopId);
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public IdentityId EmployeeId { get; private set; } = default!;

    public Employee? Employee { get; private set; }

    public EmployeeSigningRole SigningRole { get; private set; }

    /// <summary>
    /// Görevin bağlı olduğu atölye (<see cref="ChartOfAccountType.Workshop"/>).
    /// Kurum geneli görevlerde <c>null</c>'dır.
    /// </summary>
    public IdentityId? WorkshopId { get; private set; }

    public ChartOfAccount? Workshop { get; private set; }

    /// <summary>
    /// Aynı personelin aynı görevi iki kez (veya aynı görevi iki farklı
    /// atölyede) tanımlanmasını engeller. Atölye yoksa "-", "-1" gibi bir
    /// yer tutucu ile birleştirilir; iki farklı atölye farklı anahtar üretir.
    /// </summary>
    public static string? BuildDuplicateKey(IdentityId employeeId, EmployeeSigningRole role, IdentityId? workshopId)
    {
        // Atölye yoksa null kutuya girer ve DuplicateKeyRule bunu "-"
        // olarak ayırır; iki farklı atölye farklı anahtar üretir.
        object?[] parts = [employeeId.Value, (int)role, workshopId?.Value];

        return DuplicateKeyRule.From(parts);
    }

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(EmployeeId, SigningRole, WorkshopId));

    public void SetEmployee(IdentityId employeeId)
    {
        EmployeeId = employeeId;
        ResolveDuplicateKey();
    }

    public void SetSigningRole(EmployeeSigningRole signingRole)
    {
        SigningRole = signingRole;
        ResolveDuplicateKey();
    }

    public void SetWorkshop(IdentityId? workshopId)
    {
        WorkshopId = workshopId;
        ResolveDuplicateKey();
    }
}