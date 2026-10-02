using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Görevlendirme satırında ihtiyaç duyulan görev tanımı bilgisi. Görevler
/// veritabanında tanımlı olduğu için "atölye seçimi zorunlu mu" sorusunun
/// cevabı kayıttan okunur ve forma bu haliyle taşınır.
/// </summary>
public sealed record EmployeeDutyRoleInfo(Guid Id, string Name, bool RequiresWorkshop);

/// <summary>
/// Görev listesi üzerindeki ortak kurallar. Hem komut işleyicileri hem de
/// düzenleme formu aynı doğrulamayı kullanır; iki yerde farklı kural
/// uygulanması imza satırlarının boş kalmasına yol açardı.
///
/// <para>
/// Görev tanımları veritabanında olduğu için "atölye seçimi zorunlu mu"
/// sorusunun cevabı kayıttan okunur. Düzenleme formu bu bilgiyi zaten
/// yüklediği için <see cref="FindProblem"/> ile anında, komut işleyicileri ise
/// <see cref="FindProblemAsync"/> ile veritabanından doğrular.
/// </para>
///
/// <para>
/// İki kural birlikte uygulanır: atölyeye bağlı görevlerde atölye
/// <b>zorunludur</b>, kurum geneli görevlerde ise atölye
/// <b>seçilemez</b>. İkinci kural şarttır: DuplicateKey görev + atölye
/// çiftinden üretildiği için kurum geneli bir görev ("Sabit Görevli")
/// atölyeye bağlanırsa her atölyede ayrı bir kayıt gibi görünür ve aynı
/// görevin mükerrer atanması denetlenemez.
/// </para>
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
            .Select(d => (d.SigningRoleId, d.WorkshopId))
            .Distinct()
            .Count() != duties.Count;

    /// <summary>
    /// Görev hücresi boş bırakıldığında görev kimliği <c>null</c> olur ve kayıt
    /// satırı hangi görevi taşıdığını bilmez. Böyle bir satır kaydedilirse
    /// imza satırına kim basacağı çözülemez.
    /// </summary>
    public static bool HasUnselectedRole(IReadOnlyList<EmployeeDutyInput> duties)
        => duties.Any(d => d.SigningRoleId is null || d.SigningRoleId.Value == Guid.Empty);

    /// <summary>Yalnızca gerçekten bir atölye seçilmiş kayıtların kimliklerini döner.</summary>
    public static List<Guid> WorkshopIds(IReadOnlyList<EmployeeDutyInput> duties)
        => [.. duties
            .Where(d => d.WorkshopId is not null)
            .Select(d => d.WorkshopId!.Value)
            .Distinct()];

    /// <summary>
    /// Görev ile atölye eşleşmesini denetler. Görev tanımları veritabanında
    /// olduğu için senkron doğrulama tek başına yeterli değildir; çağıran taraf
    /// rolleri önceden yükler.
    /// </summary>
    /// <returns>
    /// Sorun yoksa <c>null</c>; aksi hâlde kullanıcıya gösterilecek mesaj.
    /// </returns>
    public static string? FindProblem(
        IReadOnlyList<EmployeeDutyInput> duties,
        IReadOnlyDictionary<Guid, EmployeeDutyRoleInfo> roles)
    {
        foreach (EmployeeDutyInput duty in duties)
        {
            if (duty.SigningRoleId is not Guid roleId
                || !roles.TryGetValue(roleId, out EmployeeDutyRoleInfo? role))
            {
                continue;
            }

            if (role.RequiresWorkshop && duty.WorkshopId is null)
            {
                return $"'{role.Name}' görevi bir atölyeye bağlıdır; atölye seçmelisiniz.";
            }

            if (!role.RequiresWorkshop && duty.WorkshopId is not null)
            {
                return $"'{role.Name}' kurum geneli bir görevdir; bu göreve atölye seçilemez.";
            }
        }

        return null;
    }

    /// <summary>
    /// Seçilen görevlerin gerçekten tanımlı ve atölye kuralına uygun olduğunu
    /// veritabanından doğrular. Kurallar görev tanımına bağlı olduğu için
    /// senkron kontroller yeterli değildir.
    /// </summary>
    /// <returns>
    /// Sorun yoksa <c>null</c>; aksi hâlde kullanıcıya gösterilecek mesaj.
    /// </returns>
    public static async Task<string?> FindProblemAsync(
        IReadOnlyList<EmployeeDutyInput> duties,
        IEmployeeSigningRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        List<Guid> roleIds = [.. duties
            .Where(d => d.SigningRoleId is not null)
            .Select(d => d.SigningRoleId!.Value)
            .Distinct()];

        if (roleIds.Count == 0)
        {
            return EmployeeMessages.UnselectedRole;
        }

        List<EmployeeSigningRole> roles = await roleRepository.GetAllIncludingDeletedAsync(cancellationToken);

        Dictionary<Guid, EmployeeDutyRoleInfo> byId = roles
            .Where(r => r.Id is not null && roleIds.Contains(r.Id.Value))
            .GroupBy(r => r.Id!.Value)
            .ToDictionary(
                g => g.Key,
                g => new EmployeeDutyRoleInfo(
                    g.Key,
                    g.First().Name?.Value ?? string.Empty,
                    g.First().RequiresWorkshop));

        // Silinmiş ya da adı okunamayan görev tanımı "bulunamadı" sayılır:
        // geçmiş belgelerde adı görünse de yeni bir görevlendirme bu göreve
        // verilemez.
        foreach (Guid roleId in roleIds)
        {
            if (!byId.TryGetValue(roleId, out EmployeeDutyRoleInfo? role)
                || string.IsNullOrWhiteSpace(role.Name))
            {
                return EmployeeMessages.UnknownRole;
            }
        }

        return FindProblem(duties, byId);
    }

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
