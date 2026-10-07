using System.ComponentModel;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed partial class AtelierTransferStockForm : XtraForm
    {
        private readonly BindingList<AtelierTransferWorkshopProductDto> _rows = [];

        public AtelierTransferStockForm()
        {
            InitializeComponent();

            ConfigureGrid();

            Load += AtelierTransferStockForm_Load;
            btnRefresh.Click += async (_, _) => await LoadDataAsync();
        }

        private void ConfigureGrid()
        {
            gridMastersView.OptionsBehavior.AutoPopulateColumns = false;
            gridMastersView.OptionsView.ColumnAutoWidth = false;
            gridMastersView.OptionsView.ShowGroupPanel = true;
            gridMastersView.OptionsBehavior.AutoExpandAllGroups = true;
            gridMastersView.OptionsSelection.MultiSelect = false;
            gridMastersView.RowHeight = 26;
            gridMasters.DataSource = _rows;

            gridMastersView.Columns.AddRange(
            [
                MakeColumn("Atölye Kodu", nameof(AtelierTransferWorkshopProductDto.WorkshopCode), 130, nearText: true),
                MakeColumn("Atölye Adı", nameof(AtelierTransferWorkshopProductDto.WorkshopName), 300),
                MakeColumn("Ürün Kodu", nameof(AtelierTransferWorkshopProductDto.ProductCode), 130, nearText: true),
                MakeColumn("Ürün Adı", nameof(AtelierTransferWorkshopProductDto.ProductName), 300),
                MakeColumn("Birim", nameof(AtelierTransferWorkshopProductDto.UnitTypeName), 90, center: true),
                MakeMoneyColumn("Transfer Edilen", nameof(AtelierTransferWorkshopProductDto.TransferredQuantity), 140),
                MakeMoneyColumn("Tüketilen", nameof(AtelierTransferWorkshopProductDto.ConsumedQuantity), 130),
                MakeMoneyColumn("Taslak (Bekleyen)", nameof(AtelierTransferWorkshopProductDto.DraftQuantity), 150),
                MakeMoneyColumn("Bakiye", nameof(AtelierTransferWorkshopProductDto.AvailableQuantity), 130),
                MakeMoneyColumn("Toplam Tutar", nameof(AtelierTransferWorkshopProductDto.TotalAmount), 160)
            ]);

            foreach (GridColumn col in gridMastersView.Columns)
            {
                col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
            }

            GridColumn workshopGroupCol = gridMastersView.Columns[nameof(AtelierTransferWorkshopProductDto.WorkshopName)];
            workshopGroupCol.GroupIndex = 0;

            AddGroupSummary(SummaryItemType.Sum, nameof(AtelierTransferWorkshopProductDto.TransferredQuantity), "Transfer: {0:n2}");
            AddGroupSummary(SummaryItemType.Sum, nameof(AtelierTransferWorkshopProductDto.ConsumedQuantity), "Tüketim: {0:n2}");
            AddGroupSummary(SummaryItemType.Sum, nameof(AtelierTransferWorkshopProductDto.DraftQuantity), "Taslak: {0:n2}");
            AddGroupSummary(SummaryItemType.Sum, nameof(AtelierTransferWorkshopProductDto.AvailableQuantity), "Bakiye: {0:n2}");
            AddGroupSummary(SummaryItemType.Sum, nameof(AtelierTransferWorkshopProductDto.TotalAmount), "Genel Toplam: {0:n2}");
            gridMastersView.ExpandAllGroups();
        }

        private static GridColumn MakeColumn(string caption, string field, int width, bool nearText = false, bool center = false)
        {
            GridColumn column = CreateBaseColumn(caption, field, width);
            if (nearText)
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            }
            else if (center)
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            }
            return column;
        }

        private static GridColumn MakeMoneyColumn(string caption, string field, int width)
        {
            GridColumn column = CreateBaseColumn(caption, field, width);
            column.DisplayFormat.FormatType = FormatType.Numeric;
            column.DisplayFormat.FormatString = "n2";
            column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            return column;
        }

        private void AddGroupSummary(SummaryItemType summaryType, string fieldName, string displayFormat)
        {
            gridMastersView.GroupSummary.Add(
                summaryType,
                fieldName,
                gridMastersView.Columns[fieldName],
                displayFormat);
        }

        private static GridColumn CreateBaseColumn(string caption, string field, int width)
            => new() { Caption = caption, FieldName = field, Visible = true, Width = width };

        private async void AtelierTransferStockForm_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadingHelper.RunAsync(
                LoadDataCoreAsync,
                caption: "Atölye transfer raporu yükleniyor...",
                description: "Lütfen bekleyin...");
        }

        private async Task LoadDataCoreAsync()
        {
            btnRefresh.Enabled = false;
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await mediator.Send(new AtelierTransferStockQuery(), CancellationToken.None);

                List<AtelierTransferWorkshopDto> workshops = result.Data ?? [];

                _rows.Clear();
                foreach (AtelierTransferWorkshopDto workshop in workshops)
                {
                    foreach (AtelierTransferWorkshopProductDto product in workshop.Products)
                    {
                        product.WorkshopCode = workshop.WorkshopCode;
                        product.WorkshopName = workshop.WorkshopName;
                        _rows.Add(product);
                    }
                }

                lblSummary.Text = workshops.Count == 0
                    ? "Henüz atölyeye transfer yapılmamış."
                    : $"{workshops.Count} atölye, toplam {_rows.Count} ürün kalemi transfer edildi.";

                gridMastersView.ExpandAllGroups();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Rapor yüklenirken hata oluştu: " + ex.Message, ToastType.Error, 6000);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }
    }
}