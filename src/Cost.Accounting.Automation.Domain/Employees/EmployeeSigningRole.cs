using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Employees;

/// <summary>
/// Bir belgenin imza bölümünü dolduran yetkili görev tanımı.
///
/// Raporların imza alanları bu görevler üzerinden çözülür: her imza yuvası için
/// bir <see cref="EmployeeDuty"/> kaydı aranır ve o kaydın personeli imza
/// satırına adı soyadı ve ünvanı ile basılır.
///
/// <para>
/// Görevler enum değil, veritabanı tablosudur. Böylece kurumun imza
/// bloklarında ihtiyaç duyduğu yeni bir görev (örn. "Sayım Kontrol Sorumlusu")
/// kod değiştirilmeden tanımlanabilir. Kayıt yıl veritabanında
/// <c>EmployeeSigningRoles</c> tablosunda tutulur; görev geçmişi tutulmaz,
/// çünkü raporlar yalnızca güncel yetkiliyi gösterir.
/// </para>
/// </summary>
public sealed class EmployeeSigningRole : Entity
{
    private EmployeeSigningRole()
    {
    }

    public EmployeeSigningRole(
        Name name,
        string description,
        bool requiresWorkshop,
        int sortOrder)
    {
        SetName(name);
        SetDescription(description);
        SetRequiresWorkshop(requiresWorkshop);
        SetSortOrder(sortOrder);
        ResolveDuplicateKey();
    }

    /// <summary>Görevin seçim kutularında ve raporlarda görünen adı.</summary>
    public Name Name { get; private set; } = default!;

    /// <summary>Görevin ne anlama geldiğini anlatan açıklama.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Bu görev bir atölyeye bağlı mı? <c>true</c> ise görevlendirme
    /// kaydında atölye seçilmesi zorunludur ("Atölye Şefi" gibi).
    /// Kurum geneli görevlerde <c>false</c>'tır ve atölye seçilmez.
    /// </summary>
    public bool RequiresWorkshop { get; private set; }

    /// <summary>Seçim kutusundaki sıra; rapor imza bloklarının okunabilirliği için.</summary>
    public int SortOrder { get; private set; }

    public static string? BuildDuplicateKey(string name)
        => DuplicateKeyRule.From(name);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(Name.Value));

    public void SetName(Name name)
    {
        Name = name;
        ResolveDuplicateKey();
    }

    public void SetDescription(string description) => Description = description;

    public void SetRequiresWorkshop(bool requiresWorkshop)
        => RequiresWorkshop = requiresWorkshop;

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
