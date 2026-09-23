using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;

public sealed partial class RecipeEditForm
{
    private System.ComponentModel.IContainer components = null;

    private DevExpress.XtraEditors.PanelControl pnlHeader;
    private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
    private DevExpress.XtraEditors.LabelControl lblTitle;
    private DevExpress.XtraEditors.LabelControl lblSubtitle;
    private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
    private System.Windows.Forms.Panel pnlBody;
    private DevExpress.XtraEditors.SearchLookUpEdit lookUpProduct;
    private DevExpress.XtraEditors.LabelControl lblWorkshopLabel;
    private DevExpress.XtraEditors.SearchLookUpEdit lookUpWorkshop;
    private DevExpress.XtraGrid.Views.Grid.GridView lookUpProductView;
    private DevExpress.XtraGrid.Views.Grid.GridView lookUpWorkshopView;
    private DevExpress.XtraGrid.GridControl gridLinesControl;
    private DevExpress.XtraGrid.Views.Grid.GridView gridLinesView;
    private System.Windows.Forms.Panel pnlLinesActions;
    private DevExpress.XtraEditors.SimpleButton btnAddLine;
    private DevExpress.XtraEditors.SimpleButton btnDeleteLine;
    private DevExpress.XtraEditors.PanelControl pnlFooter;
    private DevExpress.XtraEditors.PanelControl pnlFooterLine;
    private DevExpress.XtraEditors.SimpleButton btnSave;
    private DevExpress.XtraEditors.SimpleButton btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecipeEditForm));
        pnlHeader = new DevExpress.XtraEditors.PanelControl();
        lblSubtitle = new DevExpress.XtraEditors.LabelControl();
        lblTitle = new DevExpress.XtraEditors.LabelControl();
        lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
        pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
        pnlBody = new Panel();
        gridLinesControl = new DevExpress.XtraGrid.GridControl();
        gridLinesView = new DevExpress.XtraGrid.Views.Grid.GridView();
        lookUpProduct = new DevExpress.XtraEditors.SearchLookUpEdit();
        lookUpProductView = new DevExpress.XtraGrid.Views.Grid.GridView();
        lblWorkshopLabel = new DevExpress.XtraEditors.LabelControl();
        lookUpWorkshop = new DevExpress.XtraEditors.SearchLookUpEdit();
        lookUpWorkshopView = new DevExpress.XtraGrid.Views.Grid.GridView();
        pnlLinesActions = new Panel();
        btnAddLine = new DevExpress.XtraEditors.SimpleButton();
        btnDeleteLine = new DevExpress.XtraEditors.SimpleButton();
        pnlFooter = new DevExpress.XtraEditors.PanelControl();
        pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
        btnSave = new DevExpress.XtraEditors.SimpleButton();
        btnCancel = new DevExpress.XtraEditors.SimpleButton();
        labelControl1 = new DevExpress.XtraEditors.LabelControl();
        ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
        pnlHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
        pnlBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridLinesControl).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridLinesView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lookUpProduct.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lookUpProductView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
        pnlFooter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.Appearance.BackColor = Color.FromArgb(248, 249, 250);
        pnlHeader.Appearance.Options.UseBackColor = true;
        pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblHeaderIcon);
        pnlHeader.Controls.Add(pnlHeaderLine);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Margin = new Padding(3, 2, 3, 2);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(549, 55);
        pnlHeader.TabIndex = 0;
        // 
        // lblSubtitle
        // 
        lblSubtitle.Appearance.Font = new Font("Segoe UI", 9F);
        lblSubtitle.Appearance.ForeColor = Color.FromArgb(107, 114, 128);
        lblSubtitle.Appearance.Options.UseFont = true;
        lblSubtitle.Appearance.Options.UseForeColor = true;
        lblSubtitle.Location = new Point(50, 31);
        lblSubtitle.Margin = new Padding(3, 2, 3, 2);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(99, 15);
        lblSubtitle.TabIndex = 3;
        lblSubtitle.Text = "Alt açıklama metni";
        // 
        // lblTitle
        // 
        lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 11.5F);
        lblTitle.Appearance.ForeColor = Color.FromArgb(17, 24, 39);
        lblTitle.Appearance.Options.UseFont = true;
        lblTitle.Appearance.Options.UseForeColor = true;
        lblTitle.Location = new Point(50, 11);
        lblTitle.Margin = new Padding(3, 2, 3, 2);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(45, 20);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Reçete";
        // 
        // lblHeaderIcon
        // 
        lblHeaderIcon.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("lblHeaderIcon.ImageOptions.SvgImage");
        lblHeaderIcon.ImageOptions.SvgImageSize = new Size(24, 24);
        lblHeaderIcon.Location = new Point(17, 16);
        lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
        lblHeaderIcon.Name = "lblHeaderIcon";
        lblHeaderIcon.Size = new Size(24, 24);
        lblHeaderIcon.TabIndex = 2;
        // 
        // pnlHeaderLine
        // 
        pnlHeaderLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
        pnlHeaderLine.Appearance.Options.UseBackColor = true;
        pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        pnlHeaderLine.Dock = DockStyle.Bottom;
        pnlHeaderLine.Location = new Point(0, 54);
        pnlHeaderLine.Margin = new Padding(3, 2, 3, 2);
        pnlHeaderLine.Name = "pnlHeaderLine";
        pnlHeaderLine.Size = new Size(549, 1);
        pnlHeaderLine.TabIndex = 2;
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(gridLinesControl);
        pnlBody.Controls.Add(lblWorkshopLabel);
        pnlBody.Controls.Add(lookUpWorkshop);
        pnlBody.Controls.Add(labelControl1);
        pnlBody.Controls.Add(lookUpProduct);
        pnlBody.Controls.Add(pnlLinesActions);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 55);
        pnlBody.Margin = new Padding(3, 2, 3, 2);
        pnlBody.Name = "pnlBody";
        pnlBody.Size = new Size(549, 333);
        pnlBody.TabIndex = 1;
        // 
        // gridLinesControl
        // 
        gridLinesControl.EmbeddedNavigator.Margin = new Padding(3, 2, 3, 2);
        gridLinesControl.Location = new Point(21, 62);
        gridLinesControl.MainView = gridLinesView;
        gridLinesControl.Margin = new Padding(3, 2, 3, 2);
        gridLinesControl.Name = "gridLinesControl";
        gridLinesControl.Size = new Size(507, 206);
        gridLinesControl.TabIndex = 3;
        gridLinesControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridLinesView });
        // 
        // gridLinesView
        // 
        gridLinesView.DetailHeight = 284;
        gridLinesView.GridControl = gridLinesControl;
        gridLinesView.Name = "gridLinesView";
        gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
        gridLinesView.OptionsEditForm.PopupEditFormWidth = 686;
        gridLinesView.OptionsView.ColumnAutoWidth = false;
        gridLinesView.OptionsView.ShowGroupPanel = false;
        gridLinesView.RowHeight = 21;
        // 
        // lblWorkshopLabel
        // 
        lblWorkshopLabel.Appearance.Font = new Font("Segoe UI", 9.5F);
        lblWorkshopLabel.Appearance.ForeColor = SystemColors.ActiveCaptionText;
        lblWorkshopLabel.Appearance.Options.UseFont = true;
        lblWorkshopLabel.Appearance.Options.UseForeColor = true;
        lblWorkshopLabel.Location = new Point(50, 10);
        lblWorkshopLabel.Margin = new Padding(3, 2, 3, 2);
        lblWorkshopLabel.Name = "lblWorkshopLabel";
        lblWorkshopLabel.Size = new Size(39, 17);
        lblWorkshopLabel.TabIndex = 15;
        lblWorkshopLabel.Text = "Atölye:";
        // 
        // lookUpWorkshop
        // 
        lookUpWorkshop.Location = new Point(103, 10);
        lookUpWorkshop.Margin = new Padding(3, 2, 3, 2);
        lookUpWorkshop.Name = "lookUpWorkshop";
        lookUpWorkshop.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
        lookUpWorkshop.Properties.NullText = "Atölye seçiniz...";
        lookUpWorkshop.Properties.PopupView = lookUpWorkshopView;
        lookUpWorkshop.Size = new Size(419, 20);
        lookUpWorkshop.TabIndex = 1;
        // 
        // lookUpWorkshopView
        // 
        lookUpWorkshopView.DetailHeight = 284;
        lookUpWorkshopView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
        lookUpWorkshopView.Name = "lookUpWorkshopView";
        lookUpWorkshopView.OptionsBehavior.AutoPopulateColumns = false;
        lookUpWorkshopView.OptionsEditForm.PopupEditFormWidth = 588;
        lookUpWorkshopView.OptionsSelection.EnableAppearanceFocusedCell = false;
        lookUpWorkshopView.OptionsView.ShowGroupPanel = false;
        // 
        // labelControl1
        // 
        labelControl1.Appearance.Font = new Font("Segoe UI", 9.5F);
        labelControl1.Appearance.ForeColor = SystemColors.ActiveCaptionText;
        labelControl1.Appearance.Options.UseFont = true;
        labelControl1.Appearance.Options.UseForeColor = true;
        labelControl1.Location = new Point(16, 32);
        labelControl1.Margin = new Padding(3, 2, 3, 2);
        labelControl1.Name = "labelControl1";
        labelControl1.Size = new Size(79, 17);
        labelControl1.TabIndex = 16;
        labelControl1.Text = "Mamül Ürün :";
        // 
        // lookUpProduct
        // 
        lookUpProduct.Location = new Point(103, 32);
        lookUpProduct.Margin = new Padding(3, 2, 3, 2);
        lookUpProduct.Name = "lookUpProduct";
        lookUpProduct.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
        lookUpProduct.Properties.NullText = "Mamül ürün seçiniz...";
        lookUpProduct.Properties.PopupView = lookUpProductView;
        lookUpProduct.Size = new Size(419, 20);
        lookUpProduct.TabIndex = 2;
        // 
        // pnlLinesActions
        // 
        pnlLinesActions.Location = new Point(21, 275);
        pnlLinesActions.Margin = new Padding(3, 2, 3, 2);
        pnlLinesActions.Name = "pnlLinesActions";
        pnlLinesActions.Size = new Size(507, 24);
        pnlLinesActions.TabIndex = 4;
        // 
        // btnAddLine
        // 
        btnAddLine.Location = new Point(0, 0);
        btnAddLine.Name = "btnAddLine";
        btnAddLine.Size = new Size(130, 30);
        btnAddLine.TabIndex = 0;
        btnAddLine.Text = "Malzeme Ekle";
        // 
        // btnDeleteLine
        // 
        btnDeleteLine.Location = new Point(140, 0);
        btnDeleteLine.Name = "btnDeleteLine";
        btnDeleteLine.Size = new Size(130, 30);
        btnDeleteLine.TabIndex = 1;
        btnDeleteLine.Text = "Satır Sil";
        // 
        // pnlFooter
        // 
        pnlFooter.Appearance.BackColor = Color.FromArgb(248, 249, 250);
        pnlFooter.Appearance.Options.UseBackColor = true;
        pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        pnlFooter.Controls.Add(pnlFooterLine);
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Location = new Point(0, 388);
        pnlFooter.Margin = new Padding(3, 2, 3, 2);
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Size = new Size(549, 49);
        pnlFooter.TabIndex = 2;
        // 
        // pnlFooterLine
        // 
        pnlFooterLine.Appearance.BackColor = Color.FromArgb(229, 231, 235);
        pnlFooterLine.Appearance.Options.UseBackColor = true;
        pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        pnlFooterLine.Dock = DockStyle.Top;
        pnlFooterLine.Location = new Point(0, 0);
        pnlFooterLine.Margin = new Padding(3, 2, 3, 2);
        pnlFooterLine.Name = "pnlFooterLine";
        pnlFooterLine.Size = new Size(549, 1);
        pnlFooterLine.TabIndex = 2;
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSave.Appearance.Font = new Font("Segoe UI Semibold", 9.5F);
        btnSave.Appearance.Options.UseFont = true;
        btnSave.Cursor = Cursors.Hand;
        btnSave.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
        btnSave.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnSave.ImageOptions.SvgImage");
        btnSave.ImageOptions.SvgImageSize = new Size(18, 18);
        btnSave.Location = new Point(324, 11);
        btnSave.Margin = new Padding(3, 2, 3, 2);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(77, 28);
        btnSave.TabIndex = 0;
        btnSave.Text = "Kaydet";
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
        btnCancel.Appearance.ForeColor = Color.FromArgb(75, 85, 99);
        btnCancel.Appearance.Options.UseFont = true;
        btnCancel.Appearance.Options.UseForeColor = true;
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
        btnCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnCancel.ImageOptions.SvgImage");
        btnCancel.ImageOptions.SvgImageSize = new Size(16, 16);
        btnCancel.Location = new Point(405, 11);
        btnCancel.Margin = new Padding(3, 2, 3, 2);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(77, 28);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Vazgeç";
        // 
        // labelControl1
        // 
        labelControl1.Appearance.Font = new Font("Segoe UI", 9.5F);
        labelControl1.Appearance.ForeColor = SystemColors.ActiveCaptionText;
        labelControl1.Appearance.Options.UseFont = true;
        labelControl1.Appearance.Options.UseForeColor = true;
        labelControl1.Location = new Point(16, 10);
        labelControl1.Margin = new Padding(3, 2, 3, 2);
        labelControl1.Name = "labelControl1";
        labelControl1.Size = new Size(79, 17);
        labelControl1.TabIndex = 16;
        labelControl1.Text = "Mamül Ürün :";
        // 
        // RecipeEditForm
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(549, 437);
        Controls.Add(pnlBody);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        IconOptions.ShowIcon = false;
        Margin = new Padding(3, 2, 3, 2);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "RecipeEditForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Yeni Reçete";
        ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridLinesControl).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridLinesView).EndInit();
        ((System.ComponentModel.ISupportInitialize)lookUpProduct.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)lookUpProductView).EndInit();
        ((System.ComponentModel.ISupportInitialize)lookUpWorkshop.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)lookUpWorkshopView).EndInit();
        ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
        pnlFooter.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private DevExpress.XtraEditors.LabelControl labelControl1;
}