using System.Security.Cryptography;

namespace Cost.Accounting.Automation.Domain.Updates;

/// <summary>
/// Master veritabanında tutulan uygulama sürüm kaydı. İndirilebilir sürümler
/// kurulum dosyasını (<see cref="FileContent"/>) taşır; yalnızca sürüm
/// numarasını temsil eden "baseline" kayıtların içeriği boştur.
/// </summary>
public sealed class AppRelease
{
    private AppRelease()
    {
    }

    private AppRelease(string version, long versionSort, string? notes, bool isMandatory)
    {
        Id = Guid.CreateVersion7();
        Version = version;
        VersionSort = versionSort;
        Notes = notes;
        IsMandatory = isMandatory;
    }

    public Guid Id { get; private set; }

    public string Version { get; private set; } = string.Empty;

    /// <summary>
    /// Sürümlerin doğru sıralanması için hesaplanan 64 bit anahtar. Sürüm
    /// numarası metinsel olarak karşılaştırıldığında "1.0.0.10" &lt; "1.0.0.9"
    /// sonucu doğurur; bu alan bunu engeller.
    /// </summary>
    public long VersionSort { get; private set; }

    public string? Notes { get; private set; }

    public bool IsMandatory { get; private set; }

    /// <summary>İndirilebilir bir yayın mı, yoksa yalnızca sürüm işareti mi.</summary>
    public bool IsPublished { get; private set; }

    public string? FileName { get; private set; }

    public long FileSizeBytes { get; private set; }

    public string? Sha256 { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public byte[]? FileContent { get; private set; }

    /// <summary>
    /// Kurulu sürümü temsil eden, indirilebilir içeriği olmayan kayıt üretir.
    /// İlk kurulumda çağrılır ki bir sonraki derleme doğru sürümden +1 artsın.
    /// </summary>
    public static AppRelease CreateBaseline(string version, long versionSort)
        => new(version, versionSort, notes: null, isMandatory: false);

    public void Publish(string fileName, byte[] content, string? notes, bool isMandatory, long versionSort)
    {
        VersionSort = versionSort;
        Notes = notes;
        IsMandatory = isMandatory;
        FileName = fileName;
        FileContent = content;
        FileSizeBytes = content.LongLength;
        Sha256 = Convert.ToHexString(SHA256.HashData(content));
        IsPublished = true;
        PublishedAt = DateTimeOffset.Now;
    }

    /// <summary>
    /// Kurulum dosyasını tablodan temizler ama sürüm kaydını silmez. Yalnızca
    /// en güncel sürümün dosyası saklanır; eski yayınların ikilisi bu metotla
    /// atılır ve tablo şişmez.
    /// </summary>
    public void ClearContent()
    {
        FileContent = null;
        FileName = null;
        FileSizeBytes = 0;
        Sha256 = null;
        IsPublished = false;
    }
}
