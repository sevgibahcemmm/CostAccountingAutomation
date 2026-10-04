using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms
{
    public sealed partial class SuppliersListForm : CrmPartnerListFormBase<SupplierGetAllQuery, SupplierDto, SupplierEditForm>
    {
        public SuppliersListForm() : base("Tedarikçiler")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Suppliers;

        protected override string DeleteItemLabel => "tedarikçi";

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

        /// <summary>
        /// Cari hareket denetimi tedarikçi repository'sinin silme denetim
        /// metodu üzerinden tek sorguda yapılır.
        /// </summary>
        protected override async Task<HashSet<Guid>> GetMovementBlockedIdsAsync(
            IReadOnlyCollection<Guid> partnerIds,
            CancellationToken cancellationToken)
        {
            using var scope = Program.Services.CreateScope();
            ISupplierRepository repository = scope.ServiceProvider.GetRequiredService<ISupplierRepository>();
            DeletionCheck check = await repository.GetDeletionCheckAsync(partnerIds, cancellationToken);
            return check.MovementIds.ToHashSet();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(SupplierDto item)
            => new SupplierDeleteCommand(item.Id);

        /// <summary>Seçili tedarikçiler tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<SupplierDto> items)
            => new BulkDeleteSuppliersCommand(items.Select(item => item.Id).ToList());

        protected override string GetDeleteSummary(SupplierDto item) => item.Name;

        protected override SupplierGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(SupplierDto item)
            => new SupplierRestoreCommand(item.Id);
    }
}