using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms
{
    public sealed partial class PaymentCollectionListForm : CrudListFormBase<CurrentAccountMovementGetAllQuery, CurrentAccountMovementDto, CurrentAccountMovementEditForm>
    {
        private readonly CurrentAccountMovementType? _targetType;

        public PaymentCollectionListForm() : this(null)
        {
        }

        public PaymentCollectionListForm(CurrentAccountMovementType? targetType)
            : base(targetType switch
            {
                CurrentAccountMovementType.Payment => "Ödemeler",
                CurrentAccountMovementType.Collection => "Tahsilatlar",
                _ => "Ödeme / Tahsilat"
            })
        {
            _targetType = targetType;
        }

        protected override SvgImage ModuleIcon => DxIcon.Payments;

        protected override XtraForm CreateNewEditor()
            => new PaymentCollectionEditForm(_targetType);

        /// <summary>
        /// Ödeme/tahsilat kayıtları kullanıcı tarafından bu ekrandan girildiği
        /// için burada silme yeteneği korunur; yanlış girilen kayıt ancak
        /// bu ekrandan düzeltilebilir.
        /// </summary>
        protected override string DeleteItemLabel => "ödeme/tahsilat";

        protected override string[] SearchFieldNames =>
        [
            nameof(CurrentAccountMovementDto.CurrentAccountName),
            nameof(CurrentAccountMovementDto.DocumentNo),
            nameof(CurrentAccountMovementDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.OptionsView.ColumnAutoWidth = true;
            View.Columns[nameof(CurrentAccountMovementDto.MovementTypeName)]!.Visible = !_targetType.HasValue;
            View.Columns[nameof(CurrentAccountMovementDto.DocumentNo)]!.Caption = "Belge / Fatura No";
        }

        protected override CurrentAccountMovementGetAllQuery BuildListQuery()
        {
            CurrentAccountMovementType[]? types = _targetType.HasValue
                ? [_targetType.Value]
                : [CurrentAccountMovementType.Payment, CurrentAccountMovementType.Collection];

            return new(AccountType: null, CustomerId: null, SupplierId: null, OnlyDeleted: ShowDeleted, MovementTypes: types);
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(CurrentAccountMovementDto item)
            => new CurrentAccountMovementDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CurrentAccountMovementDto item)
            => $"{item.CurrentAccountName} ({item.MovementTypeName} - {(item.Debit > 0 ? item.Debit : item.Credit):n2} ₺)";

        protected override bool SupportsRestore => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(CurrentAccountMovementDto item)
            => new CurrentAccountMovementRestoreCommand(item.Id);
    }
}