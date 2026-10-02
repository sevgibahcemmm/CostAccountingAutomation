using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Görev listesi üzerindeki ortak kurallar. Hem komut işleyicileri hem de
/// düzenleme formu aynı doğrulamayı kullanır; iki yerde farklı kural
/// uygulanması imza satırlarının boş kalmasına yol açardı.
/// </summary>
public static class EmployeeDutyValidator
{
    /// <summary>
    /// Aynı personel için aynı görevi (aynı atölyede) iki kez tanımlamak
    /// geçersizdir. DuplicateKey üzerinde benzersiz bir indeks
    /// <c>AuditedDbContext</c> tarafından zaten eklenen indeksle çakıştığı için
    /// bu kural veritabanında değil uygulama düzeyinde korunur.
    /// </summary>
    public static bool HasDuplicate(IReadOnlyList<EmployeeDutyInput> duties)
        => duties
            .Select(d => (d.SigningRole, d.WorkshopId))
            .Distinct()
            .Count() != duties.Count;

    /// <summary>Atölye seçilmesi zorunlu görevlerde seçim yapılmamış mı?</summary>
    public static bool HasMissingWorkshop(IReadOnlyList<EmployeeDutyInput> duties)
        => duties.Any(d =>
            EmployeeSigningRoleRules.RequiresWorkshop(d.SigningRole)
            && d.WorkshopId is null);

    /// <summary>
    /// Görev hücresi boş bırakıldığında enum değeri 0 olur; 0 hiçbir görevi
    /// temsil etmez ve veritabanına yanlış değer yazılır. Sıfır değer
    /// <c>tinyint</c> olarak saklandığı için hatayı veritabanı yakalamaz.
    /// </summary>
    public static bool HasUnselectedRole(IReadOnlyList<EmployeeDutyInput> duties)
        => duties.Any(d => !Enum.IsDefined(d.SigningRole));

    /// <summary>Yalnızca gerçekten bir atölye seçilmiş kayıtların kimliklerini döner.</summary>
    public static List<Guid> WorkshopIds(IReadOnlyList<EmployeeDutyInput> duties)
        => [.. duties
            .Where(d => d.WorkshopId is not null)
            .Select(d => d.WorkshopId!.Value)
            .Distinct()];

    /// <summary>
    /// Seçilen atölyelerin gerçekten <see cref="ChartOfAccountType.Workshop"/>
    /// türünde ve var olduğunu doğrular. Atölye olmayan bir hesap (depo, kategori)
    /// görevlendirmeye kabul edilmez.
    /// </summary>
    public static async Task<bool> WorkshopsExistAsync(
        IReadOnlyList<EmployeeDutyInput> duties,
        IChartOfAccountRepository chartOfAccountRepository,
        CancellationToken cancellationToken)
    {
        List<Guid> ids = WorkshopIds(duties);

        if (ids.Count == 0)
        {
            return true;
        }

        int found = await chartOfAccountRepository.CountAsync(
            a => ids.Contains(a.Id) && a.Type == ChartOfAccountType.Workshop,
            cancellationToken);

        return found == ids.Count;
    }
}