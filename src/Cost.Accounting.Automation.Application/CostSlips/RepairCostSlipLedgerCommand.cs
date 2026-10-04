using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Domain.Products;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

/// <summary>Onarımın etkilediği kayıt sayıları.</summary>
/// <param name="SlipCount">Taslağa düşürülen maliyet pusulası sayısı.</param>
/// <param name="MovementCount">Yumuşak silinen stok hareketi sayısı.</param>
/// <param name="LedgerCount">Yumuşak silinen yevmiye kaydı sayısı.</param>
/// <param name="Details">Kullanıcıya gösterilen özet satırlar.</param>
public sealed record CostSlipLedgerRepairResult(
    int SlipCount,
    int MovementCount,
    int LedgerCount,
    IReadOnlyList<string> Details);

/// <summary>
/// Tek taraflı yazılmış maliyet pusulası yan etkilerini geri alır.
///
/// <para>
/// <b>Neden gerekiyor?</b> Maliyet pusulası onayı, malzeme tüketimi için
/// yalnızca ALACAK yazıyordu ve üretilen ürün için hiç yevmiye kaydı
/// üretmiyordu. Böylece 15x stok hesapları tüketim alacaklarıyla birikiyor,
/// üretim borcu gelmediği için gerçek stok değerini gösteremiyor ve yevmiye
/// borç/alacak toplamı eşit olmuyordu.
/// </para>
///
/// <para>
/// Bu komut <b>yalnızca maliyet pusulası kaynaklı</b> kayıtlara dokunur.
/// İrsaliye, stok çıkışı ve atölye transferi kayıtlarına dokunulmaz; onların
/// yevmiye kayıtları farklı kaynak tipleriyle (<c>StokTuketimi</c>,
/// <c>AtolyeTransferi</c>, <c>SatinalmaFaturasi</c>) yazılmıştır ve
/// <see cref="ConsumptionSourceType"/> filtresi bunları ayırır.
/// </para>
///
/// <para>
/// <b>Sıra önemli:</b> pusulalar yalnızca yan etkileri geri alındıktan sonra
/// taslağa döner. Aksi hâlde pusula yeniden onaylandığında aynı stok hareketleri
/// ikinci kez yazılır.
/// </para>
///
/// <para>
/// <b>Güvenlik:</b> varsayılan <c>DryRun = true</c> — hiçbir değişiklik
/// yapılmaz, yalnızca ne silineceğini raporlar. Tüm yazmalar tek
/// transaction'dadır.
/// </para>
/// </summary>
[Permission("costslip:manage")]
public sealed record RepairCostSlipLedgerCommand(bool DryRun = true)
    : IRequest<Result<CostSlipLedgerRepairResult>>;

