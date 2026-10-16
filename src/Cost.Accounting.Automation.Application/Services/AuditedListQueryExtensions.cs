using Cost.Accounting.Automation.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Denetimli liste sorgularını veritabanında çalıştırmak için ortak uzantılar.
///
/// <para>
/// <b>Desen:</b> bir liste DTO'su için yazılan <c>MapTo</c> projeksiyonu
/// (<c>.Select(s =&gt; new XDto { ... })</c>) yalnızca kaynak sorgu gerçek bir
/// SQL sorgusuysa veritabanında çalışır. Kaynak belleğe alınmış bir
/// <c>IQueryable</c> ise aynı ifade ağacı LINQ-to-Objects olarak değerlendirilir
/// ve <b>filtreler SQL'e hiç inmez</b>.
/// </para>
///
/// <para>
/// Bu uzantı, sorgunun veritabanında çalışmasını garanti eder ve denetim
/// kullanıcı adlarını (master katalogda olduğu için SQL'e katılamayan kısım)
/// sonradan doldurur.
/// </para>
/// </summary>
internal static class AuditedListQueryExtensions
{
    /// <summary>
    /// Veritabanında projekte edilmiş DTO listesini çeker ve denetim kullanıcı
    /// adlarını doldurur.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dönen değer bellek içi bir sorgudur: çağıran taraf (liste ekranı) DTO
    /// üzerinde sıralama/arama yapıyorsa bu iş bellekte olur. Filtreleme,
    /// sayfalama ve alan projeksiyonu ise <b>veritabanında</b> yapılmış olur.
    /// </para>
    /// <para>
    /// Kayıt sayısı büyüdükçe bellek içi sıralamanın maliyeti artacağı için
    /// liste ekranlarının büyük tablolarda sayfalaması gerekir.
    /// </para>
    /// </remarks>
    public static async Task<IQueryable<TDto>> ToAuditedListAsync<TDto>(
        this IQueryable<TDto> projectedQuery,
        IAuditUserNameResolver auditUserNames,
        CancellationToken cancellationToken)
        where TDto : EntityDto
    {
        List<TDto> items = await projectedQuery.ToListAsync(cancellationToken);

        await auditUserNames.FillAsync(items, cancellationToken);

        return items.AsQueryable();
    }
}
