using Cost.Accounting.Automation.Domain.StockIssues;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms
{
    public sealed class AtelierTransfersListForm : StockIssueListFormBase<AtelierTransferEditForm>
    {
        public AtelierTransfersListForm() : base(StockIssueType.AtelierTransfer, "Atölye Transferi")
        {
        }

        /// <summary>
        /// Atölye transferi için menüde ayrı bir "yeni belge" girdisi yoktur;
        /// kayıt yalnızca bu listeden oluşturulabildiği için "Yeni" düğmesi
        /// gösterilir.
        /// </summary>
        protected override bool AllowCreate => true;
    }
}
