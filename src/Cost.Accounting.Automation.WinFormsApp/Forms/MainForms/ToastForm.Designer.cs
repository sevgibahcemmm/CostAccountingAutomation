namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class ToastForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl _pnlAccentBar;
        private DevExpress.XtraEditors.PanelControl _pnlContentArea;
        private DevExpress.XtraEditors.LabelControl _lblIcon;
        private DevExpress.XtraEditors.LabelControl _lblTitle;
        private DevExpress.XtraEditors.LabelControl _lblMessage;
        private DevExpress.XtraEditors.LabelControl _btnClose;
        private DevExpress.XtraEditors.PanelControl _progressTrack;
        private DevExpress.XtraEditors.PanelControl _progressFill;

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
            _pnlAccentBar = new DevExpress.XtraEditors.PanelControl();
            _pnlContentArea = new DevExpress.XtraEditors.PanelControl();
            _lblIcon = new DevExpress.XtraEditors.LabelControl();
            _lblTitle = new DevExpress.XtraEditors.LabelControl();
            _lblMessage = new DevExpress.XtraEditors.LabelControl();
            _btnClose = new DevExpress.XtraEditors.LabelControl();
            _progressTrack = new DevExpress.XtraEditors.PanelControl();
            _progressFill = new DevExpress.XtraEditors.PanelControl();

            ((System.ComponentModel.ISupportInitialize)_pnlAccentBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_pnlContentArea).BeginInit();
            _pnlContentArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_progressTrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_progressFill).BeginInit();
            SuspendLayout();

            // 
            // _pnlAccentBar
            // 
            _pnlAccentBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _pnlAccentBar.Location = new System.Drawing.Point(0, 0);
            _pnlAccentBar.Name = "_pnlAccentBar";
            _pnlAccentBar.Size = new System.Drawing.Size(6, 92);
            _pnlAccentBar.TabIndex = 0;
            _pnlAccentBar.Appearance.Options.UseBackColor = true;

            // 
            // _pnlContentArea
            // 
            _pnlContentArea.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _pnlContentArea.Controls.Add(_lblIcon);
            _pnlContentArea.Controls.Add(_lblTitle);
            _pnlContentArea.Controls.Add(_lblMessage);
            _pnlContentArea.Controls.Add(_btnClose);
            _pnlContentArea.Location = new System.Drawing.Point(6, 0);
            _pnlContentArea.Name = "_pnlContentArea";
            _pnlContentArea.Size = new System.Drawing.Size(354, 88);
            _pnlContentArea.TabIndex = 1;
            _pnlContentArea.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            _pnlContentArea.Appearance.Options.UseBackColor = true;

            // 
            // _lblIcon
            // 
            _lblIcon.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            _lblIcon.Appearance.Options.UseFont = true;
            _lblIcon.Appearance.Options.UseForeColor = true;
            _lblIcon.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            _lblIcon.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            _lblIcon.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            _lblIcon.Location = new System.Drawing.Point(12, 26);
            _lblIcon.Name = "_lblIcon";
            _lblIcon.Size = new System.Drawing.Size(36, 36);
            _lblIcon.TabIndex = 0;

            // 
            // _lblTitle
            // 
            _lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            _lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            _lblTitle.Appearance.Options.UseFont = true;
            _lblTitle.Appearance.Options.UseForeColor = true;
            _lblTitle.Location = new System.Drawing.Point(56, 16);
            _lblTitle.Name = "_lblTitle";
            _lblTitle.Size = new System.Drawing.Size(260, 20);
            _lblTitle.TabIndex = 1;

            // 
            // _lblMessage
            // 
            _lblMessage.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            _lblMessage.Appearance.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            _lblMessage.Appearance.Options.UseFont = true;
            _lblMessage.Appearance.Options.UseForeColor = true;
            _lblMessage.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            _lblMessage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            _lblMessage.Location = new System.Drawing.Point(56, 40);
            _lblMessage.Name = "_lblMessage";
            _lblMessage.Size = new System.Drawing.Size(268, 36);
            _lblMessage.TabIndex = 2;

            // 
            // _btnClose
            // 
            _btnClose.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            _btnClose.Appearance.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            _btnClose.Appearance.Options.UseFont = true;
            _btnClose.Appearance.Options.UseForeColor = true;
            _btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnClose.Location = new System.Drawing.Point(332, 8);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new System.Drawing.Size(16, 16);
            _btnClose.TabIndex = 3;
            _btnClose.Text = "✕";

            // 
            // _progressTrack
            // 
            _progressTrack.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _progressTrack.Location = new System.Drawing.Point(6, 88);
            _progressTrack.Name = "_progressTrack";
            _progressTrack.Size = new System.Drawing.Size(354, 4);
            _progressTrack.TabIndex = 2;
            _progressTrack.Appearance.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            _progressTrack.Appearance.Options.UseBackColor = true;

            // 
            // _progressFill
            // 
            _progressFill.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            _progressFill.Location = new System.Drawing.Point(6, 88);
            _progressFill.Name = "_progressFill";
            _progressFill.Size = new System.Drawing.Size(354, 4);
            _progressFill.TabIndex = 3;
            _progressFill.Appearance.Options.UseBackColor = true;

            // 
            // ToastForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(250, 250, 252);
            ClientSize = new System.Drawing.Size(360, 92);
            Controls.Add(_progressFill);
            Controls.Add(_progressTrack);
            Controls.Add(_pnlContentArea);
            Controls.Add(_pnlAccentBar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ToastForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            TopMost = true;

            ((System.ComponentModel.ISupportInitialize)_pnlAccentBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)_pnlContentArea).EndInit();
            _pnlContentArea.ResumeLayout(false);
            _pnlContentArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_progressTrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)_progressFill).EndInit();
            ResumeLayout(false);
        }
    }
}