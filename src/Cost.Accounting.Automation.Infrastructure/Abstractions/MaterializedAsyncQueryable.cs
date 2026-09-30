using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Cost.Accounting.Automation.Infrastructure.Abstractions;

/// <summary>
/// Zaten bellekte materyalize edilmis bir listeyi hem senkron LINQ hem de
/// <c>ToListAsync</c> gibi asenkron terminal operatorlerle calisabilir hale getiren
/// sarmalayici.
///
/// Neden gerekiyor: <c>AuditableRepository</c> denetim kullanici adlarini master
/// veritabanindan okuyup DTO listesini bellekte kuruyor. Donus tipi yine de
/// <see cref="IQueryable{T}"/> olmak zorunda; cagiran taraf uzerinde
/// <c>Where</c>/<c>OrderBy</c>/<c>Select</c> kurup <c>ToListAsync()</c> cagriyor.
///
/// Uzerine operator kuruldugunda LINQ mutlaka <see cref="IQueryProvider.CreateQuery{TElement}"/>
/// cagirir ve agacin kokundeki <c>Expression</c> sabitini kullanir. Bu yuzden
/// <c>Expression</c> gercek bir <c>EnumerableQuery&lt;T&gt;</c> sabitine baglanir ve
/// <see cref="Provider"/> dogrudan onun standart saglayicisini dondurur. Boylece
/// ifade agaci framework'un kendi yolundan gecer; sonuc yine de asenkron
/// enumerasyon destekleyen bir sarmalayiciya sarilir, boylece bir sonraki
/// <c>ToListAsync()</c> da calisir.
/// </summary>
internal sealed class MaterializedAsyncQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>, IOrderedQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public MaterializedAsyncQueryable(IReadOnlyList<T> source)
    {
        // AsQueryable() bir EnumerableQuery<T> uretir; Expression'i ve Provider'i
        // standart LINQ-to-Objects zincirinden gelir.
        _inner = (source ?? Array.Empty<T>()).AsQueryable();
        Expression = _inner.Expression;
        Provider = new MaterializedAsyncQueryProvider(_inner);
        ElementType = typeof(T);
    }

    private MaterializedAsyncQueryable(IQueryable<T> inner)
    {
        _inner = inner;
        Expression = inner.Expression;
        // Provider daima kendi sarmalayicimiz olmali. inner.Provider dogrus
        // verilseydi bir sonraki Take/Skip arasi zincir sarmalayiciyi atlayip
        // duz EnumerableQuery dondurecek ve ToListAsync "IAsyncEnumerable
        // uygulanmiyor" hatasi verirdi.
        Provider = new MaterializedAsyncQueryProvider(inner);
        ElementType = typeof(T);
    }

    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (T item in _inner)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private sealed class MaterializedAsyncQueryProvider(IQueryable<T> inner)
        : IQueryProvider, IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
            => inner.Provider.CreateQuery(expression);

        // Operator sonucu standart EnumerableQuery olarak uretilir, sonra yeniden
        // sarmalanir: boylece zincirleme (Where -> OrderBy -> ThenBy -> ToListAsync)
        // boyle degismeden surdurulur.
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new MaterializedAsyncQueryable<TElement>(inner.Provider.CreateQuery<TElement>(expression));

        public object? Execute(Expression expression) => inner.Provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => inner.Provider.Execute<TResult>(expression);

        /// <summary>
        /// EF Core'un asenkron terminal operatorleri (<c>CountAsync</c>,
        /// <c>AnyAsync</c>, <c>FirstOrDefaultAsync</c>, <c>SumAsync</c> vb.)
        /// <c>Provider</c> uzerinde <c>IAsyncQueryProvider</c> arar; saglanmazsa
        /// "doesn't implement IAsyncQueryProvider" hatasi firlatir.
        ///
        /// Veri zaten bellekte materyalize edildigi icin ifade agaci senkron
        /// calistirilip sonuc <see cref="Task{TResult}"/> ya da
        /// <see cref="ValueTask{TResult}"/> icine sarilir.
        /// </summary>
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            Type resultType = typeof(TResult);

            if (!resultType.IsGenericType
                || (resultType.GetGenericTypeDefinition() != typeof(Task<>)
                    && resultType.GetGenericTypeDefinition() != typeof(ValueTask<>)))
            {
                return Execute<TResult>(expression);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Type valueType = resultType.GetGenericArguments()[0];

            // Terminal operator ifadesi (orn. Queryable.Count(...)) dogrudan
            // calistirilir; sonuc boxed olarak doner.
            object? value = inner.Provider.Execute(expression);

            if (resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return (TResult)Activator.CreateInstance(resultType, value)!;
            }

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(valueType)
                .Invoke(null, [value])!;
        }
    }
}
