namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    partial class NewMessageForm
    {
        private DevExpress.XtraEditors.PanelControl pnlHeader = null!;
        private DevExpress.XtraEditors.LabelControl lblTitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSubtitle = null!;
        private DevExpress.XtraEditors.LabelControl lblSearch = null!;
        private DevExpress.XtraEditors.TextEdit txtSearch = null!;
        private DevExpress.XtraGrid.GridControl gridRecipients = null!;
        private DevExpress.XtraGrid.Views.Grid.GridView viewRecipients = null!;
        private DevExpress.XtraEditors.LabelControl lblSelection = null!;
        private DevExpress.XtraEditors.LabelControl lblSubject = null!;
        private DevExpress.XtraEditors.TextEdit txtSubject = null!;
        private DevExpress.XtraEditors.LabelControl lblBody = null!;
        private DevExpress.XtraEditors.MemoEdit txtBody = null!;
        private DevExpress.XtraEditors.SimpleButton btnSend = null!;
        private DevExpress.XtraEditors.SimpleButton btnCancel = null!;
        private DevExpress.XtraEditors.SimpleButton btnClearSelection = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                skinBinding?.Dispose();
                _searchTimer?.Dispose();
                gridRecipients?.Dispose();
                btnSend?.Dispose();
                btnCancel?.Dispose();
                btnClearSelection?.Dispose();
                txtSearch?.Dispose();
                txtSubject?.Dispose();
                txtBody?.Dispose();
                lblTitle?.Dispose();
                lblSubtitle?.Dispose();
                lblSearch?.Dispose();
                lblSelection?.Dispose();
                lblSubject?.Dispose();
                lblBody?.Dispose();
                base.Dispose(disposing);
            }
        }

        private void InitializeComponent()
        {
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblSearch = new DevExpress.XtraEditors.LabelControl();
            txtSearch = new DevExpress.XtraEditors.TextEdit();
            gridRecipients = new DevExpress.XtraGrid.GridControl();
            viewRecipients = new DevExpress.XtraGrid.Views.Grid.GridView();
            lblSelection = new DevExpress.XtraEditors.LabelControl();
            btnClearSelection = new DevExpress.XtraEditors.SimpleButton();
            lblSubject = new DevExpress.XtraEditors.LabelControl();
            txtSubject = new DevExpress.XtraEditors.TextEdit();
            lblBody = new DevExpress.XtraEditors.LabelControl();
            txtBody = new DevExpress.XtraEditors.MemoEdit();
            btnSend = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtSubject.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtBody.Properties).BeginInit();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(16, 12, 16, 12);
            pnlHeader.Size = new Size(880, 62);
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
            lblTitle.Text = "Yeni Mesaj";
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
            // lblSearch
            //
            lblSearch.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSearch.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblSearch.Appearance.Options.UseFont = true;
            lblSearch.Appearance.Options.UseForeColor = true;
            lblSearch.Location = new Point(20, 76);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(140, 14);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Sicil / TC / Ad / Kullanıcı";
            //
            // txtSearch
            //
            txtSearch.Location = new Point(20, 94);
            txtSearch.Name = "txtSearch";
            txtSearch.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtSearch.Properties.Appearance.Options.UseFont = true;
            txtSearch.Properties.NullText = "Sicil numarası, TC kimlik numarası, ad soyad veya kullanıcı adı yazın";
            txtSearch.Properties.Padding = new Padding(26, 2, 2, 2);
            txtSearch.Size = new Size(840, 26);
            txtSearch.TabIndex = 1;
            //
            // gridRecipients
            //
            gridRecipients.Location = new Point(20, 128);
            gridRecipients.MainView = viewRecipients;
            gridRecipients.Name = "gridRecipients";
            gridRecipients.Size = new Size(840, 250);
            gridRecipients.TabIndex = 2;
            gridRecipients.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewRecipients });
            //
            // viewRecipients
            //
            viewRecipients.GridControl = gridRecipients;
            viewRecipients.Name = "viewRecipients";
            viewRecipients.OptionsBehavior.Editable = false;
            viewRecipients.OptionsSelection.MultiSelect = true;
            viewRecipients.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            viewRecipients.OptionsView.ShowGroupPanel = false;
            viewRecipients.OptionsView.ShowIndicator = false;
            viewRecipients.OptionsView.ColumnAutoWidth = false;
            //
            // lblSelection
            //
            lblSelection.Appearance.Options.UseTextOptions = true;
            lblSelection.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblSelection.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSelection.Location = new Point(20, 384);
            lblSelection.Name = "lblSelection";
            lblSelection.Size = new Size(560, 18);
            lblSelection.TabIndex = 3;
            lblSelection.Text = "Kimse seçilmedi";
            //
            // btnClearSelection
            //
            btnClearSelection.Location = new Point(690, 382);
            btnClearSelection.Name = "btnClearSelection";
            btnClearSelection.Size = new Size(170, 26);
            btnClearSelection.TabIndex = 4;
            btnClearSelection.Text = "Seçimi Temizle";
            //
            // lblSubject
            //
            lblSubject.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubject.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblSubject.Appearance.Options.UseFont = true;
            lblSubject.Appearance.Options.UseForeColor = true;
            lblSubject.Location = new Point(20, 416);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(34, 14);
            lblSubject.TabIndex = 5;
            lblSubject.Text = "Konu";
            //
            // txtSubject
            //
            txtSubject.Location = new Point(20, 434);
            txtSubject.Name = "txtSubject";
            txtSubject.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtSubject.Properties.Appearance.Options.UseFont = true;
            txtSubject.Properties.NullText = "Konu (isteğe bağlı)";
            txtSubject.Properties.Padding = new Padding(26, 2, 2, 2);
            txtSubject.Size = new Size(840, 26);
            txtSubject.TabIndex = 6;
            //
            // lblBody
            //
            lblBody.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblBody.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblBody.Appearance.Options.UseFont = true;
            lblBody.Appearance.Options.UseForeColor = true;
            lblBody.Location = new Point(20, 472);
            lblBody.Name = "lblBody";
            lblBody.Size = new Size(60, 14);
            lblBody.TabIndex = 7;
            lblBody.Text = "Mesajınız";
            //
            // txtBody
            //
            txtBody.Location = new Point(20, 490);
            txtBody.Name = "txtBody";
            txtBody.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtBody.Properties.Appearance.Options.UseFont = true;
            txtBody.Properties.NullText = "Mesajınızı yazın...";
            txtBody.Properties.Padding = new Padding(4, 4, 4, 4);
            txtBody.Size = new Size(840, 110);
            txtBody.TabIndex = 8;
            //
            // btnSend
            //
            btnSend.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSend.Location = new Point(690, 610);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(170, 36);
            btnSend.TabIndex = 9;
            btnSend.Text = "Gönder";
            //
            // btnCancel
            //
            btnCancel.Location = new Point(600, 610);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 36);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Vazgeç";
            //
            // NewMessageForm
            //
            ClientSize = new Size(880, 660);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            Controls.Add(gridRecipients);
            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(lblSelection);
            Controls.Add(btnClearSelection);
            Controls.Add(lblSubject);
            Controls.Add(txtSubject);
            Controls.Add(lblBody);
            Controls.Add(txtBody);
            Controls.Add(btnSend);
            Controls.Add(btnCancel);
            Controls.Add(pnlHeader);
            MinimumSize = new Size(700, 560);
            Name = "NewMessageForm";
            ShowInTaskbar = false;
            Text = "Yeni Mesaj";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSearch.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewRecipients).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridRecipients).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtSubject.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtBody.Properties).EndInit();
            ResumeLayout(false);
        }
    }
}