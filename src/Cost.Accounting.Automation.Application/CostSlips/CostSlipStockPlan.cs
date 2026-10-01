using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Application.CostSlips;

/// <summary>
/// Bir maliyet pusulasının onaylanırken üreteceği stok ve defter hareketleri.
/// Henüz KAYDEDILMEMİŞTİR; yalnızca hesaplanmıştır.
/// </summary>
/// <param name="Movements">Üretilecek ürün hareketleri (çıkış + üretim girişi).</param>
/// <param name="LedgerEntries">Üretilecek yevmiye kayıtları.</param>
internal sealed record CostSlipStockPlan(
    IReadOnlyList<Domain.Products.ProductMovement> Movements,
    IReadOnlyList<PlannedLedgerEntry> LedgerEntries);

/// <summary>
/// Henüz yazılmamış bir yevmiye kaydı. <see cref="ChartOfAccounts.IChartOfAccountLedgerPoster"/>
/// yalnızca <c>AddAsync</c> çağırdığı için, kayıtlar aynı DbContext'te birikerek
/// tek bir <c>SaveChangesAsync</c> ile tek transaction içinde yazılır.
/// </summary>
internal sealed record PlannedLedgerEntry(
    IdentityId AccountId,
    decimal Debit,
    decimal Credit,
    string SourceType,
    IdentityId? SourceId);