using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class PaymentCollectionEditForm : XtraForm
    {
        private CurrentAccountBalanceDto? _selected;

        public PaymentCollectionEditForm(CurrentAccountMovementType? targetType = null)
        {
            InitializeComponent();

            Text = "Ödeme / Tahsilat";
            lblTitle.Text = "Ödeme / Tahsilat İşlemi";
            lblSubtitle.Text = "Cari borç/alacak listesinden kayıt seçin, tutarı teyit edip işlemi tamamlayın";

            dtDate.DateTime = DateTime.Today;

            spinAmount.Properties.MinValue = 0;
            spinAmount.Properties.MaxValue = decimal.MaxValue;
            spinAmount.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            spinAmount.Properties.DisplayFormat.FormatString = "n2";
            spinAmount.EditValue = 0m;

            SetButtonsEnabled(false, false);

            Load += async (_, _) => await LoadBalancesAsync();
            gridView.FocusedRowChanged += (_, _) => ApplySelectedRow();
            btnCollect.Click += async (_, _) => await ExecuteAsync(CurrentAccountMovementType.Collection);
            btnPay.Click += async (_, _) => await ExecuteAsync(CurrentAccountMovementType.Payment);
            btnCancel.Click += (_, _) => Close();
        }

        private async Task LoadBalancesAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                List<CurrentAccountBalanceDto> items = (await mediator.Send(new CurrentAccountBalanceQuery(), CancellationToken.None))
                    .Where(x => Math.Abs(x.Balance) >= 0.01m)
                    .OrderByDescending(x => Math.Abs(x.Balance))
                    .ToList();

                GridColumnFactory.ConfigureFromAttributes(gridView, typeof(CurrentAccountBalanceDto));

                gridControl.DataSource = null;
                gridControl.DataSource = items;
                gridView.BestFitColumns();

                if (items.Count == 0)
                {
                    lblHint.Text = "Borcu veya alacağı bulunan cari hesap bulunamadı.";
                    SetButtonsEnabled(false, false);
                    return;
                }

                ApplySelectedRow();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("PaymentCollection.Load", ex);
                ToastHelper.Show("Cari borç/alacak listesi yüklenemedi: " + ex.Message, ToastType.Error, 6000);
            }
        }

        private void ApplySelectedRow()
        {
            if (gridView.GetFocusedRow() is not CurrentAccountBalanceDto dto)
            {
                return;
            }

            _selected = dto;
            lblCariTuruValue.Text = dto.AccountTypeName;
            lblCariValue.Text = dto.AccountName;

            bool isReceivable = dto.Balance > 0;

            lblMovementTypeValue.Text = isReceivable ? "Tahsilat" : "Ödeme";
            lblMovementTypeValue.Appearance.ForeColor = isReceivable
                ? Color.FromArgb(16, 185, 129)
                : Color.FromArgb(245, 158, 11);

            lblAmountCaption.Text = isReceivable ? "Alacak Tutarı:" : "Borç Tutarı:";
            spinAmount.EditValue = Math.Abs(dto.Balance);

            SetButtonsEnabled(isReceivable, !isReceivable);

            lblHint.Text = isReceivable
                ? $"{dto.AccountName} ({dto.AccountTypeName}) size {dto.Balance:n2} ₺ borçludur. 'Tahsil Et' ile tahsilatı (alacak kaydı) oluşturun."
                : $"{dto.AccountName} ({dto.AccountTypeName}) {(-dto.Balance):n2} ₺ alacaklıdır. 'Borç Öde' ile ödemeyi (borç kaydı) oluşturun.";
        }

        private void SetButtonsEnabled(bool collect, bool pay)
        {
            btnCollect.Enabled = collect;
            btnPay.Enabled = pay;
        }

        private async Task ExecuteAsync(CurrentAccountMovementType movementType)
        {
            if (_selected is null)
            {
                ToastHelper.Show("Lütfen listeden bir cari seçin.", ToastType.Warning);
                return;
            }

            bool isCollect = movementType == CurrentAccountMovementType.Collection;
            decimal amount = spinAmount.Value;

            if (amount <= 0)
            {
                ToastHelper.Show(isCollect ? "Tahsil edilecek tutar girilmelidir." : "Ödenecek tutar girilmelidir.", ToastType.Warning);
                return;
            }

            string description = isCollect
                ? $"{_selected.AccountName} tahsilatı"
                : $"{_selected.AccountName} ödemesi";

            string documentNo = $"{(isCollect ? "TAH" : "ODM")}-{DateTime.Now:yyMMddHHmmss}";

            CurrentAccountMovementCreateCommand command = new(
                CurrentAccountType: _selected.AccountType,
                CustomerId: _selected.AccountType == CurrentAccountType.Customer ? _selected.Id : null,
                SupplierId: _selected.AccountType == CurrentAccountType.Supplier ? _selected.Id : null,
                Date: DateOnly.FromDateTime(dtDate.DateTime),
                MovementType: movementType,
                DocumentNo: documentNo,
                Debit: isCollect ? 0m : amount,
                Credit: isCollect ? amount : 0m,
                Description: description);

            bool ok = await CrudExecutor.ExecuteAsync(command);
            if (ok)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}