namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    abstract partial class StockIssueEditFormBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblDocumentNumber;
        private System.Windows.Forms.Label lblCosting;
        private System.Windows.Forms.Label lblWarehouse;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblLines;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblTotalValue;
        private DevExpress.XtraEditors.DateEdit dtDate;
        private DevExpress.XtraEditors.TextEdit txtDocumentNumber;
        private DevExpress.XtraEditors.TextEdit txtWarehouse;
        private DevExpress.XtraEditors.ComboBoxEdit cmbCosting;
        private DevExpress.XtraEditors.SearchLookUpEdit lookUpTarget;
        private DevExpress.XtraGrid.Views.Grid.GridView lookUpTargetView;
        private DevExpress.XtraEditors.MemoEdit memoDescription;
        private DevExpress.XtraGrid.GridControl gridLinesControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridLinesView;
        private DevExpress.XtraEditors.SimpleButton btnAddLine;
        private DevExpress.XtraEditors.SimpleButton btnDeleteLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private DevExpress.XtraEditors.SimpleButton btnPrintSlip;

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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblDocumentNumber = new System.Windows.Forms.Label();
            this.lblCosting = new System.Windows.Forms.Label();
            this.lblWarehouse = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblLines = new System.Windows.Forms.Label();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.dtDate = new DevExpress.XtraEditors.DateEdit();
            this.txtDocumentNumber = new DevExpress.XtraEditors.TextEdit();
            this.txtWarehouse = new DevExpress.XtraEditors.TextEdit();
            this.cmbCosting = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lookUpTarget = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.lookUpTargetView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.memoDescription = new DevExpress.XtraEditors.MemoEdit();
            this.gridLinesControl = new DevExpress.XtraGrid.GridControl();
            this.gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnAddLine = new DevExpress.XtraEditors.SimpleButton();
            this.btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnPrintSlip = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)this.dtDate.Properties.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.dtDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.txtDocumentNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.txtWarehouse.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.cmbCosting.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.lookUpTarget.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.lookUpTargetView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.memoDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridLinesControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridLinesView).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(24, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Tüketim";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Location = new System.Drawing.Point(26, 50);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Text = "";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(24, 92);
            this.lblDate.Name = "lblDate";
            this.lblDate.Text = "Tarih:";
            // 
            // dtDate
            // 
            this.dtDate.EditValue = null;
            this.dtDate.Location = new System.Drawing.Point(96, 89);
            this.dtDate.Name = "dtDate";
            this.dtDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDate.Size = new System.Drawing.Size(140, 24);
            this.dtDate.TabIndex = 0;
            // 
            // lblDocumentNumber
            // 
            this.lblDocumentNumber.AutoSize = true;
            this.lblDocumentNumber.Location = new System.Drawing.Point(260, 92);
            this.lblDocumentNumber.Name = "lblDocumentNumber";
            this.lblDocumentNumber.Text = "Belge No:";
            // 
            // txtDocumentNumber
            // 
            this.txtDocumentNumber.Location = new System.Drawing.Point(336, 89);
            this.txtDocumentNumber.Name = "txtDocumentNumber";
            this.txtDocumentNumber.Size = new System.Drawing.Size(190, 30);
            this.txtDocumentNumber.TabIndex = 1;
            // 
            // lblCosting
            // 
            this.lblCosting.AutoSize = true;
            this.lblCosting.Location = new System.Drawing.Point(550, 92);
            this.lblCosting.Name = "lblCosting";
            this.lblCosting.Text = "Değerleme:";
            // 
            // cmbCosting
            // 
            this.cmbCosting.Location = new System.Drawing.Point(640, 89);
            this.cmbCosting.Name = "cmbCosting";
            this.cmbCosting.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbCosting.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbCosting.Size = new System.Drawing.Size(140, 30);
            this.cmbCosting.TabIndex = 2;
            // 
            // lblWarehouse
            // 
            this.lblWarehouse.AutoSize = true;
            this.lblWarehouse.Location = new System.Drawing.Point(24, 132);
            this.lblWarehouse.Name = "lblWarehouse";
            this.lblWarehouse.Text = "Kaynak Depo:";
            // 
            // txtWarehouse
            // 
            this.txtWarehouse.Location = new System.Drawing.Point(140, 129);
            this.txtWarehouse.Name = "txtWarehouse";
            this.txtWarehouse.Properties.ReadOnly = true;
            this.txtWarehouse.Size = new System.Drawing.Size(300, 30);
            this.txtWarehouse.TabIndex = 3;
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(470, 132);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Text = "Hedef Hesap:";
            // 
            // lookUpTarget
            // 
            this.lookUpTarget.EditValue = null;
            this.lookUpTarget.Location = new System.Drawing.Point(580, 129);
            this.lookUpTarget.Name = "lookUpTarget";
            this.lookUpTarget.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpTarget.Properties.PopupView = this.lookUpTargetView;
            this.lookUpTarget.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.lookUpTarget.Size = new System.Drawing.Size(356, 24);
            this.lookUpTarget.TabIndex = 4;
            // 
            // lookUpTargetView
            // 
            this.lookUpTargetView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.lookUpTargetView.Name = "lookUpTargetView";
            this.lookUpTargetView.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.lookUpTargetView.OptionsView.ShowGroupPanel = false;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(24, 172);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Text = "Açıklama:";
            // 
            // memoDescription
            // 
            this.memoDescription.Location = new System.Drawing.Point(140, 169);
            this.memoDescription.Name = "memoDescription";
            this.memoDescription.Size = new System.Drawing.Size(796, 46);
            this.memoDescription.TabIndex = 5;
            // 
            // lblLines
            // 
            this.lblLines.AutoSize = true;
            this.lblLines.Location = new System.Drawing.Point(24, 228);
            this.lblLines.Name = "lblLines";
            this.lblLines.Text = "Kalemler:";
            // 
            // gridLinesControl
            // 
            this.gridLinesControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridLinesControl.Location = new System.Drawing.Point(24, 252);
            this.gridLinesControl.MainView = this.gridLinesView;
            this.gridLinesControl.Name = "gridLinesControl";
            this.gridLinesControl.Size = new System.Drawing.Size(912, 300);
            this.gridLinesControl.TabIndex = 6;
            this.gridLinesControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridLinesView });
            // 
            // gridLinesView
            // 
            this.gridLinesView.GridControl = this.gridLinesControl;
            this.gridLinesView.Name = "gridLinesView";
            // 
            // btnAddLine
            // 
            this.btnAddLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddLine.Location = new System.Drawing.Point(24, 562);
            this.btnAddLine.Name = "btnAddLine";
            this.btnAddLine.Size = new System.Drawing.Size(110, 30);
            this.btnAddLine.TabIndex = 7;
            this.btnAddLine.Text = "Satır Ekle";
            // 
            // btnDeleteLine
            // 
            this.btnDeleteLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteLine.Location = new System.Drawing.Point(142, 562);
            this.btnDeleteLine.Name = "btnDeleteLine";
            this.btnDeleteLine.Size = new System.Drawing.Size(110, 30);
            this.btnDeleteLine.TabIndex = 8;
            this.btnDeleteLine.Text = "Satır Sil";
            // 
            // lblTotalCaption
            // 
            this.lblTotalCaption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalCaption.AutoSize = true;
            this.lblTotalCaption.Location = new System.Drawing.Point(640, 568);
            this.lblTotalCaption.Name = "lblTotalCaption";
            this.lblTotalCaption.Text = "Toplam Tutar:";
            // 
            // lblTotalValue
            // 
            this.lblTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalValue.Location = new System.Drawing.Point(740, 568);
            this.lblTotalValue.Name = "lblTotalValue";
            this.lblTotalValue.Size = new System.Drawing.Size(196, 20);
            this.lblTotalValue.TabIndex = 9;
            this.lblTotalValue.Text = "0,00";
            this.lblTotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(744, 612);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 32);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(844, 612);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 32);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Kapat";
            // 
            // btnPrintSlip
            // 
            this.btnPrintSlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPrintSlip.Enabled = false;
            this.btnPrintSlip.Location = new System.Drawing.Point(24, 612);
            this.btnPrintSlip.Name = "btnPrintSlip";
            this.btnPrintSlip.Size = new System.Drawing.Size(180, 32);
            this.btnPrintSlip.TabIndex = 12;
            this.btnPrintSlip.Text = "Taşınır İşlem Fişi Yazdır";
            // 
            // StockIssueEditFormBase
            // 
            this.ClientSize = new System.Drawing.Size(960, 660);
            this.Controls.Add(this.lblLines);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnPrintSlip);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblTotalValue);
            this.Controls.Add(this.lblTotalCaption);
            this.Controls.Add(this.btnDeleteLine);
            this.Controls.Add(this.btnAddLine);
            this.Controls.Add(this.gridLinesControl);
            this.Controls.Add(this.memoDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lookUpTarget);
            this.Controls.Add(this.lblTarget);
            this.Controls.Add(this.txtWarehouse);
            this.Controls.Add(this.lblWarehouse);
            this.Controls.Add(this.cmbCosting);
            this.Controls.Add(this.lblCosting);
            this.Controls.Add(this.txtDocumentNumber);
            this.Controls.Add(this.lblDocumentNumber);
            this.Controls.Add(this.dtDate);
            this.Controls.Add(this.lblDate);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StockIssueEditFormBase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tüketim";
            ((System.ComponentModel.ISupportInitialize)this.dtDate.Properties.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.dtDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.txtDocumentNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.txtWarehouse.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.cmbCosting.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.lookUpTarget.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.lookUpTargetView).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.memoDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridLinesControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridLinesView).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}