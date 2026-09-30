using Cost.Accounting.Automation.WinFormsApp.Utils;
namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductDetailForm
    {
        private System.ComponentModel.IContainer components = null!;
        private DevExpress.XtraEditors.PanelControl accentBar = default!;
        private DevExpress.XtraEditors.PanelControl headerPanel = default!;
        private DevExpress.XtraEditors.PictureEdit picHeader = default!;
        private DevExpress.XtraEditors.LabelControl lblHeaderTitle = default!;
        private DevExpress.XtraEditors.LabelControl lblHeaderSub = default!;
        private DevExpress.XtraEditors.PanelControl summaryPanel = default!;
        private DevExpress.XtraEditors.LabelControl lblSummary = default!;
        private DevExpress.XtraEditors.LabelControl lblInfo = default!;
        private DevExpress.XtraEditors.LabelControl lblMovementTitle = default!;
        private DevExpress.XtraGrid.GridControl gridMovements = default!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewMovements = default!;
        private DevExpress.XtraEditors.PanelControl footerDivider = default!;
        private DevExpress.XtraEditors.SimpleButton btnPrices = default!;
        private DevExpress.XtraEditors.SimpleButton btnClose = default!;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                skinBinding?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductDetailForm));
            accentBar = new DevExpress.XtraEditors.PanelControl();
            headerPanel = new DevExpress.XtraEditors.PanelControl();
            lblHeaderSub = new DevExpress.XtraEditors.LabelControl();
            lblHeaderTitle = new DevExpress.XtraEditors.LabelControl();
            picHeader = new DevExpress.XtraEditors.PictureEdit();
            summaryPanel = new DevExpress.XtraEditors.PanelControl();
            lblSummary = new DevExpress.XtraEditors.LabelControl();
            lblInfo = new DevExpress.XtraEditors.LabelControl();
            lblMovementTitle = new DevExpress.XtraEditors.LabelControl();
            gridMovements = new DevExpress.XtraGrid.GridControl();
            viewMovements = new DevExpress.XtraGrid.Views.Grid.GridView();
            footerDivider = new DevExpress.XtraEditors.PanelControl();
            btnPrices = new DevExpress.XtraEditors.SimpleButton();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)accentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).BeginInit();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)summaryPanel).BeginInit();
            summaryPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMovements).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewMovements).BeginInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).BeginInit();
            SuspendLayout();
            // 
            // accentBar
            // 
            accentBar.Appearance.Options.UseBackColor = true;
            accentBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            accentBar.Location = new Point(0, 0);
            accentBar.Margin = new Padding(0);
            accentBar.Name = "accentBar";
            accentBar.Size = new Size(1120, 8);
            accentBar.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Appearance.Options.UseBackColor = true;
            headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerPanel.Controls.Add(lblHeaderSub);
            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(picHeader);
            headerPanel.Location = new Point(0, 4);
            headerPanel.Margin = new Padding(0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1120, 64);
            headerPanel.TabIndex = 1;
            // 
            // lblHeaderSub
            // 
            lblHeaderSub.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblHeaderSub.Appearance.Options.UseFont = true;
            lblHeaderSub.Appearance.Options.UseForeColor = true;
            lblHeaderSub.Appearance.Options.UseTextOptions = true;
            lblHeaderSub.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblHeaderSub.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblHeaderSub.Location = new Point(66, 34);
            lblHeaderSub.Margin = new Padding(3, 2, 3, 2);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(5, 15);
            lblHeaderSub.TabIndex = 2;
            lblHeaderSub.Text = "-";
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblHeaderTitle.Appearance.Options.UseFont = true;
            lblHeaderTitle.Appearance.Options.UseForeColor = true;
            lblHeaderTitle.Appearance.Options.UseTextOptions = true;
            lblHeaderTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblHeaderTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblHeaderTitle.Location = new Point(66, 8);
            lblHeaderTitle.Margin = new Padding(3, 2, 3, 2);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(46, 28);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Ürün";
            // 
            // picHeader
            // 
            picHeader.EditValue = resources.GetObject("picHeader.EditValue");
            picHeader.Location = new Point(20, 14);
            picHeader.Margin = new Padding(3, 2, 3, 2);
            picHeader.Name = "picHeader";
            picHeader.Properties.Appearance.BackColor = Color.Transparent;
            picHeader.Properties.Appearance.Options.UseBackColor = true;
            picHeader.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picHeader.Size = new Size(34, 34);
            picHeader.TabIndex = 0;
            // 
            // summaryPanel
            // 
            summaryPanel.Appearance.Options.UseBackColor = true;
            summaryPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            summaryPanel.Controls.Add(lblSummary);
            summaryPanel.Location = new Point(20, 76);
            summaryPanel.Margin = new Padding(3, 2, 3, 2);
            summaryPanel.Name = "summaryPanel";
            summaryPanel.Size = new Size(1080, 40);
            summaryPanel.TabIndex = 2;
            // 
            // lblSummary
            // 
            lblSummary.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblSummary.Appearance.Options.UseFont = true;
            lblSummary.Appearance.Options.UseForeColor = true;
            lblSummary.Appearance.Options.UseTextOptions = true;
            lblSummary.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblSummary.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblSummary.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSummary.Location = new Point(6, 6);
            lblSummary.Margin = new Padding(3, 2, 3, 2);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new Size(5, 17);
            lblSummary.TabIndex = 0;
            lblSummary.Text = "-";
            // 
            // lblInfo
            // 
            lblInfo.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblInfo.Appearance.Options.UseTextOptions = true;
            lblInfo.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblInfo.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblInfo.Location = new Point(22, 122);
            lblInfo.Margin = new Padding(3, 2, 3, 2);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(5, 15);
            lblInfo.TabIndex = 3;
            lblInfo.Text = "-";
            // 
            // lblMovementTitle
            // 
            lblMovementTitle.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMovementTitle.Appearance.Options.UseFont = true;
            lblMovementTitle.Appearance.Options.UseForeColor = true;
            lblMovementTitle.Appearance.Options.UseTextOptions = true;
            lblMovementTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblMovementTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblMovementTitle.Location = new Point(22, 142);
            lblMovementTitle.Margin = new Padding(3, 2, 3, 2);
            lblMovementTitle.Name = "lblMovementTitle";
            lblMovementTitle.Size = new Size(145, 17);
            lblMovementTitle.TabIndex = 4;
            lblMovementTitle.Text = "Belgeli Stok Hareketleri";
            // 
            // gridMovements
            // 
            gridMovements.Location = new Point(20, 164);
            gridMovements.MainView = viewMovements;
            gridMovements.Name = "gridMovements";
            gridMovements.Size = new Size(1080, 486);
            gridMovements.TabIndex = 5;
            gridMovements.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewMovements });
            // 
            // viewMovements
            // 
            viewMovements.GridControl = gridMovements;
            viewMovements.Name = "viewMovements";
            viewMovements.OptionsBehavior.AutoPopulateColumns = false;
            viewMovements.OptionsBehavior.Editable = false;
            viewMovements.OptionsView.ShowGroupPanel = false;
            viewMovements.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            viewMovements.OptionsView.ShowIndicator = false;
            // 
            // footerDivider
            // 
            footerDivider.Appearance.Options.UseBackColor = true;
            footerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            footerDivider.Location = new Point(20, 656);
            footerDivider.Margin = new Padding(0);
            footerDivider.Name = "footerDivider";
            footerDivider.Size = new Size(1080, 1);
            footerDivider.TabIndex = 6;
            // 
            // btnPrices
            // 
            btnPrices.Appearance.Font = new Font("Segoe UI", 10F);
            btnPrices.Appearance.Options.UseFont = true;
            btnPrices.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnPrices.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnPrices.ImageOptions.SvgImage");
            btnPrices.ImageOptions.SvgImageSize = new Size(18, 18);
            btnPrices.Location = new Point(20, 672);
            btnPrices.Margin = new Padding(3, 2, 3, 2);
            btnPrices.Name = "btnPrices";
            btnPrices.Size = new Size(135, 36);
            btnPrices.TabIndex = 7;
            btnPrices.Text = "Fiyat Geçmişi";
            // 
            // btnClose
            // 
            btnClose.Appearance.Font = new Font("Segoe UI", 10F);
            btnClose.Appearance.Options.UseFont = true;
            btnClose.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClose.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnClose.ImageOptions.SvgImage");
            btnClose.ImageOptions.SvgImageSize = new Size(18, 18);
            btnClose.Location = new Point(992, 672);
            btnClose.Margin = new Padding(3, 2, 3, 2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(108, 36);
            btnClose.TabIndex = 8;
            btnClose.Text = "Kapat";
            // 
            // ProductDetailForm
            // 
            CancelButton = btnClose;
            ClientSize = new Size(1120, 724);
            Controls.Add(gridMovements);
            Controls.Add(lblMovementTitle);
            Controls.Add(lblInfo);
            Controls.Add(summaryPanel);
            Controls.Add(headerPanel);
            Controls.Add(footerDivider);
            Controls.Add(btnClose);
            Controls.Add(btnPrices);
            Controls.Add(accentBar);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("ProductDetailForm.IconOptions.SvgImage");
            MinimumSize = new Size(900, 600);
            Name = "ProductDetailForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ürün Detayı";
            ((System.ComponentModel.ISupportInitialize)accentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).EndInit();
            headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)summaryPanel).EndInit();
            summaryPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridMovements).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewMovements).EndInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).EndInit();
            ResumeLayout(false);
        }
        #endregion
    }
}