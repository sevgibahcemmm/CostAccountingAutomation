using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Dashboard grafikleri için canlı, skin'e duyarlı renk paleti.
///
/// Koyu temalarda (DarkSide) koyu tonlar okunmadığı için renkler otomatik
/// olarak açıklaştırılır; açık temalarda ise doygunluk korunur.
/// </summary>
internal static class DashboardChartPalette
{
    private static readonly Color[] Base =
    [
        Color.FromArgb(0x2E, 0x75, 0xB6), // mavi
        Color.FromArgb(0xE8, 0x3E, 0x8C), // pembe
        Color.FromArgb(0x14, 0xA0, 0x8A), // turkuaz
        Color.FromArgb(0xF2, 0x9E, 0x1B), // kehribar
        Color.FromArgb(0x6F, 0x42, 0xC1), // mor
        Color.FromArgb(0xDC, 0x35, 0x45), // kırmızı
        Color.FromArgb(0x2E, 0xCC, 0x71), // yeşil
        Color.FromArgb(0x00, 0x9C, 0xE0), // camgöbeği
        Color.FromArgb(0xFF, 0x6B, 0x6B), // mercan
        Color.FromArgb(0x84, 0x8E, 0x96)  // gri
    ];

    /// <summary>Doughnut/alan grafikleri için sıralı döngüsel palet.</summary>
    public static Color[] Categorical(int count)
    {
        Color[] result = new Color[Math.Max(count, 0)];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = At(i);
        }

        return result;
    }

    /// <summary>Gruptaki sıraya göre renk döndürür (döngüsel).</summary>
    public static Color At(int index) => Normalize(Base[Math.Abs(index) % Base.Length]);

    public static Color Primary => At(0);
    public static Color Secondary => At(2);
    public static Color Success => At(6);
    public static Color Danger => At(5);
    public static Color Warning => At(3);
    public static Color Accent => At(4);

    /// <summary>Koyu temada rengi okunur hâle getirir.</summary>
    public static Color Normalize(Color color)
        => SkinTheme.IsDarkSkin
            ? SkinTheme.Blend(color, Color.White, 0.22F)
            : color;

    /// <summary>
    /// Dolgu/alan serileri için dikey degrade üretir: rengin açık varyantından
    /// doygun varyantına iner.
    /// </summary>
    public static (Color Top, Color Bottom) Gradient(Color color)
    {
        Color top = SkinTheme.Blend(color, Color.White, 0.35F);
        Color bottom = SkinTheme.IsDarkSkin
            ? SkinTheme.Blend(color, Color.Black, 0.25F)
            : color;

        return (top, bottom);
    }
}
