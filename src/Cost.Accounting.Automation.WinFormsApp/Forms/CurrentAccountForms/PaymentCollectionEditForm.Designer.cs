namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class PaymentCollectionEditForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;

        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;

        private DevExpress.XtraEditors.PanelControl pnlDetail;

        private DevExpress.XtraEditors.LabelControl lblCariTuru;
        private DevExpress.XtraEditors.LabelControl lblCariTuruValue;
        private DevExpress.XtraEditors.LabelControl lblMovementType;
        private DevExpress.XtraEditors.LabelControl lblMovementTypeValue;
        private DevExpress.XtraEditors.LabelControl lblCari;
        private DevExpress.XtraEditors.LabelControl lblCariValue;

        private DevExpress.XtraEditors.LabelControl lblDateCaption;
        private DevExpress.XtraEditors.DateEdit dtDate;
        private DevExpress.XtraEditors.LabelControl lblAmountCaption;
        private DevExpress.XtraEditors.SpinEdit spinAmount;

        private DevExpress.XtraEditors.LabelControl lblHint;

        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.SimpleButton btnCollect;
        private DevExpress.XtraEditors.SimpleButton btnPay;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

        private System.Windows.Forms.TableLayoutPanel tlpDetailFields;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaymentCollectionEditForm));
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            gridControl = new DevExpress.XtraGrid.GridControl();
            gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlDetail = new DevExpress.XtraEditors.PanelControl();
            tlpDetailFields = new TableLayoutPanel();
            lblCariTuru = new DevExpress.XtraEditors.LabelControl();
            lblCariTuruValue = new DevExpress.XtraEditors.LabelControl();
            lblMovementType = new DevExpress.XtraEditors.LabelControl();
            lblMovementTypeValue = new DevExpress.XtraEditors.LabelControl();
            lblCari = new DevExpress.XtraEditors.LabelControl();
            lblCariValue = new DevExpress.XtraEditors.LabelControl();
            lblDateCaption = new DevExpress.XtraEditors.LabelControl();
            dtDate = new DevExpress.XtraEditors.DateEdit();
            lblAmountCaption = new DevExpress.XtraEditors.LabelControl();
            spinAmount = new DevExpress.XtraEditors.SpinEdit();
            lblHint = new DevExpress.XtraEditors.LabelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            btnPay = new DevExpress.XtraEditors.SimpleButton();
            btnCollect = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlDetail).BeginInit();
            pnlDetail.SuspendLayout();
            tlpDetailFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinAmount.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.BackColor = Color.FromArgb(37, 47, 63);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(820, 70);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(200, 205, 214);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(20, 40);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(389, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Cari borç/alacak listesinden kayıt seçin, tutarı teyit edip işlemi tamamlayın";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 13F);
            lblTitle.Appearance.ForeColor = Color.White;
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(20, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(180, 23);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Ödeme / Tahsilat İşlemi";
            // 
            // gridControl
            // 
            gridControl.Dock = DockStyle.Top;
            gridControl.Location = new Point(0, 70);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.Size = new Size(820, 277);
            gridControl.TabIndex = 1;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            // 
            // gridView
            // 
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.ShowIndicator = false;
            // 
            // pnlDetail
            // 
            pnlDetail.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlDetail.Controls.Add(tlpDetailFields);
            pnlDetail.Controls.Add(lblHint);
            pnlDetail.Dock = DockStyle.Fill;
            pnlDetail.Location = new Point(0, 347);
            pnlDetail.Name = "pnlDetail";
            pnlDetail.Padding = new Padding(20, 16, 20, 16);
            pnlDetail.Size = new Size(820, 163);
            pnlDetail.TabIndex = 2;
            // 
            // tlpDetailFields
            // 
            tlpDetailFields.ColumnCount = 4;
            tlpDetailFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tlpDetailFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDetailFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpDetailFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDetailFields.Controls.Add(lblCariTuru, 0, 0);
            tlpDetailFields.Controls.Add(lblCariTuruValue, 1, 0);
            tlpDetailFields.Controls.Add(lblMovementType, 2, 0);
            tlpDetailFields.Controls.Add(lblMovementTypeValue, 3, 0);
            tlpDetailFields.Controls.Add(lblCari, 0, 1);
            tlpDetailFields.Controls.Add(lblCariValue, 1, 1);
            tlpDetailFields.Controls.Add(lblDateCaption, 0, 2);
            tlpDetailFields.Controls.Add(dtDate, 1, 2);
            tlpDetailFields.Controls.Add(lblAmountCaption, 2, 2);
            tlpDetailFields.Controls.Add(spinAmount, 3, 2);
            tlpDetailFields.Dock = DockStyle.Top;
            tlpDetailFields.Location = new Point(20, 16);
            tlpDetailFields.Name = "tlpDetailFields";
            tlpDetailFields.RowCount = 3;
            tlpDetailFields.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDetailFields.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDetailFields.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tlpDetailFields.Size = new Size(780, 96);
            tlpDetailFields.TabIndex = 0;
            // 
            // lblCariTuru
            // 
            lblCariTuru.Anchor = AnchorStyles.Left;
            lblCariTuru.Location = new Point(3, 9);
            lblCariTuru.Name = "lblCariTuru";
            lblCariTuru.Size = new Size(48, 13);
            lblCariTuru.TabIndex = 0;
            lblCariTuru.Text = "Cari Türü:";
            // 
            // lblCariTuruValue
            // 
            lblCariTuruValue.Anchor = AnchorStyles.Left;
            lblCariTuruValue.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblCariTuruValue.Appearance.Options.UseFont = true;
            lblCariTuruValue.Location = new Point(113, 8);
            lblCariTuruValue.Name = "lblCariTuruValue";
            lblCariTuruValue.Size = new Size(5, 15);
            lblCariTuruValue.TabIndex = 1;
            lblCariTuruValue.Text = "-";
            // 
            // lblMovementType
            // 
            lblMovementType.Anchor = AnchorStyles.Left;
            lblMovementType.Location = new Point(383, 9);
            lblMovementType.Name = "lblMovementType";
            lblMovementType.Size = new Size(54, 13);
            lblMovementType.TabIndex = 2;
            lblMovementType.Text = "İşlem Türü:";
            // 
            // lblMovementTypeValue
            // 
            lblMovementTypeValue.Anchor = AnchorStyles.Left;
            lblMovementTypeValue.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblMovementTypeValue.Appearance.Options.UseFont = true;
            lblMovementTypeValue.Location = new Point(513, 8);
            lblMovementTypeValue.Name = "lblMovementTypeValue";
            lblMovementTypeValue.Size = new Size(5, 15);
            lblMovementTypeValue.TabIndex = 3;
            lblMovementTypeValue.Text = "-";
            // 
            // lblCari
            // 
            lblCari.Anchor = AnchorStyles.Left;
            lblCari.Location = new Point(3, 41);
            lblCari.Name = "lblCari";
            lblCari.Size = new Size(23, 13);
            lblCari.TabIndex = 4;
            lblCari.Text = "Cari:";
            // 
            // lblCariValue
            // 
            lblCariValue.Anchor = AnchorStyles.Left;
            lblCariValue.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblCariValue.Appearance.Options.UseFont = true;
            lblCariValue.Location = new Point(113, 40);
            lblCariValue.Name = "lblCariValue";
            lblCariValue.Size = new Size(5, 15);
            lblCariValue.TabIndex = 5;
            lblCariValue.Text = "-";
            // 
            // lblDateCaption
            // 
            lblDateCaption.Anchor = AnchorStyles.Left;
            lblDateCaption.Location = new Point(3, 73);
            lblDateCaption.Name = "lblDateCaption";
            lblDateCaption.Size = new Size(28, 13);
            lblDateCaption.TabIndex = 6;
            lblDateCaption.Text = "Tarih:";
            // 
            // dtDate
            // 
            dtDate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            dtDate.EditValue = null;
            dtDate.Location = new Point(113, 70);
            dtDate.Name = "dtDate";
            dtDate.Properties.Mask.EditMask = "dd.MM.yyyy";
            dtDate.Size = new Size(264, 20);
            dtDate.TabIndex = 7;
            // 
            // lblAmountCaption
            // 
            lblAmountCaption.Anchor = AnchorStyles.Left;
            lblAmountCaption.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            lblAmountCaption.Appearance.Options.UseFont = true;
            lblAmountCaption.Location = new Point(383, 72);
            lblAmountCaption.Name = "lblAmountCaption";
            lblAmountCaption.Size = new Size(31, 15);
            lblAmountCaption.TabIndex = 8;
            lblAmountCaption.Text = "Tutar:";
            // 
            // spinAmount
            // 
            spinAmount.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            spinAmount.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinAmount.Location = new Point(513, 68);
            spinAmount.Name = "spinAmount";
            spinAmount.Properties.Appearance.Font = new Font("Segoe UI Semibold", 10F);
            spinAmount.Properties.Appearance.Options.UseFont = true;
            spinAmount.Size = new Size(264, 24);
            spinAmount.TabIndex = 9;
            // 
            // lblHint
            // 
            lblHint.Appearance.BackColor = Color.White;
            lblHint.Appearance.ForeColor = Color.FromArgb(107, 114, 128);
            lblHint.Appearance.Options.UseBackColor = true;
            lblHint.Appearance.Options.UseForeColor = true;
            lblHint.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblHint.Dock = DockStyle.Bottom;
            lblHint.Location = new Point(20, 118);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(780, 29);
            lblHint.TabIndex = 1;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnPay);
            pnlFooter.Controls.Add(btnCollect);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 510);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(820, 60);
            pnlFooter.TabIndex = 3;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.ImageOptions.Image = (Image)resources.GetObject("btnCancel.ImageOptions.Image");
            btnCancel.Location = new Point(700, 14);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Vazgeç";
            // 
            // btnPay
            // 
            btnPay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPay.Appearance.BackColor = Color.FromArgb(245, 158, 11);
            btnPay.Appearance.ForeColor = Color.White;
            btnPay.Appearance.Options.UseBackColor = true;
            btnPay.Appearance.Options.UseForeColor = true;
            btnPay.ImageOptions.Image = (Image)resources.GetObject("btnPay.ImageOptions.Image");
            btnPay.Location = new Point(570, 14);
            btnPay.Name = "btnPay";
            btnPay.Size = new Size(120, 34);
            btnPay.TabIndex = 1;
            btnPay.Text = "Borç Öde";
            // 
            // btnCollect
            // 
            btnCollect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCollect.Appearance.BackColor = Color.FromArgb(16, 185, 129);
            btnCollect.Appearance.ForeColor = Color.White;
            btnCollect.Appearance.Options.UseBackColor = true;
            btnCollect.Appearance.Options.UseForeColor = true;
            btnCollect.ImageOptions.Image = (Image)resources.GetObject("btnCollect.ImageOptions.Image");
            btnCollect.Location = new Point(440, 14);
            btnCollect.Name = "btnCollect";
            btnCollect.Size = new Size(120, 34);
            btnCollect.TabIndex = 0;
            btnCollect.Text = "Tahsil Et";
            // 
            // PaymentCollectionEditForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(820, 570);
            Controls.Add(pnlDetail);
            Controls.Add(gridControl);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            IconOptions.Image = (Image)resources.GetObject("PaymentCollectionEditForm.IconOptions.Image");
            MinimumSize = new Size(720, 520);
            Name = "PaymentCollectionEditForm";
            StartPosition = FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlDetail).EndInit();
            pnlDetail.ResumeLayout(false);
            tlpDetailFields.ResumeLayout(false);
            tlpDetailFields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinAmount.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}