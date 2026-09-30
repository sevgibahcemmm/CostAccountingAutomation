using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports;
using Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public sealed partial class CostSlipsListForm : CrudListFormBase<CostSlipGetAllQuery, CostSlipListDto, CostSlipEditForm>
    {
public CostSlipsListForm() : base("Maliyet Pusulası")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Module;

        protected override string[] SearchFieldNames =>
        [
            nameof(CostSlipListDto.SlipNumber),
            nameof(CostSlipListDto.WorkshopName),
            nameof(CostSlipListDto.ProducedProductName),
            nameof(CostSlipListDto.Description)
        ];

protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            ConfigureMasterSummary();
            ConfigureItemsDetail();
        }

        private void ConfigureMasterSummary()
        {
            View.OptionsView.ShowFooter = true;

            GridColumn quantityColumn = View.Columns[nameof(CostSlipListDto.Quantity)];
            quantityColumn.Summary.Add(SummaryItemType.Sum, nameof(CostSlipListDto.Quantity), "Toplam: {0:n0}");

            GridColumn grandTotalColumn = View.Columns[nameof(CostSlipListDto.GrandTotal)];
            grandTotalColumn.Summary.Add(SummaryItemType.Sum, nameof(CostSlipListDto.GrandTotal), "Toplam: {0:n2}");
        }

        private void ConfigureItemsDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = false;

            GridView itemsView = new(BaseGrid)
            {
                Name = "CostSlipItemsView"
            };
            itemsView.OptionsBehavior.Editable = false;
            itemsView.OptionsView.ShowGroupPanel = false;
            itemsView.OptionsView.EnableAppearanceEvenRow = true;
            itemsView.OptionsView.EnableAppearanceOddRow = true;

            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductName), "Ürün / Masraf", 220);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ProductUnitTypeName), "Birim", 80, alignment: "Center");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.ExpenseAccountTypeName), "Hesap", 250);
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Quantity), "Miktar", 90, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.UnitPrice), "Birim Fiyat", 100, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.TotalAmount), "Tutar", 110, "n2", "Right");
            AddDetailColumn(itemsView, nameof(CostSlipItemDto.Description), "Açıklama", 200);

            itemsView.OptionsView.ShowFooter = true;
            itemsView.Columns[nameof(CostSlipItemDto.Quantity)].Summary.Add(
                SummaryItemType.Sum, nameof(CostSlipItemDto.Quantity), "Toplam: {0:n2}");
            itemsView.Columns[nameof(CostSlipItemDto.TotalAmount)].Summary.Add(
                SummaryItemType.Sum, nameof(CostSlipItemDto.TotalAmount), "Toplam: {0:n2}");

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = "CostSlipItems",
                LevelTemplate = itemsView
            });

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "CostSlipItems";
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as CostSlipListDto)?.CostSlipItems;
        }

        private static void AddDetailColumn(GridView view, string fieldName, string caption, int width,
            string? format = null, string? alignment = null)
        {
            GridColumn column = new()
            {
                Caption = caption,
                FieldName = fieldName,
                Width = width,
                Visible = true,
                OptionsColumn = { AllowEdit = false }
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            if (alignment == "Right")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            }
            else if (alignment == "Center")
            {
                column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }

            view.Columns.Add(column);
        }

        protected override CostSlipGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(CostSlipListDto item)
            => new CostSlipDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CostSlipListDto item)
            => item.SlipNumber;

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        protected override bool SupportsSlipReport => true;

        protected override bool SupportsDistributionReport => true;

        protected override bool SupportsProductDeclarationReport => true;

        protected override bool AllowsApprove(CostSlipListDto item) => item.Status == CostSlipStatus.Draft;

        protected override bool AllowsEdit(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override bool AllowsDelete(CostSlipListDto item) => item.Status != CostSlipStatus.Approved;

        protected override IRequest<Result<string>>? BuildApproveCommand(CostSlipListDto item)
            => item.Status == CostSlipStatus.Draft ? new CostSlipApproveCommand(item.Id) : null;

        protected override IRequest<Result<string>> BuildRestoreCommand(CostSlipListDto item)
            => new CostSlipRestoreCommand(item.Id);

        protected override Task ShowSlipReportAsync(CostSlipListDto item)
            => CostSlipReportPresenter.ShowAsync(item);

        protected override async Task ShowDistributionReportAsync(CostSlipListDto? item)
        {
            using var dateForm = new DateRangePromptForm(item?.CostDate, item?.CostDate);
            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Bekleme penceresi yalnızca veri hazırlığını kapsar; önizleme
            // modal bir pencere olduğu için bekleme kapandıktan sonra açılır.
            ICostAllocationTableReport? report = await LoadingHelper.RunAsync(
                async () =>
                {
                    using var scope = Program.Services.CreateScope();
                    ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    GiderDagilimReportResult result = await mediator.Send(
                        new GiderDagilimReportQuery(dateForm.StartDate, dateForm.EndDate, dateForm.CostSlipType));

                    if (result.Rows.Count == 0)
                    {
                        ToastHelper.Show("Seçilen tarih aralığında atölye kaydı bulunamadı.", ToastType.Warning);
                        return null;
                    }

                    ICostAllocationTableReport built = dateForm.CostSlipType is CostSlipType.Service or CostSlipType.SemiFinishedService
                        ? new ServiceCostAllocationTable()
                        : new ProductCostAllocationTableReport();

                    CompanyDto company = await LoadCompanyAsync();

                    built.SetData(dateForm.StartDate, dateForm.EndDate, result, company.Letterhead, dateForm.CostSlipType);
                    return built;
                },
                caption: "Rapor hazırlanıyor...",
                description: "Lütfen bekleyin...");

            report?.PrintReport();
        }

        protected override async Task ShowProductDeclarationReportAsync(CostSlipListDto? item)
        {
            using var dateForm = new DateRangePromptForm(
                item?.CostDate,
                item?.CostDate,
                showTypeSelector: false,
                headerTitle: "Mamül Üretim Beyanı");
            if (dateForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // Yalnızca veri çekilişi bekleme penceresinde bekler; sonrasındaki
            // atölye seçimi ve rapor önizlemesi modal pencereler oldukları için
            // bekleme penceresi kapalıyken çalışır.
            List<ProductDeclarationRowDto> rows = await LoadingHelper.RunAsync(
                async () =>
                {
                    using var scope = Program.Services.CreateScope();
                    ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                    return await mediator.Send(
                        new ProductDeclarationReportQuery(dateForm.StartDate, dateForm.EndDate),
                        CancellationToken.None);
                },
                caption: "Rapor hazırlanıyor...",
                description: "Lütfen bekleyin...");

            if (rows.Count == 0)
            {
                ToastHelper.Show("Seçilen tarih aralığında onaylı mamül fişi bulunamadı.", ToastType.Warning);
                return;
            }

            List<string> workshops = rows
                .Select(r => r.WorkshopName)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (workshops.Count > 1)
            {
                using var prompt = new WorkshopSelectionPromptForm(workshops);
                if (prompt.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<string> selected = prompt.SelectedWorkshops;
                rows = selected.Count == workshops.Count
                    ? rows
                    : rows.Where(r => selected.Contains(r.WorkshopName)).ToList();
            }

            CompanyDto company = await LoadCompanyAsync();

            var report = new ProductDeclarationReport();
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
                CrashLog.WriteException("DistributionReport.Company", ex);
            }

            return new CompanyDto();
        }
    }
}