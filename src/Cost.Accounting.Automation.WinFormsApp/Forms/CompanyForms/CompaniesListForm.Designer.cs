namespace Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms
{
    public sealed partial class CompaniesListForm
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>Liste formuna özel araç çubuğu butonu.</summary>
        private DevExpress.XtraEditors.SimpleButton btnAccountingYear;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnAccountingYear = new DevExpress.XtraEditors.SimpleButton();

            btnAccountingYear.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnAccountingYear.Appearance.Options.UseFont = true;
            btnAccountingYear.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnAccountingYear.ImageOptions.SvgImage = Cost.Accounting.Automation.WinFormsApp.Utils.DxIcon.Recipe;
            btnAccountingYear.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnAccountingYear.Location = new System.Drawing.Point(1122, 16);
            btnAccountingYear.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnAccountingYear.Name = "btnAccountingYear";
            btnAccountingYear.Size = new System.Drawing.Size(126, 36);
            btnAccountingYear.TabIndex = 20;
            btnAccountingYear.Text = "Mali Yıl Aç";

            flpToolbar.Controls.Add(btnAccountingYear);

            // Başlıkta ikon + iki satırlık başlık kullanıldığı için kısa listelerde
            // fazla yer kaplamasın diye yükseklik 88'e çekilir.
            HeaderPanel.Height = 88;
        }
    }
}
