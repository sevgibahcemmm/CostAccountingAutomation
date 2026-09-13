using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class CurrentAccountMovementsListForm : CrudListFormBase<CurrentAccountMovementGetAllQuery, CurrentAccountMovementDto, CurrentAccountMovementEditForm>
    {
        private readonly CurrentAccountType? _accountType;
        private readonly Guid? _customerId;
        private readonly Guid? _supplierId;

        public CurrentAccountMovementsListForm() : this(null, null, null)
        {
        }

        public CurrentAccountMovementsListForm(CurrentAccountType? accountType, Guid? customerId = null, Guid? supplierId = null)
            : base(accountType switch
            {
                CurrentAccountType.Customer => "Müşteri Hareketleri",
                CurrentAccountType.Supplier => "Tedarikçi Hareketleri",
                _ => "Cari Hareketler"
            })
        {
            _accountType = accountType;
            _customerId = customerId;
            _supplierId = supplierId;
        }

        protected override SvgImage ModuleIcon => SvgIcons.Modules[5];

        protected override string[] SearchFieldNames =>
        [
            nameof(CurrentAccountMovementDto.CurrentAccountName),
            nameof(CurrentAccountMovementDto.DocumentNo),
            nameof(CurrentAccountMovementDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Tarih", FieldName = nameof(CurrentAccountMovementDto.Date), Visible = true, Width = 95, DisplayFormat = { FormatType = FormatType.DateTime, FormatString = "dd.MM.yyyy" } },
                new() { Caption = "Cari Türü", FieldName = nameof(CurrentAccountMovementDto.CurrentAccountTypeName), Visible = !_accountType.HasValue, Width = 85 },
                new() { Caption = "Cari Adı", FieldName = nameof(CurrentAccountMovementDto.CurrentAccountName), Visible = true, Width = 220 },
                new() { Caption = "İşlem Türü", FieldName = nameof(CurrentAccountMovementDto.MovementTypeName), Visible = true, Width = 130 },
                new() { Caption = "Belge / Fatura No", FieldName = nameof(CurrentAccountMovementDto.DocumentNo), Visible = true, Width = 120 },
                new() { Caption = "Borç", FieldName = nameof(CurrentAccountMovementDto.Debit), Visible = true, Width = 100, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Alacak", FieldName = nameof(CurrentAccountMovementDto.Credit), Visible = true, Width = 100, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Bakiye", FieldName = nameof(CurrentAccountMovementDto.Balance), Visible = true, Width = 110, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Açıklama", FieldName = nameof(CurrentAccountMovementDto.Description), Visible = true, Width = 200 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override CurrentAccountMovementGetAllQuery BuildListQuery()
            => new(AccountType: _accountType, CustomerId: _customerId, SupplierId: _supplierId, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(CurrentAccountMovementDto item)
            => new CurrentAccountMovementDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CurrentAccountMovementDto item)
            => $"{item.CurrentAccountName} ({item.MovementTypeName} - {(item.Debit > 0 ? item.Debit : item.Credit):n2} ₺)";

        protected override bool SupportsRestore => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(CurrentAccountMovementDto item)
            => new CurrentAccountMovementRestoreCommand(item.Id);
    }
}
