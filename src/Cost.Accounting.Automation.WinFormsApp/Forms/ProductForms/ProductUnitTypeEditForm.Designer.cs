namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public partial class ProductUnitTypeEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblNameLabel;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.CheckEdit chkActive;
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new System.Windows.Forms.Panel();
            lblNameLabel = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlHeaderLine.SuspendLayout();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.Options.UseBackColor = false;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(480, 58);
            pnlHeader.TabIndex = 0;
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.Location = new System.Drawing.Point(18, 13);
            lblHeaderIcon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new System.Drawing.Size(32, 32);
            lblHeaderIcon.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new System.Drawing.Point(62, 12);
            lblTitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(120, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "-";
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(130, 138, 150);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new System.Drawing.Point(62, 35);
            lblSubtitle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(220, 14);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "-";
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = System.Drawing.Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlHeaderLine.Location = new System.Drawing.Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new System.Drawing.Size(480, 1);
            pnlHeaderLine.TabIndex = 2;
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(chkActive);
            pnlBody.Controls.Add(txtName);
            pnlBody.Controls.Add(lblNameLabel);
            pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlBody.Location = new System.Drawing.Point(0, 58);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new System.Drawing.Size(480, 220);
            pnlBody.TabIndex = 1;
            // 
            // lblNameLabel
            // 
            lblNameLabel.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            lblNameLabel.Appearance.Options.UseFont = true;
            lblNameLabel.Location = new System.Drawing.Point(24, 26);
            lblNameLabel.Name = "lblNameLabel";
            lblNameLabel.Size = new System.Drawing.Size(90, 16);
            lblNameLabel.TabIndex = 0;
            lblNameLabel.Text = "Birim Cinsi";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(24, 46);
            txtName.Name = "txtName";
            txtName.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtName.Properties.Appearance.Options.UseFont = true;
            txtName.Properties.NullText = "Örn. Adet, Kilogram, Litre...";
            txtName.Size = new System.Drawing.Size(432, 30);
            txtName.TabIndex = 1;
            // 
            // chkActive
            // 
            chkActive.Location = new System.Drawing.Point(24, 96);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Caption = "Aktif";
            chkActive.Size = new System.Drawing.Size(120, 24);
            chkActive.TabIndex = 2;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlFooter.Location = new System.Drawing.Point(0, 278);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new System.Drawing.Size(480, 60);
            pnlFooter.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(300, 13);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(84, 34);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Vazgeç";
            // 
            // btnSave
            // 
            btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.Location = new System.Drawing.Point(392, 13);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(76, 34);
            btnSave.TabIndex = 2;
            btnSave.Text = "Kaydet";
            // 
            // ProductUnitTypeEditForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(480, 338);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProductUnitTypeEditForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlHeaderLine.ResumeLayout(false);
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}