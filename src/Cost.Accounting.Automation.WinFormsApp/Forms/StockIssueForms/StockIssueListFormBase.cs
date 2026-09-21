using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public abstract class StockIssueListFormBase<TEditForm> : CrudListFormBase<StockIssueGetAllQuery, StockIssueListDto, TEditForm>
        where TEditForm : XtraForm
    {
        private readonly StockIssueType _issueType;

        protected StockIssueListFormBase(StockIssueType issueType, string formTitle) : base(formTitle)
        {
            _issueType = issueType;
        }

        protected override SvgImage ModuleIcon => DxIcon.StockIssue;

        protected override string[] SearchFieldNames =>
        [
            nameof(StockIssueListDto.DocumentNumber),
            nameof(StockIssueListDto.SourceWarehouseName),
            nameof(StockIssueListDto.TargetAccountCode),
            nameof(StockIssueListDto.TargetAccountName),
            nameof(StockIssueListDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.OptionsView.ColumnAutoWidth = false;
        }

        protected override StockIssueGetAllQuery BuildListQuery()
            => new(IssueType: _issueType, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(StockIssueListDto item)
            => new StockIssueDeleteCommand(item.Id);

        protected override string GetDeleteSummary(StockIssueListDto item)
            => $"{item.DocumentNumber} - {item.TargetAccountName}";

        protected override bool SupportsRestore => false;

        protected override bool SupportsSlipPrint => true;

        protected override async Task<MovableAssetTransactionSlipData?> BuildSlipDataAsync(StockIssueListDto item)
        {
            StockIssueDto? issue;
            using (var scope = Program.Services.CreateScope())
            {
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new StockIssueGetByIdQuery(item.Id), CancellationToken.None);
                if (!result.IsSuccessful || result.Data is null)
                {
                    ToastHelper.Show("Belge bulunamadı.", ToastType.Warning);
                    return null;
                }

                issue = result.Data;
            }

            List<StockIssueLineDto> lines = issue.Lines
                .Where(l => l.ProductId != Guid.Empty && l.Quantity > 0)
                .ToList();

            if (lines.Count == 0)
            {
                ToastHelper.Show("Belgede taşınır işlem fişine aktarılacak kalem yok.", ToastType.Warning);
                return null;
            }

            CompanyDto company = await MovableAssetTransactionSlipPresenter.LoadCompanyAsync();
            List<ChartOfAccountLookUpDto> accounts = await MovableAssetTransactionSlipPresenter.LoadAccountsAsync();
            Dictionary<Guid, ProductDto> productsById = await MovableAssetTransactionSlipPresenter.LoadProductsByIdAsync();

            ChartOfAccountLookUpDto? warehouse = accounts.FirstOrDefault(a => a.Id == issue.SourceWarehouseId);
            string warehouseName = warehouse?.Name ?? issue.SourceWarehouseName;
            string warehouseCode = warehouse?.Code ?? string.Empty;

            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            DateTime date = issue.Date.ToDateTime(TimeOnly.MinValue);

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = issue.DocumentNumber,
                Date = date,
                IslemCesidi = issue.IssueType == StockIssueType.Consumption ? "Tüketim" : "Atölye Transferi",
                NeredenGeldigi = warehouseName,
                KimeVerildigi = issue.TargetAccountName,
                NereyeVerildigi = string.IsNullOrWhiteSpace(issue.TargetAccountCode)
                    ? issue.TargetAccountName
                    : $"{issue.TargetAccountCode} - {issue.TargetAccountName}",
                IlIlceAdi = ilIlce,
                IlIlceKodu = string.Empty,
                HarcamaBirimiAdi = company.ExpenditureUnitName ?? string.Empty,
                HarcamaBirimiKodu = company.ExpenditureUnitCode ?? string.Empty,
                AmbarAdi = warehouseName,
                AmbarKodu = warehouseCode,
                MuhasebeBirimiAdi = company.AccountingUnitName ?? string.Empty,
                MuhasebeBirimiKodu = company.AccountingUnitCode ?? string.Empty,
                DayanakTarihi = date,
                DayanakKodu = issue.DocumentNumber,
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(accounts)
            };

            foreach (StockIssueLineDto line in lines)
            {
                ProductDto? product = productsById.GetValueOrDefault(line.ProductId);
                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    Kodu = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, line.ProductCode),
                    DepoKodu = product?.WarehouseCode ?? string.Empty,
                    DepoAdi = product?.WarehouseName ?? string.Empty,
                    BarkodNo = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? line.ProductName,
                    OlcuBirimi = product?.ProductUnitTypeName ?? line.UnitTypeName,
                    Miktari = line.Quantity,
                    BirimFiyati = line.UnitCost,
                    Tutari = line.Quantity * line.UnitCost
                });
            }

            data.Prepare();

            return data;
        }
    }
}
