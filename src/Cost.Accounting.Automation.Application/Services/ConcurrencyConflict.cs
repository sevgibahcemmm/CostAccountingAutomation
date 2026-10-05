using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Bir kayıt üzerinde eşzamanlı yazma çakışması.
/// </summary>
/// <param name="EntityName">Çakışan kaydın tür adı (örn. <c>Invoice</c>).</param>
/// <param name="PrimaryKey">Kaydın birincil anahtarı.</param>
/// <param name="ChangedByUserName">
/// Kaydı değiştiren kullanıcının adı. Kullanıcı master veritabanında
/// bulunamazsa <see langword="null"/> olur ve mesaj kullanıcı adı yerine
/// "başka bir kullanıcı" der.
/// </param>
/// <param name="ChangedAt">Değişikliğin yapıldığı zaman.</param>
public sealed record ConcurrencyConflict(
    string EntityName,
    string PrimaryKey,
    string? ChangedByUserName,
    DateTimeOffset? ChangedAt);

/// <summary>
/// Eşzamanlılık çakışmalarını kullanıcıya gösterilecek tek bir mesaja dönüştürür.
/// </summary>
/// <remarks>
/// <para>
/// EF çakışmayı <c>DbUpdateConcurrencyException</c> ile bildirir. Ham istisna
/// kullanıcıya iletilirse ekranda SQL hatası görünür ve kullanıcı ne yapması
/// gerektiğini anlamaz. Bu arayüz, mesajı veritabanında zaten duran denetim
/// alanlarından (<c>UpdatedBy</c>, <c>UpdatedAt</c>) üretir; yalnızca kullanıcı
/// adını master veritabanından çözerken bir sorgu yapılır.
/// </para>
/// </remarks>
public interface IConcurrencyConflictResolver
{
    /// <summary>
    /// Çakışma istisnasından kullanıcıya gösterilecek mesajı üretir.
    /// </summary>
    /// <remarks>
    /// Uygulama bu metodu çağırmak zorunda değildir; çağırmadığı durumda
    /// işlem <c>DbUpdateConcurrencyException</c> ile sonlanır. Bu bir hata
    /// değildir, yalnızca mesajın daha az ayrıntılı olması anlamına gelir.
    /// </remarks>
    Task<string> BuildMessageAsync(
        DbUpdateConcurrencyException exception,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Çakışma bir <c>Result&lt;T&gt;</c> ile taşınamıyorsa (örneğin dönüş tipi olmayan
/// isteklerde) fırlatılır. Çağıran bu istisnayı yakalayıp kullanıcıya göstermelidir.
/// </summary>
public sealed class ConcurrencyConflictException(string message)
    : Exception(message)
{
}
