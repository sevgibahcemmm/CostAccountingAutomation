namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    partial class DateRangePromptForm
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
        private DevExpress.XtraEditors.LabelControl lblSectionDate;
        private DevExpress.XtraEditors.LabelControl lblStart;
        private DevExpress.XtraEditors.LabelControl lblEnd;
        private DevExpress.XtraEditors.DateEdit dateStart;
        private DevExpress.XtraEditors.DateEdit dateEnd;
        private DevExpress.XtraEditors.LabelControl lblQuick;
        private DevExpress.XtraEditors.SimpleButton btnThisMonth;
        private DevExpress.XtraEditors.SimpleButton btnLastMonth;
        private DevExpress.XtraEditors.SimpleButton btnLast30;
        private DevExpress.XtraEditors.SimpleButton btnThisYear;
        private DevExpress.XtraEditors.PanelControl rangeBadge;
        private DevExpress.XtraEditors.PanelControl rangeAccentStrip;
        private DevExpress.XtraEditors.LabelControl lblRange;
        private DevExpress.XtraEditors.LabelControl lblInfo;
        private DevExpress.XtraEditors.LabelControl lblType;
        private DevExpress.XtraEditors.LookUpEdit lookupType;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateRangePromptForm));
            accentBar = new DevExpress.XtraEditors.PanelControl();
            headerPanel = new DevExpress.XtraEditors.PanelControl();
            lblHeaderSub = new DevExpress.XtraEditors.LabelControl();
            lblHeaderTitle = new DevExpress.XtraEditors.LabelControl();
            picHeader = new DevExpress.XtraEditors.PictureEdit();
            headerDivider = new DevExpress.XtraEditors.PanelControl();
            lblSectionDate = new DevExpress.XtraEditors.LabelControl();
            lblStart = new DevExpress.XtraEditors.LabelControl();
            lblEnd = new DevExpress.XtraEditors.LabelControl();
            dateStart = new DevExpress.XtraEditors.DateEdit();
            dateEnd = new DevExpress.XtraEditors.DateEdit();
            lblQuick = new DevExpress.XtraEditors.LabelControl();
            btnThisMonth = new DevExpress.XtraEditors.SimpleButton();
            btnLastMonth = new DevExpress.XtraEditors.SimpleButton();
            btnLast30 = new DevExpress.XtraEditors.SimpleButton();
            btnThisYear = new DevExpress.XtraEditors.SimpleButton();
            rangeBadge = new DevExpress.XtraEditors.PanelControl();
            lblRange = new DevExpress.XtraEditors.LabelControl();
            rangeAccentStrip = new DevExpress.XtraEditors.PanelControl();
            lblInfo = new DevExpress.XtraEditors.LabelControl();
            lblType = new DevExpress.XtraEditors.LabelControl();
            lookupType = new DevExpress.XtraEditors.LookUpEdit();
            footerDivider = new DevExpress.XtraEditors.PanelControl();
            btnOk = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)accentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).BeginInit();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dateStart.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dateStart.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dateEnd.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dateEnd.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rangeBadge).BeginInit();
            rangeBadge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rangeAccentStrip).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookupType.Properties).BeginInit();
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
            accentBar.Size = new Size(470, 10);
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
            headerPanel.Location = new Point(0, 4);
            headerPanel.Margin = new Padding(3, 2, 3, 2);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(470, 64);
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
            lblHeaderSub.Size = new Size(213, 15);
            lblHeaderSub.TabIndex = 2;
            lblHeaderSub.Text = "Yazdırmak istediğiniz tarih aralığını seçin";
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblHeaderTitle.Appearance.Options.UseFont = true;
            lblHeaderTitle.Location = new Point(66, 10);
            lblHeaderTitle.Margin = new Padding(3, 2, 3, 2);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(213, 28);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Gider Dağıtım Tablosu";
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
            // 
            // headerDivider
            // 
            headerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            headerDivider.Appearance.Options.UseBackColor = true;
            headerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerDivider.Location = new Point(0, 68);
            headerDivider.Margin = new Padding(0);
            headerDivider.Name = "headerDivider";
            headerDivider.Size = new Size(440, 1);
            headerDivider.TabIndex = 2;
            // 
            // lblSectionDate
            // 
            lblSectionDate.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblSectionDate.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblSectionDate.Appearance.Options.UseFont = true;
            lblSectionDate.Appearance.Options.UseForeColor = true;
            lblSectionDate.Location = new Point(24, 84);
            lblSectionDate.Margin = new Padding(3, 2, 3, 2);
            lblSectionDate.Name = "lblSectionDate";
            lblSectionDate.Size = new Size(78, 13);
            lblSectionDate.TabIndex = 3;
            lblSectionDate.Text = "TARİH ARALIĞI";
            // 
            // lblStart
            // 
            lblStart.Appearance.Font = new Font("Segoe UI", 9F);
            lblStart.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblStart.Appearance.Options.UseFont = true;
            lblStart.Appearance.Options.UseForeColor = true;
            lblStart.Location = new Point(24, 106);
            lblStart.Margin = new Padding(3, 2, 3, 2);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(83, 15);
            lblStart.TabIndex = 4;
            lblStart.Text = "Başlangıç Tarihi";
            // 
            // lblEnd
            // 
            lblEnd.Appearance.Font = new Font("Segoe UI", 9F);
            lblEnd.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblEnd.Appearance.Options.UseFont = true;
            lblEnd.Appearance.Options.UseForeColor = true;
            lblEnd.Location = new Point(248, 106);
            lblEnd.Margin = new Padding(3, 2, 3, 2);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(55, 15);
            lblEnd.TabIndex = 5;
            lblEnd.Text = "Bitiş Tarihi";
            // 
            // dateStart
            // 
            dateStart.EditValue = null;
            dateStart.Location = new Point(24, 124);
            dateStart.Margin = new Padding(3, 2, 3, 2);
            dateStart.Name = "dateStart";
            dateStart.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            dateStart.Properties.Appearance.Options.UseFont = true;
            dateStart.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dateStart.Properties.Mask.EditMask = "dd.MM.yyyy";
            dateStart.Properties.Mask.UseMaskAsDisplayFormat = true;
            dateStart.Properties.MaxDate = new DateTime(2036, 12, 31, 0, 0, 0, 0);
            dateStart.Properties.MinDate = new DateTime(2016, 1, 1, 0, 0, 0, 0);
            dateStart.Size = new Size(208, 24);
            dateStart.TabIndex = 6;
            // 
            // dateEnd
            // 
            dateEnd.EditValue = null;
            dateEnd.Location = new Point(248, 124);
            dateEnd.Margin = new Padding(3, 2, 3, 2);
            dateEnd.Name = "dateEnd";
            dateEnd.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            dateEnd.Properties.Appearance.Options.UseFont = true;
            dateEnd.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            dateEnd.Properties.Mask.UseMaskAsDisplayFormat = true;
            dateEnd.Properties.MaskSettings.Set("mask", "dd.MM.yyyy");
            dateEnd.Properties.MaxDate = new DateTime(2036, 12, 31, 0, 0, 0, 0);
            dateEnd.Properties.MinDate = new DateTime(2016, 1, 1, 0, 0, 0, 0);
            dateEnd.Size = new Size(208, 24);
            dateEnd.TabIndex = 7;
            // 
            // lblQuick
            // 
            lblQuick.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblQuick.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblQuick.Appearance.Options.UseFont = true;
            lblQuick.Appearance.Options.UseForeColor = true;
            lblQuick.Location = new Point(24, 170);
            lblQuick.Margin = new Padding(3, 2, 3, 2);
            lblQuick.Name = "lblQuick";
            lblQuick.Size = new Size(63, 13);
            lblQuick.TabIndex = 8;
            lblQuick.Text = "HIZLI SEÇİM";
            // 
            // btnThisMonth
            // 
            btnThisMonth.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnThisMonth.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnThisMonth.Appearance.Options.UseBorderColor = true;
            btnThisMonth.Appearance.Options.UseFont = true;
            btnThisMonth.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnThisMonth.AppearanceHovered.Options.UseBackColor = true;
            btnThisMonth.ImageOptions.Image = (Image)resources.GetObject("btnThisMonth.ImageOptions.Image");
            btnThisMonth.Location = new Point(24, 192);
            btnThisMonth.Margin = new Padding(3, 2, 3, 2);
            btnThisMonth.Name = "btnThisMonth";
            btnThisMonth.Size = new Size(89, 30);
            btnThisMonth.TabIndex = 9;
            btnThisMonth.Text = "Bu Ay";
            // 
            // btnLastMonth
            // 
            btnLastMonth.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnLastMonth.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnLastMonth.Appearance.Options.UseBorderColor = true;
            btnLastMonth.Appearance.Options.UseFont = true;
            btnLastMonth.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnLastMonth.AppearanceHovered.Options.UseBackColor = true;
            btnLastMonth.ImageOptions.Image = (Image)resources.GetObject("btnLastMonth.ImageOptions.Image");
            btnLastMonth.Location = new Point(123, 192);
            btnLastMonth.Margin = new Padding(3, 2, 3, 2);
            btnLastMonth.Name = "btnLastMonth";
            btnLastMonth.Size = new Size(105, 30);
            btnLastMonth.TabIndex = 10;
            btnLastMonth.Text = "Geçen Ay";
            // 
            // btnLast30
            // 
            btnLast30.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnLast30.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnLast30.Appearance.Options.UseBorderColor = true;
            btnLast30.Appearance.Options.UseFont = true;
            btnLast30.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnLast30.AppearanceHovered.Options.UseBackColor = true;
            btnLast30.ImageOptions.Image = (Image)resources.GetObject("btnLast30.ImageOptions.Image");
            btnLast30.Location = new Point(238, 192);
            btnLast30.Margin = new Padding(3, 2, 3, 2);
            btnLast30.Name = "btnLast30";
            btnLast30.Size = new Size(119, 30);
            btnLast30.TabIndex = 11;
            btnLast30.Text = "Son 30 Gün";
            // 
            // btnThisYear
            // 
            btnThisYear.Appearance.BorderColor = Color.FromArgb(220, 223, 228);
            btnThisYear.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnThisYear.Appearance.Options.UseBorderColor = true;
            btnThisYear.Appearance.Options.UseFont = true;
            btnThisYear.AppearanceHovered.BackColor = Color.FromArgb(235, 242, 250);
            btnThisYear.AppearanceHovered.Options.UseBackColor = true;
            btnThisYear.ImageOptions.Image = (Image)resources.GetObject("btnThisYear.ImageOptions.Image");
            btnThisYear.Location = new Point(367, 192);
            btnThisYear.Margin = new Padding(3, 2, 3, 2);
            btnThisYear.Name = "btnThisYear";
            btnThisYear.Size = new Size(89, 30);
            btnThisYear.TabIndex = 12;
            btnThisYear.Text = "Bu Yıl";
            // 
            // rangeBadge
            // 
            rangeBadge.Appearance.BackColor = Color.FromArgb(235, 242, 250);
            rangeBadge.Appearance.Options.UseBackColor = true;
            rangeBadge.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            rangeBadge.Controls.Add(lblRange);
            rangeBadge.Controls.Add(rangeAccentStrip);
            rangeBadge.Location = new Point(24, 238);
            rangeBadge.Margin = new Padding(0);
            rangeBadge.Name = "rangeBadge";
            rangeBadge.Size = new Size(432, 44);
            rangeBadge.TabIndex = 13;
            // 
            // lblRange
            // 
            lblRange.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblRange.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblRange.Appearance.Options.UseFont = true;
            lblRange.Appearance.Options.UseForeColor = true;
            lblRange.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblRange.Location = new Point(20, 14);
            lblRange.Margin = new Padding(3, 2, 3, 2);
            lblRange.Name = "lblRange";
            lblRange.Size = new Size(396, 18);
            lblRange.TabIndex = 1;
            lblRange.Text = "-";
            // 
            // rangeAccentStrip
            // 
            rangeAccentStrip.Appearance.BackColor = Color.FromArgb(64, 120, 200);
            rangeAccentStrip.Appearance.Options.UseBackColor = true;
            rangeAccentStrip.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            rangeAccentStrip.Location = new Point(0, 0);
            rangeAccentStrip.Margin = new Padding(0);
            rangeAccentStrip.Name = "rangeAccentStrip";
            rangeAccentStrip.Size = new Size(4, 44);
            rangeAccentStrip.TabIndex = 0;
            // 
            // lblInfo
            // 
            lblInfo.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblInfo.Appearance.ForeColor = Color.FromArgb(130, 136, 146);
            lblInfo.Appearance.Options.UseFont = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblInfo.Location = new Point(24, 292);
            lblInfo.Margin = new Padding(3, 2, 3, 2);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(273, 13);
            lblInfo.TabIndex = 14;
            lblInfo.Text = "Seçilen aralık raporda dönem başlığı olarak kullanılır.";
            // 
            // lblType
            // 
            lblType.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblType.Appearance.ForeColor = Color.FromArgb(64, 120, 200);
            lblType.Appearance.Options.UseFont = true;
            lblType.Appearance.Options.UseForeColor = true;
            lblType.Location = new Point(24, 322);
            lblType.Margin = new Padding(3, 2, 3, 2);
            lblType.Name = "lblType";
            lblType.Size = new Size(75, 13);
            lblType.TabIndex = 15;
            lblType.Text = "PUSULA TÜRÜ";
            // 
            // lookupType
            // 
            lookupType.Location = new Point(24, 344);
            lookupType.Margin = new Padding(3, 2, 3, 2);
            lookupType.Name = "lookupType";
            lookupType.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            lookupType.Properties.Appearance.Options.UseFont = true;
            lookupType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookupType.Properties.DropDownRows = 8;
            lookupType.Properties.ShowHeader = false;
            lookupType.Size = new Size(432, 24);
            lookupType.TabIndex = 16;
            // 
            // footerDivider
            // 
            footerDivider.Appearance.BackColor = Color.FromArgb(230, 232, 236);
            footerDivider.Appearance.Options.UseBackColor = true;
            footerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            footerDivider.Location = new Point(24, 386);
            footerDivider.Margin = new Padding(0);
            footerDivider.Name = "footerDivider";
            footerDivider.Size = new Size(392, 1);
            footerDivider.TabIndex = 17;
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
            btnOk.ImageOptions.Image = (Image)resources.GetObject("btnOk.ImageOptions.Image");
            btnOk.Location = new Point(296, 402);
            btnOk.Margin = new Padding(3, 2, 3, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(160, 34);
            btnOk.TabIndex = 19;
            btnOk.Text = "Raporu Yazdır";
            // 
            // btnCancel
            // 
            btnCancel.Appearance.Font = new Font("Segoe UI", 10F);
            btnCancel.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.AppearanceHovered.BackColor = Color.FromArgb(244, 245, 247);
            btnCancel.AppearanceHovered.Options.UseBackColor = true;
            btnCancel.ImageOptions.Image = (Image)resources.GetObject("btnCancel.ImageOptions.Image");
            btnCancel.Location = new Point(191, 402);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(96, 34);
            btnCancel.TabIndex = 18;
            btnCancel.Text = "İptal";
            // 
            // DateRangePromptForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(470, 456);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(footerDivider);
            Controls.Add(lookupType);
            Controls.Add(lblType);
            Controls.Add(lblInfo);
            Controls.Add(rangeBadge);
            Controls.Add(btnThisYear);
            Controls.Add(btnLast30);
            Controls.Add(btnLastMonth);
            Controls.Add(btnThisMonth);
            Controls.Add(lblQuick);
            Controls.Add(dateEnd);
            Controls.Add(dateStart);
            Controls.Add(lblEnd);
            Controls.Add(lblStart);
            Controls.Add(lblSectionDate);
            Controls.Add(headerDivider);
            Controls.Add(headerPanel);
            Controls.Add(accentBar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DateRangePromptForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gider Dağıtım Tablosu";
            ((System.ComponentModel.ISupportInitialize)accentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerPanel).EndInit();
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)headerDivider).EndInit();
            ((System.ComponentModel.ISupportInitialize)dateStart.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dateStart.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dateEnd.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dateEnd.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)rangeBadge).EndInit();
            rangeBadge.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rangeAccentStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookupType.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)footerDivider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}