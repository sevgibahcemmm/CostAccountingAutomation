using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
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
        }

        protected override SvgImage ModuleIcon => SvgIcons.Modules[4];

        protected override string[] SearchFieldNames =>
        [
            nameof(InvoiceDto.InvoiceNumber),
            nameof(InvoiceDto.CustomerName),
            nameof(InvoiceDto.SupplierName),
            nameof(InvoiceDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Fatura No", FieldName = nameof(InvoiceDto.InvoiceNumber), Visible = true, Width = 130 },
                new() { Caption = "Fatura Türü", FieldName = nameof(InvoiceDto.InvoiceTypeName), Visible = !_targetType.HasValue, Width = 130 },
                new() { Caption = "Tarih", FieldName = nameof(InvoiceDto.Date), Visible = true, Width = 100, DisplayFormat = { FormatType = FormatType.DateTime, FormatString = "dd.MM.yyyy" } },
                new() { Caption = "Cari Adı", FieldName = nameof(InvoiceDto.CurrentAccountName), Visible = true, Width = 230 },
                new() { Caption = "Ara Toplam", FieldName = nameof(InvoiceDto.SubTotal), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "KDV Tutarı", FieldName = nameof(InvoiceDto.TaxTotal), Visible = true, Width = 100, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Genel Toplam", FieldName = nameof(InvoiceDto.GrandTotal), Visible = true, Width = 120, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Açıklama", FieldName = nameof(InvoiceDto.Description), Visible = true, Width = 200 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override InvoiceGetAllQuery BuildListQuery()
            => new(InvoiceType: _targetType, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(InvoiceDto item)
            => new InvoiceDeleteCommand(item.Id);

        protected override string GetDeleteSummary(InvoiceDto item)
            => $"{item.InvoiceNumber} ({item.CurrentAccountName})";

        protected override bool SupportsRestore => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(InvoiceDto item)
            => new InvoiceRestoreCommand(item.Id);
    }
}
