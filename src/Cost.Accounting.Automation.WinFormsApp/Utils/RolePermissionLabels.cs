namespace Cost.Accounting.Automation.WinFormsApp.Utils;

public static class RolePermissionLabels
{
    /// <summary>
    /// İzin gruplarının görünüm sırası ve başlıkları.
    /// Sıra burada açıkça tanımlanır: aksi halde gruplar
    /// <c>PermissionService.GetAll</c> içindeki <see cref="HashSet{T}"/>
    /// sırasına göre kararsız biçimde dizilir.
    /// Sıra <c>PermissionService.GetAll</c> ile üretilen gerçek izin
    /// kataloğuyla eşleşmelidir.
    /// </summary>
    private static readonly (string Key, string Caption)[] Groups =
    [
        ("dashboard", "Panel"),
        ("permission", "Yetki Yönetimi"),
        ("user", "Kullanıcı"),
        ("role", "Rol"),
        ("company", "Şirket"),
        ("customer", "Müşteri"),
        ("supplier", "Tedarikçi"),
        ("chartofaccount", "Hesap Planı"),
        ("devir", "Devir"),
        ("current_account_movement", "Cari Hesap Hareketi"),
        ("product", "Ürün"),
        ("recipe", "Reçete"),
        ("stock_movement", "Stok Hareketi"),
        ("stockmovement", "Stok Hareket Raporu"),
        ("stock_issue", "Stok Fişi"),
        ("invoice", "Fatura"),
        ("costslip", "Maliyet Pusulası"),
        ("message", "Mesajlaşma"),
    ];

    private static readonly Dictionary<string, string> GroupCaptions =
        Groups.ToDictionary(g => g.Key, g => g.Caption, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, int> GroupIndexes =
        Groups.Select((g, i) => (g.Key, Index: i))
            .ToDictionary(g => g.Key, g => g.Index, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> ActionTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["view"] = "Görüntüle",
        ["create"] = "Oluştur",
        ["update"] = "Güncelle",
        ["edit"] = "Düzenle",
        ["delete"] = "Sil",
        ["restore"] = "Geri Yükle",
        ["approve"] = "Onayla",
        ["import"] = "İçe Aktar",
        ["manage"] = "Yönet",
        ["update_permissions"] = "Yetkileri Güncelle",
        ["reset_password"] = "Şifre Sıfırlama Kodu Üret",
        ["send"] = "Mesaj Gönder",
        ["announce"] = "Duyuru Gönder",
    };

    public static string GetGroup(string? permission)
    {
        if (string.IsNullOrEmpty(permission))
        {
            return string.Empty;
        }

        int separator = permission.IndexOf(':');
        return separator > 0 ? permission[..separator] : permission;
    }

    public static string GetGroupCaption(string? group)
    {
        if (string.IsNullOrEmpty(group))
        {
            return string.Empty;
        }

        return GroupCaptions.TryGetValue(group, out string? caption)
            ? caption
            : group;
    }

    /// <summary>
    /// Tanımlı grupları liste sırasına göre, tanımsız grupları en sona alır.
    /// </summary>
    public static int GroupOrder(string? group)
        => group is not null && GroupIndexes.TryGetValue(group, out int index)
            ? index
            : int.MaxValue;

    public static string GetLabel(string? permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return string.Empty;
        }

        int separator = permission.IndexOf(':');
        string action = separator > 0 ? permission[(separator + 1)..] : permission;
        string actionText = ActionTexts.TryGetValue(action, out string? text) ? text : action;
        return $"{actionText} ({permission})";
    }
}
