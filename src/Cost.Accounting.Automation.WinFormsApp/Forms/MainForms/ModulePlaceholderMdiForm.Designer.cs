namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class ModulePlaceholderMdiForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHeader;
        private DevExpress.XtraEditors.PictureEdit picIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblNote;

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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblSub = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.picIcon = new DevExpress.XtraEditors.PictureEdit();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.lblNote = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.picIcon);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1024, 120);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblSub
            // 
            this.lblSub.AutoSize = true;
            this.lblSub.BackColor = System.Drawing.Color.Transparent;
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblSub.Location = new System.Drawing.Point(148, 74);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(16, 25);
            this.lblSub.TabIndex = 2;
            this.lblSub.Text = "-";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(144, 28);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(83, 46);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Modül";
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(48, 26);
            this.picIcon.Name = "picIcon";
            this.picIcon.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.picIcon.Properties.Appearance.Options.UseBackColor = true;
            this.picIcon.Properties.NullText = " ";
            this.picIcon.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.picIcon.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.None;
            this.picIcon.Size = new System.Drawing.Size(72, 72);
            this.picIcon.TabIndex = 0;
            // 
            // pnlBody
            // 
            this.pnlBody.Controls.Add(this.lblNote);
            this.pnlBody.Controls.Add(this.lblDesc);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 120);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(1024, 560);
            this.pnlBody.TabIndex = 1;
            // 
            // lblNote
            // 
            this.lblNote.AutoSize = true;
            this.lblNote.BackColor = System.Drawing.Color.Transparent;
            this.lblNote.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblNote.Location = new System.Drawing.Point(340, 268);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(235, 21);
            this.lblNote.TabIndex = 1;
            this.lblNote.Text = "Bu modül geliştirme aşamasındadır.";
            this.lblNote.Anchor = System.Windows.Forms.AnchorStyles.None;
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.BackColor = System.Drawing.Color.Transparent;
            this.lblDesc.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDesc.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblDesc.Location = new System.Drawing.Point(300, 226);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(323, 37);
            this.lblDesc.TabIndex = 0;
            this.lblDesc.Text = "Modül içeriği burada görüntülenecek";
            this.lblDesc.Anchor = System.Windows.Forms.AnchorStyles.None;
            // 
            // ModulePlaceholderMdiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.ClientSize = new System.Drawing.Size(1024, 680);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlHeader);
            this.Name = "ModulePlaceholderMdiForm";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}