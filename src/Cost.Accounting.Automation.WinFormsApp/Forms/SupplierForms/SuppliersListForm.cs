using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms
{
    public sealed partial class SuppliersListForm : CrudListFormBase<SupplierGetAllQuery, SupplierDto, SupplierEditForm>
    {
        public SuppliersListForm() : base("Tedarikçiler")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Suppliers;

        protected override string[] SearchFieldNames =>
        [
            nameof(SupplierDto.Name),
            nameof(SupplierDto.TaxOffice),
            nameof(SupplierDto.TaxNumber),
            nameof(SupplierDto.City),
            nameof(SupplierDto.District),
            nameof(SupplierDto.PhoneNumber1),
            nameof(SupplierDto.Email)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(SupplierDto item)
            => new SupplierDeleteCommand(item.Id);

        protected override async Task<List<SupplierDto>> GetUndeletableAsync(
            List<SupplierDto> selected, CancellationToken cancellationToken)
        {
            List<SupplierDto> blocked = [];
            using var scope = Program.Services.CreateScope();
            ICurrentAccountMovementRepository movements = scope.ServiceProvider.GetRequiredService<ICurrentAccountMovementRepository>();
            foreach (SupplierDto item in selected)
            {
                if (await movements.AnyAsync(m => m.SupplierId == new IdentityId(item.Id), cancellationToken))
                {
                    blocked.Add(item);
                }
            }
            return blocked;
        }

        protected override string GetDeleteSummary(SupplierDto item) => item.Name;

        protected override bool SupportsRestore => true;

        protected override SupplierGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(SupplierDto item)
            => new SupplierRestoreCommand(item.Id);
    }
}