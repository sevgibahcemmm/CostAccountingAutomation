using System.Drawing;
using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class CurrentAccountBalanceForm : XtraFormMdiBase
    {
        private GridControl _grid = null!;
        private GridView _view = null!;
        private SimpleButton _btnRefresh = null!;
        private SimpleButton _btnDebtors = null!;
        private SimpleButton _btnCreditors = null!;
        private LabelControl _lblTitle = null!;
        private LabelControl _lblSub = null!;
        private CurrentAccountBalanceScope _scope = CurrentAccountBalanceScope.All;

        public CurrentAccountBalanceForm()
            : base("Cari Borç / Alacak Özeti")
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            IconOptions.SvgImage = DxIcon.Balance;

            _lblTitle = new LabelControl
            {
                Text = "Cari Borç / Alacak Özeti",
                Location = new Point(82, 16)
            };
            _lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            _lblTitle.Size = new Size(900, 26);
            _lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            _lblTitle.Appearance.Options.UseFont = true;

            _lblSub = new LabelControl
            {
                Text = "Yükleniyor...",
                Location = new Point(84, 66)
            };
            _lblSub.AutoSizeMode = LabelAutoSizeMode.None;
            _lblSub.Size = new Size(900, 18);
            _lblSub.Appearance.Font = new Font("Segoe UI", 10F);
            _lblSub.Appearance.ForeColor = SkinTheme.SecondaryText;
            _lblSub.Appearance.Options.UseFont = true;
            _lblSub.Appearance.Options.UseForeColor = true;

            var pnlHeader = new PanelControl
            {
                Dock = DockStyle.Top,
                Width = 1280,
                Height = 110
            };
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(_lblTitle);
            pnlHeader.Controls.Add(_lblSub);

            var pic = new DevExpress.XtraEditors.PictureEdit
            {
                Location = new Point(28, 34),
                Size = new Size(42, 42),
                BackColor = Color.Transparent
            };
            pic.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pic.Properties.SvgImageColorizationMode = DevExpress.Utils.SvgImageColorizationMode.Default;
            pic.SvgImage = DxIcon.Balance;
            pnlHeader.Controls.Add(pic);

            var btnClose = new SimpleButton
            {
                Text = "Kapat",
                Size = new Size(94, 36),
                Location = new Point(1170, 37),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.ImageOptions.SvgImage = DxIcon.Close;
            btnClose.ImageOptions.SvgImageSize = new Size(16, 16);
            btnClose.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnClose.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClose.Appearance.Options.UseFont = true;
            btnClose.Click += (_, _) => Close();
            pnlHeader.Controls.Add(btnClose);

            _btnRefresh = new SimpleButton { Text = "Yenile", Dock = DockStyle.Left, Width = 100, Height = 36 };
            _btnRefresh.Click += async (_, _) => await LoadDataAsync();

            _btnDebtors = new SimpleButton { Text = "Borçlular", Dock = DockStyle.Left, Width = 100, Height = 36 };
            _btnDebtors.Click += async (_, _) => { _scope = CurrentAccountBalanceScope.Debtors; await LoadDataAsync(); };

            _btnCreditors = new SimpleButton { Text = "Alacaklılar", Dock = DockStyle.Left, Width = 110, Height = 36 };
            _btnCreditors.Click += async (_, _) => { _scope = CurrentAccountBalanceScope.Creditors; await LoadDataAsync(); };

            var pnlToolbar = new DevExpress.XtraEditors.PanelControl
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(16, 10, 16, 10)
            };
            pnlToolbar.Controls.Add(_btnRefresh);
            pnlToolbar.Controls.Add(_btnDebtors);
            pnlToolbar.Controls.Add(_btnCreditors);

            _view = new GridView();
            _grid = new GridControl
            {
                Dock = DockStyle.Fill,
                MainView = _view
            };
            _view.GridControl = _grid;
            _view.OptionsView.ShowGroupPanel = false;
            _view.OptionsView.EnableAppearanceEvenRow = true;
            _view.OptionsView.EnableAppearanceOddRow = true;
            _view.OptionsBehavior.Editable = false;

            GridColumnFactory.ConfigureFromAttributes(_view, typeof(CurrentAccountBalanceDto));

            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 680);
            Controls.Add(_grid);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);

            ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _lblSub.Text = "Yükleniyor...";
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                List<CurrentAccountBalanceDto> items = await mediator.Send(new CurrentAccountBalanceQuery(_scope), CancellationToken.None);

                _grid.DataSource = items;

                _view.BestFitColumns();
                _lblSub.Text = $"{items.Count} cari hesap listeleniyor";
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CurrentAccountBalance.Load", ex);
                ToastHelper.Show("Bakiye listesi yüklenemedi: " + ex.Message, ToastType.Error);
                _lblSub.Text = "Yükleme hatası";
            }
        }
    }
}