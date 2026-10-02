using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
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
            _btnClose.MouseHover += (_, _) => _btnClose.Appearance.ForeColor = SkinTheme.HighContrastAccent(SkinTheme.HighContrastSurface);
            _btnClose.MouseLeave += (_, _) => _btnClose.Appearance.ForeColor = SkinTheme.HighContrastText;
            _btnClose.Appearance.ForeColor = SkinTheme.HighContrastText;

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

            // Metin ve zemin SkinTheme'deki zıt çiftten gelir: açık temada koyu metin
            // / açık zemin, koyu temada açık metin / koyu zemin. Skin değiştiğinde
            // bir sonraki toast yeni renkleri otomatik alır.
            Color textColor = SkinTheme.HighContrastText;
            Color surface = SkinTheme.HighContrastSurface;

            BackColor = surface;
            _pnlContentArea.Appearance.BackColor = surface;
            _pnlContentArea.Appearance.Options.UseBackColor = true;

            _lblTitle.Appearance.ForeColor = textColor;
            _lblTitle.Appearance.Options.UseForeColor = true;
            _lblMessage.Appearance.ForeColor = textColor;
            _lblMessage.Appearance.Options.UseForeColor = true;
            _btnClose.Appearance.ForeColor = textColor;
            _btnClose.Appearance.Options.UseForeColor = true;

            // Tasarımdaki sabit 268x36 alan uzun metinleri kırpıyordu; form yalnızca
            // gerektiği kadar uyar (genişlik, renkler, ikonlar ve ilerleme çubuğu
            // animasyonu olduğu gibi kalır).
            FitToMessage(message);

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

        /// <summary>
        /// Mesajı tasarımdaki sarım genişliğinde ölçer ve yalnızca dikeyde gerektiği
        /// kadar büyür. Genişlik, ikon, renkler ve çubuk animasyonu değişmez.
        /// </summary>
        private void FitToMessage(string message)
        {
            const int textTop = 40;
            const int bottomGap = 12;
            const int progressHeight = 4;

            int textWidth = _lblMessage.Width;

            Font font = _lblMessage.Appearance.Font ?? Font;
            Size measured = TextRenderer.MeasureText(
                message,
                font,
                new Size(textWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

            int messageHeight = Math.Max(_lblMessage.Height, measured.Height);
            int contentHeight = textTop + messageHeight + bottomGap;
            int formHeight = contentHeight + progressHeight;

            _lblMessage.Height = messageHeight;
            _pnlContentArea.Height = contentHeight;
            _pnlAccentBar.Height = formHeight;
            _progressTrack.Top = contentHeight;
            _progressFill.Top = contentHeight;

            ClientSize = new Size(ClientSize.Width, formHeight);
            Region = new Region(AuthFormStyles.GetRoundedRectPath(new Rectangle(0, 0, Width, Height), CornerRadius));
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
            ToastType.Success => SkinTheme.Success,
            ToastType.Warning => SkinTheme.Warning,
            ToastType.Error => SkinTheme.Danger,
            ToastType.Info => SkinTheme.Question,
            _ => SkinTheme.SecondaryText
        };
    }
}