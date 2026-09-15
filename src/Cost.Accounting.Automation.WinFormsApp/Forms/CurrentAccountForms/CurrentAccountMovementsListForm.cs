using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
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

        protected override SvgImage ModuleIcon => DxIcon.CurrentAccounts;

        protected override string[] SearchFieldNames =>
        [
            nameof(CurrentAccountMovementDto.CurrentAccountName),
            nameof(CurrentAccountMovementDto.DocumentNo),
            nameof(CurrentAccountMovementDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.Columns[nameof(CurrentAccountMovementDto.CurrentAccountTypeName)]!.Visible = !_accountType.HasValue;
            View.Columns[nameof(CurrentAccountMovementDto.DocumentNo)]!.Caption = "Belge / Fatura No";
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
