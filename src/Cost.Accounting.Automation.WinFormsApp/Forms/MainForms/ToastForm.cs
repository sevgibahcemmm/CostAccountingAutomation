using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastForm : Form
    {
        private const int CornerRadius = 12;
        private const int Offset = 16;
        private const int StackSpacing = 8;
        private const int SlideDistance = 80;
        private const int EnterDurationMs = 160;
        private const int ExitDurationMs = 220;

        private enum ToastPhase
        {
            Enter,
            Hold,
            Exit
        }

        private static readonly List<ToastForm> ActiveToasts = new();

        private readonly Panel _pnlBadge;
        private readonly Label _lblIcon;
        private readonly Label _lblTitle;
        private readonly Label _lblMessage;
        private readonly Panel _progressTrack;
        private readonly Panel _progressFill;
        private readonly System.Windows.Forms.Timer _timer;

        private readonly Stopwatch _enterWatch = new();
        private readonly Stopwatch _holdWatch = new();
        private readonly Stopwatch _exitWatch = new();

        private ToastPhase _phase;
        private int _durationMs;
        private int _progressStartWidth;
        private Point _targetLocation;

        public ToastForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            ShowIcon = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(320, 90);

            Region = new Region(AuthFormStyles.GetRoundedRectPath(new Rectangle(0, 0, Width, Height), CornerRadius));

            _pnlBadge = new Panel
            {
                Location = new Point(18, 23),
                Size = new Size(44, 44)
            };

            _lblIcon = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblTitle = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Location = new Point(78, 18),
                Size = new Size(220, 20)
            };

            _lblMessage = new Label
            {
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(78, 42),
                Size = new Size(224, 34)
            };

            _progressTrack = new Panel
            {
                BackColor = Color.FromArgb(229, 231, 235),
                Location = new Point(14, 80),
                Size = new Size(292, 5)
            };

            _progressFill = new Panel
            {
                Location = new Point(14, 80),
                Size = new Size(292, 5)
            };

            _pnlBadge.Controls.Add(_lblIcon);
            _pnlBadge.Region = CreateCircleRegion(_pnlBadge);

            var lblClose = new Label
            {
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(294, 6),
                AutoSize = true,
                Text = "×",
                Cursor = Cursors.Hand
            };
            lblClose.Click += (_, _) => Dismiss();

            Controls.Add(_pnlBadge);
            Controls.Add(_lblTitle);
            Controls.Add(_lblMessage);
            Controls.Add(_progressTrack);
            Controls.Add(_progressFill);
            Controls.Add(lblClose);

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += OnAnimationTick;
        }

        public void ShowToast(string message, ToastType type, int durationMs = 3000)
        {
            Color accent = GetColor(type);
            _pnlBadge.BackColor = accent;
            _lblIcon.Text = GetGlyph(type);
            _lblTitle.Text = GetTitle(type);
            _lblMessage.Text = message;
            _progressFill.BackColor = accent;
            _progressStartWidth = _progressTrack.Width;
            _durationMs = Math.Max(500, durationMs);

            PositionTopRight();

            Opacity = 0;

            if (!IsHandleCreated)
            {
                CreateHandle();
            }

            Show();
            BringToFront();

            _phase = ToastPhase.Enter;
            _enterWatch.Restart();
            _timer.Interval = 10;
            _timer.Start();
        }

        private void PositionTopRight()
        {
            ActiveToasts.Add(this);
            FormClosed += OnToastClosed;

            Rectangle area = Screen.PrimaryScreen!.WorkingArea;
            int index = ActiveToasts.Count - 1;
            _targetLocation = new Point(
                area.Right - Width - Offset,
                area.Top + Offset + index * (Height + StackSpacing));
            Location = new Point(_targetLocation.X + SlideDistance, _targetLocation.Y);
        }

        private void Reposition(int index)
        {
            Rectangle area = Screen.PrimaryScreen!.WorkingArea;
            _targetLocation = new Point(
                area.Right - Width - Offset,
                area.Top + Offset + index * (Height + StackSpacing));
            Location = _targetLocation;
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            switch (_phase)
            {
                case ToastPhase.Enter:
                {
                    double t = Math.Min(1, _enterWatch.ElapsedMilliseconds / (double)EnterDurationMs);
                    Opacity = t;
                    Location = new Point(
                        _targetLocation.X + (int)(SlideDistance * (1 - EaseOut(t))),
                        _targetLocation.Y);
                    if (t >= 1)
                    {
                        _phase = ToastPhase.Hold;
                        _holdWatch.Restart();
                        _timer.Interval = 50;
                    }
                    break;
                }

                case ToastPhase.Hold:
                {
                    double remaining = Math.Max(0, 1 - _holdWatch.ElapsedMilliseconds / (double)_durationMs);
                    _progressFill.Width = (int)(_progressStartWidth * remaining);
                    if (remaining <= 0)
                    {
                        _phase = ToastPhase.Exit;
                        _exitWatch.Restart();
                        _timer.Interval = 10;
                    }
                    break;
                }

                case ToastPhase.Exit:
                {
                    double t = Math.Min(1, _exitWatch.ElapsedMilliseconds / (double)ExitDurationMs);
                    Opacity = 1 - t;
                    Location = new Point(
                        _targetLocation.X + (int)(SlideDistance * EaseOut(t)),
                        _targetLocation.Y);
                    if (t >= 1)
                    {
                        _timer.Stop();
                        Close();
                    }
                    break;
                }
            }
        }

        private void Dismiss()
        {
            if (_phase == ToastPhase.Enter || _phase == ToastPhase.Hold)
            {
                _phase = ToastPhase.Exit;
                _exitWatch.Restart();
                _timer.Interval = 10;
            }
        }

        private void OnToastClosed(object? sender, FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= OnAnimationTick;
            ActiveToasts.Remove(this);

            for (int i = 0; i < ActiveToasts.Count; i++)
            {
                ActiveToasts[i].Reposition(i);
            }

            Dispose();
        }

        private static double EaseOut(double t) => 1 - (1 - t) * (1 - t);

        private static Region CreateCircleRegion(Control control)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, control.Width, control.Height);
            return new Region(path);
        }

        private static string GetGlyph(ToastType type) => type switch
        {
            ToastType.Info => "ℹ",
            ToastType.Success => "✓",
            ToastType.Warning => "!",
            ToastType.Error => "✕",
            _ => "●"
        };

        private static string GetTitle(ToastType type) => type switch
        {
            ToastType.Info => "Bilgi",
            ToastType.Success => "Başarılı",
            ToastType.Warning => "Uyarı",
            ToastType.Error => "Hata",
            _ => "Bildirim"
        };

        private static Color GetColor(ToastType type) => type switch
        {
            ToastType.Success => Color.FromArgb(22, 163, 74),
            ToastType.Warning => Color.FromArgb(220, 38, 38),
            ToastType.Error => Color.FromArgb(185, 28, 28),
            ToastType.Info => Color.FromArgb(59, 130, 246),
            _ => Color.FromArgb(107, 114, 128)
        };
    }
}