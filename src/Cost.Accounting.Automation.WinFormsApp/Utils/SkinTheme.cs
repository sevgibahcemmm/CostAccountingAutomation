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

        /// <summary>Aktif skin birincil metin rengi.</summary>
        public static Color Text => Resolve(DXSkinColors.ForeColors.WindowText);

        /// <summary>Aktif skin ikincil (soluk) metin rengi.</summary>
        public static Color SecondaryText => Resolve(DXSkinColors.ForeColors.DisabledText);

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

                return !IsColorDark(Text);
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
    }
}