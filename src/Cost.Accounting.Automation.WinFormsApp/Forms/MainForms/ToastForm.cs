using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    /// <summary>
    /// Bildirim türlerini barındıran enum yapısı.
    /// </summary>
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Modern, akıcı animasyonlu ve DevExpress uyumlu Toast bildirim formu.
    /// </summary>
    public partial class ToastForm : XtraForm
    {
        private const int CornerRadius = 10;
        private const int Offset = 18;
        private const int StackSpacing = 10;
        private const int SlideDistance = 90;
        private const int EnterDurationMs = 180;
        private const int ExitDurationMs = 240;

        private enum ToastPhase
        {
            Enter,
            Hold,
            Exit
        }

        private static readonly List<ToastForm> ActiveToasts = [];

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
            InitializeComponent();

            // Form köşelerini modern kavisli hale getiriyoruz
            Region = new Region(AuthFormStyles.GetRoundedRectPath(new Rectangle(0, 0, Width, Height), CornerRadius));

            _btnClose.Click += (_, _) => Dismiss();
            _btnClose.MouseHover += (_, _) => _btnClose.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            _btnClose.MouseLeave += (_, _) => _btnClose.Appearance.ForeColor = Color.FromArgb(148, 163, 184);

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += OnAnimationTick;
        }

        /// <summary>
        /// Belirtilen mesaj ve tip ile toast bildirimi ekrana getirir.
        /// </summary>
        public void ShowToast(string message, ToastType type, int durationMs = 3000)
        {
            Color accentColor = GetThemeColor(type);

            _pnlAccentBar.Appearance.BackColor = accentColor;
            _progressFill.Appearance.BackColor = accentColor;

            _lblIcon.Text = GetGlyph(type);
            _lblIcon.Appearance.ForeColor = accentColor;

            _lblTitle.Text = GetTitle(type);
            _lblMessage.Text = message;

            _progressStartWidth = _progressTrack.Width;
            _durationMs = Math.Max(600, durationMs);

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

            Rectangle workingArea = Screen.PrimaryScreen!.WorkingArea;
            int index = ActiveToasts.Count - 1;

            _targetLocation = new Point(
                workingArea.Right - Width - Offset,
                workingArea.Top + Offset + index * (Height + StackSpacing));

            Location = new Point(_targetLocation.X + SlideDistance, _targetLocation.Y);
        }

        private void Reposition(int index)
        {
            Rectangle workingArea = Screen.PrimaryScreen!.WorkingArea;
            _targetLocation = new Point(
                workingArea.Right - Width - Offset,
                workingArea.Top + Offset + index * (Height + StackSpacing));
            Location = _targetLocation;
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            switch (_phase)
            {
                case ToastPhase.Enter:
                    {
                        double progress = Math.Min(1, _enterWatch.ElapsedMilliseconds / (double)EnterDurationMs);
                        Opacity = progress;
                        Location = new Point(
                            _targetLocation.X + (int)(SlideDistance * (1 - EaseOut(progress))),
                            _targetLocation.Y);

                        if (progress >= 1)
                        {
                            _phase = ToastPhase.Hold;
                            _holdWatch.Restart();
                            _timer.Interval = 40;
                        }
                        break;
                    }

                case ToastPhase.Hold:
                    {
                        double remainingRatio = Math.Max(0, 1 - _holdWatch.ElapsedMilliseconds / (double)_durationMs);
                        _progressFill.Width = (int)(_progressStartWidth * remainingRatio);

                        if (remainingRatio <= 0)
                        {
                            _phase = ToastPhase.Exit;
                            _exitWatch.Restart();
                            _timer.Interval = 10;
                        }
                        break;
                    }

                case ToastPhase.Exit:
                    {
                        double progress = Math.Min(1, _exitWatch.ElapsedMilliseconds / (double)ExitDurationMs);
                        Opacity = 1 - progress;
                        Location = new Point(
                            _targetLocation.X + (int)(SlideDistance * EaseOut(progress)),
                            _targetLocation.Y);

                        if (progress >= 1)
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

        private static string GetGlyph(ToastType type) => type switch
        {
            ToastType.Info => "🛈",
            ToastType.Success => "✔",
            ToastType.Warning => "⚠",
            ToastType.Error => "✕",
            _ => "●"
        };

        private static string GetTitle(ToastType type) => type switch
        {
            ToastType.Info => "Bilgilendirme",
            ToastType.Success => "İşlem Başarılı",
            ToastType.Warning => "Dikkat / Uyarı",
            ToastType.Error => "Hata Oluştu",
            _ => "Bildirim"
        };

        private static Color GetThemeColor(ToastType type) => type switch
        {
            ToastType.Success => Color.FromArgb(16, 185, 129),  // Emerald Yeşil
            ToastType.Warning => Color.FromArgb(245, 158, 11),  // Amber Sarı
            ToastType.Error => Color.FromArgb(239, 68, 68),    // Rose Kırmızı
            ToastType.Info => Color.FromArgb(59, 130, 246),     // Royal Mavi
            _ => Color.FromArgb(107, 114, 128)
        };
    }
}