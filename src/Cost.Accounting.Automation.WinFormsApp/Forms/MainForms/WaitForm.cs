using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class WaitForm : DevExpress.XtraWaitForm.WaitForm
    {
        /// <summary>
        /// DevExpress <c>ProgressPanel.AnimationSpeed</c> yalnızca pozitif değer
        /// kabul eder; 0 atamak ArgumentException fırlatır. Onay penceresi
        /// kısa süre göründüğü için animasyon pratikte donmuş gibi görünen bu
        /// değer kullanılır.
        /// </summary>
        private const float FrozenAnimationSpeed = 0.01f;

        private string? _pendingCaption;
        private string? _pendingDescription;

        public WaitForm()
        {
            InitializeComponent();
            this.progressPanel1.AutoHeight = true;
        }

        #region Overrides

        public override void SetCaption(string caption)
        {
            base.SetCaption(caption);
            _pendingCaption = caption;

            // Pencere çizilmeden (handle oluşmadan) yapılan atamalar DevExpress
            // ProgressPanel yerleşimi hesaplanırken NullReferenceException
            // fırlatabiliyor; bu durumda metin OnShown'da uygulanır.
            if (this.IsHandleCreated && this.Visible)
            {
                ApplyProgressPanel(panel => panel.Caption = caption);
            }
        }

        public override void SetDescription(string description)
        {
            base.SetDescription(description);
            _pendingDescription = description;

            if (this.IsHandleCreated && this.Visible)
            {
                ApplyProgressPanel(panel => panel.Description = description);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            string? pendingCaption = _pendingCaption;
            if (pendingCaption is not null)
            {
                ApplyProgressPanel(panel => panel.Caption = pendingCaption);
            }

            string? pendingDescription = _pendingDescription;
            if (pendingDescription is not null)
            {
                ApplyProgressPanel(panel => panel.Description = pendingDescription);
            }
        }

        #endregion

        /// <summary>
        /// İşlem bittiğinde kullanılır: animasyonu yavaşlatır ve başlığı başarı
        /// rengine çevirerek "tüm veriler yüklendi" onayını net gösterir.
        /// </summary>
        public void SetCompleted(string caption, string description)
        {
            SetCaption(caption);
            SetDescription(description);

            ApplyProgressPanel(panel =>
            {
                // Onay görünümü hiçbir koşulda bozulmamalı: animasyon ayarı
                // DevExpress tarafından reddedilirse renk ve metin yine de
                // uygulanır.
                try
                {
                    panel.AnimationSpeed = FrozenAnimationSpeed;
                }
                catch (ArgumentException)
                {
                }

                panel.AppearanceCaption.ForeColor = SkinTheme.Success;
                panel.AppearanceCaption.Options.UseForeColor = true;
            });
        }

        /// <summary>
        /// ProgressPanel'e yapılan görsel atamaları güvenle uygular.
        ///
        /// Bilinen DevExpress davranışı: yerleşim hesabında
        /// (<c>GetBestSize</c>) bazı durumlarda null dönüp
        /// <c>NullReferenceException</c> fırlatır. Görsel güncelleme başarısız
        /// olsa bile yükleme akışı ve pencerenin kapanışı bozulmamalıdır.
        /// </summary>
        private void ApplyProgressPanel(
            Action<DevExpress.XtraWaitForm.ProgressPanel> apply)
        {
            try
            {
                apply(this.progressPanel1);
            }
            catch (Exception ex) when (
                ex is NullReferenceException or InvalidOperationException)
            {
            }
        }
    }
}
