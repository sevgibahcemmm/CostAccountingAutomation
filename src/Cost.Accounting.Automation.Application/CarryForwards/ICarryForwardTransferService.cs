namespace Cost.Accounting.Automation.Application.CarryForwards;

/// <summary>
/// Devir (açılış bakiyesi aktarımı) işlemini yürüten servis. Uygulama katmanı
/// yıl veritabanının hangisi olduğunu bilmediği için bu yetenek bir port
/// olarak tanımlanır; uygulaması altyapı katmanındadır.
/// </summary>
public interface ICarryForwardTransferService
{
    /// <summary>
    /// Hedef yıl için devrin çalıştırılıp çalıştırılamayacağını ve kaynak
    /// yıldan neyin aktarılacağını hesaplar. Hiçbir şey yazmaz.
    /// </summary>
    Task<CarryForwardPreviewResult> BuildPreviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaynak mali yılın seçili veri gruplarını hedef yıl veritabanına yazar.
    /// </summary>
    Task<CarryForwardTransferResult> TransferAsync(
        CarryForwardOptions options,
        CancellationToken cancellationToken = default);
}
