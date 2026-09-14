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

        public static readonly Color ButtonGradientStart = Color.FromArgb(99, 102, 241);
        public static readonly Color ButtonGradientEnd = Color.FromArgb(139, 92, 246);
        public static readonly Color ButtonHoverStart = Color.FromArgb(79, 70, 229);
        public static readonly Color ButtonHoverEnd = Color.FromArgb(124, 58, 237);
        public static readonly Color ButtonGlow = Color.FromArgb(30, 99, 102, 241);

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

        public static void Button_Paint(object? sender, PaintEventArgs e)
        {
            var control = (Control)sender!;
            Rectangle rect = control.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle glowRect = new(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4);
            using GraphicsPath glowPath = GetRoundedRectPath(glowRect, ButtonCornerRadius + 3);
            using var glowBrush = new SolidBrush(ButtonGlow);
            e.Graphics.FillPath(glowBrush, glowPath);

            Rectangle mainRect = new(rect.X, rect.Y + 1, rect.Width, rect.Height - 1);
            using GraphicsPath roundPath = GetRoundedRectPath(mainRect, ButtonCornerRadius);

            using var gradBrush = new LinearGradientBrush(
                mainRect,
                ButtonGradientStart,
                ButtonGradientEnd,
                LinearGradientMode.Horizontal);
            e.Graphics.FillPath(gradBrush, roundPath);

            control.Region = new Region(roundPath);
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
