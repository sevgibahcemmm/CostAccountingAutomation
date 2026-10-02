using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Employees;

public sealed class EmployeeDuty : Entity
{
    private EmployeeDuty()
    {
    }

    public EmployeeDuty(
        IdentityId employeeId,
        IdentityId signingRoleId,
        IdentityId? workshopId,
        bool isActive)
    {
        SetEmployee(employeeId);
        SetSigningRole(signingRoleId);
        SetWorkshop(workshopId);
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public IdentityId EmployeeId { get; private set; } = default!;

    public Employee? Employee { get; private set; }

    public IdentityId SigningRoleId { get; private set; } = default!;

    public EmployeeSigningRole? SigningRole { get; private set; }

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
    public static string? BuildDuplicateKey(
        IdentityId employeeId,
        IdentityId signingRoleId,
        IdentityId? workshopId)
    {
        // Atölye yoksa null kutuya girer ve DuplicateKeyRule bunu "-"
        // olarak ayırır; iki farklı atölye farklı anahtar üretir.
        object?[] parts = [employeeId.Value, signingRoleId.Value, workshopId?.Value];

        return DuplicateKeyRule.From(parts);
    }

    public void ResolveDuplicateKey()
    {
        // EF Core materyalizasyonda alanları sözleşme gereği sırayla
        // doldurmaz. "SigningRoleId" (artık bir foreign key olduğu için
        // yazıcısı SetSigningRole olan bir özellik) "EmployeeId"'den önce
        // yazılırsa EmployeeId henüz null'dır ve erişim patlar.
        //
        // Bu yüzden anahtar üç parçanın tamamı mevcut olduğunda üretilir;
        // eksikken mevcut değer korunur. Sıralama ne olursa olsun üçüncü ve
        // son atanan alan anahtarı doğru şekilde hesaplar. Yeni kayıtlar
        // kurucudan geçtiği için zaten eksiksizdir.
        if (EmployeeId is null || SigningRoleId is null)
        {
            return;
        }

        SetDuplicateKey(BuildDuplicateKey(EmployeeId, SigningRoleId, WorkshopId));
    }

    public void SetEmployee(IdentityId employeeId)
    {
        EmployeeId = employeeId;
        ResolveDuplicateKey();
    }

    public void SetSigningRole(IdentityId signingRoleId)
    {
        SigningRoleId = signingRoleId;
        ResolveDuplicateKey();
    }

    public void SetWorkshop(IdentityId? workshopId)
    {
        WorkshopId = workshopId;
        ResolveDuplicateKey();
    }
}
