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