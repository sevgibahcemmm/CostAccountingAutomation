namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    sealed partial class CurrentAccountBalanceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.PictureEdit picHeader;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSub;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.PanelControl pnlToolbar;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.SimpleButton btnDebtors;
        private DevExpress.XtraEditors.SimpleButton btnCreditors;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;

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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new DevExpress.XtraEditors.PanelControl();
            this.picHeader = new DevExpress.XtraEditors.PictureEdit();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblSub = new DevExpress.XtraEditors.LabelControl();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.pnlToolbar = new DevExpress.XtraEditors.PanelControl();
            this.btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.btnDebtors = new DevExpress.XtraEditors.SimpleButton();
            this.btnCreditors = new DevExpress.XtraEditors.SimpleButton();
            this.gridControl = new DevExpress.XtraGrid.GridControl();
            this.gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)this.pnlHeader).BeginInit();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.picHeader.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.pnlToolbar).BeginInit();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridView).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSub);
            this.pnlHeader.Controls.Add(this.picHeader);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1280, 110);
            this.pnlHeader.TabIndex = 0;
            // 
            // picHeader
            // 
            this.picHeader.BackColor = System.Drawing.Color.Transparent;
            this.picHeader.Location = new System.Drawing.Point(28, 34);
            this.picHeader.Name = "picHeader";
            this.picHeader.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.picHeader.Properties.Appearance.Options.UseBackColor = true;
            this.picHeader.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.picHeader.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
            this.picHeader.Size = new System.Drawing.Size(42, 42);
            this.picHeader.TabIndex = 2;
            this.picHeader.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle.Location = new System.Drawing.Point(82, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(900, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Cari Borç / Alacak Özeti";
            // 
            // lblSub
            // 
            this.lblSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSub.Appearance.ForeColor = System.Drawing.Color.FromArgb(130, 136, 146);
            this.lblSub.Appearance.Options.UseFont = true;
            this.lblSub.Appearance.Options.UseForeColor = true;
            this.lblSub.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblSub.Location = new System.Drawing.Point(84, 66);
            this.lblSub.Name = "lblSub";
            this.lblSub.Size = new System.Drawing.Size(900, 18);
            this.lblSub.TabIndex = 1;
            this.lblSub.Text = "Yükleniyor...";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.Appearance.Options.UseFont = true;
            this.btnClose.Location = new System.Drawing.Point(1170, 37);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(94, 36);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Kapat";
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.Controls.Add(this.btnRefresh);
            this.pnlToolbar.Controls.Add(this.btnDebtors);
            this.pnlToolbar.Controls.Add(this.btnCreditors);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 110);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.pnlToolbar.Size = new System.Drawing.Size(1280, 56);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 36);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Yenile";
            // 
            // btnDebtors
            // 
            this.btnDebtors.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnDebtors.Name = "btnDebtors";
            this.btnDebtors.Size = new System.Drawing.Size(100, 36);
            this.btnDebtors.TabIndex = 1;
            this.btnDebtors.Text = "Borçlular";
            // 
            // btnCreditors
            // 
            this.btnCreditors.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCreditors.Name = "btnCreditors";
            this.btnCreditors.Size = new System.Drawing.Size(110, 36);
            this.btnCreditors.TabIndex = 2;
            this.btnCreditors.Text = "Alacaklılar";
            // 
            // gridControl
            // 
            this.gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl.Location = new System.Drawing.Point(0, 166);
            this.gridControl.MainView = this.gridView;
            this.gridControl.Name = "gridControl";
            this.gridControl.Size = new System.Drawing.Size(1280, 514);
            this.gridControl.TabIndex = 2;
            this.gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridView });
            // 
            // gridView
            // 
            this.gridView.GridControl = this.gridControl;
            this.gridView.Name = "gridView";
            this.gridView.OptionsBehavior.Editable = false;
            this.gridView.OptionsView.EnableAppearanceEvenRow = true;
            this.gridView.OptionsView.EnableAppearanceOddRow = true;
            this.gridView.OptionsView.ShowGroupPanel = false;
            // 
            // CurrentAccountBalanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 680);
            this.Controls.Add(this.gridControl);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.Name = "CurrentAccountBalanceForm";
            this.Text = "Cari Borç / Alacak Özeti";
            ((System.ComponentModel.ISupportInitialize)this.pnlHeader).EndInit();
            this.pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.picHeader.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.pnlToolbar).EndInit();
            this.pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridView).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}