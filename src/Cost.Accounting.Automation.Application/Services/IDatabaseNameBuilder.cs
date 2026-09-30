namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Mali yıl veritabanı adının nasıl üretileceğini tanımlar. Ad, yılın başında
/// geldiği için Object Explorer'da veritabanları yıla göre gruplanır.
/// </summary>
public interface IDatabaseNameBuilder
{
    /// <summary>
    /// <paramref name="companyName"/> ve <paramref name="year"/> için önerilen
    /// veritabanı adını üretir.
    /// </summary>
    /// <param name="occurrence">
    /// Önerilen ad zaten kullanımdaysa artan sıra numarası (1 tabanlı).
    /// </param>
    string Build(string companyName, int year, int occurrence = 1);

    /// <summary>
    /// Aday adın SQL Server tanımlayıcı kurallarına uyup uymadığını denetler.
    /// </summary>
    bool IsValidName(string? candidate, out string? error);

    /// <summary>
    /// Henüz kullanılmayan ilk uygun adı önerir. Mevcut yıl kayıtları ve
    /// master veritabanının adıyla çakışan adlar atlanır.
    /// </summary>
    Task<string> SuggestAvailableAsync(
        string companyName,
        int year,
        CancellationToken cancellationToken = default);
}
