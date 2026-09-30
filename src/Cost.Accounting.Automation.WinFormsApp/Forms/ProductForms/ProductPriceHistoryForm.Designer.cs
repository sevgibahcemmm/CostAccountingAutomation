namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    partial class ProductPriceHistoryForm
    {
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblCurrent = null!;
        private DevExpress.XtraEditors.LabelControl lblEmpty = null!;
        private DevExpress.XtraGrid.GridControl gridPrices = null!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewPrices = null!;
        private DevExpress.XtraEditors.SimpleButton btnClose = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                skinBinding?.Dispose();
                gridPrices?.Dispose();
                btnClose?.Dispose();
                lblTitle?.Dispose();
                lblCurrent?.Dispose();
                lblEmpty?.Dispose();
                base.Dispose(disposing);
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductPriceHistoryForm));
            gridPrices = new DevExpress.XtraGrid.GridControl();
            viewPrices = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblCurrent = new DevExpress.XtraEditors.LabelControl();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            lblEmpty = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)gridPrices).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewPrices).BeginInit();
            SuspendLayout();
            // 
            // gridPrices
            // 
            gridPrices.Location = new Point(0, 0);
            gridPrices.MainView = viewPrices;
            gridPrices.Name = "gridPrices";
            gridPrices.Size = new Size(766, 369);
            gridPrices.TabIndex = 2;
            gridPrices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewPrices });
            // 
            // viewPrices
            // 
            viewPrices.GridControl = gridPrices;
            viewPrices.Name = "viewPrices";
            viewPrices.OptionsBehavior.Editable = false;
            viewPrices.OptionsView.ShowGroupPanel = false;
            viewPrices.OptionsView.ShowIndicator = false;
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseTextOptions = true;
            lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(0, 17);
            lblTitle.TabIndex = 0;
            // 
            // lblCurrent
            // 
            lblCurrent.Appearance.Options.UseTextOptions = true;
            lblCurrent.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblCurrent.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblCurrent.Location = new Point(0, 0);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new Size(0, 13);
            lblCurrent.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClose.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnClose.ImageOptions.SvgImage");
            btnClose.ImageOptions.SvgImageSize = new Size(18, 18);
            btnClose.Location = new Point(0, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(180, 34);
            btnClose.TabIndex = 3;
            btnClose.Text = "Kapat";
            // 
            // lblEmpty
            // 
            lblEmpty.Appearance.Font = new Font("Segoe UI", 10F);
            lblEmpty.Appearance.Options.UseFont = true;
            lblEmpty.Appearance.Options.UseTextOptions = true;
            lblEmpty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblEmpty.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblEmpty.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblEmpty.Location = new Point(0, 0);
            lblEmpty.Name = "lblEmpty";
            lblEmpty.Size = new Size(0, 17);
            lblEmpty.TabIndex = 4;
            lblEmpty.Text = "Bu ürün için tanımlı fiyat kaydı bulunmuyor. Fiyat & Stok Listesi'ndeki\r\n\"Satış Fiyatı / Fotoğraf\" butonu ile satış fiyatı tanımlayabilirsiniz.";
            // 
            // ProductPriceHistoryForm
            // 
            ClientSize = new Size(778, 437);
            Controls.Add(lblTitle);
            Controls.Add(lblCurrent);
            Controls.Add(gridPrices);
            Controls.Add(btnClose);
            Controls.Add(lblEmpty);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("ProductPriceHistoryForm.IconOptions.SvgImage");
            MinimumSize = new Size(600, 320);
            Name = "ProductPriceHistoryForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Fiyat Geçmişi";
            ((System.ComponentModel.ISupportInitialize)gridPrices).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewPrices).EndInit();
            ResumeLayout(false);
        }
    }
}
