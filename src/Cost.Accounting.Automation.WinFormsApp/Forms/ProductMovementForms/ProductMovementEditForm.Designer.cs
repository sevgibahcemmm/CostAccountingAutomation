namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductMovementForms
{
    partial class ProductMovementEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblProductLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpProduct;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpProductView;
        private DevExpress.XtraEditors.LabelControl lblTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbMovementType;
        private DevExpress.XtraEditors.LabelControl lblQuantityLabel;
        private DevExpress.XtraEditors.SpinEdit spinQuantity;
        private DevExpress.XtraEditors.LabelControl lblPriceLabel;
        private DevExpress.XtraEditors.SpinEdit spinPrice;
        private DevExpress.XtraEditors.LabelControl lblDateLabel;
        private DevExpress.XtraEditors.DateEdit dtDate;
        private DevExpress.XtraEditors.LabelControl lblRefLabel;
        private DevExpress.XtraEditors.TextEdit txtReferenceNo;
        private DevExpress.XtraEditors.LabelControl lblDescLabel;
        private DevExpress.XtraEditors.TextEdit txtDescription;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new DevExpress.XtraEditors.PanelControl();
            this.lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            this.pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.lblProductLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpProduct = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpProductView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblTypeLabel = new DevExpress.XtraEditors.LabelControl();
            this.cmbMovementType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblQuantityLabel = new DevExpress.XtraEditors.LabelControl();
            this.spinQuantity = new DevExpress.XtraEditors.SpinEdit();
            this.lblPriceLabel = new DevExpress.XtraEditors.LabelControl();
            this.spinPrice = new DevExpress.XtraEditors.SpinEdit();
            this.lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            this.dtDate = new DevExpress.XtraEditors.DateEdit();
            this.lblRefLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtReferenceNo = new DevExpress.XtraEditors.TextEdit();
            this.lblDescLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtDescription = new DevExpress.XtraEditors.TextEdit();
            this.pnlFooter = new DevExpress.XtraEditors.PanelControl();
            this.pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).BeginInit();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).BeginInit();
            this.pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProduct.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProductView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMovementType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReferenceNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).BeginInit();
            this.pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).BeginInit();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlHeader.Appearance.Options.UseBackColor = true;
            this.pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblHeaderIcon);
            this.pnlHeader.Controls.Add(this.pnlHeaderLine);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(520, 68);
            this.pnlHeader.TabIndex = 0;

            // 
            // lblHeaderIcon
            // 
            this.lblHeaderIcon.Location = new System.Drawing.Point(20, 18);
            this.lblHeaderIcon.Name = "lblHeaderIcon";
            this.lblHeaderIcon.Size = new System.Drawing.Size(32, 32);

            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(62, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(95, 21);
            this.lblTitle.Text = "Stok Hareketi";

            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSubtitle.Appearance.Options.UseFont = true;
            this.lblSubtitle.Appearance.Options.UseForeColor = true;
            this.lblSubtitle.Location = new System.Drawing.Point(62, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(200, 13);
            this.lblSubtitle.Text = "Stok giriş veya çıkış fiş bilgilerini giriniz";

            // 
            // pnlHeaderLine
            // 
            this.pnlHeaderLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlHeaderLine.Appearance.Options.UseBackColor = true;
            this.pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHeaderLine.Location = new System.Drawing.Point(0, 67);
            this.pnlHeaderLine.Name = "pnlHeaderLine";
            this.pnlHeaderLine.Size = new System.Drawing.Size(520, 1);

            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.txtDescription);
            this.pnlBody.Controls.Add(this.lblDescLabel);
            this.pnlBody.Controls.Add(this.txtReferenceNo);
            this.pnlBody.Controls.Add(this.lblRefLabel);
            this.pnlBody.Controls.Add(this.dtDate);
            this.pnlBody.Controls.Add(this.lblDateLabel);
            this.pnlBody.Controls.Add(this.spinPrice);
            this.pnlBody.Controls.Add(this.lblPriceLabel);
            this.pnlBody.Controls.Add(this.spinQuantity);
            this.pnlBody.Controls.Add(this.lblQuantityLabel);
            this.pnlBody.Controls.Add(this.cmbMovementType);
            this.pnlBody.Controls.Add(this.lblTypeLabel);
            this.pnlBody.Controls.Add(this.lookUpProduct);
            this.pnlBody.Controls.Add(this.lblProductLabel);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 68);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBody.Size = new System.Drawing.Size(520, 312);
            this.pnlBody.TabIndex = 1;

            // 
            // lblProductLabel
            // 
            this.lblProductLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblProductLabel.Location = new System.Drawing.Point(20, 15);
            this.lblProductLabel.Name = "lblProductLabel";
            this.lblProductLabel.Size = new System.Drawing.Size(30, 15);
            this.lblProductLabel.Text = "Ürün:";

            // 
            // lookUpProduct
            // 
            this.lookUpProduct.Location = new System.Drawing.Point(20, 35);
            this.lookUpProduct.Name = "lookUpProduct";
            this.lookUpProduct.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpProduct.Properties.PopupView = this.lookUpProductView;
            this.lookUpProduct.Size = new System.Drawing.Size(480, 26);

            // 
            // lblTypeLabel
            // 
            this.lblTypeLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTypeLabel.Location = new System.Drawing.Point(20, 75);
            this.lblTypeLabel.Name = "lblTypeLabel";
            this.lblTypeLabel.Size = new System.Drawing.Size(73, 15);
            this.lblTypeLabel.Text = "Hareket Türü:";

            // 
            // cmbMovementType
            // 
            this.cmbMovementType.Location = new System.Drawing.Point(20, 95);
            this.cmbMovementType.Name = "cmbMovementType";
            this.cmbMovementType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbMovementType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbMovementType.Size = new System.Drawing.Size(150, 26);

            // 
            // lblQuantityLabel
            // 
            this.lblQuantityLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuantityLabel.Location = new System.Drawing.Point(185, 75);
            this.lblQuantityLabel.Name = "lblQuantityLabel";
            this.lblQuantityLabel.Size = new System.Drawing.Size(40, 15);
            this.lblQuantityLabel.Text = "Miktar:";

            // 
            // spinQuantity
            // 
            this.spinQuantity.EditValue = new decimal(new int[] { 1, 0, 0, 0 });
            this.spinQuantity.Location = new System.Drawing.Point(185, 95);
            this.spinQuantity.Name = "spinQuantity";
            this.spinQuantity.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinQuantity.Properties.MinValue = new decimal(new int[] { 1, 0, 0, 262144 });
            this.spinQuantity.Size = new System.Drawing.Size(145, 26);

            // 
            // lblPriceLabel
            // 
            this.lblPriceLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPriceLabel.Location = new System.Drawing.Point(345, 75);
            this.lblPriceLabel.Name = "lblPriceLabel";
            this.lblPriceLabel.Size = new System.Drawing.Size(61, 15);
            this.lblPriceLabel.Text = "Birim Fiyat:";

            // 
            // spinPrice
            // 
            this.spinPrice.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            this.spinPrice.Location = new System.Drawing.Point(345, 95);
            this.spinPrice.Name = "spinPrice";
            this.spinPrice.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinPrice.Size = new System.Drawing.Size(155, 26);

            // 
            // lblDateLabel
            // 
            this.lblDateLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDateLabel.Location = new System.Drawing.Point(20, 135);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(32, 15);
            this.lblDateLabel.Text = "Tarih:";

            // 
            // dtDate
            // 
            this.dtDate.EditValue = null;
            this.dtDate.Location = new System.Drawing.Point(20, 155);
            this.dtDate.Name = "dtDate";
            this.dtDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDate.Size = new System.Drawing.Size(150, 26);

            // 
            // lblRefLabel
            // 
            this.lblRefLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRefLabel.Location = new System.Drawing.Point(185, 135);
            this.lblRefLabel.Name = "lblRefLabel";
            this.lblRefLabel.Size = new System.Drawing.Size(117, 15);
            this.lblRefLabel.Text = "Belge / Referans No:";

            // 
            // txtReferenceNo
            // 
            this.txtReferenceNo.Location = new System.Drawing.Point(185, 155);
            this.txtReferenceNo.Name = "txtReferenceNo";
            this.txtReferenceNo.Size = new System.Drawing.Size(315, 26);

            // 
            // lblDescLabel
            // 
            this.lblDescLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDescLabel.Location = new System.Drawing.Point(20, 195);
            this.lblDescLabel.Name = "lblDescLabel";
            this.lblDescLabel.Size = new System.Drawing.Size(53, 15);
            this.lblDescLabel.Text = "Açıklama:";

            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(20, 215);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(480, 26);

            // 
            // pnlFooter
            // 
            this.pnlFooter.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            this.pnlFooter.Appearance.Options.UseBackColor = true;
            this.pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Controls.Add(this.pnlFooterLine);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 380);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(520, 60);
            this.pnlFooter.TabIndex = 2;

            // 
            // pnlFooterLine
            // 
            this.pnlFooterLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.pnlFooterLine.Appearance.Options.UseBackColor = true;
            this.pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFooterLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFooterLine.Location = new System.Drawing.Point(0, 0);
            this.pnlFooterLine.Name = "pnlFooterLine";
            this.pnlFooterLine.Size = new System.Drawing.Size(520, 1);

            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(300, 14);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 34);
            this.btnSave.Text = "Kaydet";

            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Location = new System.Drawing.Point(406, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 34);
            this.btnCancel.Text = "Vazgeç";

            // 
            // ProductMovementEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 440);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProductMovementEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Stok Hareketi";

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProduct.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpProductView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMovementType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinQuantity.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinPrice.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtReferenceNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
