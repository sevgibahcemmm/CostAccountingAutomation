namespace Cost.Accounting.Automation.Application.Updates;

/// <summary>
/// İstemciye sunulan, indirilebilir bir yayın sürümünün özeti. Kurulum
/// dosyasının kendisi bu nesnede taşınmaz; içerik ayrıca indirilir.
/// </summary>
public sealed record AppReleaseInfo(
    string Version,
    string? Notes,
    bool IsMandatory,
    long FileSizeBytes,
    DateTimeOffset? PublishedAt);

/// <summary>
/// Master veritabanındaki sürüm kayıtlarını yönetir. Hem istemci (güncelleme
/// kontrolü/indirme) hem de dağıtım araçları (caa-provision) bu servisi kullanır.
/// </summary>
public interface IAppReleaseService
{
    /// <summary>İçeriği (dosyası) hazır en güncel yayın sürümü; yoksa null.</summary>
    Task<AppReleaseInfo?> GetLatestPublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kayıtlı en yüksek sürüm (yayın veya baseline). Derleme sırasında "+1"
    /// artırmak için kullanılır. Hiç kayıt yoksa null.
    /// </summary>
    Task<string?> GetLatestVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>Belirtilen sürümün kurulum dosyası; bulunamazsa null.</summary>
    Task<byte[]?> GetContentAsync(string version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni (veya var olan) bir sürümü dosyasıyla birlikte yayınlar ve eski
    /// sürümlerin dosyalarını temizler.
    /// </summary>
    Task PublishAsync(
        string version,
        string fileName,
        byte[] content,
        string? notes,
        bool isMandatory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kurulu sürüm için içeriksiz bir kayıt yoksa oluşturur. İlk kurulumda
    /// çağrılır; böylece bir sonraki derleme doğru sürümden başlar.
    /// </summary>
    Task EnsureBaselineAsync(string version, CancellationToken cancellationToken = default);
}
