namespace Cost.Accounting.Automation.WinFormsApp.Utils;

public static class RolePermissionLabels
{
    private static readonly Dictionary<string, string> GroupCaptions = new()
    {
        ["dashboard"] = "Panel",
        ["user"] = "Kullanıcı",
        ["role"] = "Rol",
        ["company"] = "Şirket",
        ["customer"] = "Müşteri",
        ["supplier"] = "Tedarikçi",
        ["chartofaccount"] = "Hesap Planı",
        ["permission"] = "Yetki Yönetimi",
    };

    private static readonly Dictionary<string, string> ActionTexts = new()
    {
        ["view"] = "Görüntüle",
        ["create"] = "Oluştur",
        ["update"] = "Güncelle",
        ["edit"] = "Düzenle",
        ["delete"] = "Sil",
        ["restore"] = "Geri Yükle",
        ["import"] = "İçe Aktar",
        ["update_permissions"] = "Yetkileri Güncelle",
    };

    public static string GetGroup(string permission)
    {
        int separator = permission.IndexOf(':');
        return separator > 0 ? permission[..separator] : permission;
    }

    public static string GetGroupCaption(string group)
        => GroupCaptions.TryGetValue(group, out string? caption)
            ? caption
            : char.ToUpperInvariant(group[0]) + group[1..];

    public static int GroupOrder(string group) => GroupCaptions.ContainsKey(group) ? 0 : 1;

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