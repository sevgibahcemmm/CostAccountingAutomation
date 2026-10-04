using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CustomerForms
{
    public sealed partial class CustomersListForm : CrmPartnerListFormBase<CustomerGetAllQuery, CustomerDto, CustomerEditForm>
    {
        public CustomersListForm() : base("Müşteriler")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Customers;

        protected override string DeleteItemLabel => "müşteri";

        protected override string[] SearchFieldNames =>
        [
            nameof(CustomerDto.Name),
            nameof(CustomerDto.TaxOffice),
            nameof(CustomerDto.TaxNumber),
            nameof(CustomerDto.City),
            nameof(CustomerDto.District),
            nameof(CustomerDto.PhoneNumber1),
            nameof(CustomerDto.Email)
        ];

        /// <summary>
        /// Cari hareket denetimi müşteri repository'sinin silme denetim
        /// metodu üzerinden tek sorguda yapılır.
        /// </summary>
        protected override async Task<HashSet<Guid>> GetMovementBlockedIdsAsync(
            IReadOnlyCollection<Guid> partnerIds,
            CancellationToken cancellationToken)
        {
            using var scope = Program.Services.CreateScope();
            ICustomerRepository repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            DeletionCheck check = await repository.GetDeletionCheckAsync(partnerIds, cancellationToken);
            return check.MovementIds.ToHashSet();
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(CustomerDto item)
            => new CustomerDeleteCommand(item.Id);

        /// <summary>Seçili müşteriler tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<CustomerDto> items)
            => new BulkDeleteCustomersCommand(items.Select(item => item.Id).ToList());

        protected override string GetDeleteSummary(CustomerDto item) => item.Name;

        protected override CustomerGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(CustomerDto item)
            => new CustomerRestoreCommand(item.Id);
    }
}