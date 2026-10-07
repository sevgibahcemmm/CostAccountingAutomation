namespace Cost.Accounting.Automation.Application.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Dosyaların yazıldığı kök klasör (ör. proje içindeki wwwroot). Tüm yollar buradan yönetilir.
    /// </summary>
    string RootPath { get; }

    /// <summary>
    /// Verilen byte içeriği kalıcı depoya yazar ve erişilebilir relative path döner.
    /// Yalnızca resim uzantılarına izin verir (profil fotoğrafı gibi görsel
    /// bekleyen yüklemeleri korur); resim olmayan uzantı .png'ye çevrilir.
    /// </summary>
    Task<string> SaveAsync(byte[] content, string fileName, string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen byte içeriği kalıcı depoya yazar; <paramref name="allowAnyExtension"/>
    /// <c>true</c> iken her tür uzantıya izin verir (sohbet ekleri gibi genel
    /// dosya gönderimlerinde kullanılır). Yine de yol/dosya sistemini bozabilecek
    /// karakterler temizlenir. Resim olmayan uzantılar bu kipte korunur.
    /// </summary>
    Task<string> SaveAsync(
        byte[] content,
        string fileName,
        string folder,
        bool allowAnyExtension,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Depodaki relative yolu (ör. "ProductImages\\x.png") tam fiziksel yola çevirir.
    /// </summary>
    string GetFullPath(string relativePath);
}