internal sealed class RepairCostSlipLedgerCommandHandler(
    ICostSlipRepository costSlipRepository,
    IProductMovementRepository productMovementRepository,
    IChartOfAccountLedgerRepository ledgerRepository)
    : IRequestHandler<RepairCostSlipLedgerCommand, Result<CostSlipLedgerRepairResult>>
{
    /// <summary>
    /// CostSlipStockHelper'ın malzeme tüketimi yevmiyesinde kullandığı kaynak
    /// tipi. Diğer kaynak tipleri (stok çıkışı, transfer, irsaliye) onarımın
    /// kapsamı dışındadır.
    /// </summary>
    private const string ConsumptionSourceType = "MaliyetTuketimi";

    public async Task<Result<CostSlipLedgerRepairResult>> Handle(
        RepairCostSlipLedgerCommand request,
        CancellationToken cancellationToken)
    {
        var details = new List<string>();

        // Yalnızca onaylı pusulalar ele alınır: yan etkileri onayda yazılır.
        List<CostSlip> slips = await costSlipRepository.GetAll()
            .Include(s => s.CostSlipItems)
            .Where(s => !s.IsDeleted && s.Status == CostSlipStatus.Approved)
            .ToListAsync(cancellationToken);

        if (slips.Count == 0)
        {
            details.Add("Onaylı maliyet pusulası bulunamadı; onarım için işlem yapılmadı.");
            return Result<CostSlipLedgerRepairResult>.Succeed(
                new CostSlipLedgerRepairResult(0, 0, 0, details));
        }

        List<string> slipNumbers = slips.Select(s => s.SlipNumber).Distinct().ToList();
        details.Add($"{slips.Count} onaylı maliyet pusulası: {string.Join(", ", slipNumbers)}");

        // Maliyet pusulasının yarattığı stok hareketleri referans numarasıyla
        // (pusula numarası) eşleşir. Stok çıkışı belgeleri de ürün hareketi
        // üretir ancak onların referans numarası stok belgesi numarasıdır ve
        // DocumentNumber alanında tutulur; ayrıca aşağıdaki ürün filtresi de
        // yalnızca bu pusulaların ürettiği/ tükettiği ürünleri kapsar.
        HashSet<IdentityId> productIds = slips
            .SelectMany(s => s.CostSlipItems)
            .Where(i => i.ProductId is not null)
            .Select(i => i.ProductId!)
            .Concat(slips
                .Where(s => s.ProducedProductId is not null)
                .Select(s => s.ProducedProductId!))
            .ToHashSet();

        List<ProductMovement> movements = await productMovementRepository.GetAll()
            .Where(m => !m.IsDeleted
                && productIds.Contains(m.ProductId)
                && m.ReferenceNo != null
                && slipNumbers.Contains(m.ReferenceNo))
            .ToListAsync(cancellationToken);

        details.Add($"{movements.Count} maliyet pusulası stok hareketi bulundu.");

        // Yevmiye: her hareketin kaynak kimliği (movement id) üzerinden
        // çekilir. Tek taraflı eski kayıt tek taraflıydı; yeni kayıt iki
        // satırdır (borç + alacak) ve ikisi de aynı SourceId'yi taşır, bu
        // yüzden ikisi birden geri alınır.
        var ledgerEntries = new List<ChartOfAccountLedger>();
        foreach (ProductMovement movement in movements)
        {
            List<ChartOfAccountLedger> found = await ledgerRepository.GetBySourceAsync(
                ConsumptionSourceType, movement.Id.Value, cancellationToken);

            ledgerEntries.AddRange(found);
        }

        // Aynı kaynak kimliği birden fazla kez dönebilir (aynı hareket için
        // birden çok yevmiye satırı); kimliğe göre tekilleştirilir.
        List<ChartOfAccountLedger> distinctEntries =
            [.. ledgerEntries.GroupBy(e => e.Id.Value).Select(g => g.First())];

        details.Add($"{distinctEntries.Count} maliyet pusulası yevmiye kaydı bulundu.");

        if (request.DryRun)
        {
            details.Add("DRY RUN: hiçbir değişiklik yapılmadı. Uygulamak için DryRun: false gönderin.");
            return Result<CostSlipLedgerRepairResult>.Succeed(
                new CostSlipLedgerRepairResult(slips.Count, movements.Count, distinctEntries.Count, details));
        }

        // Sıra: önce hareketler, sonra yevmiye, en son pusula durumu.
        productMovementRepository.SoftDeleteRange(movements);
        ledgerRepository.SoftDeleteRange(distinctEntries);
        costSlipRepository.UpdateRange(slips);

        foreach (CostSlip slip in slips)
        {
            slip.ReturnToDraft();
        }

        details.Add($"{slips.Count} pusula taslağa döndürüldü.");
        details.Add(
            "Stok hareketleri ve yevmiye kayıtları yumuşak silindi (Silinenler listesinden geri alınabilir). "
            + "Pusulaları düzeltilmiş kodla yeniden onaylayın.");

        return Result<CostSlipLedgerRepairResult>.Succeed(
            new CostSlipLedgerRepairResult(slips.Count, movements.Count, distinctEntries.Count, details));
    }
}