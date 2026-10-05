namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Liste satırlarının varsayılan sırasını belirleyen ortak kurallar.
/// </summary>
public static class ListOrder
{
    /// <summary>
    /// Stok hareketi listesi için sıra: en yeni hareket üstte, en eski altta.
    /// </summary>
    /// <param name="items">Sıralanacak satırlar.</param>
    /// <param name="documentDate">Hareketin kendi tarihi.</param>
    /// <param name="createdAt">Kaydın sistemde oluşturulma zamanı.</param>
    /// <param name="id">Kayıt kimliği; son çare sıralama anahtarı.</param>
    /// <remarks>
    /// <para>
    /// Genel liste kuralı <b>kayıt tarihine</b> göredir
    /// (<c>CrudListFormBase.ApplyDefaultOrder</c>). Bu metot o kuralın tek
    /// istisnasıdır ve yalnızca <b>stok hareketleri</b> listesinde kullanılır.
    /// </para>
    /// <para>
    /// Gerekçe: FIFO'da tüketim sırası <b>hareket tarihine</b> göre kurulur.
    /// Kullanıcı hareket listesini denetlerken gördüğü sıra ile malın
    /// tüketileceği sıra aynı olmalıdır; aksi halde liste, katman kırılımının
    /// gerçekten hangi girişten yapıldığını göstermez. Bu yüzden kayıt
    /// zamanına göre sıralama burada yanlış okuma yaratır.
    /// </para>
    /// <para>
    /// Aynı tarihli hareketlerde <b>kaydedilme sırası korunur</b>: o gün önce
    /// giren kayıt üstte, sonra giren altta.
    /// </para>
    /// </remarks>
    public static IEnumerable<T> NewestDocumentFirst<T>(
        IEnumerable<T> items,
        Func<T, DateOnly> documentDate,
        Func<T, DateTimeOffset> createdAt,
        Func<T, Guid> id)
        => items
            .OrderByDescending(documentDate)
            .ThenBy(createdAt)
            .ThenBy(id);
}