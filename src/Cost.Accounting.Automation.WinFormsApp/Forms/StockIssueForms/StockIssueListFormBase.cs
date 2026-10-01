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
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
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
            ConfigureItemsDetail();
        }

        /// <summary>
        /// Master satır genişletildiğinde gösterilecek "Kalemler" detay görünümünü kurar.
        /// Böylece otomatik üretilen İngilizce "Lines" detayı yerine Türkçe sütunlu kalem listesi gelir.
        /// </summary>
        private void ConfigureItemsDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = false;

            GridView linesView = new(BaseGrid)
            {
                Name = "StockIssueLinesView",
                ViewCaption = "Kalemler"
            };
            linesView.OptionsBehavior.Editable = false;
            linesView.OptionsView.ShowGroupPanel = false;
            linesView.OptionsView.EnableAppearanceEvenRow = true;
            linesView.OptionsView.EnableAppearanceOddRow = true;

            AddDetailColumn(linesView, nameof(StockIssueLineDto.ProductCode), "Ürün Kodu", 110, alignment: "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.ProductName), "Ürün Adı", 220);
            AddDetailColumn(linesView, nameof(StockIssueLineDto.UnitTypeName), "Birim", 70, alignment: "Center");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.Quantity), "Miktar", 90, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.UnitCost), "Birim Maliyet", 110, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.TotalAmount), "Toplam Tutar", 120, "n2", "Right");
            AddDetailColumn(linesView, nameof(StockIssueLineDto.Description), "Açıklama", 200);

            linesView.OptionsView.ShowFooter = true;
            linesView.Columns[nameof(StockIssueLineDto.Quantity)].Summary.Add(
                SummaryItemType.Sum, nameof(StockIssueLineDto.Quantity), "Toplam: {0:n2}");
            linesView.Columns[nameof(StockIssueLineDto.TotalAmount)].Summary.Add(
                SummaryItemType.Sum, nameof(StockIssueLineDto.TotalAmount), "Toplam: {0:n2}");

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = "StockIssueLines",
                LevelTemplate = linesView
            });

            View.MasterRowGetRelationName += (_, e) => e.RelationName = "StockIssueLines";
            View.MasterRowGetChildList += (_, e) =>
                e.ChildList = (View.GetRow(e.RowHandle) as StockIssueListDto)?.Lines;
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
                column.DisplayFormat.FormatType = FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            if (alignment == "Right")
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
            }
            else if (alignment == "Center")
            {
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
            }

            view.Columns.Add(column);
        }

        protected override StockIssueGetAllQuery BuildListQuery()
            => new(IssueType: _issueType, OnlyDeleted: ShowDeleted);

        /// <summary>
        /// Stok çıkışı/tüketim kaydı bu liste ekranından oluşturulmaz; belge
        /// ilgili menüden açılır. Bu yüzden "Yeni" düğmesi varsayılan olarak
        /// gösterilmez. Menüde ayrı bir "yeni belge" girdisi bulunmayan belgeler
        /// (örn. atölye transferi) bu davranışı kendi listesinde geçersiz kılar.
        /// </summary>
        protected override bool AllowCreate => false;

        /// <summary>Listedeki tek eylem seçili çıkışın detayını incelemektir.</summary>
        protected override string EditButtonCaption => "Detay";

        protected override bool DoubleClickOpensEditor => false;

        protected override Task ShowItemDetailAsync(StockIssueListDto item)
        {
            // Kayıtla açılan form zaten salt okunur "İncele" modunda gelir
            // (kaydet düğmesi gizli, alanlar kilitli).
            using XtraForm form = CreateEditEditor(item);
            form.ShowDialog(this);
            return Task.CompletedTask;
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(StockIssueListDto item)
            => new StockIssueDeleteCommand(item.Id);

        protected override string GetDeleteSummary(StockIssueListDto item)
            => $"{item.DocumentNumber} - {item.TargetAccountName}";

        protected override bool SupportsRestore => false;

        /// <summary>
        /// Taslak transfer/tüketim belgeleri listeden onaylanabilir.
        /// </summary>
        protected override bool SupportsApprove => true;

        protected override bool AllowsApprove(StockIssueListDto item)
            => item.Status == StockIssueStatus.Draft;

        protected override bool AllowsEdit(StockIssueListDto item)
            => item.Status == StockIssueStatus.Draft;

        protected override bool AllowsDelete(StockIssueListDto item)
            => item.Status == StockIssueStatus.Draft;

        protected override IRequest<Result<string>>? BuildApproveCommand(StockIssueListDto item)
            => item.Status == StockIssueStatus.Draft ? new StockIssueApproveCommand(item.Id) : null;

        /// <summary>
        /// Seçili taslak belgeleri tek transaction'da toplu onaylar: aynı
        /// malzemeyi kullanan belgelerde ya hep birlikte ya hiç onaylanır.
        /// </summary>
        protected override IRequest<Result<string>>? BuildBulkApproveCommand(IReadOnlyList<StockIssueListDto> items)
        {
            List<Guid> draftIds = items
                .Where(i => i.Status == StockIssueStatus.Draft)
                .Select(i => i.Id)
                .ToList();

            return draftIds.Count > 0 ? new BulkApproveStockIssuesCommand(draftIds) : null;
        }

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
            Dictionary<Guid, ProductCatalogDto> productsById = await MovableAssetTransactionSlipPresenter.LoadProductsByIdAsync();

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
                ProductCatalogDto? product = productsById.GetValueOrDefault(line.ProductId);
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
