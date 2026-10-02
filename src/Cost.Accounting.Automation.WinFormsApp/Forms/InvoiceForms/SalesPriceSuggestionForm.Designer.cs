using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    public sealed partial class SalesPriceSuggestionForm
    {
        private System.ComponentModel.IContainer components = null!;

        private DevExpress.XtraEditors.PanelControl accentBar = default!;
        private DevExpress.XtraEditors.PanelControl headerPanel = default!;
        private DevExpress.XtraEditors.PictureEdit picHeader = default!;
        private DevExpress.XtraEditors.LabelControl lblHeaderTitle = default!;
        private DevExpress.XtraEditors.LabelControl lblHeaderSub = default!;
        private DevExpress.XtraEditors.PanelControl headerDivider = default!;
        private DevExpress.XtraEditors.GroupControl infoPanel = default!;
        private DevExpress.XtraEditors.LabelControl lblCostCaption = default!;
        private DevExpress.XtraEditors.LabelControl lblCostValue = default!;
        private DevExpress.XtraEditors.LabelControl lblTaxCaption = default!;
        private DevExpress.XtraEditors.LabelControl lblTaxValue = default!;
        private DevExpress.XtraEditors.LabelControl lblFormulaCaption = default!;
        private DevExpress.XtraEditors.LabelControl lblFormulaValue = default!;
        private DevExpress.XtraEditors.LabelControl lblPriceCaption = default!;
        private DevExpress.XtraEditors.SpinEdit spPrice = default!;
        private DevExpress.XtraEditors.CheckEdit chkSaveAsSalePrice = default!;
        private DevExpress.XtraEditors.LabelControl lblInfo = default!;
        private DevExpress.XtraEditors.PanelControl footerDivider = default!;
        private DevExpress.XtraEditors.SimpleButton btnUse = default!;
        private DevExpress.XtraEditors.SimpleButton btnCancel = default!;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SalesPriceSuggestionForm));
            accentBar = new DevExpress.XtraEditors.PanelControl();
            headerPanel = new DevExpress.XtraEditors.PanelControl();
            lblHeaderSub = new DevExpress.XtraEditors.LabelControl();
            lblHeaderTitle = new DevExpress.XtraEditors.LabelControl();
            picHeader = new DevExpress.XtraEditors.PictureEdit();
            headerDivider = new DevExpress.XtraEditors.PanelControl();
            infoPanel = new DevExpress.XtraEditors.GroupControl();
            lblFormulaValue = new DevExpress.XtraEditors.LabelControl();
            lblFormulaCaption = new DevExpress.XtraEditors.LabelControl();
            lblTaxValue = new DevExpress.XtraEditors.LabelControl();
            lblTaxCaption = new DevExpress.XtraEditors.LabelControl();
            lblCostValue = new DevExpress.XtraEditors.LabelControl();
            lblCostCaption = new DevExpress.XtraEditors.LabelControl();
            lblPriceCaption = new DevExpress.XtraEditors.LabelControl();
            spPrice = new DevExpress.XtraEditors.SpinEdit();
            chkSaveAsSalePrice = new DevExpress.XtraEditors.CheckEdit();
            lblInfo = new DevExpress.XtraEditors.LabelControl();
            footerDivider = new DevExpress.XtraEditors.PanelControl();
            btnUse = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)accentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).BeginInit();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)infoPanel).BeginInit();
            infoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)spPrice.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkSaveAsSalePrice.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).BeginInit();
            SuspendLayout();
            // 
            // accentBar
            // 
            accentBar.Appearance.BackColor = Color.FromArgb(64, 120, 200);
            accentBar.Appearance.Options.UseBackColor = true;
            accentBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            accentBar.Location = new Point(0, 0);
            accentBar.Margin = new Padding(0);
            accentBar.Name = "accentBar";
            accentBar.Size = new Size(516, 10);
            accentBar.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Appearance.BackColor = Color.FromArgb(248, 249, 251);
            headerPanel.Appearance.Options.UseBackColor = true;
            headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerPanel.Controls.Add(lblHeaderSub);
            headerPanel.Controls.Add(lblHeaderTitle);
            headerPanel.Controls.Add(picHeader);
            headerPanel.Location = new Point(0, 4);
            headerPanel.Margin = new Padding(3, 2, 3, 2);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(516, 64);
            headerPanel.TabIndex = 1;
            // 
            // lblHeaderSub
            // 
            lblHeaderSub.Appearance.Font = new Font("Segoe UI", 9F);
            lblHeaderSub.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblHeaderSub.Appearance.Options.UseFont = true;
            lblHeaderSub.Appearance.Options.UseForeColor = true;
            lblHeaderSub.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblHeaderSub.Location = new Point(66, 39);
            lblHeaderSub.Margin = new Padding(3, 2, 3, 2);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(400, 15);
            lblHeaderSub.TabIndex = 2;
            lblHeaderSub.Text = "-";
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblHeaderTitle.Appearance.Options.UseFont = true;
            lblHeaderTitle.Location = new Point(66, 10);
            lblHeaderTitle.Margin = new Padding(3, 2, 3, 2);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(199, 28);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Satış Fiyatı Belirleme";
            // 
            // picHeader
            // 
            picHeader.EditValue = resources.GetObject("picHeader.EditValue");
            picHeader.Location = new Point(20, 15);
            picHeader.Margin = new Padding(3, 2, 3, 2);
            picHeader.Name = "picHeader";
            picHeader.Properties.Appearance.BackColor = Color.Transparent;
            picHeader.Properties.Appearance.Options.UseBackColor = true;
            picHeader.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picHeader.Size = new Size(34, 34);
            picHeader.TabIndex = 0;
            // 
            // headerDivider
            // 
            headerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            headerDivider.Appearance.Options.UseBackColor = true;
            headerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerDivider.Location = new Point(0, 68);
            headerDivider.Margin = new Padding(0);
            headerDivider.Name = "headerDivider";
            headerDivider.Size = new Size(516, 1);
            headerDivider.TabIndex = 2;
            // 
            // infoPanel
            // 
            infoPanel.AllowHtmlText = true;
            infoPanel.AppearanceCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            infoPanel.AppearanceCaption.ForeColor = Color.FromArgb(64, 120, 200);
            infoPanel.AppearanceCaption.Options.UseFont = true;
            infoPanel.AppearanceCaption.Options.UseForeColor = true;
            infoPanel.Controls.Add(lblFormulaValue);
            infoPanel.Controls.Add(lblFormulaCaption);
            infoPanel.Controls.Add(lblTaxValue);
            infoPanel.Controls.Add(lblTaxCaption);
            infoPanel.Controls.Add(lblCostValue);
            infoPanel.Controls.Add(lblCostCaption);
            infoPanel.Location = new Point(24, 84);
            infoPanel.Margin = new Padding(3, 2, 3, 2);
            infoPanel.Name = "infoPanel";
            infoPanel.Size = new Size(468, 128);
            infoPanel.TabIndex = 3;
            infoPanel.Text = "FİYAT HESABI";
            // 
            // lblFormulaValue
            // 
            lblFormulaValue.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblFormulaValue.Appearance.ForeColor = Color.FromArgb(130, 136, 146);
            lblFormulaValue.Appearance.Options.UseFont = true;
            lblFormulaValue.Appearance.Options.UseForeColor = true;
            lblFormulaValue.Location = new Point(120, 98);
            lblFormulaValue.Margin = new Padding(3, 2, 3, 2);
            lblFormulaValue.Name = "lblFormulaValue";
            lblFormulaValue.Size = new Size(186, 13);
            lblFormulaValue.TabIndex = 5;
            lblFormulaValue.Text = "(Maliyet + KDV) -> %10 Kâr -> + KDV";
            // 
            // lblFormulaCaption
            // 
            lblFormulaCaption.Appearance.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblFormulaCaption.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblFormulaCaption.Appearance.Options.UseFont = true;
            lblFormulaCaption.Appearance.Options.UseForeColor = true;
            lblFormulaCaption.Location = new Point(24, 98);
            lblFormulaCaption.Margin = new Padding(3, 2, 3, 2);
            lblFormulaCaption.Name = "lblFormulaCaption";
            lblFormulaCaption.Size = new Size(38, 13);
            lblFormulaCaption.TabIndex = 4;
            lblFormulaCaption.Text = "HESAP:";
            // 
            // lblTaxValue
            // 
            lblTaxValue.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTaxValue.Appearance.Options.UseFont = true;
            lblTaxValue.Appearance.Options.UseTextOptions = true;
            lblTaxValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblTaxValue.Location = new Point(228, 62);
            lblTaxValue.Margin = new Padding(3, 2, 3, 2);
            lblTaxValue.Name = "lblTaxValue";
            lblTaxValue.Size = new Size(11, 17);
            lblTaxValue.TabIndex = 3;
            lblTaxValue.Text = "%";
            // 
            // lblTaxCaption
            // 
            lblTaxCaption.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblTaxCaption.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblTaxCaption.Appearance.Options.UseFont = true;
            lblTaxCaption.Appearance.Options.UseForeColor = true;
            lblTaxCaption.Location = new Point(24, 62);
            lblTaxCaption.Margin = new Padding(3, 2, 3, 2);
            lblTaxCaption.Name = "lblTaxCaption";
            lblTaxCaption.Size = new Size(61, 17);
            lblTaxCaption.TabIndex = 2;
            lblTaxCaption.Text = "KDV Oranı";
            // 
            // lblCostValue
            // 
            lblCostValue.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCostValue.Appearance.Options.UseFont = true;
            lblCostValue.Appearance.Options.UseTextOptions = true;
            lblCostValue.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblCostValue.Location = new Point(228, 32);
            lblCostValue.Margin = new Padding(3, 2, 3, 2);
            lblCostValue.Name = "lblCostValue";
            lblCostValue.Size = new Size(37, 17);
            lblCostValue.TabIndex = 1;
            lblCostValue.Text = "0,00 ₺";
            // 
            // lblCostCaption
            // 
            lblCostCaption.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblCostCaption.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblCostCaption.Appearance.Options.UseFont = true;
            lblCostCaption.Appearance.Options.UseForeColor = true;
            lblCostCaption.Location = new Point(24, 32);
            lblCostCaption.Margin = new Padding(3, 2, 3, 2);
            lblCostCaption.Name = "lblCostCaption";
            lblCostCaption.Size = new Size(75, 17);
            lblCostCaption.TabIndex = 0;
            lblCostCaption.Text = "Maliyet Fiyatı";
            // 
            // lblPriceCaption
            // 
            lblPriceCaption.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPriceCaption.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblPriceCaption.Appearance.Options.UseFont = true;
            lblPriceCaption.Appearance.Options.UseForeColor = true;
            lblPriceCaption.Location = new Point(24, 232);
            lblPriceCaption.Margin = new Padding(3, 2, 3, 2);
            lblPriceCaption.Name = "lblPriceCaption";
            lblPriceCaption.Size = new Size(149, 17);
            lblPriceCaption.TabIndex = 4;
            lblPriceCaption.Text = "Önerilen Satış Fiyatı (₺):";
            // 
            // spPrice
            // 
            spPrice.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spPrice.Location = new Point(196, 228);
            spPrice.Margin = new Padding(3, 2, 3, 2);
            spPrice.Name = "spPrice";
            spPrice.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spPrice.Properties.DisplayFormat.FormatString = "n2";
            spPrice.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            spPrice.Properties.EditFormat.FormatString = "n2";
            spPrice.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            spPrice.Properties.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            spPrice.Properties.MaxValue = new decimal(new int[] { 999999999, 0, 0, 0 });
            spPrice.Properties.MinValue = new decimal(new int[] { 1, 0, 0, 0 });
            spPrice.Size = new Size(296, 20);
            spPrice.TabIndex = 5;
            // 
            // chkSaveAsSalePrice
            // 
            chkSaveAsSalePrice.EditValue = true;
            chkSaveAsSalePrice.Location = new Point(24, 282);
            chkSaveAsSalePrice.Margin = new Padding(3, 2, 3, 2);
            chkSaveAsSalePrice.Name = "chkSaveAsSalePrice";
            chkSaveAsSalePrice.Properties.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            chkSaveAsSalePrice.Properties.Appearance.Options.UseFont = true;
            chkSaveAsSalePrice.Properties.Caption = "Bu fiyatı ürünün satış fiyatı olarak KAYDET";
            chkSaveAsSalePrice.Size = new Size(468, 21);
            chkSaveAsSalePrice.TabIndex = 6;
            // 
            // lblInfo
            // 
            lblInfo.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblInfo.Appearance.ForeColor = Color.FromArgb(130, 136, 146);
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblInfo.Location = new Point(24, 314);
            lblInfo.Margin = new Padding(3, 2, 3, 2);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(340, 13);
            lblInfo.TabIndex = 7;
            lblInfo.Text = "Kaydedilirse sonraki satış faturalarında bu fiyat otomatik kullanılır.";
            // 
            // footerDivider
            // 
            footerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            footerDivider.Appearance.Options.UseBackColor = true;
            footerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            footerDivider.Location = new Point(24, 402);
            footerDivider.Margin = new Padding(0);
            footerDivider.Name = "footerDivider";
            footerDivider.Size = new Size(468, 1);
            footerDivider.TabIndex = 8;
            // 
            // btnUse
            // 
            btnUse.Appearance.BackColor = Color.FromArgb(64, 120, 200);
            btnUse.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUse.Appearance.ForeColor = Color.White;
            btnUse.Appearance.Options.UseBackColor = true;
            btnUse.Appearance.Options.UseFont = true;
            btnUse.Appearance.Options.UseForeColor = true;
            btnUse.AppearanceHovered.BackColor = Color.FromArgb(46, 94, 166);
            btnUse.AppearanceHovered.ForeColor = Color.White;
            btnUse.AppearanceHovered.Options.UseBackColor = true;
            btnUse.AppearanceHovered.Options.UseForeColor = true;
            btnUse.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnUse.ImageOptions.SvgImage");
            btnUse.Location = new Point(328, 430);
            btnUse.Margin = new Padding(3, 2, 3, 2);
            btnUse.Name = "btnUse";
            btnUse.Size = new Size(164, 36);
            btnUse.TabIndex = 10;
            btnUse.Text = "Fiyatı Kullan";
            // 
            // btnCancel
            // 
            btnCancel.Appearance.Font = new Font("Segoe UI", 10F);
            btnCancel.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.AppearanceHovered.BackColor = Color.FromArgb(244, 245, 247);
            btnCancel.AppearanceHovered.Options.UseBackColor = true;
            btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
            btnCancel.Location = new Point(224, 430);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(96, 36);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Vazgeç";
            // 
            // SalesPriceSuggestionForm
            // 
            AcceptButton = btnUse;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(516, 490);
            Controls.Add(btnCancel);
            Controls.Add(btnUse);
            Controls.Add(footerDivider);
            Controls.Add(lblInfo);
            Controls.Add(chkSaveAsSalePrice);
            Controls.Add(spPrice);
            Controls.Add(lblPriceCaption);
            Controls.Add(infoPanel);
            Controls.Add(headerDivider);
            Controls.Add(headerPanel);
            Controls.Add(accentBar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SalesPriceSuggestionForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Satış Fiyatı Belirleme";
            ((System.ComponentModel.ISupportInitialize)accentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).EndInit();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).EndInit();
            ((System.ComponentModel.ISupportInitialize)infoPanel).EndInit();
            infoPanel.ResumeLayout(false);
            infoPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)spPrice.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkSaveAsSalePrice.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}