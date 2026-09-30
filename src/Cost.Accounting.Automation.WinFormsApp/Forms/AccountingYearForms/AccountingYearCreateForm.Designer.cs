using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.AccountingYearForms
{
    sealed partial class AccountingYearCreateForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblCompanyLabel;
        private DevExpress.XtraEditors.TextEdit txtCompany;
        private DevExpress.XtraEditors.LabelControl lblYearLabel;
        private DevExpress.XtraEditors.SpinEdit spnYear;
        private DevExpress.XtraEditors.LabelControl lblDatabaseNameLabel;
        private DevExpress.XtraEditors.TextEdit txtDatabaseName;
        private DevExpress.XtraEditors.LabelControl lblPreview;
        private DevExpress.XtraEditors.LabelControl lblOpeningDateLabel;
        private DevExpress.XtraEditors.DateEdit dtOpeningDate;
        private DevExpress.XtraEditors.LabelControl lblInfo;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccountingYearCreateForm));
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new Panel();
            lblInfo = new DevExpress.XtraEditors.LabelControl();
            dtOpeningDate = new DevExpress.XtraEditors.DateEdit();
            lblOpeningDateLabel = new DevExpress.XtraEditors.LabelControl();
            lblPreview = new DevExpress.XtraEditors.LabelControl();
            txtDatabaseName = new DevExpress.XtraEditors.TextEdit();
            lblDatabaseNameLabel = new DevExpress.XtraEditors.LabelControl();
            spnYear = new DevExpress.XtraEditors.SpinEdit();
            lblYearLabel = new DevExpress.XtraEditors.LabelControl();
            txtCompany = new DevExpress.XtraEditors.TextEdit();
            lblCompanyLabel = new DevExpress.XtraEditors.LabelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dtOpeningDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtOpeningDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDatabaseName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spnYear.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtCompany.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 2, 3, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(446, 55);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 9F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(107, 114, 128);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(50, 31);
            lblSubtitle.Margin = new Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(221, 15);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Yıl veritabanını açar ve şemasını oluşturur.";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 11.5F);
            lblTitle.Appearance.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(50, 11);
            lblTitle.Margin = new Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(72, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Mali Yıl Aç";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
            lblHeaderIcon.ImageOptions.SvgImageSize = new Size(24, 24);
            lblHeaderIcon.Location = new Point(17, 16);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(24, 24);
            lblHeaderIcon.TabIndex = 2;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 54);
            pnlHeaderLine.Margin = new Padding(3, 2, 3, 2);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(446, 1);
            pnlHeaderLine.TabIndex = 2;
            // 
            // pnlBody
            // 
            pnlBody.BackColor = Color.White;
            pnlBody.Controls.Add(lblInfo);
            pnlBody.Controls.Add(dtOpeningDate);
            pnlBody.Controls.Add(lblOpeningDateLabel);
            pnlBody.Controls.Add(lblPreview);
            pnlBody.Controls.Add(txtDatabaseName);
            pnlBody.Controls.Add(lblDatabaseNameLabel);
            pnlBody.Controls.Add(spnYear);
            pnlBody.Controls.Add(lblYearLabel);
            pnlBody.Controls.Add(txtCompany);
            pnlBody.Controls.Add(lblCompanyLabel);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 55);
            pnlBody.Margin = new Padding(3, 2, 3, 2);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new Size(446, 282);
            pnlBody.TabIndex = 1;
            // 
            // lblInfo
            // 
            lblInfo.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblInfo.Appearance.ForeColor = Color.FromArgb(156, 163, 175);
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblInfo.Appearance.Options.UseTextOptions = true;
            lblInfo.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblInfo.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblInfo.Location = new Point(219, 222);
            lblInfo.Margin = new Padding(3, 2, 3, 2);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(206, 24);
            lblInfo.TabIndex = 9;
            lblInfo.Text = "Veritabanı kaydedildikten sonra adı değiştirilemez.";
            // 
            // dtOpeningDate
            // 
            dtOpeningDate.EditValue = new DateTime(2026, 9, 30, 0, 0, 0, 0);
            dtOpeningDate.Location = new Point(21, 222);
            dtOpeningDate.Margin = new Padding(3, 2, 3, 2);
            dtOpeningDate.Name = "dtOpeningDate";
            dtOpeningDate.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            dtOpeningDate.Properties.Appearance.Options.UseFont = true;
            dtOpeningDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtOpeningDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dtOpeningDate.Properties.Padding = new Padding(8, 4, 8, 4);
            dtOpeningDate.Size = new Size(189, 34);
            dtOpeningDate.TabIndex = 8;
            // 
            // lblOpeningDateLabel
            // 
            lblOpeningDateLabel.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblOpeningDateLabel.Appearance.ForeColor = Color.FromArgb(55, 65, 81);
            lblOpeningDateLabel.Appearance.Options.UseFont = true;
            lblOpeningDateLabel.Appearance.Options.UseForeColor = true;
            lblOpeningDateLabel.Location = new Point(21, 203);
            lblOpeningDateLabel.Margin = new Padding(3, 2, 3, 2);
            lblOpeningDateLabel.Name = "lblOpeningDateLabel";
            lblOpeningDateLabel.Size = new Size(67, 17);
            lblOpeningDateLabel.TabIndex = 7;
            lblOpeningDateLabel.Text = "Açılış Tarihi";
            // 
            // lblPreview
            // 
            lblPreview.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPreview.Appearance.ForeColor = Color.FromArgb(107, 114, 128);
            lblPreview.Appearance.Options.UseFont = true;
            lblPreview.Appearance.Options.UseForeColor = true;
            lblPreview.Appearance.Options.UseTextOptions = true;
            lblPreview.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblPreview.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblPreview.Location = new Point(21, 170);
            lblPreview.Margin = new Padding(3, 2, 3, 2);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(405, 26);
            lblPreview.TabIndex = 6;
            // 
            // txtDatabaseName
            // 
            txtDatabaseName.Location = new Point(21, 142);
            txtDatabaseName.Margin = new Padding(3, 2, 3, 2);
            txtDatabaseName.Name = "txtDatabaseName";
            txtDatabaseName.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtDatabaseName.Properties.Appearance.Options.UseFont = true;
            txtDatabaseName.Properties.Padding = new Padding(8, 4, 8, 4);
            txtDatabaseName.Size = new Size(405, 34);
            txtDatabaseName.TabIndex = 5;
            // 
            // lblDatabaseNameLabel
            // 
            lblDatabaseNameLabel.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblDatabaseNameLabel.Appearance.ForeColor = Color.FromArgb(55, 65, 81);
            lblDatabaseNameLabel.Appearance.Options.UseFont = true;
            lblDatabaseNameLabel.Appearance.Options.UseForeColor = true;
            lblDatabaseNameLabel.Location = new Point(21, 124);
            lblDatabaseNameLabel.Margin = new Padding(3, 2, 3, 2);
            lblDatabaseNameLabel.Name = "lblDatabaseNameLabel";
            lblDatabaseNameLabel.Size = new Size(85, 17);
            lblDatabaseNameLabel.TabIndex = 4;
            lblDatabaseNameLabel.Text = "Veritabanı Adı";
            // 
            // spnYear
            // 
            spnYear.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spnYear.Location = new Point(21, 89);
            spnYear.Margin = new Padding(3, 2, 3, 2);
            spnYear.Name = "spnYear";
            spnYear.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            spnYear.Properties.Appearance.Options.UseFont = true;
            spnYear.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Up), new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Down) });
            spnYear.Properties.Padding = new Padding(8, 4, 8, 4);
            spnYear.Size = new Size(120, 34);
            spnYear.TabIndex = 3;
            // 
            // lblYearLabel
            // 
            lblYearLabel.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblYearLabel.Appearance.ForeColor = Color.FromArgb(55, 65, 81);
            lblYearLabel.Appearance.Options.UseFont = true;
            lblYearLabel.Appearance.Options.UseForeColor = true;
            lblYearLabel.Location = new Point(21, 70);
            lblYearLabel.Margin = new Padding(3, 2, 3, 2);
            lblYearLabel.Name = "lblYearLabel";
            lblYearLabel.Size = new Size(43, 17);
            lblYearLabel.TabIndex = 2;
            lblYearLabel.Text = "Mali Yıl";
            // 
            // txtCompany
            // 
            txtCompany.Location = new Point(21, 35);
            txtCompany.Margin = new Padding(3, 2, 3, 2);
            txtCompany.Name = "txtCompany";
            txtCompany.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            txtCompany.Properties.Appearance.Options.UseFont = true;
            txtCompany.Properties.Padding = new Padding(8, 4, 8, 4);
            txtCompany.Properties.ReadOnly = true;
            txtCompany.Size = new Size(405, 34);
            txtCompany.TabIndex = 1;
            // 
            // lblCompanyLabel
            // 
            lblCompanyLabel.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            lblCompanyLabel.Appearance.ForeColor = Color.FromArgb(55, 65, 81);
            lblCompanyLabel.Appearance.Options.UseFont = true;
            lblCompanyLabel.Appearance.Options.UseForeColor = true;
            lblCompanyLabel.Location = new Point(21, 16);
            lblCompanyLabel.Margin = new Padding(3, 2, 3, 2);
            lblCompanyLabel.Name = "lblCompanyLabel";
            lblCompanyLabel.Size = new Size(41, 17);
            lblCompanyLabel.TabIndex = 0;
            lblCompanyLabel.Text = "Kurum";
            // 
            // pnlFooter
            // 
            pnlFooter.Appearance.BackColor = Color.FromArgb(248, 249, 250);
            pnlFooter.Appearance.Options.UseBackColor = true;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 337);
            pnlFooter.Margin = new Padding(3, 2, 3, 2);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(446, 49);
            pnlFooter.TabIndex = 2;
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Margin = new Padding(3, 2, 3, 2);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(446, 1);
            pnlFooterLine.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Cursor = Cursors.Hand;
            btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSave.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnSave.ImageOptions.SvgImage");
            btnSave.ImageOptions.SvgImageSize = new Size(18, 18);
            btnSave.Location = new Point(350, 11);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(77, 28);
            btnSave.TabIndex = 0;
            btnSave.Text = "Aç";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnCancel.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
            btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
            btnCancel.Location = new Point(267, 11);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(77, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";
            // 
            // AccountingYearCreateForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(446, 386);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            IconOptions.ShowIcon = false;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AccountingYearCreateForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Mali Yıl Aç";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dtOpeningDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtOpeningDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDatabaseName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spnYear.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtCompany.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}
