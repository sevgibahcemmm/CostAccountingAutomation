using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Roles;
public sealed class Role : Entity, IAggregate
{
    private readonly List<Permission> _permissions = new();
    private Role() { }

    public Role(Name name, bool isActive)
    {
        SetName(name);
        SetStatus(isActive);
    }
    public Name Name { get; private set; } = default!;
    public IReadOnlyCollection<Permission> Permissions => _permissions;

    public static string? BuildDuplicateKey(string name)
        => DuplicateKeyRule.From(name);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(Name.Value));

    #region Behaviors
    public void SetName(Name name)
    {
        Name = name;
        ResolveDuplicateKey();
    }

    /// <summary>
    /// İzin listesini verilen küme eşit olacak şekilde günceller.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Uyarı: silip baştan ekleme yapılmaz.</b> <c>Permission</c>, rolün
    /// sahip olduğu bir tablo-ağılmış (owned) koleksiyondur ve her birinin veritabanı
    /// anahtarı EF tarafından gölge özellik olarak üretilir. Koleksiyon tamamen
    /// yeniden oluşturulursa yeni örnekler izlenmez, gölge anahtarları bilinmez ve
    /// kaydetme şu hatayla düşer:
    /// </para>
    /// <code>
    /// The value of shadow key property 'Permission.Id' is unknown when
    /// attempting to save changes.
    /// </code>
    /// <para>
    /// Bu yüzden **var olan örnekler korunur**; yalnızca gerçekten yeni olanlar
    /// eklenir ve istenmeyenler çıkarılır. Bu sayede rol düzenleme ekranı ile
    /// yetki tamamlama servisleri aynı metodu güvenle kullanabilir.
    /// </para>
    /// </remarks>
    public void SetPermissions(IEnumerable<Permission> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        string[] wanted = permissions
            .Select(p => p.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var wantedSet = new HashSet<string>(wanted, StringComparer.OrdinalIgnoreCase);

        _permissions.RemoveAll(p => !wantedSet.Contains(p.Value));

        var existing = new HashSet<string>(
            _permissions.Select(p => p.Value),
            StringComparer.OrdinalIgnoreCase);

        foreach (string value in wanted)
        {
            if (existing.Add(value))
            {
                _permissions.Add(new Permission(value));
            }
        }
    }
    #endregion
}

public sealed record Permission(string Value);