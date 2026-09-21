using System.Drawing;
using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class CurrentAccountBalanceForm : XtraFormMdiBase
    {
        private CurrentAccountBalanceScope _scope = CurrentAccountBalanceScope.All;

        public CurrentAccountBalanceForm()
            : base("Cari Borç / Alacak Özeti")
        {
            InitializeComponent();

            IconOptions.SvgImage = DxIcon.Balance;
            picHeader.SvgImage = DxIcon.Balance;
            lblSub.Appearance.ForeColor = SkinTheme.SecondaryText;

            GridColumnFactory.ConfigureFromAttributes(gridView, typeof(CurrentAccountBalanceDto));

            btnClose.ImageOptions.SvgImage = DxIcon.Close;
            btnClose.ImageOptions.SvgImageSize = new Size(16, 16);
            btnClose.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;
            btnClose.Click += (_, _) => Close();

            btnRefresh.Click += async (_, _) => await LoadDataAsync();
            btnDebtors.Click += async (_, _) => { _scope = CurrentAccountBalanceScope.Debtors; await LoadDataAsync(); };
            btnCreditors.Click += async (_, _) => { _scope = CurrentAccountBalanceScope.Creditors; await LoadDataAsync(); };
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
                lblSub.Text = "Yükleniyor...";
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                List<CurrentAccountBalanceDto> items = await mediator.Send(new CurrentAccountBalanceQuery(_scope), CancellationToken.None);

                gridControl.DataSource = items;

                gridView.BestFitColumns();
                lblSub.Text = $"{items.Count} cari hesap listeleniyor";
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CurrentAccountBalance.Load", ex);
                ToastHelper.Show("Bakiye listesi yüklenemedi: " + ex.Message, ToastType.Error);
                lblSub.Text = "Yükleme hatası";
            }
        }
    }
}