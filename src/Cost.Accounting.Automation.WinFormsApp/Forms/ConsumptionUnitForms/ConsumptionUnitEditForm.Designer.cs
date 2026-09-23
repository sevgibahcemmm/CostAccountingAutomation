using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms
{
    sealed partial class ConsumptionUnitEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblCodeLabel;
        private DevExpress.XtraEditors.TextEdit txtCode;
        private DevExpress.XtraEditors.LabelControl lblNameLabel;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.CheckEdit chkActive;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new System.Windows.Forms.Panel();
            lblCodeLabel = new DevExpress.XtraEditors.LabelControl();
            txtCode = new DevExpress.XtraEditors.TextEdit();
            lblNameLabel = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();

            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(460, 68);
            pnlHeader.TabIndex = 0;

            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = DxIcon.StockIssue;
            lblHeaderIcon.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            lblHeaderIcon.Location = new System.Drawing.Point(20, 20);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new System.Drawing.Size(28, 28);
            lblHeaderIcon.TabIndex = 2;

            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 11.5F);
            lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(17, 24, 39);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new System.Drawing.Point(58, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(150, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Tüketim Birimi";

            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new System.Drawing.Point(58, 38);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(320, 15);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Alt açıklama metni";

            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlHeaderLine.Location = new System.Drawing.Point(0, 67);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new System.Drawing.Size(460, 1);
            pnlHeaderLine.TabIndex = 2;

            // 
            // pnlBody
            // 
            pnlBody.BackColor = System.Drawing.Color.White;
            pnlBody.Controls.Add(chkActive);
            pnlBody.Controls.Add(txtName);
            pnlBody.Controls.Add(lblNameLabel);
            pnlBody.Controls.Add(txtCode);
            pnlBody.Controls.Add(lblCodeLabel);
            pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlBody.Location = new System.Drawing.Point(0, 68);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new System.Drawing.Size(460, 188);
            pnlBody.TabIndex = 1;

            // 
            // lblCodeLabel
            // 
            lblCodeLabel.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            lblCodeLabel.Appearance.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            lblCodeLabel.Appearance.Options.UseFont = true;
            lblCodeLabel.Appearance.Options.UseForeColor = true;
            lblCodeLabel.Location = new System.Drawing.Point(24, 20);
            lblCodeLabel.Name = "lblCodeLabel";
            lblCodeLabel.Size = new System.Drawing.Size(30, 17);
            lblCodeLabel.TabIndex = 0;
            lblCodeLabel.Text = "Kod";

            // 
            // txtCode
            // 
            txtCode.Location = new System.Drawing.Point(24, 43);
            txtCode.Name = "txtCode";
            txtCode.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            txtCode.Properties.Appearance.Options.UseFont = true;
            txtCode.Properties.NullText = "Boş bırakılırsa otomatik üretilir";
            txtCode.Properties.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            txtCode.Size = new System.Drawing.Size(412, 30);
            txtCode.TabIndex = 1;

            // 
            // lblNameLabel
            // 
            lblNameLabel.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            lblNameLabel.Appearance.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            lblNameLabel.Appearance.Options.UseFont = true;
            lblNameLabel.Appearance.Options.UseForeColor = true;
            lblNameLabel.Location = new System.Drawing.Point(24, 86);
            lblNameLabel.Name = "lblNameLabel";
            lblNameLabel.Size = new System.Drawing.Size(65, 17);
            lblNameLabel.TabIndex = 0;
            lblNameLabel.Text = "Birim Adı";

            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(24, 109);
            txtName.Name = "txtName";
            txtName.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            txtName.Properties.Appearance.Options.UseFont = true;
            txtName.Properties.NullText = "Örn. Ürün Başına İşçilik";
            txtName.Properties.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            txtName.Size = new System.Drawing.Size(412, 30);
            txtName.TabIndex = 2;

            // 
            // chkActive
            // 
            chkActive.Location = new System.Drawing.Point(24, 156);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Appearance.Options.UseForeColor = true;
            chkActive.Properties.Caption = "Kullanımda (Aktif)";
            chkActive.Size = new System.Drawing.Size(160, 24);
            chkActive.TabIndex = 3;

            // 
            // pnlFooter
            // 
            pnlFooter.Appearance.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            pnlFooter.Appearance.Options.UseBackColor = true;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlFooter.Location = new System.Drawing.Point(0, 256);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new System.Drawing.Size(460, 60);
            pnlFooter.TabIndex = 2;

            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.BackColor = System.Drawing.Color.FromArgb(229, 231, 235);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = System.Windows.Forms.DockStyle.Top;
            pnlFooterLine.Location = new System.Drawing.Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new System.Drawing.Size(460, 1);
            pnlFooterLine.TabIndex = 2;

            // 
            // btnCancel
            // 
            btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            btnCancel.Appearance.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = DxIcon.Close;
            btnCancel.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            btnCancel.Location = new System.Drawing.Point(252, 13);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 34);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";

            // 
            // btnSave
            // 
            btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSave.ImageOptions.SvgImage = DxIcon.Check;
            btnSave.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnSave.Location = new System.Drawing.Point(348, 13);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(90, 34);
            btnSave.TabIndex = 0;
            btnSave.Text = "Kaydet";

            // 
            // ConsumptionUnitEditForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(460, 316);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConsumptionUnitEditForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Tüketim Birimi";

            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}