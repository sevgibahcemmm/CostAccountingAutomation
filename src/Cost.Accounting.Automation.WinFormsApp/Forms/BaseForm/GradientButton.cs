using System.Drawing.Drawing2D;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public sealed class GradientButton : DevExpress.XtraEditors.SimpleButton
    {
        private bool _hovered;
        private bool _pressed;

        public GradientButton()
        {
            Appearance.Options.UseBackColor = false;
            Appearance.BorderColor = Color.Transparent;
            BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            if (!Enabled)
            {
                using var disabledBrush = new SolidBrush(Color.FromArgb(148, 163, 184));
                g.FillRectangle(disabledBrush, rect);
                return;
            }

            Color start = _pressed ? Color.FromArgb(67, 56, 202)
                : _hovered ? AuthFormStyles.ButtonHoverStart
                : AuthFormStyles.ButtonGradientStart;

            Color end = _pressed ? Color.FromArgb(109, 40, 217)
                : _hovered ? AuthFormStyles.ButtonHoverEnd
                : AuthFormStyles.ButtonGradientEnd;

            using (GraphicsPath path = AuthFormStyles.GetRoundedRectPath(rect, AuthFormStyles.ButtonCornerRadius))
            {
                using var brush = new LinearGradientBrush(rect, start, end, LinearGradientMode.Horizontal);
                g.FillPath(brush, path);

                Rectangle glossRect = new(rect.X, rect.Y, rect.Width, Math.Max(1, rect.Height / 2));
                using var gloss = new LinearGradientBrush(
                    glossRect,
                    Color.FromArgb(45, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical);
                g.FillRectangle(gloss, glossRect);

                g.DrawPath(new Pen(Color.FromArgb(40, 255, 255, 255), 1f), path);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }
    }
}