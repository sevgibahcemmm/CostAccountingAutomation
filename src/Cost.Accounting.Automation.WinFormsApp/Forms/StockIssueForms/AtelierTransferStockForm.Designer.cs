using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
sealed partial class AtelierTransferStockForm
{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblMasterCaption;
        private DevExpress.XtraGrid.GridControl gridMasters;
        private DevExpress.XtraGrid.Views.Grid.GridView gridMastersView;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;

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
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblMasterCaption = new System.Windows.Forms.Label();
            this.gridMasters = new DevExpress.XtraGrid.GridControl();
            this.gridMastersView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)this.gridMasters).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.gridMastersView).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(24, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "Atölye Transfer Raporu";
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.ForeColor = System.Drawing.Color.Gray;
            this.lblSummary.Location = new System.Drawing.Point(26, 46);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Text = "";
            // 
            // lblMasterCaption
            // 
            this.lblMasterCaption.AutoSize = true;
            this.lblMasterCaption.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblMasterCaption.Location = new System.Drawing.Point(24, 76);
            this.lblMasterCaption.Name = "lblMasterCaption";
            this.lblMasterCaption.Text = "Atölyeye Transfer Edilen Ürünler (atölyeye göre gruplanabilir)";
            // 
            // gridMasters
            // 
            this.gridMasters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridMasters.Location = new System.Drawing.Point(24, 100);
            this.gridMasters.MainView = this.gridMastersView;
            this.gridMasters.Name = "gridMasters";
            this.gridMasters.Size = new System.Drawing.Size(1032, 520);
            this.gridMasters.TabIndex = 0;
            this.gridMasters.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gridMastersView });
            // 
            // gridMastersView
            // 
            this.gridMastersView.GridControl = this.gridMasters;
            this.gridMastersView.Name = "gridMastersView";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(24, 640);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 30);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Yenile";
            this.btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;
            this.btnRefresh.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.btnRefresh.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            // 
            // AtelierTransferStockForm
            // 
            this.ClientSize = new System.Drawing.Size(1080, 700);
            this.Controls.Add(this.lblMasterCaption);
            this.Controls.Add(this.gridMasters);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnRefresh);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.IconOptions.SvgImage = DxIcon.AtelierTransfer;
            this.Name = "AtelierTransferStockForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Atölye Transfer Raporu";
            ((System.ComponentModel.ISupportInitialize)this.gridMasters).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.gridMastersView).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
