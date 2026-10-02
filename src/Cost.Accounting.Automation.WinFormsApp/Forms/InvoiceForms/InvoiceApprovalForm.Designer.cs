namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    public sealed partial class InvoiceApprovalForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.LabelControl lblMethod;
        private DevExpress.XtraEditors.ComboBoxEdit cmbMethod;

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
            this.lblMethod = new DevExpress.XtraEditors.LabelControl();
            this.cmbMethod = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)this.cmbMethod.Properties).BeginInit();
            this.SuspendLayout();
            // 
            // lblMethod
            // 
            this.lblMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMethod.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMethod.Appearance.Options.UseFont = true;
            this.lblMethod.Location = new System.Drawing.Point(664, 20);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(150, 20);
            this.lblMethod.TabIndex = 13;
            this.lblMethod.Text = "Maliyet Yöntemi:";
            // 
            // cmbMethod
            // 
            this.cmbMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMethod.Location = new System.Drawing.Point(824, 13);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbMethod.Size = new System.Drawing.Size(250, 30);
            this.cmbMethod.TabIndex = 14;
            // 
            // InvoiceApprovalForm
            // 
            this.ClientSize = new System.Drawing.Size(1280, 680);
            this.ToolbarPanel.Controls.Add(this.cmbMethod);
            this.ToolbarPanel.Controls.Add(this.lblMethod);
            this.Name = "InvoiceApprovalForm";
            this.Text = "Fatura Onaylama";
            ((System.ComponentModel.ISupportInitialize)this.cmbMethod.Properties).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
