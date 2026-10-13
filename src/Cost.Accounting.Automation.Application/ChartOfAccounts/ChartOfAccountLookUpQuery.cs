using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

/// <summary>
/// Hesap planı lookup sonucunun bellek cache anahtarı ve ömrü.
///
/// Lookup tüm hesapları taşır (binlerce satır) ve form açılışlarının kritik
/// yolunda çağrılır (atölye/depo comboları). Aynı sonucu her form açılışında
/// yeniden okumak yerine kısa süreli belleğe alınır; hesap planını değiştiren
/// komutlar <see cref="Invalidate"/> ile önbelleği hemen boşaltır.
/// </summary>
public static class ChartOfAccountLookUpCache
{
    public const string Key = "chartofaccounts:lookup:v1";

    /// <summary>
    /// Önbellek ömrü. Değişiklik komutları anında temizler; ayrıca giriş
    /// sırasında arka planda ısıtılır. Uzun tutulmasının nedeni, tam-tablo
    /// okumasının form açılışının kritik yoluna girmemesi ve SQL Server'daki
    /// kısa donmaların her açılışta tekrar yaşanmamasıdır.
    /// </summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    public static void Invalidate(IMemoryCache cache) => cache.Remove(Key);
}

public sealed class ChartOfAccountLookUpDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ChartOfAccountType Type { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? SemiFinishedAccountId { get; set; }
    public Guid? FinishedAccountId { get; set; }

    public string Display => $"{Code} - {Name}";
}

[Permission("chartofaccount:view")]
public sealed record ChartOfAccountLookUpQuery : IRequest<Result<List<ChartOfAccountLookUpDto>>>;

internal sealed class ChartOfAccountLookUpQueryHandler(
    IChartOfAccountRepository chartOfAccountRepository,
    IMemoryCache cache) : IRequestHandler<ChartOfAccountLookUpQuery, Result<List<ChartOfAccountLookUpDto>>>
{
    public async Task<Result<List<ChartOfAccountLookUpDto>>> Handle(ChartOfAccountLookUpQuery request, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(ChartOfAccountLookUpCache.Key, out List<ChartOfAccountLookUpDto>? cached)
            && cached is not null)
        {
            return new List<ChartOfAccountLookUpDto>(cached);
        }

        List<ChartOfAccount> accounts = await chartOfAccountRepository.GetLookUpAsync(cancellationToken);

        List<ChartOfAccountLookUpDto> result = accounts
            .Where(a => !a.IsDeleted)
            .Select(a => new ChartOfAccountLookUpDto
            {
                Id = a.Id,
                Code = a.Code.Value,
                Name = a.Name.Value,
                Type = a.Type,
                ParentId = a.ParentId == null ? null : a.ParentId.Value,
                SemiFinishedAccountId = a.SemiFinishedAccountId == null ? null : a.SemiFinishedAccountId.Value,
                FinishedAccountId = a.FinishedAccountId == null ? null : a.FinishedAccountId.Value
            })
            .ToList();

        result.Sort(Comparer<ChartOfAccountLookUpDto>.Create(
            (a, b) => AccountCodeComparer.Instance.Compare(a.Code, b.Code)));

        cache.Set(ChartOfAccountLookUpCache.Key, result, ChartOfAccountLookUpCache.Ttl);

        return result;
    }
}