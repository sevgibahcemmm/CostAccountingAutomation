namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    partial class CurrentAccountMovementEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblAccountTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbAccountType;
        private DevExpress.XtraEditors.LabelControl lblAccountLabel;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpAccount;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpAccountView;
        private DevExpress.XtraEditors.LabelControl lblMovementTypeLabel;
        private DevExpress.XtraEditors.ComboBoxEdit cmbMovementType;
        private DevExpress.XtraEditors.LabelControl lblDateLabel;
        private DevExpress.XtraEditors.DateEdit dtDate;
        private DevExpress.XtraEditors.LabelControl lblDocNoLabel;
        private DevExpress.XtraEditors.TextEdit txtDocNo;
        private DevExpress.XtraEditors.LabelControl lblDebitLabel;
        private DevExpress.XtraEditors.SpinEdit spinDebit;
        private DevExpress.XtraEditors.LabelControl lblCreditLabel;
        private DevExpress.XtraEditors.SpinEdit spinCredit;
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
            this.lblAccountTypeLabel = new DevExpress.XtraEditors.LabelControl();
            this.cmbAccountType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblAccountLabel = new DevExpress.XtraEditors.LabelControl();
            this.lookUpAccount = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpAccountView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblMovementTypeLabel = new DevExpress.XtraEditors.LabelControl();
            this.cmbMovementType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblDateLabel = new DevExpress.XtraEditors.LabelControl();
            this.dtDate = new DevExpress.XtraEditors.DateEdit();
            this.lblDocNoLabel = new DevExpress.XtraEditors.LabelControl();
            this.txtDocNo = new DevExpress.XtraEditors.TextEdit();
            this.lblDebitLabel = new DevExpress.XtraEditors.LabelControl();
            this.spinDebit = new DevExpress.XtraEditors.SpinEdit();
            this.lblCreditLabel = new DevExpress.XtraEditors.LabelControl();
            this.spinCredit = new DevExpress.XtraEditors.SpinEdit();
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
            ((System.ComponentModel.ISupportInitialize)(this.cmbAccountType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccount.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccountView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMovementType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDocNo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinDebit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCredit.Properties)).BeginInit();
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
            this.lblTitle.Text = "Cari Hareket";

            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSubtitle.Appearance.Options.UseFont = true;
            this.lblSubtitle.Appearance.Options.UseForeColor = true;
            this.lblSubtitle.Location = new System.Drawing.Point(62, 38);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(220, 13);
            this.lblSubtitle.Text = "Tahsilat, ödeme veya borç/alacak fişi kaydedin";

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
            this.pnlBody.Controls.Add(this.spinCredit);
            this.pnlBody.Controls.Add(this.lblCreditLabel);
            this.pnlBody.Controls.Add(this.spinDebit);
            this.pnlBody.Controls.Add(this.lblDebitLabel);
            this.pnlBody.Controls.Add(this.txtDocNo);
            this.pnlBody.Controls.Add(this.lblDocNoLabel);
            this.pnlBody.Controls.Add(this.dtDate);
            this.pnlBody.Controls.Add(this.lblDateLabel);
            this.pnlBody.Controls.Add(this.cmbMovementType);
            this.pnlBody.Controls.Add(this.lblMovementTypeLabel);
            this.pnlBody.Controls.Add(this.lookUpAccount);
            this.pnlBody.Controls.Add(this.lblAccountLabel);
            this.pnlBody.Controls.Add(this.cmbAccountType);
            this.pnlBody.Controls.Add(this.lblAccountTypeLabel);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 68);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBody.Size = new System.Drawing.Size(520, 312);
            this.pnlBody.TabIndex = 1;

            // 
            // lblAccountTypeLabel
            // 
            this.lblAccountTypeLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAccountTypeLabel.Location = new System.Drawing.Point(20, 15);
            this.lblAccountTypeLabel.Name = "lblAccountTypeLabel";
            this.lblAccountTypeLabel.Size = new System.Drawing.Size(53, 15);
            this.lblAccountTypeLabel.Text = "Cari Türü:";

            // 
            // cmbAccountType
            // 
            this.cmbAccountType.Location = new System.Drawing.Point(20, 35);
            this.cmbAccountType.Name = "cmbAccountType";
            this.cmbAccountType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbAccountType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbAccountType.Size = new System.Drawing.Size(140, 26);

            // 
            // lblAccountLabel
            // 
            this.lblAccountLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAccountLabel.Location = new System.Drawing.Point(175, 15);
            this.lblAccountLabel.Name = "lblAccountLabel";
            this.lblAccountLabel.Size = new System.Drawing.Size(24, 15);
            this.lblAccountLabel.Text = "Cari:";

            // 
            // lookUpAccount
            // 
            this.lookUpAccount.Location = new System.Drawing.Point(175, 35);
            this.lookUpAccount.Name = "lookUpAccount";
            this.lookUpAccount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpAccount.Properties.PopupView = this.lookUpAccountView;
            this.lookUpAccount.Size = new System.Drawing.Size(325, 26);

            // 
            // lblMovementTypeLabel
            // 
            this.lblMovementTypeLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMovementTypeLabel.Location = new System.Drawing.Point(20, 75);
            this.lblMovementTypeLabel.Name = "lblMovementTypeLabel";
            this.lblMovementTypeLabel.Size = new System.Drawing.Size(61, 15);
            this.lblMovementTypeLabel.Text = "İşlem Türü:";

            // 
            // cmbMovementType
            // 
            this.cmbMovementType.Location = new System.Drawing.Point(20, 95);
            this.cmbMovementType.Name = "cmbMovementType";
            this.cmbMovementType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbMovementType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbMovementType.Size = new System.Drawing.Size(140, 26);

            // 
            // lblDateLabel
            // 
            this.lblDateLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDateLabel.Location = new System.Drawing.Point(175, 75);
            this.lblDateLabel.Name = "lblDateLabel";
            this.lblDateLabel.Size = new System.Drawing.Size(32, 15);
            this.lblDateLabel.Text = "Tarih:";

            // 
            // dtDate
            // 
            this.dtDate.EditValue = null;
            this.dtDate.Location = new System.Drawing.Point(175, 95);
            this.dtDate.Name = "dtDate";
            this.dtDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDate.Size = new System.Drawing.Size(140, 26);

            // 
            // lblDocNoLabel
            // 
            this.lblDocNoLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocNoLabel.Location = new System.Drawing.Point(330, 75);
            this.lblDocNoLabel.Name = "lblDocNoLabel";
            this.lblDocNoLabel.Size = new System.Drawing.Size(55, 15);
            this.lblDocNoLabel.Text = "Belge No:";

            // 
            // txtDocNo
            // 
            this.txtDocNo.Location = new System.Drawing.Point(330, 95);
            this.txtDocNo.Name = "txtDocNo";
            this.txtDocNo.Size = new System.Drawing.Size(170, 26);

            // 
            // lblDebitLabel
            // 
            this.lblDebitLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDebitLabel.Location = new System.Drawing.Point(20, 135);
            this.lblDebitLabel.Name = "lblDebitLabel";
            this.lblDebitLabel.Size = new System.Drawing.Size(69, 15);
            this.lblDebitLabel.Text = "Borç Tutarı:";

            // 
            // spinDebit
            // 
            this.spinDebit.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            this.spinDebit.Location = new System.Drawing.Point(20, 155);
            this.spinDebit.Name = "spinDebit";
            this.spinDebit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinDebit.Size = new System.Drawing.Size(235, 26);

            // 
            // lblCreditLabel
            // 
            this.lblCreditLabel.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCreditLabel.Location = new System.Drawing.Point(265, 135);
            this.lblCreditLabel.Name = "lblCreditLabel";
            this.lblCreditLabel.Size = new System.Drawing.Size(76, 15);
            this.lblCreditLabel.Text = "Alacak Tutarı:";

            // 
            // spinCredit
            // 
            this.spinCredit.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            this.spinCredit.Location = new System.Drawing.Point(265, 155);
            this.spinCredit.Name = "spinCredit";
            this.spinCredit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinCredit.Size = new System.Drawing.Size(235, 26);

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
            // CurrentAccountMovementEditForm
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
            this.Name = "CurrentAccountMovementEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cari Hareket";

            ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlHeaderLine)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbAccountType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccount.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpAccountView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbMovementType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDocNo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinDebit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCredit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDescription.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooter)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlFooterLine)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
