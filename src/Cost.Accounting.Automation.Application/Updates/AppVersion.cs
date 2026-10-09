namespace Cost.Accounting.Automation.Application.Updates;

/// <summary>
/// Sürüm numaralarını (tablo/derleme/yayın) ayrıştırmak, karşılaştırmak ve
/// artırmak için kullanılan yardımcılar. Sürümler dört parçalı tutulur
/// (ör. 1.0.0.1); eksik parçalar 0 kabul edilir.
/// </summary>
public static class AppVersion
{
    public static Version Parse(string? value)
        => Version.TryParse(value, out Version? parsed) ? parsed : new Version(0, 0, 0, 0);

    /// <summary>
    /// Dört parçayı da içeren normalize edilmiş gösterim (ör. "1.0.0.1").
    /// </summary>
    public static string Normalize(string? value)
    {
        Version version = Parse(value);

        return $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}.{Math.Max(version.Revision, 0)}";
    }

    /// <summary>
    /// 64 bit sıralama anahtarı: major(16) | minor(16) | build(16) | revision(16).
    /// Metinsel sıralamanın aksine "1.0.0.10" &gt; "1.0.0.9" olmasını sağlar.
    /// </summary>
    public static long SortKey(string? value) => SortKey(Parse(value));

    public static long SortKey(Version version)
        => ((long)version.Major << 48)
         | ((long)version.Minor << 32)
         | ((long)Math.Max(version.Build, 0) << 16)
         | (long)Math.Max(version.Revision, 0);

    /// <summary>
    /// Verilen sürümün bir üstünü üretir (ör. "1.0.0.1" -&gt; "1.0.0.2").
    /// Geçersiz veya boş değerde artış "1.0.0.1" ile başlar.
    /// </summary>
    public static string Increment(string? value)
    {
        Version version = Parse(value);

        int major = version.Major;
        int minor = version.Minor;
        int build = Math.Max(version.Build, 0);
        int revision = Math.Max(version.Revision, 0) + 1;

        if (major == 0 && minor == 0 && build == 0 && revision == 1 && string.IsNullOrWhiteSpace(value))
        {
            return "1.0.0.0";
        }

        return $"{major}.{minor}.{build}.{revision}";
    }

    /// <summary>Soldaki sürüm sağdakinden yeniyse pozitif döner.</summary>
    public static int Compare(string? left, string? right)
        => SortKey(left).CompareTo(SortKey(right));
}
