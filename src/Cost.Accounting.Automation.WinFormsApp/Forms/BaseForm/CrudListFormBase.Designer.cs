using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public abstract partial class CrudListFormBase<TListQuery, TDto, TEditForm>
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.PanelControl headerAccent;
        private DevExpress.XtraEditors.PanelControl headerDivider;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSub;
        private DevExpress.XtraEditors.SimpleButton btnClosePage;
        private DevExpress.XtraEditors.PanelControl pnlToolbar;
        /// <summary>Türetilmiş formların kendi butonlarını ekleyebilmesi için korunur.</summary>
        protected System.Windows.Forms.FlowLayoutPanel flpToolbar;
        private DevExpress.XtraEditors.SimpleButton btnNew;
        private DevExpress.XtraEditors.SimpleButton btnEdit;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.SimpleButton btnSlipPrint;
        private DevExpress.XtraEditors.SimpleButton btnSlipReport;
        protected DevExpress.XtraEditors.SimpleButton btnDistributionReport;
        protected DevExpress.XtraEditors.SimpleButton btnProductDeclaration;
        protected DevExpress.XtraEditors.SimpleButton btnStockMovementsList;
        protected DevExpress.XtraEditors.SimpleButton btnStockCountList;
        private DevExpress.XtraEditors.SimpleButton btnApprove;
        private DevExpress.XtraEditors.CheckButton btnDeleted;
        private DevExpress.XtraEditors.SimpleButton btnRestore;
        private DevExpress.XtraEditors.LabelControl lblFilter;
        private DevExpress.XtraEditors.SearchLookUpEdit cmbFilter;
        private DevExpress.XtraGrid.Views.Grid.GridView cmbFilterView;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;
        private DevExpress.XtraEditors.PictureEdit picModuleIcon;
        private System.Windows.Forms.ContextMenuStrip slipMenu;
        private System.Windows.Forms.ToolStripMenuItem slipMenuItem;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _skinBinding?.Dispose();
            }

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
            headerAccent = new DevExpress.XtraEditors.PanelControl();
            headerDivider = new DevExpress.XtraEditors.PanelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSub = new DevExpress.XtraEditors.LabelControl();
            btnClosePage = new DevExpress.XtraEditors.SimpleButton();
            pnlToolbar = new DevExpress.XtraEditors.PanelControl();
            flpToolbar = new System.Windows.Forms.FlowLayoutPanel();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            btnSlipPrint = new DevExpress.XtraEditors.SimpleButton();
            btnSlipReport = new DevExpress.XtraEditors.SimpleButton();
            btnDistributionReport = new DevExpress.XtraEditors.SimpleButton();
            btnProductDeclaration = new DevExpress.XtraEditors.SimpleButton();
            btnStockMovementsList = new DevExpress.XtraEditors.SimpleButton();
            btnStockCountList = new DevExpress.XtraEditors.SimpleButton();
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
            picModuleIcon = new DevExpress.XtraEditors.PictureEdit();
            slipMenu = new System.Windows.Forms.ContextMenuStrip(components);
            slipMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            slipMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtSearch.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilter.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbFilterView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picModuleIcon.Properties).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(btnClosePage);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(picModuleIcon);
            pnlHeader.Controls.Add(headerDivider);
            pnlHeader.Controls.Add(headerAccent);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(1280, 110);
            pnlHeader.TabIndex = 0;
            // 
            // headerAccent
            // 
            headerAccent.Appearance.Options.UseBackColor = true;
            headerAccent.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerAccent.Dock = System.Windows.Forms.DockStyle.Top;
            headerAccent.Location = new System.Drawing.Point(0, 0);
            headerAccent.Margin = new System.Windows.Forms.Padding(0);
            headerAccent.Name = "headerAccent";
            headerAccent.Size = new System.Drawing.Size(1280, 4);
            headerAccent.TabIndex = 4;
            // 
            // headerDivider
            // 
            headerDivider.Appearance.Options.UseBackColor = true;
            headerDivider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            headerDivider.Dock = System.Windows.Forms.DockStyle.Bottom;
            headerDivider.Location = new System.Drawing.Point(0, 109);
            headerDivider.Margin = new System.Windows.Forms.Padding(0);
            headerDivider.Name = "headerDivider";
            headerDivider.Size = new System.Drawing.Size(1280, 1);
            headerDivider.TabIndex = 5;
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new System.Drawing.Point(82, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "-";
            // 
            // lblSub
            // 
            lblSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Location = new System.Drawing.Point(84, 66);
            lblSub.Name = "lblSub";
            lblSub.TabIndex = 1;
            lblSub.Text = "Yükleniyor...";
            // 
            // picModuleIcon
            // 
            picModuleIcon.BackColor = System.Drawing.Color.Transparent;
            picModuleIcon.Location = new System.Drawing.Point(28, 31);
            picModuleIcon.Name = "picModuleIcon";
            picModuleIcon.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            picModuleIcon.Properties.Appearance.Options.UseBackColor = true;
            picModuleIcon.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picModuleIcon.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
            picModuleIcon.Size = new System.Drawing.Size(42, 42);
            picModuleIcon.TabIndex = 3;
            picModuleIcon.TabStop = false;
            // 
            // slipMenuItem
            // 
            slipMenuItem.Name = "slipMenuItem";
            slipMenuItem.Size = new System.Drawing.Size(210, 44);
            slipMenuItem.Text = "Taşınır İşlem Fişi Yazdır";
            // 
            // slipMenu
            // 
            slipMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { slipMenuItem });
            slipMenu.Name = "slipMenu";
            slipMenu.Size = new System.Drawing.Size(211, 48);
            // 
            // btnClosePage
            // 
            btnClosePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            btnClosePage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClosePage.Appearance.Options.UseFont = true;
            btnClosePage.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClosePage.ImageOptions.SvgImage = DxIcon.Close;
            btnClosePage.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
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
            pnlToolbar.Controls.Add(flpToolbar);
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
            btnNew.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnNew.ImageOptions.SvgImage = DxIcon.Add;
            btnNew.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnNew.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnNew.Name = "btnNew";
            btnNew.Size = new System.Drawing.Size(84, 36);
            btnNew.TabIndex = 0;
            btnNew.Text = "Yeni";
            // 
            // btnEdit
            // 
            btnEdit.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnEdit.Appearance.Options.UseFont = true;
            btnEdit.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnEdit.ImageOptions.SvgImage = DxIcon.Edit;
            btnEdit.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnEdit.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(104, 36);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Düzenle";
            // 
            // btnDelete
            // 
            btnDelete.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnDelete.Appearance.Options.UseFont = true;
            btnDelete.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnDelete.ImageOptions.SvgImage = DxIcon.Delete;
            btnDelete.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnDelete.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(74, 36);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Sil";
            // 
            // btnRefresh
            // 
            btnRefresh.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnRefresh.Appearance.Options.UseFont = true;
            btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;
            btnRefresh.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(40, 36);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "";
            // 
            // btnSlipPrint
            // 
            btnSlipPrint.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnSlipPrint.Appearance.Options.UseFont = true;
            btnSlipPrint.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSlipPrint.ImageOptions.SvgImage = DxIcon.Receipt;
            btnSlipPrint.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnSlipPrint.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnSlipPrint.Name = "btnSlipPrint";
            btnSlipPrint.Size = new System.Drawing.Size(118, 36);
            btnSlipPrint.TabIndex = 9;
            btnSlipPrint.Text = "TIF Yazdır";
            // 
            // btnSlipReport
            // 
            btnSlipReport.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnSlipReport.Appearance.Options.UseFont = true;
            btnSlipReport.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnSlipReport.ImageOptions.SvgImage = DxIcon.Receipt;
            btnSlipReport.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnSlipReport.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnSlipReport.Name = "btnSlipReport";
            btnSlipReport.Size = new System.Drawing.Size(200, 36);
            btnSlipReport.TabIndex = 10;
            btnSlipReport.Text = "Maliyet Pusulası Yazdır";
            btnSlipReport.Visible = false;
            // 
            // btnDistributionReport
            // 
            btnDistributionReport.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnDistributionReport.Appearance.Options.UseFont = true;
            btnDistributionReport.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnDistributionReport.ImageOptions.SvgImage = DxIcon.Receipt;
            btnDistributionReport.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnDistributionReport.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnDistributionReport.Name = "btnDistributionReport";
            btnDistributionReport.Size = new System.Drawing.Size(235, 36);
            btnDistributionReport.TabIndex = 11;
            btnDistributionReport.Text = "Gider Dağıtım Tablosu Yazdır";
            btnDistributionReport.Visible = false;
            // 
            // btnProductDeclaration
            // 
            btnProductDeclaration.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnProductDeclaration.Appearance.Options.UseFont = true;
            btnProductDeclaration.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnProductDeclaration.ImageOptions.SvgImage = DxIcon.Receipt;
            btnProductDeclaration.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnProductDeclaration.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnProductDeclaration.Name = "btnProductDeclaration";
            btnProductDeclaration.Size = new System.Drawing.Size(200, 36);
            btnProductDeclaration.TabIndex = 12;
            btnProductDeclaration.Text = "Mamül Beyan Yazdır";
            btnProductDeclaration.Visible = false;
            // 
            // btnStockMovementsList
            // 
            btnStockMovementsList.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnStockMovementsList.Appearance.Options.UseFont = true;
            btnStockMovementsList.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnStockMovementsList.ImageOptions.SvgImage = DxIcon.Receipt;
            btnStockMovementsList.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnStockMovementsList.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnStockMovementsList.Name = "btnStockMovementsList";
            btnStockMovementsList.Size = new System.Drawing.Size(215, 36);
            btnStockMovementsList.TabIndex = 13;
            btnStockMovementsList.Text = "Stok Hareket Listesi Yazdır";
            btnStockMovementsList.Visible = false;
            // 
            // btnStockCountList
            // 
            btnStockCountList.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnStockCountList.Appearance.Options.UseFont = true;
            btnStockCountList.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnStockCountList.ImageOptions.SvgImage = DxIcon.StockBox;
            btnStockCountList.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnStockCountList.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnStockCountList.Name = "btnStockCountList";
            btnStockCountList.Size = new System.Drawing.Size(205, 36);
            btnStockCountList.TabIndex = 14;
            btnStockCountList.Text = "Stok Sayım Listesi Yazdır";
            btnStockCountList.Visible = false;
            // 
            // btnApprove
            // 
            btnApprove.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnApprove.Appearance.Options.UseFont = true;
            btnApprove.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnApprove.ImageOptions.SvgImage = DxIcon.Check;
            btnApprove.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnApprove.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new System.Drawing.Size(84, 36);
            btnApprove.TabIndex = 7;
            btnApprove.Text = "Onayla";
            // 
            // btnDeleted
            // 
            btnDeleted.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnDeleted.Appearance.Options.UseFont = true;
            btnDeleted.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnDeleted.ImageOptions.SvgImage = DxIcon.Delete;
            btnDeleted.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnDeleted.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnDeleted.Name = "btnDeleted";
            btnDeleted.Size = new System.Drawing.Size(94, 36);
            btnDeleted.TabIndex = 5;
            btnDeleted.Text = "Silinenler";
            // 
            // btnRestore
            // 
            btnRestore.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnRestore.Appearance.Options.UseFont = true;
            btnRestore.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnRestore.ImageOptions.SvgImage = DxIcon.Restore;
            btnRestore.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnRestore.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new System.Drawing.Size(90, 36);
            btnRestore.TabIndex = 6;
            btnRestore.Text = "Geri Yükle";
            // 
            // flpToolbar
            // 
            flpToolbar.Controls.Add(btnNew);
            flpToolbar.Controls.Add(btnEdit);
            flpToolbar.Controls.Add(btnDelete);
            flpToolbar.Controls.Add(btnRefresh);
            flpToolbar.Controls.Add(btnSlipPrint);
            flpToolbar.Controls.Add(btnSlipReport);
            flpToolbar.Controls.Add(btnDistributionReport);
            flpToolbar.Controls.Add(btnProductDeclaration);
            flpToolbar.Controls.Add(btnStockMovementsList);
            flpToolbar.Controls.Add(btnStockCountList);
            flpToolbar.Controls.Add(btnApprove);
            flpToolbar.Controls.Add(btnDeleted);
            flpToolbar.Controls.Add(btnRestore);
            flpToolbar.Location = new System.Drawing.Point(16, 10);
            flpToolbar.Name = "flpToolbar";
            flpToolbar.Size = new System.Drawing.Size(1460, 36);
            flpToolbar.TabIndex = 12;
            flpToolbar.WrapContents = false;
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

            gridControl.ContextMenuStrip = slipMenu; 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1500, 680);
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
            ((System.ComponentModel.ISupportInitialize)picModuleIcon.Properties).EndInit();
            slipMenu.ResumeLayout(false);
            slipMenu.PerformLayout();
            ResumeLayout(false);
        }
    }
}