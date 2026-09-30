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

        public WaitForm()
        {
            InitializeComponent();
            this.progressPanel1.AutoHeight = true;
        }

        #region Overrides

        public override void SetCaption(string caption)
        {
            base.SetCaption(caption);
            this.progressPanel1.Caption = caption;
        }

        public override void SetDescription(string description)
        {
            base.SetDescription(description);
            this.progressPanel1.Description = description;
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

            // Onay görünümü hiçbir koşulda bozulmamalı: animasyon ayarı
            // DevExpress tarafından reddedilirse renk ve metin yine de uygulanır.
            try
            {
                this.progressPanel1.AnimationSpeed = FrozenAnimationSpeed;
            }
            catch (ArgumentException)
            {
            }

            this.progressPanel1.AppearanceCaption.ForeColor =
                SkinTheme.Success;

            this.progressPanel1.AppearanceCaption.Options.UseForeColor = true;
        }
    }
}
