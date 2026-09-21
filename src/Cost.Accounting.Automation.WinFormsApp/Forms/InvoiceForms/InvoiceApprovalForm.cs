using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using System.Drawing;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms
{
    public sealed partial class InvoiceApprovalForm : CrudListFormBase<InvoiceGetAllQuery, InvoiceDto, InvoiceEditForm>
    {
        private readonly InvoiceType? _targetType;

        public InvoiceApprovalForm() : this(null)
        {
        }

        public InvoiceApprovalForm(InvoiceType? targetType)
            : base("Fatura Onaylama")
        {
            _targetType = targetType;

            InitializeComponent();

            cmbMethod.Properties.Items.Add("FIFO (İlk Giren İlk Çıkar)");
            cmbMethod.Properties.Items.Add("LIFO (Son Giren İlk Çıkar)");
            cmbMethod.SelectedIndex = 0;
        }

        protected override SvgImage ModuleIcon => DxIcon.Invoices;

        protected override bool AllowDelete => false;

        protected override string[] SearchFieldNames =>
        [
            nameof(InvoiceDto.InvoiceNumber),
            nameof(InvoiceDto.CustomerName),
            nameof(InvoiceDto.SupplierName)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.Columns[nameof(InvoiceDto.StatusName)]!.Visible = false;
            View.Columns[nameof(InvoiceDto.IsActive)]!.Visible = false;
        }

        protected override InvoiceGetAllQuery BuildListQuery()
            => new(InvoiceType: _targetType, OnlyDeleted: false, Status: InvoiceStatus.Draft);

        protected override bool SupportsApprove => true;

        protected override IRequest<Result<string>>? BuildApproveCommand(InvoiceDto item)
        {
            if (item.Status != InvoiceStatus.Draft)
            {
                return null;
            }

            StockCostingMethod method = cmbMethod.SelectedIndex == 1
                ? StockCostingMethod.Lifo
                : StockCostingMethod.Fifo;

            return new InvoiceApproveCommand(item.Id, method);
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(InvoiceDto item)
            => new InvoiceDeleteCommand(item.Id);

        protected override string GetDeleteSummary(InvoiceDto item)
            => $"{item.InvoiceNumber} ({item.CurrentAccountName})";

        protected override bool AllowsEdit(InvoiceDto item) => false;
    }
}