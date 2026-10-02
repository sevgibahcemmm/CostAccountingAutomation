using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms
{
    sealed partial class ManualAccountEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblParent;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblName;
        private DevExpress.XtraEditors.LookUpEdit cmbParent;
        private DevExpress.XtraEditors.TextEdit txtCode;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.ToggleSwitch chkActive;
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
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblParent = new System.Windows.Forms.Label();
            this.lblCode = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.cmbParent = new DevExpress.XtraEditors.LookUpEdit();
            this.txtCode = new DevExpress.XtraEditors.TextEdit();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.chkActive = new DevExpress.XtraEditors.ToggleSwitch();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)this.cmbParent.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.txtCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.chkActive.Properties).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(24, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Manüel Hesap / Alt Hesap Ekle";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Location = new System.Drawing.Point(26, 48);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "Üst hesabı seçerek alt kod ekleyin. 900 (Tüketimler) dahil tüm hesap planı kapsanır.";
            // 
            // lblParent
            // 
            this.lblParent.AutoSize = true;
            this.lblParent.Location = new System.Drawing.Point(24, 92);
            this.lblParent.Name = "lblParent";
            this.lblParent.Text = "Üst Hesap:";
            // 
            // cmbParent
            // 
            this.cmbParent.Location = new System.Drawing.Point(120, 88);
            this.cmbParent.Name = "cmbParent";
            this.cmbParent.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbParent.Size = new System.Drawing.Size(400, 24);
            this.cmbParent.TabIndex = 0;
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Location = new System.Drawing.Point(24, 128);
            this.lblCode.Name = "lblCode";
            this.lblCode.Text = "Kod:";
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(120, 124);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(400, 24);
            this.txtCode.TabIndex = 1;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(24, 164);
            this.lblName.Name = "lblName";
            this.lblName.Text = "Hesap Adı:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(120, 160);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(400, 24);
            this.txtName.TabIndex = 2;
            // 
            // chkActive
            // 
            this.chkActive.IsOn = true;
            this.chkActive.Location = new System.Drawing.Point(120, 200);
            this.chkActive.Name = "chkActive";
            chkActive.Properties.ShowText = true;
            chkActive.Properties.OnText = "Aktif";
            chkActive.Properties.OffText = "Pasif";
            chkActive.ToolTip = "Aktif";
            this.chkActive.Size = new System.Drawing.Size(120, 24);
            this.chkActive.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(336, 272);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(84, 34);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Kaydet";
            this.btnSave.ImageOptions.SvgImage = DxIcon.Check;
            this.btnSave.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(428, 272);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 34);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Kapat";
            this.btnCancel.ImageOptions.SvgImage = DxIcon.Close;
            this.btnCancel.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // ManualAccountEditForm
            // 
            this.ClientSize = new System.Drawing.Size(560, 340);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.cmbParent);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblCode);
            this.Controls.Add(this.lblParent);
            this.IconOptions.SvgImage = DxIcon.ChartAccounts;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManualAccountEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Yeni Hesap Kaydı";
            ((System.ComponentModel.ISupportInitialize)this.cmbParent.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.txtCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.chkActive.Properties).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
