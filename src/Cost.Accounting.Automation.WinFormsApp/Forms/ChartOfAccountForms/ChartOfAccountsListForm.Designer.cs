using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms;

public sealed partial class ChartOfAccountsListForm
{
    private PanelControl _pnlHeader = null!;
    private LabelControl _lblTitle = null!;
    private LabelControl _lblSub = null!;
    private PictureEdit _picHeaderIcon = null!;
    private SimpleButton _btnClosePage = null!;

    private PanelControl _pnlToolbar = null!;
    private SimpleButton _btnImport = null!;
    private SimpleButton _btnRefresh = null!;
    private SimpleButton _btnExpandAll = null!;
    private SimpleButton _btnCollapseAll = null!;
    private SimpleButton _btnDelete = null!;
    private SimpleButton _btnSelectAll = null!;
    private SimpleButton _btnClearSelection = null!;

    private TreeList _tree = null!;

    private void InitializeComponent()
    {
        _pnlHeader = new PanelControl();
        _lblTitle = new LabelControl();
        _lblSub = new LabelControl();
        _picHeaderIcon = new PictureEdit();
        _btnClosePage = new SimpleButton();

        _pnlToolbar = new PanelControl();
        _btnImport = new SimpleButton();
        _btnRefresh = new SimpleButton();
        _btnExpandAll = new SimpleButton();
        _btnCollapseAll = new SimpleButton();
        _btnDelete = new SimpleButton();
        _btnSelectAll = new SimpleButton();
        _btnClearSelection = new SimpleButton();

        _tree = new TreeList();

        ((System.ComponentModel.ISupportInitialize)_picHeaderIcon.Properties).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_tree).BeginInit();
        SuspendLayout();

        //
        // _pnlHeader
        //
        _pnlHeader.Dock = DockStyle.Top;
        _pnlHeader.Width = 1280;
        _pnlHeader.Height = 110;
        _pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

        //
        // _lblTitle
        //
        _lblTitle.AutoSize = false;
        _lblTitle.Location = new Point(82, 16);
        _lblTitle.Size = new Size(900, 26);
        _lblTitle.Text = "Hesap Planı";
        _lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        _lblTitle.Appearance.Options.UseFont = true;

        //
        // _lblSub
        //
        _lblSub.AutoSize = false;
        _lblSub.Location = new Point(84, 66);
        _lblSub.Size = new Size(900, 18);
        _lblSub.Text = "Yükleniyor...";
        _lblSub.Appearance.Font = new Font("Segoe UI", 10F);
        _lblSub.Appearance.ForeColor = SkinTheme.SecondaryText;
        _lblSub.Appearance.Options.UseFont = true;
        _lblSub.Appearance.Options.UseForeColor = true;

        //
        // _picHeaderIcon
        //
        _picHeaderIcon.Location = new Point(28, 31);
        _picHeaderIcon.Size = new Size(42, 42);
        _picHeaderIcon.BackColor = Color.Transparent;
        _picHeaderIcon.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
        _picHeaderIcon.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
        _picHeaderIcon.Properties.ShowMenu = false;
        _picHeaderIcon.SvgImage = SvgIcons.Modules[6];

        //
        // _btnClosePage
        //
        _btnClosePage.Text = "Kapat";
        _btnClosePage.Size = new Size(94, 36);
        _btnClosePage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _btnClosePage.Location = new Point(1170, 37);
        _btnClosePage.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnClosePage.Appearance.Options.UseFont = true;
        _btnClosePage.ImageOptions.SvgImage = SvgIcons.CloseIcon;
        _btnClosePage.ImageOptions.SvgImageSize = new Size(16, 16);
        _btnClosePage.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
        _btnClosePage.Click += BtnClosePage_Click;

        _pnlHeader.Controls.Add(_picHeaderIcon);
        _pnlHeader.Controls.Add(_lblTitle);
        _pnlHeader.Controls.Add(_lblSub);
        _pnlHeader.Controls.Add(_btnClosePage);

        //
        // _pnlToolbar
        //
        _pnlToolbar.Dock = DockStyle.Top;
        _pnlToolbar.Height = 56;
        _pnlToolbar.Padding = new Padding(16, 10, 16, 10);

        //
        // _btnImport
        //
        _btnImport.Text = "Hesap Planı İçe Aktar";
        _btnImport.Size = new Size(190, 36);
        _btnImport.Location = new Point(16, 10);
        _btnImport.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnImport.Appearance.Options.UseFont = true;
        _btnImport.Click += BtnImport_Click;

        //
        // _btnRefresh
        //
        _btnRefresh.Text = "Yenile";
        _btnRefresh.Size = new Size(92, 36);
        _btnRefresh.Location = new Point(216, 10);
        _btnRefresh.Click += BtnRefresh_Click;

        //
        // _btnExpandAll
        //
        _btnExpandAll.Text = "Tümünü Genişlet";
        _btnExpandAll.Size = new Size(118, 36);
        _btnExpandAll.Location = new Point(318, 10);
        _btnExpandAll.Click += BtnExpandAll_Click;

