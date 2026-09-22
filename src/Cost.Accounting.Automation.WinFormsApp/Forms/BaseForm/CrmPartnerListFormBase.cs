using System.Linq.Expressions;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CurrentAccounts;
using DevExpress.XtraEditors;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    public abstract partial class CrmPartnerListFormBase<TListQuery, TDto, TEditForm>
        : CrudListFormBase<TListQuery, TDto, TEditForm>
        where TListQuery : class, IRequest<IQueryable<TDto>>, new()
        where TDto : EntityDto
        where TEditForm : XtraForm
    {
        protected CrmPartnerListFormBase(string formTitle) : base(formTitle)
        {
        }

        protected abstract Expression<Func<CurrentAccountMovement, bool>> BuildMovementFilter(TDto item);

        protected override void ConfigureColumns() => AddColumnsFromAttributes();

        protected override bool SupportsRestore => true;

        protected override async Task<List<TDto>> GetUndeletableAsync(
            List<TDto> selected, CancellationToken cancellationToken)
        {
            List<TDto> blocked = [];
            using var scope = Program.Services.CreateScope();
            ICurrentAccountMovementRepository movements = scope.ServiceProvider.GetRequiredService<ICurrentAccountMovementRepository>();
            foreach (TDto item in selected)
            {
                if (await movements.AnyAsync(BuildMovementFilter(item), cancellationToken))
                {
                    blocked.Add(item);
                }
            }
            return blocked;
        }
    }
}