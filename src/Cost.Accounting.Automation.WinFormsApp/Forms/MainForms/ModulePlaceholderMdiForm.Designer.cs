using DevExpress.XtraEditors;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class ModulePlaceholderMdiForm
    {
        private System.ComponentModel.IContainer components = null;

        private PanelControl pnlHeader;
        private DevExpress.XtraEditors.PictureEdit picIcon;
        private DevExpress.XtraEditors.SimpleButton btnClosePage;
        private LabelControl lblTitle;
        private LabelControl lblSub;
        private PanelControl pnlBody;
        private LabelControl lblDesc;
        private LabelControl lblNote;

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
            this.pnlHeader = new PanelControl();
            this.lblSub = new LabelControl();
            this.lblTitle = new LabelControl();
            this.picIcon = new DevExpress.XtraEditors.PictureEdit();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            this.pnlBody = new PanelControl();
            this.lblNote = new LabelControl();
            this.lblDesc = new LabelControl();
            this.pnlHeader.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.picIcon);
            this.pnlHeader.Controls.Add(this.btnClosePage);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1024, 110);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblSub
            // 
            this.lblSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSub.Appearance.ForeColor = SkinTheme.SecondaryText;
            this.lblSub.Appearance.Options.UseFont = true;
            this.lblSub.Appearance.Options.UseForeColor = true;
            this.lblSub.Location = new System.Drawing.Point(136, 66);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(16, 25);
            this.lblSub.TabIndex = 2;
            this.lblSub.Text = "-";
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Location = new System.Drawing.Point(134, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(83, 46);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Modül";
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(48, 19);
            this.picIcon.Name = "picIcon";
            this.picIcon.Properties.NullText = " ";
            this.picIcon.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.picIcon.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
            this.picIcon.Size = new System.Drawing.Size(72, 72);
            this.picIcon.TabIndex = 0;
            // 
            // btnClosePage
            // 
            this.btnClosePage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClosePage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClosePage.Appearance.Options.UseFont = true;
            this.btnClosePage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            this.btnClosePage.ImageOptions.SvgImage = DxIcon.Close;
            this.btnClosePage.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.btnClosePage.Location = new System.Drawing.Point(916, 37);
            this.btnClosePage.Name = "btnClosePage";
            this.btnClosePage.Size = new System.Drawing.Size(92, 36);
            this.btnClosePage.TabIndex = 3;
            this.btnClosePage.Text = "Kapat";
            // 
            // pnlBody
            // 
            this.pnlBody.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlBody.Controls.Add(this.lblNote);
            this.pnlBody.Controls.Add(this.lblDesc);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 96);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(1024, 560);
            this.pnlBody.TabIndex = 1;
            // 
            // lblNote
            // 
            this.lblNote.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNote.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNote.Appearance.ForeColor = SkinTheme.SecondaryText;
            this.lblNote.Appearance.Options.UseFont = true;
            this.lblNote.Appearance.Options.UseForeColor = true;
            this.lblNote.Location = new System.Drawing.Point(340, 268);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(235, 21);
            this.lblNote.TabIndex = 1;
            this.lblNote.Text = "Bu modül geliştirme aşamasındadır.";
            // 
            // lblDesc
            // 
            this.lblDesc.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDesc.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDesc.Appearance.Options.UseFont = true;
            this.lblDesc.Location = new System.Drawing.Point(300, 226);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(323, 37);
            this.lblDesc.TabIndex = 0;
            this.lblDesc.Text = "Modül içeriği burada görüntülenecek";
            // 
            // ModulePlaceholderMdiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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