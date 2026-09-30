namespace Cost.Accounting.Automation.Application.Devirs;

/// <summary>
/// Devir (açılış bakiyesi aktarımı) işlemini yürüten servis. Uygulama katmanı
/// yıl veritabanının hangisi olduğunu bilmediği için bu yetenek bir port
/// olarak tanımlanır; uygulaması altyapı katmanındadır.
/// </summary>
public interface IDevirTransferService
{
    /// <summary>
    /// Hedef yıl için devrin çalıştırılıp çalıştırılamayacağını ve kaynak
    /// yıldan neyin aktarılacağını hesaplar. Hiçbir şey yazmaz.
    /// </summary>
    Task<DevirPreviewResult> BuildPreviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaynak mali yılın seçili veri gruplarını hedef yıl veritabanına yazar.
    /// </summary>
    Task<DevirTransferResult> TransferAsync(
        DevirOptions options,
        CancellationToken cancellationToken = default);
}
