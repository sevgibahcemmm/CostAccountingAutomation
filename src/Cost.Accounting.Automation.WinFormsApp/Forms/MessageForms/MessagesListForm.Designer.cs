namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    partial class MessagesListForm
    {
        private DevExpress.XtraEditors.PanelControl pnlHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSubtitle = null!;
        private DevExpress.XtraEditors.PanelControl pnlToolbar = null!;
        private DevExpress.XtraEditors.SimpleButton btnAnnouncement = null!;
        private DevExpress.XtraEditors.CheckButton btnSound = null!;
        private DevExpress.XtraEditors.PanelControl pnlContacts = null!;
        private DevExpress.XtraEditors.PanelControl pnlContactsHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblContactsTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblContactsCount = null!;
        private DevExpress.XtraEditors.TextEdit txtContactSearch = null!;
        private DevExpress.XtraGrid.GridControl gridUsers = null!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewUsers = null!;
        private DevExpress.XtraEditors.PanelControl pnlConversations = null!;
        private DevExpress.XtraTab.XtraTabControl tabConversations = null!;
        private DevExpress.XtraEditors.LabelControl lblEmpty = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                gridUsers?.Dispose();
                txtContactSearch?.Dispose();
                lblContactsCount?.Dispose();
                lblContactsTitle?.Dispose();
                pnlContacts?.Dispose();
                pnlContactsHeader?.Dispose();
                pnlConversations?.Dispose();
                tabConversations?.Dispose();
                btnAnnouncement?.Dispose();
                btnSound?.Dispose();
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
            btnAnnouncement = new DevExpress.XtraEditors.SimpleButton();
            btnSound = new DevExpress.XtraEditors.CheckButton();
            pnlContacts = new DevExpress.XtraEditors.PanelControl();
            pnlContactsHeader = new DevExpress.XtraEditors.PanelControl();
            lblContactsTitle = new DevExpress.XtraEditors.LabelControl();
            lblContactsCount = new DevExpress.XtraEditors.LabelControl();
            txtContactSearch = new DevExpress.XtraEditors.TextEdit();
            gridUsers = new DevExpress.XtraGrid.GridControl();
            viewUsers = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlConversations = new DevExpress.XtraEditors.PanelControl();
            tabConversations = new DevExpress.XtraTab.XtraTabControl();
            lblEmpty = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtContactSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlContactsHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlContacts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlConversations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tabConversations).BeginInit();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(16, 12, 16, 12);
            pnlHeader.Size = new Size(1080, 62);
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
            lblSubtitle.Size = new Size(860, 18);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Yükleniyor...";
            //
            // pnlToolbar
            //
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 62);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(14, 8, 14, 8);
            pnlToolbar.Size = new Size(1080, 52);
            pnlToolbar.TabIndex = 1;
            //
            // btnAnnouncement
            //
            btnAnnouncement.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnAnnouncement.Location = new Point(14, 8);
            btnAnnouncement.Name = "btnAnnouncement";
            btnAnnouncement.Size = new Size(160, 34);
            btnAnnouncement.TabIndex = 0;
            btnAnnouncement.Text = "Duyuru Gönder";
            btnAnnouncement.ToolTip = "Şirket geneline duyuru iletir (her çalışanın kutusuna ayrı satır düşer).";
            //
            // btnSound
            //
            btnSound.Location = new Point(182, 8);
            btnSound.Name = "btnSound";
            btnSound.Size = new Size(150, 34);
            btnSound.TabIndex = 1;
            btnSound.Text = "Sesli Bildirim";
            btnSound.ToolTip = "Biri oturum açtığında ve yeni mesaj geldiğinde ses çalınır.";
            //
            // pnlContacts
            //
            pnlContacts.Dock = DockStyle.Left;
            pnlContacts.Location = new Point(0, 114);
            pnlContacts.Name = "pnlContacts";
            pnlContacts.Padding = new Padding(0, 0, 1, 0);
            pnlContacts.Size = new Size(380, 486);
            pnlContacts.TabIndex = 2;
            //
            // pnlContactsHeader
            //
            // Başlık ve arama kutusu, alttaki Dock=Fill grid'in örtmemesi için
            // üst kenara yaslanmış ayrı bir panelde durur.
            pnlContactsHeader.Dock = DockStyle.Top;
            pnlContactsHeader.Location = new Point(0, 0);
            pnlContactsHeader.Name = "pnlContactsHeader";
            pnlContactsHeader.Size = new Size(380, 78);
            pnlContactsHeader.TabIndex = 0;
            //
            // lblContactsTitle
            //
            lblContactsTitle.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblContactsTitle.Appearance.Options.UseFont = true;
            lblContactsTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblContactsTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblContactsTitle.Location = new Point(14, 12);
            lblContactsTitle.Name = "lblContactsTitle";
            lblContactsTitle.Size = new Size(190, 18);
            lblContactsTitle.TabIndex = 0;
            lblContactsTitle.Text = "Aktif Kullanıcılar";
            //
            // lblContactsCount
            //
            lblContactsCount.Appearance.Font = new Font("Segoe UI", 9F);
            lblContactsCount.Appearance.Options.UseFont = true;
            lblContactsCount.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblContactsCount.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            // Sağ kenara sabitlenir: "3 / 12" gibi bir sayaç panel daraldığında
            // sola yapışıp başlığın üstüne binmemelidir.
            lblContactsCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblContactsCount.Location = new Point(180, 12);
            lblContactsCount.Name = "lblContactsCount";
            lblContactsCount.Size = new Size(186, 18);
            lblContactsCount.TabIndex = 1;
            lblContactsCount.Text = string.Empty;
            //
            // txtContactSearch
            //
            txtContactSearch.Location = new Point(12, 40);
            txtContactSearch.Name = "txtContactSearch";
            txtContactSearch.Properties.NullText = "Ad, kullanıcı adı veya sicil no ara...";
            txtContactSearch.Properties.Padding = new Padding(26, 4, 26, 4);
            txtContactSearch.Size = new Size(356, 30);
            txtContactSearch.TabIndex = 2;
            //
            // gridUsers
            //
            gridUsers.Dock = DockStyle.Fill;
            gridUsers.Location = new Point(0, 78);
            gridUsers.MainView = viewUsers;
            gridUsers.Name = "gridUsers";
            gridUsers.Size = new Size(379, 408);
            gridUsers.TabIndex = 1;
            gridUsers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewUsers });
            //
            // viewUsers
            //
            viewUsers.GridControl = gridUsers;
            viewUsers.Name = "viewUsers";
            viewUsers.OptionsBehavior.Editable = false;
            viewUsers.OptionsSelection.MultiSelect = false;
            viewUsers.OptionsView.ShowGroupPanel = false;
            viewUsers.OptionsView.ShowIndicator = false;
            viewUsers.OptionsView.ColumnAutoWidth = false;
            //
            // pnlConversations
            //
            // Sağ yarı çoklu sohbet alanıdır: her konuşma bir sekmeye açılır ve
            // sekmeler üstte yan yana sıralanır; biri açıkken diğeri kapanmaz.
            // Konuşma açık değilken lblEmpty görünür.
            pnlConversations.Dock = DockStyle.Fill;
            pnlConversations.Location = new Point(380, 114);
            pnlConversations.Name = "pnlConversations";
            pnlConversations.Padding = new Padding(0, 0, 1, 0);
            pnlConversations.Size = new Size(700, 486);
            pnlConversations.TabIndex = 3;
            //
            // tabConversations
            //
            tabConversations.Dock = DockStyle.Fill;
            tabConversations.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;
            // Çok fazla açık konuşma olduğunda başlıklar alt satıra sarar; böylece
            // tek kaydırma şeridinde kaybolmaz, hepsi yan yana görünür kalır.
            tabConversations.MultiLine = DevExpress.Utils.DefaultBoolean.True;
            tabConversations.Location = new Point(0, 0);
            tabConversations.Name = "tabConversations";
            tabConversations.Size = new Size(699, 486);
            tabConversations.TabIndex = 0;
            tabConversations.Visible = false;
            //
            // lblEmpty
            //
            lblEmpty.Appearance.Font = new Font("Segoe UI", 10F);
            lblEmpty.Appearance.Options.UseFont = true;
            lblEmpty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblEmpty.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblEmpty.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblEmpty.Dock = DockStyle.Fill;
            lblEmpty.Location = new Point(0, 0);
            lblEmpty.Name = "lblEmpty";
            lblEmpty.Size = new Size(699, 486);
            lblEmpty.TabIndex = 1;
            lblEmpty.Text = "Soldaki listeden bir kullanıcı seçin.\r\nSohbetler bu alanda sekmeler hâlinde açılır.";
            //
            // MessagesListForm
            //
            ClientSize = new Size(960, 560);
            // EKLEME SIRASI ÖNEMLİDİR: Dock=Fill denetimler önce, üst/alt kenara
            // yaslanan (Dock=Top/Bottom) denetimler sonra eklenir. Ters sırada
            // yaslanan panel, kendisinden önce çözülen Fill panelin altında
            // kalır ve içerik görünmez.
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlToolbar.Controls.Add(btnAnnouncement);
            pnlToolbar.Controls.Add(btnSound);
            pnlContactsHeader.Controls.Add(lblContactsTitle);
            pnlContactsHeader.Controls.Add(lblContactsCount);
            pnlContactsHeader.Controls.Add(txtContactSearch);
            pnlContacts.Controls.Add(gridUsers);
            pnlContacts.Controls.Add(pnlContactsHeader);
            pnlConversations.Controls.Add(lblEmpty);
            pnlConversations.Controls.Add(tabConversations);
            Controls.Add(pnlConversations);
            Controls.Add(pnlContacts);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(800, 440);
            Name = "MessagesListForm";
            ShowInTaskbar = false;
            Text = "Mesajlar";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtContactSearch.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlContactsHeader).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlConversations).EndInit();
            ((System.ComponentModel.ISupportInitialize)tabConversations).EndInit();
            ResumeLayout(false);
        }
    }
}