using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Cost.Accounting.Automation.Application.Helpers;

/// <summary>
/// Kaynak EF sağlayıcısına bağlıysa EF'in async operatörünü, bellek içi bir IQueryable ise
/// (ör. iki DbContext'in istemci tarafında birleştirildiği GetAllWithAudit) senkron karşılığını çalıştırır.
/// </summary>
internal static class QueryableAsyncExtensions
{
    public static Task<List<T>> ToListSafeAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
        => source.Provider is IAsyncQueryProvider
            ? source.ToListAsync(cancellationToken)
            : Task.FromResult(source.ToList());

    public static Task<T?> FirstOrDefaultSafeAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
        => source.Provider is IAsyncQueryProvider
            ? source.FirstOrDefaultAsync(cancellationToken)
            : Task.FromResult(source.FirstOrDefault());
}
