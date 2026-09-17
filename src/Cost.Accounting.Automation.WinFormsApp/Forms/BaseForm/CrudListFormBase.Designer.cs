namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public abstract partial class CrudListFormBase<TListQuery, TDto, TEditForm>
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSub;
        private DevExpress.XtraEditors.SimpleButton btnClosePage;
        private DevExpress.XtraEditors.PanelControl pnlToolbar;
        private DevExpress.XtraEditors.SimpleButton btnNew;
        private DevExpress.XtraEditors.SimpleButton btnEdit;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.SimpleButton btnSlipPrint;
        private DevExpress.XtraEditors.SimpleButton btnApprove;
        private DevExpress.XtraEditors.CheckButton btnDeleted;
        private DevExpress.XtraEditors.SimpleButton btnRestore;
        private DevExpress.XtraEditors.LabelControl lblFilter;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbFilter;
        private DevExpress.XtraGrid.Views.Grid.GridView cmbFilterView;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;

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
            components = new System.ComponentModel.Container();
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSub = new DevExpress.XtraEditors.LabelControl();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            pnlToolbar = new DevExpress.XtraEditors.PanelControl();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            btnSlipPrint = new DevExpress.XtraEditors.SimpleButton();
            btnDelete = new DevExpress.XtraEditors.SimpleButton();
            btnEdit = new DevExpress.XtraEditors.SimpleButton();
            btnNew = new DevExpress.XtraEditors.SimpleButton();
            btnApprove = new DevExpress.XtraEditors.SimpleButton();
            btnDeleted = new DevExpress.XtraEditors.CheckButton();
            btnRestore = new DevExpress.XtraEditors.SimpleButton();
            lblFilter = new DevExpress.XtraEditors.LabelControl();
            cmbFilter = new DevExpress.XtraEditors.SearchLookUpEdit();
            cmbFilterView = new DevExpress.XtraGrid.Views.Grid.GridView();
            txtSearch = new DevExpress.XtraEditors.TextEdit();
            gridControl = new DevExpress.XtraGrid.GridControl();
            gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilter.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilterView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(btnClosePage);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(1280, 110);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new System.Drawing.Point(28, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "-";
            // 
            // lblSub
            // 
            lblSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Location = new System.Drawing.Point(30, 68);
            lblSub.Name = "lblSub";
            lblSub.TabIndex = 1;
            lblSub.Text = "Yükleniyor...";
            // 
            // btnClosePage
            // 
            btnClosePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            btnClosePage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClosePage.Appearance.Options.UseFont = true;
            btnClosePage.Location = new System.Drawing.Point(1170, 37);
            btnClosePage.Name = "btnClosePage";
            btnClosePage.Size = new System.Drawing.Size(94, 36);
            btnClosePage.TabIndex = 2;
            btnClosePage.Text = "Kapat";
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Controls.Add(cmbFilter);
            pnlToolbar.Controls.Add(lblFilter);
            pnlToolbar.Controls.Add(btnRestore);
            pnlToolbar.Controls.Add(btnDeleted);
            pnlToolbar.Controls.Add(btnApprove);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnSlipPrint);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            pnlToolbar.Location = new System.Drawing.Point(0, 110);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            pnlToolbar.Size = new System.Drawing.Size(1280, 56);
            pnlToolbar.TabIndex = 1;
            // 
            // btnNew
            // 
            btnNew.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNew.Appearance.Options.UseFont = true;
            btnNew.Location = new System.Drawing.Point(16, 10);
            btnNew.Name = "btnNew";
            btnNew.Size = new System.Drawing.Size(140, 36);
            btnNew.TabIndex = 0;
            btnNew.Text = "Yeni";
            // 
            // btnEdit
            // 
            btnEdit.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnEdit.Appearance.Options.UseFont = true;
            btnEdit.Location = new System.Drawing.Point(162, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(120, 36);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Düzenle";
            // 
            // btnDelete
            // 
            btnDelete.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnDelete.Appearance.Options.UseFont = true;
            btnDelete.Location = new System.Drawing.Point(288, 10);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(110, 36);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Sil";
            // 
            // btnRefresh
            // 
            btnRefresh.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnRefresh.Appearance.Options.UseFont = true;
            btnRefresh.Location = new System.Drawing.Point(404, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(60, 36);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "";
            // 
            // btnSlipPrint
            // 
            btnSlipPrint.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnSlipPrint.Appearance.Options.UseFont = true;
            btnSlipPrint.Location = new System.Drawing.Point(470, 10);
            btnSlipPrint.Name = "btnSlipPrint";
            btnSlipPrint.Size = new System.Drawing.Size(120, 36);
            btnSlipPrint.TabIndex = 9;
            btnSlipPrint.Text = "TIF Yazdır";
            // 
            // btnApprove
            // 
            btnApprove.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnApprove.Appearance.Options.UseFont = true;
            btnApprove.Location = new System.Drawing.Point(596, 10);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new System.Drawing.Size(100, 36);
            btnApprove.TabIndex = 7;
            btnApprove.Text = "Onayla";
            // 
            // btnDeleted
            // 
            btnDeleted.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnDeleted.Appearance.Options.UseFont = true;
            btnDeleted.Location = new System.Drawing.Point(722, 10);
            btnDeleted.Name = "btnDeleted";
            btnDeleted.Size = new System.Drawing.Size(120, 36);
            btnDeleted.TabIndex = 5;
            btnDeleted.Text = "Silinenler";
            // 
            // btnRestore
            // 
            btnRestore.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnRestore.Appearance.Options.UseFont = true;
            btnRestore.Location = new System.Drawing.Point(848, 10);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new System.Drawing.Size(115, 36);
            btnRestore.TabIndex = 6;
            btnRestore.Text = "Geri Yükle";
            // 
            // lblFilter
            // 
            lblFilter.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblFilter.Appearance.Options.UseFont = true;
            lblFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            lblFilter.Location = new System.Drawing.Point(850, 18);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new System.Drawing.Size(46, 19);
            lblFilter.Text = "Filtre:";
            lblFilter.Visible = false;
            // 
            // cmbFilter
            // 
            cmbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            cmbFilter.Location = new System.Drawing.Point(902, 13);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            cmbFilter.Properties.NullText = "Tümü";
            cmbFilter.Properties.PopupView = cmbFilterView;
            cmbFilter.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbFilter.Size = new System.Drawing.Size(170, 30);
            cmbFilter.TabIndex = 8;
            cmbFilter.Visible = false;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            txtSearch.Location = new System.Drawing.Point(1082, 13);
            txtSearch.Name = "txtSearch";
            txtSearch.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtSearch.Properties.Appearance.Options.UseFont = true;
            txtSearch.Properties.NullText = "Ara...";
            txtSearch.Size = new System.Drawing.Size(182, 30);
            txtSearch.TabIndex = 4;
            // 
            // gridControl
            // 
            gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            gridControl.Location = new System.Drawing.Point(0, 148);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.Size = new System.Drawing.Size(1100, 532);
            gridControl.TabIndex = 2;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            // 
            // gridView
            // 
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            // 
            // CrudListFormBase
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 680);
            Controls.Add(gridControl);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            MinimumSize = new System.Drawing.Size(1000, 600);
            Name = "CrudListFormBase";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtSearch.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilter.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilterView).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ResumeLayout(false);
        }
    }
}