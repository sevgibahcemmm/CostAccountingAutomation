using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms
{
public sealed partial class RecipeMaterialSelectionForm
{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl accentBar;
        private DevExpress.XtraEditors.PanelControl headerPanel;
        private DevExpress.XtraEditors.PictureEdit picHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderTitle;
        private DevExpress.XtraEditors.LabelControl lblHeaderSub;
        private DevExpress.XtraEditors.PanelControl headerDivider;
        private DevExpress.XtraEditors.LabelControl lblSection;
        private DevExpress.XtraEditors.CheckedListBoxControl checkedList;
        private DevExpress.XtraEditors.SimpleButton btnSelectAll;
        private DevExpress.XtraEditors.SimpleButton btnClear;
        private DevExpress.XtraEditors.PanelControl badge;
        private DevExpress.XtraEditors.LabelControl lblBadge;
        private DevExpress.XtraEditors.PanelControl badgeAccentStrip;
        private DevExpress.XtraEditors.LabelControl lblInfo;
        private DevExpress.XtraEditors.PanelControl footerDivider;
        private DevExpress.XtraEditors.SimpleButton btnOk;
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
            accentBar = new DevExpress.XtraEditors.PanelControl();
            headerPanel = new DevExpress.XtraEditors.PanelControl();
            lblHeaderSub = new DevExpress.XtraEditors.LabelControl();
            lblHeaderTitle = new DevExpress.XtraEditors.LabelControl();
            picHeader = new DevExpress.XtraEditors.PictureEdit();
            headerDivider = new DevExpress.XtraEditors.PanelControl();
            lblSection = new DevExpress.XtraEditors.LabelControl();
            checkedList = new DevExpress.XtraEditors.CheckedListBoxControl();
            btnSelectAll = new DevExpress.XtraEditors.SimpleButton();
            btnClear = new DevExpress.XtraEditors.SimpleButton();
            badge = new DevExpress.XtraEditors.PanelControl();
            lblBadge = new DevExpress.XtraEditors.LabelControl();
            badgeAccentStrip = new DevExpress.XtraEditors.PanelControl();
            lblInfo = new DevExpress.XtraEditors.LabelControl();
            footerDivider = new DevExpress.XtraEditors.PanelControl();
            btnOk = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)accentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).BeginInit();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)checkedList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)badge).BeginInit();
            badge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)badgeAccentStrip).BeginInit();
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
            accentBar.Size = new Size(640, 10);
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
            headerPanel.Location = new Point(0, 9);
            headerPanel.Margin = new Padding(3, 2, 3, 2);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(640, 64);
            headerPanel.TabIndex = 1;
            // 
            // lblHeaderSub
            // 
            lblHeaderSub.Appearance.Font = new Font("Segoe UI", 9F);
            lblHeaderSub.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblHeaderSub.Appearance.Options.UseFont = true;
            lblHeaderSub.Appearance.Options.UseForeColor = true;
            lblHeaderSub.Location = new Point(68, 38);
            lblHeaderSub.Margin = new Padding(3, 2, 3, 2);
            lblHeaderSub.Name = "lblHeaderSub";
            lblHeaderSub.Size = new Size(420, 15);
            lblHeaderSub.TabIndex = 2;
            lblHeaderSub.Text = "...";
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblHeaderTitle.Appearance.Options.UseFont = true;
            lblHeaderTitle.Location = new Point(66, 10);
            lblHeaderTitle.Margin = new Padding(3, 2, 3, 2);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(330, 28);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Reçeteye Eklenecek Malzemeler";
            // 
            // picHeader
            // 
            picHeader.Location = new Point(20, 15);
            picHeader.Margin = new Padding(3, 2, 3, 2);
            picHeader.Name = "picHeader";
            picHeader.Properties.Appearance.BackColor = Color.Transparent;
            picHeader.Properties.Appearance.Options.UseBackColor = true;
            picHeader.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picHeader.Size = new Size(34, 34);
            picHeader.TabIndex = 0;
            picHeader.SvgImage = DxIcon.Recipe;
            // 
            // headerDivider
            // 
            headerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            headerDivider.Appearance.Options.UseBackColor = true;
            headerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerDivider.Location = new Point(24, 76);
            headerDivider.Margin = new Padding(0);
            headerDivider.Name = "headerDivider";
            headerDivider.Size = new Size(592, 1);
            headerDivider.TabIndex = 2;
            // 
            // lblSection
            // 
            lblSection.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblSection.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblSection.Appearance.Options.UseFont = true;
            lblSection.Appearance.Options.UseForeColor = true;
            lblSection.Location = new Point(24, 88);
            lblSection.Margin = new Padding(3, 2, 3, 2);
            lblSection.Name = "lblSection";
            lblSection.Size = new Size(100, 13);
            lblSection.TabIndex = 3;
            lblSection.Text = "MALZEME SEÇİMİ";
            // 
            // checkedList
            // 
            checkedList.Location = new Point(24, 106);
            checkedList.Margin = new Padding(3, 2, 3, 2);
            checkedList.Name = "checkedList";
            checkedList.Size = new Size(592, 216);
            checkedList.TabIndex = 4;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnSelectAll.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnSelectAll.Appearance.Options.UseBorderColor = true;
            btnSelectAll.Appearance.Options.UseFont = true;
            btnSelectAll.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnSelectAll.AppearanceHovered.Options.UseBackColor = true;
            btnSelectAll.Location = new Point(24, 328);
            btnSelectAll.Margin = new Padding(3, 2, 3, 2);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(130, 30);
            btnSelectAll.TabIndex = 5;
            btnSelectAll.Text = "Tümünü Seç";
            btnSelectAll.ImageOptions.SvgImage = DxIcon.CheckAll;
            // 
            // btnClear
            // 
            btnClear.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnClear.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnClear.Appearance.Options.UseBorderColor = true;
            btnClear.Appearance.Options.UseFont = true;
            btnClear.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnClear.AppearanceHovered.Options.UseBackColor = true;
            btnClear.Location = new Point(160, 328);
            btnClear.Margin = new Padding(3, 2, 3, 2);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(110, 30);
            btnClear.TabIndex = 6;
            btnClear.Text = "Temizle";
            btnClear.ImageOptions.SvgImage = DxIcon.Uncheck;
            // 
            // badge
            // 
            badge.Appearance.BackColor = Color.FromArgb(235, 242, 250);
            badge.Appearance.Options.UseBackColor = true;
            badge.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            badge.Controls.Add(lblBadge);
            badge.Controls.Add(badgeAccentStrip);
            badge.Location = new Point(24, 364);
            badge.Margin = new Padding(0);
            badge.Name = "badge";
            badge.Size = new Size(592, 44);
            badge.TabIndex = 7;
            // 
            // lblBadge
            // 
            lblBadge.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblBadge.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblBadge.Appearance.Options.UseFont = true;
            lblBadge.Appearance.Options.UseForeColor = true;
            lblBadge.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblBadge.Location = new Point(20, 14);
            lblBadge.Margin = new Padding(3, 2, 3, 2);
            lblBadge.Name = "lblBadge";
            lblBadge.Size = new Size(556, 18);
            lblBadge.TabIndex = 1;
            lblBadge.Text = "-";
            // 
            // badgeAccentStrip
            // 
            badgeAccentStrip.Appearance.BackColor = Color.FromArgb(64, 120, 200);
            badgeAccentStrip.Appearance.Options.UseBackColor = true;
            badgeAccentStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            badgeAccentStrip.Location = new Point(0, 0);
            badgeAccentStrip.Margin = new Padding(0);
            badgeAccentStrip.Name = "badgeAccentStrip";
            badgeAccentStrip.Size = new Size(4, 44);
            badgeAccentStrip.TabIndex = 0;
            // 
            // lblInfo
            // 
            lblInfo.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblInfo.Appearance.ForeColor = Color.FromArgb(130, 136, 146);
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblInfo.Location = new Point(24, 416);
            lblInfo.Margin = new Padding(3, 2, 3, 2);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(540, 13);
            lblInfo.TabIndex = 8;
            lblInfo.Text = "Reçeteye eklenecek malzemeleri seçin; birim miktarlar üretim adedine göre hesaplanır.";
            // 
            // footerDivider
            // 
            footerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            footerDivider.Appearance.Options.UseBackColor = true;
            footerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            footerDivider.Location = new Point(24, 444);
            footerDivider.Margin = new Padding(0);
            footerDivider.Name = "footerDivider";
            footerDivider.Size = new Size(592, 1);
            footerDivider.TabIndex = 9;
            // 
            // btnOk
            // 
            btnOk.Appearance.BackColor = Color.FromArgb(64, 120, 200);
            btnOk.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOk.Appearance.ForeColor = Color.White;
            btnOk.Appearance.Options.UseBackColor = true;
            btnOk.Appearance.Options.UseFont = true;
            btnOk.Appearance.Options.UseForeColor = true;
            btnOk.AppearanceHovered.BackColor = Color.FromArgb(46, 94, 166);
            btnOk.AppearanceHovered.ForeColor = Color.White;
            btnOk.AppearanceHovered.Options.UseBackColor = true;
            btnOk.AppearanceHovered.Options.UseForeColor = true;
            btnOk.Location = new Point(480, 464);
            btnOk.Margin = new Padding(3, 2, 3, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(136, 34);
            btnOk.TabIndex = 11;
            btnOk.Text = "Reçeteye Ekle";
            btnOk.ImageOptions.SvgImage = DxIcon.Recipe;
            // 
            // btnCancel
            // 
            btnCancel.Appearance.Font = new Font("Segoe UI", 10F);
            btnCancel.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.AppearanceHovered.BackColor = Color.FromArgb(244, 245, 247);
            btnCancel.AppearanceHovered.Options.UseBackColor = true;
            btnCancel.Location = new Point(375, 464);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(96, 34);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "İptal";
            btnCancel.ImageOptions.SvgImage = DxIcon.Close;
            // 
            // RecipeMaterialSelectionForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(640, 516);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(footerDivider);
            Controls.Add(lblInfo);
            Controls.Add(badge);
            Controls.Add(btnClear);
            Controls.Add(btnSelectAll);
            Controls.Add(checkedList);
            Controls.Add(lblSection);
            Controls.Add(headerDivider);
            Controls.Add(headerPanel);
            Controls.Add(accentBar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RecipeMaterialSelectionForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Reçeteye Eklenecek Malzemeler";
            ((System.ComponentModel.ISupportInitialize)accentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).EndInit();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).EndInit();
            ((System.ComponentModel.ISupportInitialize)checkedList).EndInit();
            ((System.ComponentModel.ISupportInitialize)badge).EndInit();
            badge.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)badgeAccentStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
