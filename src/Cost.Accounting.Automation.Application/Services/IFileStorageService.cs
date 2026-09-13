namespace Cost.Accounting.Automation.Application.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Dosyaların yazıldığı kök klasör (ör. proje içindeki wwwroot). Tüm yollar buradan yönetilir.
    /// </summary>
    string RootPath { get; }

    /// <summary>
    /// Verilen byte içeriği kalıcı depoya yazar ve erişilebilir relative path döner.
    /// </summary>
    Task<string> SaveAsync(byte[] content, string fileName, string folder, CancellationToken cancellationToken = default);

    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Depodaki relative yolu (ör. "ProductImages\\x.png") tam fiziksel yola çevirir.
    /// </summary>
    string GetFullPath(string relativePath);
}