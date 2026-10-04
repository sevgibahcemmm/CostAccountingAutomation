using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms
{
    public sealed partial class ConsumptionUnitsListForm : CrudListFormBase<ConsumptionUnitGetAllQuery, ConsumptionUnitDto, ConsumptionUnitEditForm>
    {
        public ConsumptionUnitsListForm() : base("Tüketim Birimleri")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.StockIssue;

        protected override string DeleteItemLabel => "tüketim birimi";

        protected override string[] SearchFieldNames =>
        [
            nameof(ConsumptionUnitDto.Code),
            nameof(ConsumptionUnitDto.Name)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
        }

        protected override ConsumptionUnitGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(ConsumptionUnitDto item)
            => new ConsumptionUnitDeleteCommand(item.Id);

        /// <summary>Seçili tüketim birimleri tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<ConsumptionUnitDto> items)
            => new BulkDeleteConsumptionUnitsCommand(items.Select(item => item.Id).ToList());

        /// <summary>
        /// Tüketim birimleri hesap planı kaydıdır; işlem denetimi hesap planı
        /// repository'sinin silme denetim metodu üzerinden TEK sorguda yapılır.
        /// (Eski yaklaşımda her birim için ayrı sorgu atılıyordu.)
        /// </summary>
        protected override async Task<List<ConsumptionUnitDto>> GetUndeletableAsync(
            List<ConsumptionUnitDto> selected, CancellationToken cancellationToken)
        {
            if (selected.Count == 0)
            {
                return [];
            }

            HashSet<Guid> ids = selected.Select(item => item.Id).ToHashSet();

            using var scope = Program.Services.CreateScope();
            IChartOfAccountRepository repository =
                scope.ServiceProvider.GetRequiredService<IChartOfAccountRepository>();

            DeletionCheck check = await repository.GetDeletionCheckAsync(ids, cancellationToken);
            if (check.MovementIds.Count == 0)
            {
                return [];
            }

            HashSet<Guid> blocked = check.MovementIds.ToHashSet();
            return selected.Where(item => blocked.Contains(item.Id)).ToList();
        }

        protected override string GetDeleteSummary(ConsumptionUnitDto item)
            => $"{item.Code} - {item.Name}";

        protected override bool SupportsRestore => false;
    }
}