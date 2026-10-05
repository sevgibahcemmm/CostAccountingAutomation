namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    partial class ConversationForm
    {
        private DevExpress.XtraEditors.PanelControl pnlHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSubtitle = null!;
        private DevExpress.XtraGrid.GridControl gridMessages = null!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewMessages = null!;
        private DevExpress.XtraEditors.PanelControl pnlCompose = null!;
        private DevExpress.XtraEditors.LabelControl lblReply = null!;
        private DevExpress.XtraEditors.MemoEdit txtReply = null!;
        private DevExpress.XtraEditors.LabelControl lblSubject = null!;
        private DevExpress.XtraEditors.TextEdit txtSubject = null!;
        private DevExpress.XtraEditors.SimpleButton btnSend = null!;
        private DevExpress.XtraEditors.SimpleButton btnRefresh = null!;
        private DevExpress.XtraEditors.SimpleButton btnClosePage = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                gridMessages?.Dispose();
                btnSend?.Dispose();
                btnRefresh?.Dispose();
                btnClosePage?.Dispose();
                txtReply?.Dispose();
                txtSubject?.Dispose();
                lblReply?.Dispose();
                lblSubject?.Dispose();
                lblTitle?.Dispose();
                lblSubtitle?.Dispose();
                base.Dispose(disposing);
            }
        }

        private void InitializeComponent()
        {
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            gridMessages = new DevExpress.XtraGrid.GridControl();
            viewMessages = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlCompose = new DevExpress.XtraEditors.PanelControl();
            lblReply = new DevExpress.XtraEditors.LabelControl();
            txtReply = new DevExpress.XtraEditors.MemoEdit();
            lblSubject = new DevExpress.XtraEditors.LabelControl();
            txtSubject = new DevExpress.XtraEditors.TextEdit();
            btnSend = new DevExpress.XtraEditors.SimpleButton();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtReply.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSubject.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlCompose).BeginInit();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(16, 12, 16, 12);
            pnlHeader.Size = new Size(904, 62);
            pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Appearance.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTitle.Location = new Point(20, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(420, 22);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Konuşma";
            //
            // lblSubtitle
            //
            lblSubtitle.Appearance.Options.UseTextOptions = true;
            lblSubtitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubtitle.Location = new Point(20, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(760, 18);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = string.Empty;
            //
            // gridMessages
            //
            gridMessages.Dock = DockStyle.Fill;
            gridMessages.Location = new Point(0, 122);
            gridMessages.MainView = viewMessages;
            gridMessages.Name = "gridMessages";
            gridMessages.Size = new Size(904, 380);
            gridMessages.TabIndex = 1;
            gridMessages.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewMessages });
            //
            // viewMessages
            //
            viewMessages.GridControl = gridMessages;
            viewMessages.Name = "viewMessages";
            viewMessages.OptionsBehavior.Editable = false;
            viewMessages.OptionsSelection.MultiSelect = false;
            viewMessages.OptionsView.ShowGroupPanel = false;
            viewMessages.OptionsView.ShowIndicator = false;
            viewMessages.OptionsView.ColumnAutoWidth = false;
            //
            // pnlCompose
            //
            pnlCompose.Dock = DockStyle.Bottom;
            pnlCompose.Location = new Point(0, 502);
            pnlCompose.Name = "pnlCompose";
            pnlCompose.Padding = new Padding(16, 10, 16, 12);
            pnlCompose.Size = new Size(904, 190);
            pnlCompose.TabIndex = 2;
            //
            // lblReply
            //
            lblReply.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblReply.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblReply.Appearance.Options.UseFont = true;
            lblReply.Appearance.Options.UseForeColor = true;
            lblReply.Location = new Point(20, 14);
            lblReply.Name = "lblReply";
            lblReply.Size = new Size(60, 14);
            lblReply.TabIndex = 0;
            lblReply.Text = "Yanıtınız";
            //
            // txtReply
            //
            txtReply.Location = new Point(20, 32);
            txtReply.Name = "txtReply";
            txtReply.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtReply.Properties.Appearance.Options.UseFont = true;
            txtReply.Properties.NullText = "Mesajınızı yazın...";
            txtReply.Properties.Padding = new Padding(4, 4, 4, 4);
            txtReply.Size = new Size(700, 96);
            txtReply.TabIndex = 1;
            //
            // lblSubject
            //
            lblSubject.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubject.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblSubject.Appearance.Options.UseFont = true;
            lblSubject.Appearance.Options.UseForeColor = true;
            lblSubject.Location = new Point(736, 14);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(34, 14);
            lblSubject.TabIndex = 2;
            lblSubject.Text = "Konu";
            //
            // txtSubject
            //
            txtSubject.Location = new Point(736, 32);
            txtSubject.Name = "txtSubject";
            txtSubject.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtSubject.Properties.Appearance.Options.UseFont = true;
            txtSubject.Properties.NullText = "Konu (isteğe bağlı)";
            txtSubject.Properties.Padding = new Padding(26, 2, 2, 2);
            txtSubject.Size = new Size(148, 26);
            txtSubject.TabIndex = 3;
            //
            // btnSend
            //
            btnSend.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSend.Location = new Point(736, 68);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(148, 34);
            btnSend.TabIndex = 4;
            btnSend.Text = "Gönder";
            //
            // btnRefresh
            //
            btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRefresh.Location = new Point(20, 138);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(110, 34);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Yenile";
            //
            // btnClosePage
            //
            btnClosePage.Location = new Point(140, 138);
            btnClosePage.Name = "btnClosePage";
            btnClosePage.Size = new Size(100, 34);
            btnClosePage.TabIndex = 6;
            btnClosePage.Text = "Kapat";
            //
            // ConversationForm
            //
            ClientSize = new Size(904, 692);
            // Etiketler ve düğmeler kendi panellerine eklenir; ardından paneller
            // forma eklenir. Dock yerleşimi ekleme sırasına göre çözülür.
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlCompose.Controls.Add(lblReply);
            pnlCompose.Controls.Add(txtReply);
            pnlCompose.Controls.Add(lblSubject);
            pnlCompose.Controls.Add(txtSubject);
            pnlCompose.Controls.Add(btnSend);
            pnlCompose.Controls.Add(btnRefresh);
            pnlCompose.Controls.Add(btnClosePage);
            Controls.Add(gridMessages);
            Controls.Add(pnlHeader);
            Controls.Add(pnlCompose);
            MinimumSize = new Size(680, 460);
            Name = "ConversationForm";
            ShowInTaskbar = false;
            Text = "Konuşma";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewMessages).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridMessages).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSubject.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtReply.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlCompose).EndInit();
            ResumeLayout(false);
        }
    }
}