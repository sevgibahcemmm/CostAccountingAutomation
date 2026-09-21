using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.ProductMovements;
using Cost.Accounting.Automation.Application.StockMovements;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable;
using Cost.Accounting.Automation.WinFormsApp.Reports.StockMovementsListReports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductMovementForms
{
    public sealed partial class ProductMovementsListForm : CrudListFormBase<ProductMovementGetAllQuery, ProductMovementListDto, ProductMovementEditForm>
    {
        private readonly ProductMovementType? _targetType;
        private readonly Guid? _productId;

        public ProductMovementsListForm() : this(null, null)
        {
        }

        public ProductMovementsListForm(ProductMovementType? targetType, Guid? productId = null)
            : base(targetType switch
            {
                ProductMovementType.Input => "Stok Girişleri",
                ProductMovementType.Output => "Stok Çıkışları",
                _ => "Stok Hareketleri"
            })
        {
            _targetType = targetType;
            _productId = productId;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = LoadWarehouseFilterAsync();
        }

        private async Task LoadWarehouseFilterAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                List<ChartOfAccountLookUpDto> warehouses = ((await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [])
                    .Where(w => w.Type == ChartOfAccountType.Warehouse)
                    .ToList();

                ConfigureFilter(warehouses, nameof(ChartOfAccountLookUpDto.Id), nameof(ChartOfAccountLookUpDto.Display), "Depo");
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Depo filtresi yüklenemedi: " + ex.Message, ToastType.Warning);
            }
        }

        protected override SvgImage ModuleIcon => _targetType switch
        {
            ProductMovementType.Input => DxIcon.StockInput,
            ProductMovementType.Output => DxIcon.StockOutput,
            _ => DxIcon.StockMovements
        };

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductMovementListDto.ProductName),
            nameof(ProductMovementListDto.ProductCode),
            nameof(ProductMovementListDto.Barcode),
            nameof(ProductMovementListDto.WarehouseName),
            nameof(ProductMovementListDto.ReferenceNo),
            nameof(ProductMovementListDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.OptionsView.ColumnAutoWidth = false;
            View.Columns[nameof(ProductMovementListDto.MovementTypeName)]!.Visible = !_targetType.HasValue;
        }

        protected override ProductMovementGetAllQuery BuildListQuery()
            => new(
                ProductId: _productId,
                MovementType: _targetType,
                WarehouseId: SelectedFilterGuid,
                OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductMovementListDto item)
            => new ProductMovementDeleteCommand(item.Id);

        protected override string GetDeleteSummary(ProductMovementListDto item)
            => $"{item.ProductName} ({item.MovementTypeName} - {item.Quantity:n2})";

        protected override bool SupportsRestore => true;

        protected override bool SupportsStockMovementsListReport => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductMovementListDto item)
            => new ProductMovementRestoreCommand(item.Id);

        protected override async Task ShowStockMovementsListReportAsync(ProductMovementListDto? item)
        {
            using var dateForm = new DateRangePromptForm(
                defaultStart: null,
                defaultEnd: null,
                showTypeSelector: false,
                headerTitle: "Stok Hareket Listesi");
            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            List<StockMovementReportRowDto> rows = await mediator.Send(
                new StockMovementsListReportQuery(
                    dateForm.StartDate,
                    dateForm.EndDate,
                    ProductId: _productId,
                    WarehouseId: SelectedFilterGuid),
                CancellationToken.None);

            if (rows.Count == 0)
            {
                ToastHelper.Show("Seçilen tarih aralığında stok hareketi kaydı bulunamadı.", ToastType.Warning);
                return;
            }

            CompanyDto company = await LoadCompanyAsync();

            var report = new StockMovementsListReport();
            report.SetData(dateForm.StartDate, dateForm.EndDate, rows, company.Letterhead);
            report.PrintReport();
        }

        private static async Task<CompanyDto> LoadCompanyAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();

                var result = await mediator.Send(new CompanyGetQuery(session.GetCompanyId()), CancellationToken.None);
                if (result.IsSuccessful && result.Data is not null)
                {
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("StockMovementsListReport.Company", ex);
            }

            return new CompanyDto();
        }
    }
}
