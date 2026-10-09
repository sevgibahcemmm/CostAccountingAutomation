using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.AppReleases;

/// <summary>
/// Yayınlanmış bir uygulama sürümünün kaydı (Master veritabanında).
///
/// <para>
/// İstemci açılışta bu tablodan en güncel kaydı okur; sürüm kendinden yeniyse
/// <see cref="SetupPath"/> (ör. <c>\\sunucu\Paylaşım\CostAccountingAutomation-Setup-1.0.1.exe</c>)
/// üzerinden kurulum dosyasına ulaşır. Kayıt, sunucudaki <c>sync-updates.ps1</c>
/// betiği (ya da <c>caa-provision set-version</c>) tarafından yazılır.
/// </para>
/// </summary>
public sealed class AppRelease
{
    private AppRelease() { }

    public AppRelease(string version, string setupPath, string? notes, bool mandatory)
    {
        Id = new IdentityId(Guid.CreateVersion7());
        Version = version;
        SetupPath = setupPath;
        Notes = notes;
        Mandatory = mandatory;
        PublishedAt = DateTimeOffset.UtcNow;
        IsActive = true;
    }

    public IdentityId Id { get; private set; } = default!;
    public string Version { get; private set; } = string.Empty;
    public string SetupPath { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool Mandatory { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public bool IsActive { get; private set; }
}