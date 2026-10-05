namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    partial class MessagesListForm
    {
        private DevExpress.XtraEditors.PanelControl pnlHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSubtitle = null!;
        private DevExpress.XtraEditors.PanelControl pnlToolbar = null!;
        private DevExpress.XtraEditors.SimpleButton btnNewMessage = null!;
        private DevExpress.XtraEditors.SimpleButton btnAnnouncement = null!;
        private DevExpress.XtraEditors.SimpleButton btnRefresh = null!;
        private DevExpress.XtraEditors.SimpleButton btnClosePage = null!;
        private DevExpress.XtraEditors.CheckButton btnUnreadOnly = null!;
        private DevExpress.XtraGrid.GridControl gridConversations = null!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewConversations = null!;
        private DevExpress.XtraEditors.LabelControl lblEmpty = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                gridConversations?.Dispose();
                btnNewMessage?.Dispose();
                btnAnnouncement?.Dispose();
                btnRefresh?.Dispose();
                btnClosePage?.Dispose();
                btnUnreadOnly?.Dispose();
                lblTitle?.Dispose();
                lblSubtitle?.Dispose();
                lblEmpty?.Dispose();
                base.Dispose(disposing);
            }
        }

        private void InitializeComponent()
        {
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlToolbar = new DevExpress.XtraEditors.PanelControl();
            btnNewMessage = new DevExpress.XtraEditors.SimpleButton();
            btnAnnouncement = new DevExpress.XtraEditors.SimpleButton();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            btnUnreadOnly = new DevExpress.XtraEditors.CheckButton();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            gridConversations = new DevExpress.XtraGrid.GridControl();
            viewConversations = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblEmpty = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).BeginInit();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(16, 12, 16, 12);
            pnlHeader.Size = new Size(944, 62);
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
            lblTitle.Text = "Mesajlar";
            //
            // lblSubtitle
            //
            lblSubtitle.Appearance.Options.UseTextOptions = true;
            lblSubtitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubtitle.Location = new Point(20, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(700, 18);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Yükleniyor...";
            //
            // pnlToolbar
            //
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 62);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(14, 8, 14, 8);
            pnlToolbar.Size = new Size(944, 52);
            pnlToolbar.TabIndex = 1;
            //
            // btnNewMessage
            //
            btnNewMessage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnNewMessage.Location = new Point(14, 8);
            btnNewMessage.Name = "btnNewMessage";
            btnNewMessage.Size = new Size(130, 34);
            btnNewMessage.TabIndex = 0;
            btnNewMessage.Text = "Yeni Mesaj";
            //
            // btnAnnouncement
            //
            btnAnnouncement.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnAnnouncement.Location = new Point(152, 8);
            btnAnnouncement.Name = "btnAnnouncement";
            btnAnnouncement.Size = new Size(160, 34);
            btnAnnouncement.TabIndex = 1;
            btnAnnouncement.Text = "Duyuru Gönder";
            btnAnnouncement.ToolTip = "Seçili kullanıcılara duyuru olarak gönderir.";
            //
            // btnRefresh
            //
            btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRefresh.Location = new Point(320, 8);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 34);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Yenile";
            //
            // btnUnreadOnly
            //
            btnUnreadOnly.Location = new Point(428, 8);
            btnUnreadOnly.Name = "btnUnreadOnly";
            btnUnreadOnly.Text = "Sadece Okunmamışlar";
            btnUnreadOnly.Size = new Size(170, 34);
            btnUnreadOnly.TabIndex = 3;
            //
            // btnClosePage
            //
            btnClosePage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClosePage.Location = new Point(606, 8);
            btnClosePage.Name = "btnClosePage";
            btnClosePage.Size = new Size(100, 34);
            btnClosePage.TabIndex = 4;
            btnClosePage.Text = "Kapat";
            //
            // gridConversations
            //
            gridConversations.Dock = DockStyle.Fill;
            gridConversations.Location = new Point(0, 114);
            gridConversations.MainView = viewConversations;
            gridConversations.Name = "gridConversations";
            gridConversations.Size = new Size(944, 480);
            gridConversations.TabIndex = 2;
            gridConversations.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewConversations });
            //
            // viewConversations
            //
            viewConversations.GridControl = gridConversations;
            viewConversations.Name = "viewConversations";
            viewConversations.OptionsBehavior.Editable = false;
            viewConversations.OptionsSelection.MultiSelect = false;
            viewConversations.OptionsView.ShowGroupPanel = false;
            viewConversations.OptionsView.ShowIndicator = false;
            viewConversations.OptionsView.ColumnAutoWidth = false;
            //
            // lblEmpty
            //
            lblEmpty.Appearance.Font = new Font("Segoe UI", 10F);
            lblEmpty.Appearance.Options.UseFont = true;
            lblEmpty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblEmpty.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblEmpty.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblEmpty.Location = new Point(20, 300);
            lblEmpty.Name = "lblEmpty";
            lblEmpty.Size = new Size(904, 60);
            lblEmpty.TabIndex = 5;
            lblEmpty.Text = "Henüz mesajınız yok.\r\n\"Yeni Mesaj\" ile bir kullanıcıya yazabilir, \"Duyuru Gönder\" ile şirket geneline duyuru iletebilirsiniz.";
            //
            // MessagesListForm
            //
            ClientSize = new Size(944, 594);
            // Panel içindeki kontroller önce kendi paneline eklenir, sonra
            // paneller forma eklenir. Bu sıra önemlidir: Dock yerleşimi ekleme
            // sırasına göre çözülür ve başlık ile araç çubuğu grid'in üstünde
            // kalmalıdır.
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlToolbar.Controls.Add(btnNewMessage);
            pnlToolbar.Controls.Add(btnAnnouncement);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnUnreadOnly);
            pnlToolbar.Controls.Add(btnClosePage);
            Controls.Add(gridConversations);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Controls.Add(lblEmpty);
            MinimumSize = new Size(720, 420);
            Name = "MessagesListForm";
            ShowInTaskbar = false;
            Text = "Mesajlar";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewConversations).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridConversations).EndInit();
            ResumeLayout(false);
        }
    }
}