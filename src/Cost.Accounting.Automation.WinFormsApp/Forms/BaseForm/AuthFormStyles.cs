using System.Drawing.Drawing2D;
using FluentValidation;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public static class AuthFormStyles
    {
        public static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        public static readonly Color FieldBorderIdle = Color.FromArgb(226, 232, 240);
        public static readonly Color FieldBg = Color.White;
        public const int FieldCornerRadius = 10;

        public static void RoundedField_Paint(object? sender, PaintEventArgs e)
        {
            var control = (Control)sender!;
            Rectangle rect = control.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using GraphicsPath roundPath = GetRoundedRectPath(rect, FieldCornerRadius);
            using var fullRegion = new Region(control.ClientRectangle);
            fullRegion.Exclude(roundPath);
            using var cornerBrush = new SolidBrush(FieldBg);
            e.Graphics.FillRegion(cornerBrush, fullRegion);

            bool focused = control.Tag is true;
            using var borderPen = new Pen(focused ? AccentBlue : FieldBorderIdle, focused ? 2f : 1.2f);
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

            using GraphicsPath roundPath = GetRoundedRectPath(rect, 12);
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
            {
                return;
            }

            messageLabel.Appearance.ForeColor = Color.FromArgb(239, 68, 68);
            messageLabel.Text = message;
            messageLabel.Visible = true;
        }

        public static void ShowInfo(DevExpress.XtraEditors.LabelControl? messageLabel, string message)
        {
            if (messageLabel is null)
            {
                return;
            }

            messageLabel.Appearance.ForeColor = Color.FromArgb(34, 197, 94);
            messageLabel.Text = message;
            messageLabel.Visible = true;
        }

        public static void HideMessage(DevExpress.XtraEditors.LabelControl? messageLabel)
        {
            if (messageLabel is not null)
            {
                messageLabel.Visible = false;
            }
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