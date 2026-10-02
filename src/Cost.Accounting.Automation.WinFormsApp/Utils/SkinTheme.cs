using System.Drawing;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.Utils.Colors;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// Program genelinde aktif skin'e göre çözümlenen renk paleti ve skin değişiklik
    /// bildirimini tek noktadan sunar. Tüm formlar renkleri bu sınıf üzerinden almalıdır.
    /// </summary>
    public static class SkinTheme
    {
        private static bool _subscribed;
        private static EventHandler? _changed;

        /// <summary>Skini değiştiğinde tüm görünümlerin (MDI çocuk formların) tetiklenmesi için.</summary>
        public static event EventHandler? Changed
        {
            add
            {
                EnsureSubscribed();
                _changed += value;
            }
            remove
            {
                _changed -= value;
            }
        }

        /// <summary>Aktif skin genel aksan rengi.</summary>
        public static Color Primary => Resolve(DXSkinColors.FillColors.Primary);

        /// <summary>Aktif skin başarı (yeşil) rengi.</summary>
        public static Color Success => Resolve(DXSkinColors.FillColors.Success);

        /// <summary>Aktif skin uyarı (amber) rengi.</summary>
        public static Color Warning => Resolve(DXSkinColors.FillColors.Warning);

        /// <summary>Aktif skin tehlike (kırmızı) rengi.</summary>
        public static Color Danger => Resolve(DXSkinColors.FillColors.Danger);

        /// <summary>Aktif skin bilgi (mavi) rengi.</summary>
        public static Color Question => Resolve(DXSkinColors.FillColors.Question);

        /// <summary>
        /// Aktif skin birincil metin rengi. Koyu temalarda token çözümü koyu bir palete
        /// düşebildiği için (bilinmeyen skinlerde DXSkinColorHelper eski palete dönüyor)
        /// açık temelli bir metin zorlanır; aksi halde koyu metin koyu zeminde kaybolur.
        /// </summary>
        public static Color Text => IsDarkSkin ? Color.FromArgb(232, 232, 234) : RawText;

        private static Color RawText => Resolve(DXSkinColors.ForeColors.WindowText);

        /// <summary>
        /// Skin'in gerçek zemin rengi. DevExpress koyu temalarda kontrolün <c>BackColor</c>
        /// değerini güncellemediği için zemini temaya göre seçip metin rengiyle tutarlı
        /// tutuyoruz; aksi halde koyu temada açık metin açık zemin üzerinde kayboluyor.
        /// </summary>
        public static Color SurfaceOf(Control? surface)
        {
            Color resolved = surface is { IsDisposed: false } ? surface.BackColor : Color.Empty;

            if (IsDarkSkin)
            {
                return IsColorDark(resolved) ? resolved : Color.FromArgb(48, 48, 52);
            }

            return resolved.A > 0 ? resolved : Color.FromArgb(240, 240, 240);
        }

        /// <summary>Zemin üzerinde hafif ayrışan yüzey (başlık ve kart panelleri).</summary>
        public static Color SurfaceMuted(Color surface)
            => Blend(surface, Text, IsDarkSkin ? 0.10F : 0.04F);

        /// <summary>Aksan renginin zeminle harmanlandığı yüzey (özet paneli).</summary>
        public static Color SurfaceAccent(Color surface)
            => Blend(surface, Primary, IsDarkSkin ? 0.24F : 0.10F);

        /// <summary>Panel kenarlıkları ve ayırıcı çizgiler için renk.</summary>
        public static Color BorderMuted(Color surface)
            => Blend(surface, Text, IsDarkSkin ? 0.24F : 0.14F);

        /// <summary>Buton/grid hover arka planı.</summary>
        public static Color SurfaceHover(Color surface)
            => Blend(surface, Text, IsDarkSkin ? 0.16F : 0.07F);

        /// <summary>Salt okunur (kilitli) alan arka planı.</summary>
        public static Color SurfaceReadOnly(Color surface)
            => Blend(surface, Text, IsDarkSkin ? 0.12F : 0.05F);

        /// <summary>Boş durum/ipucu metinleri için soluk metin rengi.</summary>
        public static Color MutedText(Color surface) => Blend(Text, surface, 0.35F);

        /// <summary>
        /// Yüksek kontrastlı yüzeylerde (bildirim balonu, uyarı şeridi) kullanılacak
        /// metin rengi. <see cref="MutedText"/> soluk olduğu için dar yüzeylerde
        /// okunmuyor; burada metin koyu temada açık, açık temada koyu çözülür.
        /// </summary>
        public static Color HighContrastText
            => IsDarkSkin ? Color.FromArgb(242, 244, 247) : Color.FromArgb(17, 24, 39);

        /// <summary>
        /// <see cref="HighContrastText"/> ile daima zıt olan zemin rengi. Metin ve
        /// zemin tek kaynaktan geldiği için herhangi bir temada okunabilir kaldığı
        /// garanti edilir.
        /// </summary>
        public static Color HighContrastSurface
            => IsDarkSkin ? Color.FromArgb(40, 43, 48) : Color.FromArgb(250, 250, 252);

        /// <summary>
        /// <see cref="HighContrastText"/> rengini zemin üzerinde okunur kılan vurgu
        /// rengi (ikon ve çubuk gibi küçük öğeler için).
        /// </summary>
        public static Color HighContrastAccent(Color surface)
            => IsColorDark(surface) ? Color.FromArgb(232, 236, 242) : Color.FromArgb(24, 30, 45);

        /// <summary>
        /// Skin değiştiğinde <paramref name="handler"/>'ı yeniden çalıştırır. Dönen IDisposable,
        /// form Dispose edilirken çağrılmalıdır (bkz. skinBinding alanı).
        /// </summary>
        public static IDisposable Bind(Action handler) => new Binding(handler);

        /// <summary>Aktif skin ikincil (soluk) metin rengi.</summary>
        public static Color SecondaryText
            => IsDarkSkin ? Color.FromArgb(162, 162, 168) : Resolve(DXSkinColors.ForeColors.DisabledText);

        /// <summary>Skin genel aksan koyu ise true (beyaz metin gerekir).</summary>
        public static bool IsDarkAccent => IsColorDark(Primary);

        /// <summary>Skin koyu temalı ise true (gövde/panel renkleri koyu olmalı).</summary>
        public static bool IsDarkSkin
        {
            get
            {
                string skinName =
                    UserLookAndFeel.Default.ActiveSkinName ?? string.Empty;

                if (ContainsInsensitive(skinName, "dark") ||
                    ContainsInsensitive(skinName, "black"))
                {
                    return true;
                }

                if (ContainsInsensitive(skinName, "light") ||
                    skinName.Equals("Basic", System.StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return !IsColorDark(RawText);
            }
        }

        public static void EnsureSubscribed()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            UserLookAndFeel.Default.StyleChanged += (_, _) =>
            {
                try
                {
                    _changed?.Invoke(null, EventArgs.Empty);
                }
                catch
                {
                }
            };
        }

        /// <summary>Renk üzerine alfa (şeffaflık) uygular; satır vurguları için kullanılır.</summary>
        public static Color Tint(Color color, int alpha) => Color.FromArgb(Clamp255(alpha), color);

        /// <summary>İki rengin oranlı karışımını üretir.</summary>
        public static Color Blend(Color from, Color to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            int r = (int)(from.R + (to.R - from.R) * amount);
            int g = (int)(from.G + (to.G - from.G) * amount);
            int b = (int)(from.B + (to.B - from.B) * amount);
            return Color.FromArgb(Clamp255(r), Clamp255(g), Clamp255(b));
        }

        public static bool IsColorDark(Color color)
        {
            double luminance = color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
            return luminance < 140;
        }

        /// <summary>Bir arka plan rengi üzerinde okunabilir ön plan metin rengi seçer.</summary>
        public static Color GetContrastText(Color background)
            => IsColorDark(background) ? Color.White : Color.FromArgb(17, 24, 39);

        /// <summary>
        /// WCAG karsilastirma orani (1:1 - 21:1).
        /// </summary>
        public static double ContrastRatio(Color first, Color second)
        {
            double l1 = RelativeLuminance(first);
            double l2 = RelativeLuminance(second);

            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// Bir rengi verilen zemin üzerinde okunabilir olana kadar koyultur
        /// (<see cref="IsColorDark"/> zemin) veya açar (açık zemin).
        ///
        /// <para>
        /// Tema vurgu renkleri, zemine belli oranda karıştırıldığında WCAG AA
        /// (4.5:1) eşiğinin altına düşebilir; ölçüm yapılmadan kabul edilirse
        /// metin soluk görünür. Burada renk gerçekten ölçülür ve gerekiyorsa
        /// doğru yöne doğru kademeli olarak kaydırılır.
        /// </para>
        /// </summary>
        /// <param name="color">İstenen renk (genellikle durum/vurgu rengi).</param>
        /// <param name="background">Rencin üzerinde okunacağı zemin.</param>
        /// <param name="minimumRatio">Hedef kontrast oranı.</param>
        public static Color EnsureReadable(
            Color color,
            Color background,
            double minimumRatio = 4.5)
        {
            if (ContrastRatio(color, background) >= minimumRatio)
            {
                return color;
            }

            bool lighten = IsColorDark(background);

            const int Steps = 20;

            Color result = color;

            for (int step = 1; step <= Steps; step++)
            {
                float amount = (float)step / Steps;

                result = lighten
                    ? Color.FromArgb(
                        Clamp255(color.R + (int)((255 - color.R) * amount)),
                        Clamp255(color.G + (int)((255 - color.G) * amount)),
                        Clamp255(color.B + (int)((255 - color.B) * amount)))
                    : Color.FromArgb(
                        Clamp255((int)(color.R * (1f - amount))),
                        Clamp255((int)(color.G * (1f - amount))),
                        Clamp255((int)(color.B * (1f - amount))));

                if (ContrastRatio(result, background) >= minimumRatio)
                {
                    break;
                }
            }

            return result;
        }

        private static double RelativeLuminance(Color color)
        {
            static double Channel(double value)
            {
                value /= 255.0;

                return value <= 0.03928
                    ? value / 12.92
                    : Math.Pow((value + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Channel(color.R)
                 + 0.7152 * Channel(color.G)
                 + 0.0722 * Channel(color.B);
        }

        private static bool ContainsInsensitive(string value, string search)
            => value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;

        private static Color Resolve(Color token)
            => DXSkinColorHelper.GetDXSkinColor(
                token,
                UserLookAndFeel.Default.ActiveSkinName,
                UserLookAndFeel.Default.ActiveSvgPaletteName);

        private static int Clamp255(int value) => Math.Max(0, Math.Min(255, value));

        /// <summary>Skin değişimini dinleyip Dispose ile bağı çözen hafif abonelik.</summary>
        private sealed class Binding : IDisposable
        {
            private readonly EventHandler _handler;

            public Binding(Action action)
            {
                _handler = (_, _) => action();
                Changed += _handler;
            }

            public void Dispose() => Changed -= _handler;
        }
    }
}