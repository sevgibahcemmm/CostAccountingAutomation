using System.Drawing.Drawing2D;
using FluentValidation;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public static class AuthFormStyles
    {
        public static readonly Color AccentPrimary = Color.FromArgb(99, 102, 241);
        public static readonly Color AccentPrimaryDark = Color.FromArgb(79, 70, 229);
        public static readonly Color AccentSecondary = Color.FromArgb(139, 92, 246);
        public static readonly Color AccentGradientEnd = Color.FromArgb(59, 130, 246);

        public static readonly Color FieldBorderIdle = Color.FromArgb(226, 232, 240);
        public static readonly Color FieldBorderFocus = Color.FromArgb(99, 102, 241);
        public static readonly Color FieldBg = Color.FromArgb(248, 250, 252);
        public static readonly Color FieldBgFocus = Color.White;
        public static readonly Color FieldShadow = Color.FromArgb(20, 99, 102, 241);

        /// <summary>
        /// Kimlik ekranlarındaki birincil butonun renkleri.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Değerler kasıtlı olarak koyu seçildi. Önceden burada
        /// <c>RGB(99,102,241) → RGB(139,92,246)</c> kullanılıyordu; beyaz yazı bu
        /// zeminde yalnızca <b>4.23–4.47:1</b> kontrast veriyordu, yani WCAG AA
        /// eşiği olan 4.5:1'in <b>altında</b> kalıyordu. Aşağıdaki koyu değerler
        /// beyaz yazı için 5.70:1 (varsayılan) ve 6.29:1 (üzerinde), üzerinde
        /// gelindiğinde 7.10–7.90:1 sağlar.
        /// </para>
        /// </remarks>
        public static readonly Color ButtonGradientStart = Color.FromArgb(79, 70, 229);
        public static readonly Color ButtonGradientEnd = Color.FromArgb(124, 58, 237);
        public static readonly Color ButtonHoverStart = Color.FromArgb(67, 56, 202);
        public static readonly Color ButtonHoverEnd = Color.FromArgb(109, 40, 217);

        /// <summary>Birincil butondaki yazı rengi. Zemin koyu olduğu için daima açıktır.</summary>
        public static readonly Color ButtonText = Color.White;

        /// <summary>
        /// Pasif buton zemini. Orta ton gri seçildi ki DevExpress'in soluklaştırdığı
        /// metin de okunabilir kalsın; koyu metin kullanılır.
        /// </summary>
        public static readonly Color ButtonDisabledBack = Color.FromArgb(148, 163, 184);

        /// <summary>Pasif butondaki yazı rengi (pasif zeminde 6.55:1).</summary>
        public static readonly Color ButtonDisabledText = Color.FromArgb(15, 23, 42);

        /// <summary>
        /// Etkin buton zemini (tasarımcıdaki koyu mavi; beyaz yazı için 5.17:1).
        /// </summary>
        public static readonly Color ButtonBase = Color.FromArgb(37, 99, 235);

        /// <summary>Buton üzerinde gelindiğindeki zemin.</summary>
        public static readonly Color ButtonHover = Color.FromArgb(29, 78, 216);

        /// <summary>Buton basılıyken zemin.</summary>
        public static readonly Color ButtonPressed = Color.FromArgb(30, 64, 175);

        public const int FieldCornerRadius = 12;
        public const int ButtonCornerRadius = 14;

        public static void RoundedField_Paint(object? sender, PaintEventArgs e)
        {
            var control = (Control)sender!;
            Rectangle rect = control.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            bool focused = control.Tag is true;

            if (focused)
            {
                Rectangle glowRect = new(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                using GraphicsPath glowPath = GetRoundedRectPath(glowRect, FieldCornerRadius + 3);
                using var glowBrush = new SolidBrush(Color.FromArgb(20, AccentPrimary));
                e.Graphics.FillPath(glowBrush, glowPath);
            }

            using GraphicsPath roundPath = GetRoundedRectPath(rect, FieldCornerRadius);

            using var bgBrush = new SolidBrush(focused ? FieldBgFocus : FieldBg);
            e.Graphics.FillPath(bgBrush, roundPath);

            float borderWidth = focused ? 2f : 1.2f;
            Color borderColor = focused ? FieldBorderFocus : FieldBorderIdle;

            using var borderPen = new Pen(borderColor, borderWidth);
            e.Graphics.DrawPath(borderPen, roundPath);
        }

        public static void Field_Enter(object? sender, EventArgs e)
        {
            if (sender is Control control && control.Parent is { } parent)
            {
                parent.Tag = true;
                parent.Invalidate();
            }
        }

        public static void Field_Leave(object? sender, EventArgs e)
        {
            if (sender is Control control && control.Parent is { } parent)
            {
                parent.Tag = false;
                parent.Invalidate();
            }
        }

        /// <summary>
        /// Birincil butonun tüm durum renklerini ayarlar.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Yanlış desen.</b> Zemin, butonun <c>Paint</c> olayına bağlanarak
        /// çizilirse dolgu, DevExpress'in <b>çizdiği metnin üzerine</b> biner ve yazı
        /// hiç görünmez. <c>Paint</c> denetimin kendi boyamasından sonra tetiklendiği
        /// için bu, denetimi ezmiş olur. Doğru yer <c>OnPaintBackground</c>'dir;
        /// <see cref="GradientButton"/> onu doğru yerde yapar.
        /// </para>
        /// <para>
        /// Ancak görünümün tek doğru kaynağı <b>Appearance renkleri</b> olsun ve
        /// <b>her durum açıkça</b> verilsin: zemin, üzerinde, basılı ve pasif. Böylece
        /// zeminin kim tarafından çizildiğinden bağımsız okunabilirlik garanti edilir.
        /// </para>
        /// <para>
        /// Pasif durum özellikle önemlidir: DevExpress metni varsayılan olarak
        /// soluklaştırır, koyu mavi zemin üzerinde soluk beyaz okunmaz. Bu yüzden
        /// pasif zemin açık gri, pasif metin koyu verilir.
        /// </para>
        /// <para>
        /// Renkler WCAG AA eşiğini (4.5:1) geçecek biçimde seçilmiştir: etkin
        /// zeminde beyaz yazı 5.17:1, pasif zeminde koyu yazı 6.55:1.
        /// </para>
        /// </remarks>
        public static void ApplyButtonAppearance(DevExpress.XtraEditors.SimpleButton button)
        {
            if (button is null)
            {
                return;
            }

            button.Appearance.BackColor = ButtonBase;
            button.Appearance.BorderColor = ButtonBase;
            button.Appearance.ForeColor = ButtonText;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.Options.UseForeColor = true;

            button.AppearanceHovered.BackColor = ButtonHover;
            button.AppearanceHovered.ForeColor = ButtonText;
            button.AppearanceHovered.Options.UseBackColor = true;
            button.AppearanceHovered.Options.UseForeColor = true;

            button.AppearancePressed.BackColor = ButtonPressed;
            button.AppearancePressed.ForeColor = ButtonText;
            button.AppearancePressed.Options.UseBackColor = true;
            button.AppearancePressed.Options.UseForeColor = true;

            button.AppearanceDisabled.BackColor = ButtonDisabledBack;
            button.AppearanceDisabled.ForeColor = ButtonDisabledText;
            button.AppearanceDisabled.Options.UseBackColor = true;
            button.AppearanceDisabled.Options.UseForeColor = true;
        }

        public static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            if (diameter <= 0 || diameter > rect.Width || diameter > rect.Height)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void ShowError(DevExpress.XtraEditors.LabelControl? messageLabel, string message)
        {
            if (messageLabel is null)
                return;

            messageLabel.Appearance.ForeColor = Color.FromArgb(239, 68, 68);
            messageLabel.Text = message;
            messageLabel.Visible = true;
        }

        public static void ShowInfo(DevExpress.XtraEditors.LabelControl? messageLabel, string message)
        {
            if (messageLabel is null)
                return;

            messageLabel.Appearance.ForeColor = Color.FromArgb(34, 197, 94);
            messageLabel.Text = message;
            messageLabel.Visible = true;
        }

        public static void HideMessage(DevExpress.XtraEditors.LabelControl? messageLabel)
        {
            if (messageLabel is not null)
                messageLabel.Visible = false;
        }

        public static string GetErrorText(IEnumerable<string>? errorMessages)
        {
            if (errorMessages is null || !errorMessages.Any())
                return "İşlem başarısız oldu.";

            return string.Join(Environment.NewLine, errorMessages);
        }

        public static string GetValidationText(ValidationException exception)
        {
            string message = string.Join(
                Environment.NewLine,
                exception.Errors.Select(e => e.PropertyName).Distinct());

            return string.IsNullOrWhiteSpace(message)
                ? "Girdiğiniz bilgileri kontrol edin."
                : message;
        }
    }
}
