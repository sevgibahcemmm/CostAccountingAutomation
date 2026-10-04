using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    public sealed partial class InvoicesListForm : CrudListFormBase<InvoiceGetAllQuery, InvoiceDto, InvoiceEditForm>
    {
        private readonly InvoiceType? _targetType;

        public InvoicesListForm() : this(null)
        {
        }

        public InvoicesListForm(InvoiceType? targetType)
            : base(targetType switch
            {
                InvoiceType.Purchase => "Alış Faturaları",
                InvoiceType.PurchaseReturn => "Alış İade Faturaları",
                InvoiceType.Sales => "Satış Faturaları",
                InvoiceType.SalesReturn => "Satış İade Faturaları",
                _ => "Faturalar"
            })
        {
            _targetType = targetType;
            InitializeLinesDetailView();

            // Taban sınıf master-detail ayarlarını kendi Load/ctor akışında değiştirmiş olabilir;
            // form ekrana geldiğinde ayarları ve seviye düğümünü yeniden garanti altına al.
            Shown += (_, _) => EnsureMasterDetail();
        }

        private GridView? _linesView;

        protected override SvgImage ModuleIcon => DxIcon.Invoices;

        protected override string[] SearchFieldNames =>
        [
            nameof(InvoiceDto.InvoiceNumber),
            nameof(InvoiceDto.CustomerName),
            nameof(InvoiceDto.SupplierName),
            nameof(InvoiceDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.Columns[nameof(InvoiceDto.IsActive)]!.Visible = false;

            View.OptionsView.ShowGroupPanel = true;
            View.OptionsView.ShowFooter = true;
            SetColumnWidth(nameof(InvoiceDto.InvoiceTypeName), 120);
            SetColumnWidth(nameof(InvoiceDto.StatusName), 70);
            SetColumnWidth(nameof(InvoiceDto.Date), 95);
            SetColumnWidth(nameof(InvoiceDto.CurrentAccountName), 220);
            SetColumnWidth(nameof(InvoiceDto.SubTotal), 110);
            SetColumnWidth(nameof(InvoiceDto.DiscountTotal), 110);
            SetColumnWidth(nameof(InvoiceDto.TaxTotal), 110);
            SetColumnWidth(nameof(InvoiceDto.GrandTotal), 120);

            foreach (string fieldName in new[]
                     {
                         nameof(InvoiceDto.SubTotal),
                         nameof(InvoiceDto.DiscountTotal),
                         nameof(InvoiceDto.TaxTotal),
                         nameof(InvoiceDto.GrandTotal)
                     })
            {
                if (View.Columns[fieldName] is { } col)
                {
                    col.Summary.Add(SummaryItemType.Sum, col.FieldName, "{0:n2}");
                }
            }

            if (View.Columns[nameof(InvoiceDto.CurrentAccountName)] is { } totalCol)
            {
                totalCol.Summary.Add(SummaryItemType.Sum, nameof(InvoiceDto.GrandTotal), "Genel Toplam: ");
            }

            EnsureMasterDetail();
        }

        /// <summary>
        /// Ana gridde master-detail'in açık olduğundan ve "Lines" ilişkisinin seviye ağacında
        /// bulunduğundan emin olur. Birden çok kez çağrılması güvenlidir.
        /// </summary>
        private void EnsureMasterDetail()
        {
            View.OptionsDetail.EnableMasterViewMode = true;
            View.OptionsDetail.ShowDetailTabs = false;
            View.OptionsDetail.AllowExpandEmptyDetails = false;
            View.OptionsDetail.AllowOnlyOneMasterRowExpanded = true;
            View.OptionsDetail.SmartDetailHeight = true;
            View.OptionsView.ShowDetailButtons = true;

            if (_linesView is null)
            {
                return;
            }

            foreach (GridLevelNode node in BaseGrid.LevelTree.Nodes)
            {
                if (node.RelationName == nameof(InvoiceDto.Lines))
                {
                    return;
                }
            }

            BaseGrid.LevelTree.Nodes.Add(new GridLevelNode
            {
                RelationName = nameof(InvoiceDto.Lines),
                LevelTemplate = _linesView
            });
        }

        private void SetColumnWidth(string fieldName, int width)
        {
            if (View.Columns[fieldName] is { } col)
            {
                col.Width = width;
                col.MinWidth = width;
                col.MaxWidth = width;
            }
        }

        private void InitializeLinesDetailView()
        {
            GridView linesView = new(BaseGrid)
            {
                Name = "InvoiceLinesDetailView"
            };
            _linesView = linesView;
            linesView.OptionsBehavior.Editable = false;
            linesView.OptionsView.ShowGroupPanel = false;
            linesView.OptionsView.ShowFooter = true;
            linesView.RowHeight = 26;
            linesView.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            linesView.Appearance.HeaderPanel.Options.UseFont = true;

            GridColumnFactory.ConfigureFromAttributes(linesView, typeof(InvoiceLineDto));

            linesView.Columns[nameof(InvoiceLineDto.ProductId)]!.Visible = false;

            View.MasterRowExpanded += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is InvoiceDto invoice
                    && View.GetDetailView(e.RowHandle, e.RelationIndex) is GridView detailView
                    && detailView.Columns[nameof(InvoiceLineDto.UnitPrice)] is { } priceCol)
                {
                    priceCol.Caption = invoice.InvoiceType.IsPurchaseSide()
                        ? "Alış Fiyatı"
                        : "Satış Fiyatı";
                }
            };

            GridColumn colProductCode = linesView.Columns[nameof(InvoiceLineDto.ProductCode)]
                ?? linesView.Columns.AddField(nameof(InvoiceLineDto.ProductCode));
            colProductCode.Caption = "Ürün Kodu";
            colProductCode.Visible = true;
            colProductCode.VisibleIndex = 0;
            colProductCode.Width = 90;

            GridColumn colProductName = linesView.Columns[nameof(InvoiceLineDto.ProductName)]
                ?? linesView.Columns.AddField(nameof(InvoiceLineDto.ProductName));
            colProductName.Caption = "Ürün Adı";
            colProductName.Visible = true;
            colProductName.VisibleIndex = 1;
            colProductName.Width = 180;

            foreach (GridColumn col in linesView.Columns)
            {
                if (col.FieldName is nameof(InvoiceLineDto.Quantity)
                    or nameof(InvoiceLineDto.UnitPrice)
                    or nameof(InvoiceLineDto.DiscountRate)
                    or nameof(InvoiceLineDto.DiscountAmount)
                    or nameof(InvoiceLineDto.TaxRateRate)
                    or nameof(InvoiceLineDto.TaxAmount)
                    or nameof(InvoiceLineDto.TotalAmount))
                {
                    col.DisplayFormat.FormatType = FormatType.Numeric;
                    col.DisplayFormat.FormatString = "n2";
                    col.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                    col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;

                    col.Summary.Add(SummaryItemType.Sum, col.FieldName, "{0:n2}");
                }
            }

            View.MasterRowEmpty += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is InvoiceDto invoice)
                {
                    e.IsEmpty = invoice.Lines is null || invoice.Lines.Count == 0;
                }
            };

            EnsureMasterDetail();
        }

        protected override InvoiceGetAllQuery BuildListQuery()
            => new(InvoiceType: _targetType, OnlyDeleted: ShowDeleted);

        /// <summary>
        /// En yeni fatura üstte. Geçmiş tarihli fatura bugün kaydedilse bile
        /// kendi tarihine göre aşağıda kalır.
        /// </summary>
        protected override IEnumerable<InvoiceDto> ApplyDefaultOrder(IEnumerable<InvoiceDto> items)
            => Utils.ListOrder.NewestDocumentFirst(items, x => x.Date, x => x.CreatedAt, x => x.Id);

        protected override IRequest<Result<string>> BuildDeleteCommand(InvoiceDto item)
            => new InvoiceDeleteCommand(item.Id);

        protected override string GetDeleteSummary(InvoiceDto item)
            => $"{item.InvoiceNumber} ({item.CurrentAccountName})";

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        /// <summary>Bu liste onaylı faturaları da içerir; bekleyenler taslak durumdadır.</summary>
        protected override string PendingItemLabel => "fatura";

        protected override bool IsPendingApproval(InvoiceDto item) => item.Status == InvoiceStatus.Draft;

        /// <summary>"Tamam" ile ayrı onay ekranına geçilir.</summary>
        protected override Type? PendingApprovalFormType => typeof(InvoiceApprovalForm);

        protected override string? PendingApprovalFormTitle => "Fatura Onaylama";

        protected override bool AllowsEdit(InvoiceDto item) => item.Status != InvoiceStatus.Approved;

        protected override bool AllowsDelete(InvoiceDto item) => item.Status != InvoiceStatus.Approved;

        protected override IRequest<Result<string>>? BuildApproveCommand(InvoiceDto item)
            => item.Status == InvoiceStatus.Draft ? new InvoiceApproveCommand(item.Id) : null;

        protected override IRequest<Result<string>> BuildRestoreCommand(InvoiceDto item)
            => new InvoiceRestoreCommand(item.Id);

        protected override bool SupportsSlipPrint => _targetType is null || _targetType == InvoiceType.Purchase;

        protected override bool CanPrintSlip(InvoiceDto item) => item.InvoiceType == InvoiceType.Purchase;

        protected override async Task<MovableAssetTransactionSlipData?> BuildSlipDataAsync(InvoiceDto invoice)
        {
            List<InvoiceLineDto> lines = invoice.Lines
                .Where(l => l.ProductId != Guid.Empty && l.Quantity > 0)
                .ToList();

            if (lines.Count == 0)
            {
                ToastHelper.Show("Faturada taşınır işlem fişine aktarılacak kalem yok.", ToastType.Warning);
                return null;
            }

            CompanyDto company = await MovableAssetTransactionSlipPresenter.LoadCompanyAsync();
            List<ChartOfAccountLookUpDto> accounts = await MovableAssetTransactionSlipPresenter.LoadAccountsAsync();
            Dictionary<Guid, ProductCatalogDto> productsById = await MovableAssetTransactionSlipPresenter.LoadProductsByIdAsync();

            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            DateTime date = invoice.Date.ToDateTime(TimeOnly.MinValue);

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = invoice.InvoiceNumber,
                Date = date,
                OperationType = "Giriş",
                SourceParty = invoice.SupplierName ?? string.Empty,
                RecipientParty = string.Empty,
                DestinationParty = string.Empty,
                ProvinceDistrictName = ilIlce,
                ProvinceDistrictCode = string.Empty,
                ExpenditureUnitName = company.ExpenditureUnitName ?? string.Empty,
                ExpenditureUnitCode = company.ExpenditureUnitCode ?? string.Empty,
                AccountingUnitName = company.AccountingUnitName ?? string.Empty,
                AccountingUnitCode = company.AccountingUnitCode ?? string.Empty,
                ReferenceDate = date,
                ReferenceCode = invoice.InvoiceNumber,
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(accounts)
            };

            foreach (InvoiceLineDto line in lines)
            {
                ProductCatalogDto? product = productsById.GetValueOrDefault(line.ProductId);
                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    Code = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, line.ProductCode),
                    WarehouseCode = product?.WarehouseCode ?? string.Empty,
                    WarehouseName = product?.WarehouseName ?? string.Empty,
                    Barcode = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? line.ProductName,
                    UnitOfMeasure = product?.ProductUnitTypeName ?? string.Empty,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    Amount = line.Quantity * line.UnitPrice
                });
            }

            data.Prepare();

            return data;
        }
    }
}