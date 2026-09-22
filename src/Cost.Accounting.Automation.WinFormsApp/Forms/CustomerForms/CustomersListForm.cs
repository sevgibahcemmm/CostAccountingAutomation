using System.Linq.Expressions;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
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

        protected override Expression<Func<CurrentAccountMovement, bool>> BuildMovementFilter(CustomerDto item)
            => m => m.CustomerId == new IdentityId(item.Id);

        protected override IRequest<Result<string>> BuildDeleteCommand(CustomerDto item)
            => new CustomerDeleteCommand(item.Id);

        protected override string GetDeleteSummary(CustomerDto item) => item.Name;

        protected override CustomerGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildRestoreCommand(CustomerDto item)
            => new CustomerRestoreCommand(item.Id);
    }
}