namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    partial class MessagesListForm
    {
        private DevExpress.XtraEditors.PanelControl pnlHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSubtitle = null!;
        private DevExpress.XtraEditors.PanelControl pnlToolbar = null!;
        private DevExpress.XtraEditors.SimpleButton btnAnnouncement = null!;
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
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).BeginInit();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtContactSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlContactsHeader).BeginInit();
            pnlContactsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlContacts).BeginInit();
            pnlContacts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)viewUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlConversations).BeginInit();
            pnlConversations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tabConversations).BeginInit();
            tabConversations.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1080, 50);
            pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 12F, System.Drawing.FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblTitle.Location = new Point(14, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Mesajlar";
            //
            // lblSubtitle
            //
            lblSubtitle.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 8.5F, System.Drawing.FontStyle.Regular);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSubtitle.Location = new Point(14, 28);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(500, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Yükleniyor...";
            //
            // pnlToolbar
            //
            pnlToolbar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlToolbar.Controls.Add(btnAnnouncement);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 50);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1080, 42);
            pnlToolbar.TabIndex = 1;
            //
            // btnAnnouncement
            //
            btnAnnouncement.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 9F, System.Drawing.FontStyle.Bold); // SemiBold -> Bold
            btnAnnouncement.Appearance.Options.UseFont = true;
            btnAnnouncement.Location = new Point(12, 4);
            btnAnnouncement.Name = "btnAnnouncement";
            btnAnnouncement.Size = new Size(135, 30);
            btnAnnouncement.TabIndex = 0;
            btnAnnouncement.Text = "Duyuru Gönder";
            //
            // pnlContacts
            //
            pnlContacts.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlContacts.Controls.Add(gridUsers);
            pnlContacts.Controls.Add(pnlContactsHeader);
            pnlContacts.Dock = DockStyle.Left;
            pnlContacts.Location = new Point(0, 92);
            pnlContacts.Name = "pnlContacts";
            pnlContacts.Size = new Size(320, 540);
            pnlContacts.TabIndex = 2;
            //
            // pnlContactsHeader
            //
            pnlContactsHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlContactsHeader.Controls.Add(lblContactsTitle);
            pnlContactsHeader.Controls.Add(lblContactsCount);
            pnlContactsHeader.Controls.Add(txtContactSearch);
            pnlContactsHeader.Dock = DockStyle.Top;
            pnlContactsHeader.Location = new Point(0, 0);
            pnlContactsHeader.Name = "pnlContactsHeader";
            pnlContactsHeader.Size = new Size(320, 72);
            pnlContactsHeader.TabIndex = 0;
            //
            // lblContactsTitle
            //
            lblContactsTitle.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 9.5F, System.Drawing.FontStyle.Bold);
            lblContactsTitle.Appearance.Options.UseFont = true;
            lblContactsTitle.Location = new Point(12, 10);
            lblContactsTitle.Name = "lblContactsTitle";
            lblContactsTitle.Size = new Size(54, 16);
            lblContactsTitle.TabIndex = 0;
            lblContactsTitle.Text = "Sohbetler";
            //
            // lblContactsCount
            //
            lblContactsCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblContactsCount.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 8.25F, System.Drawing.FontStyle.Regular);
            lblContactsCount.Appearance.Options.UseFont = true;
            lblContactsCount.Appearance.Options.UseTextOptions = true;
            lblContactsCount.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblContactsCount.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblContactsCount.Location = new Point(160, 10);
            lblContactsCount.Name = "lblContactsCount";
            lblContactsCount.Size = new Size(148, 16);
            lblContactsCount.TabIndex = 1;
            //
            // txtContactSearch
            //
            txtContactSearch.Location = new Point(12, 34);
            txtContactSearch.Name = "txtContactSearch";
            txtContactSearch.Properties.NullText = "Ara veya yeni başlat...";
            txtContactSearch.Properties.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 9F, System.Drawing.FontStyle.Regular);
            txtContactSearch.Properties.Appearance.Options.UseFont = true;
            txtContactSearch.Size = new Size(296, 26);
            txtContactSearch.TabIndex = 2;
            //
            // gridUsers
            //
            gridUsers.Dock = DockStyle.Fill;
            gridUsers.Location = new Point(0, 72);
            gridUsers.MainView = viewUsers;
            gridUsers.Name = "gridUsers";
            gridUsers.Size = new Size(320, 468);
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
            viewUsers.OptionsView.ColumnAutoWidth = true;
            //
            // pnlConversations
            //
            pnlConversations.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlConversations.Controls.Add(lblEmpty);
            pnlConversations.Controls.Add(tabConversations);
            pnlConversations.Dock = DockStyle.Fill;
            pnlConversations.Location = new Point(320, 92);
            pnlConversations.Name = "pnlConversations";
            pnlConversations.Size = new Size(760, 540);
            pnlConversations.TabIndex = 3;
            //
            // lblEmpty
            //
            lblEmpty.Appearance.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Segoe UI"), 11F, System.Drawing.FontStyle.Regular);
            lblEmpty.Appearance.Options.UseFont = true;
            lblEmpty.Appearance.Options.UseTextOptions = true;
            lblEmpty.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblEmpty.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblEmpty.Dock = DockStyle.Fill;
            lblEmpty.Location = new Point(0, 0);
            lblEmpty.Name = "lblEmpty";
            lblEmpty.Size = new Size(760, 540);
            lblEmpty.TabIndex = 0;
            lblEmpty.Text = "Mesajlaşmaya başlamak için soldan bir kişi seçin.";
            //
            // tabConversations
            //
            tabConversations.Dock = DockStyle.Fill;
            tabConversations.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;
            tabConversations.Location = new Point(0, 0);
            tabConversations.Name = "tabConversations";
            tabConversations.Size = new Size(760, 540);
            tabConversations.TabIndex = 1;
            tabConversations.Visible = false;
            //
            // MessagesListForm
            //
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1080, 632);
            Controls.Add(pnlConversations);
            Controls.Add(pnlContacts);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Name = "MessagesListForm";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlToolbar).EndInit();
            pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtContactSearch.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlContactsHeader).EndInit();
            pnlContactsHeader.ResumeLayout(false);
            pnlContactsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlContacts).EndInit();
            pnlContacts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)viewUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlConversations).EndInit();
            pnlConversations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tabConversations).EndInit();
            tabConversations.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}