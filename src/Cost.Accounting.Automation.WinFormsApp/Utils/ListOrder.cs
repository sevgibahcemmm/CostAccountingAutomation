namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Liste satırlarının varsayılan sırasını belirleyen ortak kurallar.
/// </summary>
public static class ListOrder
{
    /// <summary>
    /// Belge listeleri için istenen sıra: en yeni belge üstte, en eski altta.
    /// </summary>
    /// <param name="items">Sıralanacak satırlar.</param>
    /// <param name="documentDate">Belgenin kendi tarihi (tarih alanı olan her listede vardır).</param>
    /// <param name="createdAt">Kaydın sistemde oluşturulma zamanı.</param>
    /// <param name="id">Kayıt kimliği; son çare sıralama anahtarı.</param>
    /// <remarks>
    /// <para>
    /// Sıralama <b>belge tarihine</b> göre yapılır, kayıt zamanına göre
    /// değil. Kullanıcı bugünün tarihiyle geçmiş tarihli bir belge kaydederse
    /// (geçmişe dönük fatura/stok çıkışı) belge, kaydedildiği gün en yeni
    /// olduğu için <i>yanlışlıkla</i> listenin en üstünde görünürdü.
    /// </para>
    /// <para>
    /// Aynı tarihli belgelerde <b>kaydedilme sırası korunur</b>: o gün önce
    /// giren kayıt üstte, sonra giren altta. Yani "ilk giren altta kalır"
    /// kuralı, belgeler arasında tarih, aynı gün içinde ise saat sırasıyla
    /// uygulanır.
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