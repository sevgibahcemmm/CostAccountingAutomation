using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
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
                InvoiceType.Sales => "Satış Faturaları",
                InvoiceType.Purchase => "Satın Alma Faturaları",
                _ => "Faturalar"
            })
        {
            _targetType = targetType;
            InitializeLinesDetailView();
        }

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
        }

        private void InitializeLinesDetailView()
        {
            GridView linesView = new(BaseGrid)
            {
                Name = "InvoiceLinesDetailView"
            };
            linesView.OptionsBehavior.Editable = false;
            linesView.OptionsDetail.AllowOnlyOneMasterRowExpanded = true;
            linesView.OptionsView.ShowGroupPanel = false;
            linesView.OptionsView.ShowFooter = true;
            linesView.RowHeight = 26;
            linesView.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            linesView.Appearance.HeaderPanel.Options.UseFont = true;

            GridColumnFactory.ConfigureFromAttributes(linesView, typeof(InvoiceLineDto));

            linesView.Columns[nameof(InvoiceLineDto.ProductId)]!.Visible = false;

            GridColumn colProductCode = linesView.Columns.AddField(nameof(InvoiceLineDto.ProductCode));
            colProductCode.Caption = "Ürün Kodu";
            colProductCode.Visible = true;
            colProductCode.VisibleIndex = 0;
            colProductCode.Width = 90;

            GridColumn colProductName = linesView.Columns.AddField(nameof(InvoiceLineDto.ProductName));
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

                    col.Summary.Add(SummaryItemType.Sum, col.FieldName, "Toplam: {0:n2}");
                }
            }

            View.MasterRowEmpty += (_, e) =>
            {
                if (View.GetRow(e.RowHandle) is InvoiceDto invoice)
                {
                    e.IsEmpty = invoice.Lines is null || invoice.Lines.Count == 0;
                }
            };

            GridLevelNode levelNode = new()
            {
                RelationName = nameof(InvoiceDto.Lines),
                LevelTemplate = linesView
            };
            BaseGrid.LevelTree.Nodes.Add(levelNode);
        }

        protected override InvoiceGetAllQuery BuildListQuery()
            => new(InvoiceType: _targetType, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(InvoiceDto item)
            => new InvoiceDeleteCommand(item.Id);

        protected override string GetDeleteSummary(InvoiceDto item)
            => $"{item.InvoiceNumber} ({item.CurrentAccountName})";

        protected override bool SupportsRestore => true;

        protected override bool SupportsApprove => true;

        protected override bool AllowsEdit(InvoiceDto item) => item.Status != InvoiceStatus.Approved;

        protected override bool AllowsDelete(InvoiceDto item) => item.Status != InvoiceStatus.Approved;

        protected override IRequest<Result<string>>? BuildApproveCommand(InvoiceDto item)
            => item.Status == InvoiceStatus.Draft ? new InvoiceApproveCommand(item.Id) : null;

        protected override IRequest<Result<string>> BuildRestoreCommand(InvoiceDto item)
            => new InvoiceRestoreCommand(item.Id);

        protected override bool SupportsSlipPrint => _targetType != InvoiceType.Sales;

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
            Dictionary<Guid, ProductDto> productsById = await MovableAssetTransactionSlipPresenter.LoadProductsByIdAsync();

            string city = string.IsNullOrWhiteSpace(company.City) ? string.Empty : company.City.Trim();
            string district = string.IsNullOrWhiteSpace(company.District) ? string.Empty : company.District.Trim();
            string ilIlce = city.Length > 0 && district.Length > 0 ? $"{city} / {district}" : city + district;

            DateTime date = invoice.Date.ToDateTime(TimeOnly.MinValue);

            MovableAssetTransactionSlipData data = new()
            {
                DocumentNumber = invoice.InvoiceNumber,
                Date = date,
                IslemCesidi = "Giriş",
                NeredenGeldigi = invoice.SupplierName ?? string.Empty,
                KimeVerildigi = string.Empty,
                NereyeVerildigi = string.Empty,
                IlIlceAdi = ilIlce,
                IlIlceKodu = string.Empty,
                HarcamaBirimiAdi = company.ExpenditureUnitName ?? string.Empty,
                HarcamaBirimiKodu = company.ExpenditureUnitCode ?? string.Empty,
                MuhasebeBirimiAdi = company.AccountingUnitName ?? string.Empty,
                MuhasebeBirimiKodu = company.AccountingUnitCode ?? string.Empty,
                DayanakTarihi = date,
                DayanakKodu = invoice.InvoiceNumber,
                AccountNames = MovableAssetTransactionSlipPresenter.BuildAccountNameMap(accounts)
            };

            foreach (InvoiceLineDto line in lines)
            {
                ProductDto? product = productsById.GetValueOrDefault(line.ProductId);
                data.Rows.Add(new MovableAssetTransactionSlipRow
                {
                    Kodu = MovableAssetTransactionSlipPresenter.ResolveItemCode(product, line.ProductCode),
                    DepoKodu = product?.WarehouseCode ?? string.Empty,
                    DepoAdi = product?.WarehouseName ?? string.Empty,
                    BarkodNo = product?.Barcode ?? string.Empty,
                    Adi = product?.Name ?? line.ProductName,
                    OlcuBirimi = product?.ProductUnitTypeName ?? string.Empty,
                    Miktari = line.Quantity,
                    BirimFiyati = line.UnitPrice,
                    Tutari = line.Quantity * line.UnitPrice
                });
            }

            data.Prepare();

            return data;
        }
    }
}
