using DevExpress.XtraEditors;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class UpdateHistoryForm
    {
        private System.ComponentModel.IContainer components = null;

        private PanelControl pnlHeader;
        private DevExpress.XtraEditors.PictureEdit picIcon;
        private LabelControl lblTitle;
        private LabelControl lblSub;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
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
            pnlHeader = new PanelControl();
            picIcon = new DevExpress.XtraEditors.PictureEdit();
            lblSub = new LabelControl();
            lblTitle = new LabelControl();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            gridControl = new DevExpress.XtraGrid.GridControl();
            gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(picIcon);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnRefresh);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1280, 96);
            pnlHeader.TabIndex = 0;
            // 
            // picIcon
            // 
            picIcon.Location = new Point(48, 14);
            picIcon.Name = "picIcon";
            picIcon.Properties.NullText = " ";
            picIcon.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            picIcon.Size = new Size(64, 64);
            picIcon.TabIndex = 0;
            // 
            // lblSub
            // 
            lblSub.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            lblSub.Appearance.ForeColor = SkinTheme.SecondaryText;
            lblSub.Appearance.Options.UseFont = true;
            lblSub.Appearance.Options.UseForeColor = true;
            lblSub.Location = new Point(128, 56);
            lblSub.Name = "lblSub";
            lblSub.Size = new Size(300, 25);
            lblSub.TabIndex = 2;
            lblSub.Text = "Yayınlanmış sürümlerin listesi";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(126, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(280, 46);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Sürüm Geçmişi";
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.ImageOptions.SvgImage = DxIcon.Refresh;
            btnRefresh.Location = new Point(1160, 30);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(96, 36);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "Yenile";
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // gridControl
            // 
            gridControl.Dock = DockStyle.Fill;
            gridControl.Location = new Point(0, 96);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.Size = new Size(1280, 584);
            gridControl.TabIndex = 1;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            // 
            // gridView
            // 
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.ShowAutoFilterRow = false;
            // 
            // UpdateHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 680);
            Controls.Add(gridControl);
            Controls.Add(pnlHeader);
            Name = "UpdateHistoryForm";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ResumeLayout(false);
        }
    }
}