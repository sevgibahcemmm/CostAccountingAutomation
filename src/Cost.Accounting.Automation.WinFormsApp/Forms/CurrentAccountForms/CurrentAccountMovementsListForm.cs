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

        /// <summary>
        /// Cari hareket bir ikincil kayıttır: fatura, maliyet pusulası, stok
        /// çıkışı ve ödeme/tahsilat kayıtlarından türetilir. Bu ekrandan
        /// silinirse kaynak belge ile cari hesap arasındaki bağ kopar. Düzeltme
        /// kaynak belge üzerinden yapılır; bu yüzden "Sil" düğmesi gizlenir.
        /// </summary>
        protected override bool AllowDelete => false;

        protected override string[] SearchFieldNames =>
        [
            nameof(CurrentAccountMovementDto.CurrentAccountName),
            nameof(CurrentAccountMovementDto.DocumentNo),
            nameof(CurrentAccountMovementDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.OptionsView.ColumnAutoWidth = false;
            View.Columns[nameof(CurrentAccountMovementDto.CurrentAccountTypeName)]!.Visible = !_accountType.HasValue;
            View.Columns[nameof(CurrentAccountMovementDto.DocumentNo)]!.Caption = "Belge / Fatura No";

            // Taban sınıf yükleme sonrası BestFitColumns çağırıyor; bu değerler
            // minimum genişlik olarak korunur, aksi halde kısa içerikli sütunlar
            // (Cari Adı, Açıklama) okunamayacak kadar daralıyor.
            SetColumnWidth(nameof(CurrentAccountMovementDto.Date), 95);
            SetColumnWidth(nameof(CurrentAccountMovementDto.CurrentAccountTypeName), 110);
            SetColumnWidth(nameof(CurrentAccountMovementDto.CurrentAccountName), 260);
            SetColumnWidth(nameof(CurrentAccountMovementDto.MovementTypeName), 150);
            SetColumnWidth(nameof(CurrentAccountMovementDto.DocumentNo), 140);
            SetColumnWidth(nameof(CurrentAccountMovementDto.Debit), 120);
            SetColumnWidth(nameof(CurrentAccountMovementDto.Credit), 120);
            SetColumnWidth(nameof(CurrentAccountMovementDto.Balance), 120);
            SetColumnWidth(nameof(CurrentAccountMovementDto.Description), 260);
        }

        private void SetColumnWidth(string fieldName, int width)
        {
            if (View.Columns[fieldName] is not { } col)
            {
                return;
            }

            col.Width = width;
            col.MinWidth = width;
        }

        protected override CurrentAccountMovementGetAllQuery BuildListQuery()
            => new(AccountType: _accountType, CustomerId: _customerId, SupplierId: _supplierId, OnlyDeleted: ShowDeleted);

        /// <summary>En yeni cari hareket üstte, ilk hareket altta.</summary>
        protected override IEnumerable<CurrentAccountMovementDto> ApplyDefaultOrder(IEnumerable<CurrentAccountMovementDto> items)
            => Utils.ListOrder.NewestDocumentFirst(items, x => x.Date, x => x.CreatedAt, x => x.Id);

        protected override string GetDeleteSummary(CurrentAccountMovementDto item)
            => $"{item.CurrentAccountName} ({item.MovementTypeName} - {(item.Debit > 0 ? item.Debit : item.Credit):n2} ₺)";

        protected override bool SupportsRestore => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(CurrentAccountMovementDto item)
            => new CurrentAccountMovementRestoreCommand(item.Id);
    }
}
