using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms
{
    sealed partial class ConsumptionUnitEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblName;
        private DevExpress.XtraEditors.TextEdit txtCode;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.CheckEdit chkActive;
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
            this.lblCode = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtCode = new DevExpress.XtraEditors.TextEdit();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.chkActive = new DevExpress.XtraEditors.CheckEdit();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)this.txtCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.chkActive.Properties).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(24, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Tüketim Birimi";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Location = new System.Drawing.Point(26, 50);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "900 (Tüketimler) altında yeni bir tüketim birimi tanımlayın.";
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Location = new System.Drawing.Point(24, 96);
            this.lblCode.Name = "lblCode";
            this.lblCode.Text = "Kod:";
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(120, 93);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(360, 24);
            this.txtCode.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(24, 136);
            this.lblName.Name = "lblName";
            this.lblName.Text = "Birim Adı:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(120, 133);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(360, 24);
            this.txtName.TabIndex = 1;
            // 
            // chkActive
            // 
            this.chkActive.Location = new System.Drawing.Point(120, 173);
            this.chkActive.Name = "chkActive";
            this.chkActive.Properties.Caption = "Aktif";
            this.chkActive.Size = new System.Drawing.Size(120, 24);
            this.chkActive.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(304, 240);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(84, 32);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(396, 240);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 32);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Kapat";
            // 
            // ConsumptionUnitEditForm
            // 
            this.ClientSize = new System.Drawing.Size(520, 300);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblCode);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.IconOptions.SvgImage = DxIcon.StockIssue;
            this.Name = "ConsumptionUnitEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tüketim Birimi";
            ((System.ComponentModel.ISupportInitialize)this.txtCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.chkActive.Properties).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}