namespace Cost.Accounting.Automation.Application.Deletion;

/// <summary>
/// Silme engeli ve uyarı metinlerinin tek kaynağı.
///
/// <para>
/// "hareket gördüğü için silinemez" uyarısı hem liste ekranlarının ön
/// kontrolünde hem de silme komutlarının içinde üretilir. Metinleri burada
/// toplamak, program genelinde aynı ifadenin kullanılmasını ve ileride
/// değiştirildiğinde tüm ekranların birlikte güncellenmesini sağlar.
/// </para>
/// </summary>
public static class DeletionMessages
{
    /// <summary>Onay penceresinde gösterilecek en fazla kayıt sayısı.</summary>
    public const int PreviewLimit = 3;

    /// <summary>Hareket görmüş kayıtlar için engelleme mesajı.</summary>
    public static string MovementBlocked(string entityLabel, int count)
        => $"{count} {entityLabel} hareket/işlem gördüğü için silinemez.";

    /// <summary>
    /// Hareket görmüş kayıtlar için, engellenen kayıtların okunabilir
    /// adlarını içeren engelleme mesajı.
    /// </summary>
    public static string MovementBlocked(
        string entityLabel,
        int count,
        IEnumerable<string> labels)
        => $"{MovementBlocked(entityLabel, count)} {BuildPreview(labels)}";

    /// <summary>
    /// Silme gerçekleşti ancak ilişkili kayıtlarda kullanıldığı için uyarı
    /// gereken durumdaki mesaj.
    /// </summary>
    public static string RelatedWarning(string entityLabel, int deletedCount, int relatedCount)
        => $"{deletedCount} {entityLabel} silindi. NOT: ilişkili {relatedCount} kayıtta kullanıldığı için "
           + "ilgili kayıtların gözden geçirilmesi gerekir.";

    /// <summary>Toplu silme başarı mesajı.</summary>
    public static string Deleted(int deletedCount, string entityLabel)
        => $"{deletedCount} {entityLabel} silindi.";

    /// <summary>Silinecek kayıt seçilmediğinde gösterilen mesaj.</summary>
    public static string NoSelection(string entityLabel)
        => $"Silinecek {entityLabel} seçilmedi.";

    /// <summary>
    /// Uzun listelerde okunabilirliği korumak için ilk birkaç kayıt adını
    /// gösterip kalanı sayı olarak özetler.
    /// </summary>
    public static string BuildPreview(IEnumerable<string> labels)
    {
        List<string> list = labels.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (list.Count == 0)
        {
            return string.Empty;
        }

        string preview = string.Join(", ", list.Take(PreviewLimit));
        if (list.Count > PreviewLimit)
        {
            preview += $" ve {list.Count - PreviewLimit} kayıt daha";
        }

        return preview.TrimEnd();
    }
}