using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.AccountingYears.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.AccountingYears;

/// <summary>
/// Bir şirketin bir mali yılı. Master veritabanında tutulur; yıl veritabanı
/// bu kaydın işaret ettiği <see cref="DatabaseName"/> ile açılır ve
/// <see cref="CompanyId"/> ile o yılın iş verisinin ait olduğu şirketi belirtir.
/// </summary>
public sealed class CompanyYear : Entity
{
    private CompanyYear()
    {
    }

    public CompanyYear(IdentityId companyId, Year year, DatabaseName databaseName)
    {
        // Alanlar önce atanır, DuplicateKey en sonda çözülür: anahtar CompanyId ve
        // Year'ın ikisini de gerektirir, kısmi atamada çözülmeye çalışılırsa NRE.
        CompanyId = companyId;
        Year = year;
        DatabaseName = databaseName;
        SetStatus(true);

        ResolveDuplicateKey();
    }

    public IdentityId CompanyId { get; private set; } = default!;
    public Year Year { get; private set; } = default!;
    public DatabaseName DatabaseName { get; private set; } = default!;

    /// <summary>
    /// Yılın kapanıp kapanmadığı. Kapatılmış yıla yeni hareket girilemez.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Bu yılın işlemlerinin başlangıç tarihi (devir bakiyelerinin tarihi).
    /// </summary>
    public DateTimeOffset OpeningDate { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public static string? BuildDuplicateKey(IdentityId companyId, Year year)
        => DuplicateKeyRule.From(companyId.Value, year.Value);

    /// <summary>
    /// DuplicateKey, CompanyId ve Year'ın ikisinden türetilir. İkisi de atanmadan
    /// önce çağrılırsa anahtar üretilmez (ör. EF materializasyonu sırasında).
    /// </summary>
    public void ResolveDuplicateKey()
    {
        if (CompanyId is null || Year is null)
        {
            return;
        }

        SetDuplicateKey(BuildDuplicateKey(CompanyId, Year));
    }

    public void SetCompanyId(IdentityId companyId)
    {
        CompanyId = companyId;
        ResolveDuplicateKey();
    }

    public void SetYear(Year year)
    {
        Year = year;
        ResolveDuplicateKey();
    }
    public void SetDatabaseName(DatabaseName databaseName) => DatabaseName = databaseName;

    public void SetOpeningDate(DateTimeOffset openingDate) => OpeningDate = openingDate;

    public void Close()
    {
        IsClosed = true;
        ClosedAt = DateTimeOffset.UtcNow;
    }

    public void Reopen()
    {
        IsClosed = false;
        ClosedAt = null;
    }

    /// <summary>
    /// Mali yıl ve şirket adından güvenli bir veritabanı adı üretir.
    ///
    /// Ad <c>{prefix}_{year}_{company}</c> biçimindedir. Yılın adın başında
    /// yer alması, SQL Server Object Explorer'da veritabanlarının yıla göre
    /// gruplanmış ve sıralanmış görünmesini sağlar: aynı yılın veritabanları
    /// isim alanında arka arkaya toplanır.
    ///
    /// <paramref name="occurrence"/> aynı ad önerilen ama başka bir kayda
    /// (ör. master) aitse eklenen sıra numarasıdır (1 tabanlı).
    /// </summary>
    public static DatabaseName BuildDatabaseName(string prefix, Name companyName, Year year, int occurrence = 1)
    {
        var head = Sanitize(prefix);
        var tail = Sanitize(companyName.Value);

        var suffix = occurrence > 1 ? $"_{occurrence}" : string.Empty;

        // Yıl belirleyici olduğu için en sonda kalır; şirket adı gerekiyorsa
        // kısaltılır, aksi halde yıl numarası kesilip kaybolur.
        var budget = DatabaseName.MaxLength
            - head.Length
            - year.Value.ToString().Length
            - suffix.Length
            - 2;

        if (budget < 1)
        {
            budget = 1;
        }

        if (tail.Length > budget)
        {
            tail = tail[..budget].TrimEnd('_', '-', ' ');
        }

        if (tail.Length == 0)
        {
            tail = "SIRKET";
        }

        return new DatabaseName($"{head}_{year.Value}_{tail}{suffix}");
    }

    private static string Sanitize(string companyName)
    {
        var buffer = new System.Text.StringBuilder(companyName.Length);

        foreach (var c in companyName)
        {
            buffer.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        var result = buffer.ToString().Trim('_', ' ');

        while (result.Contains("__"))
        {
            result = result.Replace("__", "_", StringComparison.Ordinal);
        }

        if (result.Length == 0 || !char.IsLetterOrDigit(result[0]))
        {
            result = $"S{result}";
        }

        return result;
    }
}
