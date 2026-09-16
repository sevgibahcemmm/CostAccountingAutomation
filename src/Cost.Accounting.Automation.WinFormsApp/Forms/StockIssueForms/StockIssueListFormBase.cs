using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.StockIssues;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public abstract class StockIssueListFormBase<TEditForm> : CrudListFormBase<StockIssueGetAllQuery, StockIssueListDto, TEditForm>
        where TEditForm : XtraForm
    {
        private readonly StockIssueType _issueType;

        protected StockIssueListFormBase(StockIssueType issueType, string formTitle) : base(formTitle)
        {
            _issueType = issueType;
        }

        protected override SvgImage ModuleIcon => DxIcon.StockIssue;

        protected override string[] SearchFieldNames =>
        [
            nameof(StockIssueListDto.DocumentNumber),
            nameof(StockIssueListDto.SourceWarehouseName),
            nameof(StockIssueListDto.TargetAccountCode),
            nameof(StockIssueListDto.TargetAccountName),
            nameof(StockIssueListDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            AddColumnsFromAttributes();
            View.OptionsView.ColumnAutoWidth = false;
        }

        protected override StockIssueGetAllQuery BuildListQuery()
            => new(IssueType: _issueType, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(StockIssueListDto item)
            => new StockIssueDeleteCommand(item.Id);

        protected override string GetDeleteSummary(StockIssueListDto item)
            => $"{item.DocumentNumber} - {item.TargetAccountName}";

        protected override bool SupportsRestore => false;
    }
}
