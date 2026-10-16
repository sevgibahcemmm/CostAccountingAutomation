using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Domain.DirectProcurement;

/// <summary>
/// Doğrudan temin dosyaları için veri erişim sözleşmesi.
/// </summary>
public interface IDirectProcurementFileRepository : IAuditableRepository<DirectProcurementFile>
{
    /// <summary>
    /// Silme öncesi hareket denetimi. Bir dosyanın altında teklif, kazanan
    /// firma, sözleşme veya bütçe kaydı varsa dosya silinemez; ekran bu
    /// sonucu kullanıcıya gösterir.
    /// </summary>
    Task<DeletionCheck> GetDeletionCheckAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ana kayıtla birlikte tüm alt kayıtları getirir. Düzenleme ekranı
    /// ihtiyaç listesi, teklifler, görevliler ve bütçe seçimlerini tek
    /// çağrıda okur; ayrı ayrı sorgulamak kartesian çarpımına ve N+1'e
    /// yol açardı.
    /// </summary>
    Task<DirectProcurementFile?> GetByIdWithDetailsAsync(
        IdentityId id,
        CancellationToken cancellationToken = default);
}