        //
        // _btnCollapseAll
        //
        _btnCollapseAll.Text = "Tümünü Daralt";
        _btnCollapseAll.Size = new Size(118, 36);
        _btnCollapseAll.Location = new Point(446, 10);
        _btnCollapseAll.Click += BtnCollapseAll_Click;

        //
        // _btnDelete
        //
        _btnDelete.Text = "Seçileni Sil";
        _btnDelete.Size = new Size(96, 36);
        _btnDelete.Location = new Point(574, 10);
        _btnDelete.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _btnDelete.Appearance.Options.UseFont = true;
        _btnDelete.Click += BtnDelete_Click;

        //
        // _btnSelectAll
        //
        _btnSelectAll.Text = "Hepsini Seç";
        _btnSelectAll.Size = new Size(100, 36);
        _btnSelectAll.Location = new Point(682, 10);
        _btnSelectAll.Click += BtnSelectAll_Click;

        //
        // _btnClearSelection
        //
        _btnClearSelection.Text = "Seçili Olanları Kaldır";
        _btnClearSelection.Size = new Size(150, 36);
        _btnClearSelection.Location = new Point(794, 10);
        _btnClearSelection.Click += BtnClearSelection_Click;

        SetIcon(_btnImport, SvgIcons.ImportIcon, 18);
        SetIcon(_btnRefresh, SvgIcons.RefreshIcon, 18);
        SetIcon(_btnExpandAll, SvgIcons.ExpandAllIcon, 18);
        SetIcon(_btnCollapseAll, SvgIcons.CollapseAllIcon, 18);
        SetIcon(_btnDelete, SvgIcons.TrashIcon, 18);
        SetIcon(_btnSelectAll, SvgIcons.CheckAllIcon, 18);
        SetIcon(_btnClearSelection, SvgIcons.UncheckIcon, 18);

        _pnlToolbar.Controls.Add(_btnImport);
        _pnlToolbar.Controls.Add(_btnRefresh);
        _pnlToolbar.Controls.Add(_btnExpandAll);
        _pnlToolbar.Controls.Add(_btnCollapseAll);
        _pnlToolbar.Controls.Add(_btnDelete);
        _pnlToolbar.Controls.Add(_btnSelectAll);
        _pnlToolbar.Controls.Add(_btnClearSelection);

        //
        // _tree
        //
        ConfigureTree();
        _tree.UseDirectXPaint = DevExpress.Utils.DefaultBoolean.False;
        _tree.CustomDrawNodeCell += Tree_CustomDrawNodeCell;
        _tree.AfterCheckNode += Tree_AfterCheckNode;

        Controls.Add(_tree);
        Controls.Add(_pnlToolbar);
        Controls.Add(_pnlHeader);

        IconOptions.SvgImage = SvgIcons.Modules[6];
        AutoScaleDimensions = new SizeF(7F, 16F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 680);

        ((System.ComponentModel.ISupportInitialize)_picHeaderIcon.Properties).EndInit();
        ((System.ComponentModel.ISupportInitialize)_tree).EndInit();
        ResumeLayout(false);
    }

    private void ConfigureTree()
    {
        _tree.Dock = DockStyle.Fill;
        _tree.Margin = new Padding(0);
        _tree.KeyFieldName = nameof(ChartOfAccountDto.Id);
        _tree.ParentFieldName = nameof(ChartOfAccountDto.ParentId);

        _tree.OptionsSelection.MultiSelect = true;
        _tree.OptionsBehavior.Editable = false;
        _tree.OptionsView.AutoWidth = true;
        _tree.OptionsView.ShowCheckBoxes = true;
        _tree.OptionsView.ShowHorzLines = true;

        AddColumn(_tree, "Kod", nameof(ChartOfAccountDto.Code), 130);
        AddColumn(_tree, "Hesap Adı", nameof(ChartOfAccountDto.Name), 360);
        AddColumn(_tree, "Tür", nameof(ChartOfAccountDto.TypeText), 90);
        AddColumn(_tree, "Yarı Mamul Hesabı (151)", nameof(ChartOfAccountDto.SemiFinishedCode), 160);
        AddColumn(_tree, "Mamul Hesabı (152)", nameof(ChartOfAccountDto.FinishedCode), 160);

        _tree.Columns[nameof(ChartOfAccountDto.Code)].SortOrder = System.Windows.Forms.SortOrder.Ascending;
        _tree.OptionsCustomization.AllowSort = false;
    }

    private static void AddColumn(TreeList tree, string caption, string fieldName, int width)
    {
        tree.Columns.Add(new TreeListColumn
        {
            Caption = caption,
            FieldName = fieldName,
            Width = width,
            Visible = true
        });
    }

    private static void SetIcon(SimpleButton button, SvgImage icon, int size)
    {
        button.ImageOptions.SvgImage = icon;
        button.ImageOptions.SvgImageSize = new Size(size, size);
        button.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
    }
}