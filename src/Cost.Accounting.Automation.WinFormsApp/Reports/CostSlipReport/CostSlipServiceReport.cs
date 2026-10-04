namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips
{
    public partial class CostSlipServiceReport : DevExpress.XtraReports.UI.XtraReport
    {
        public CostSlipServiceReport()
        {
            InitializeComponent();
        }

        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string SlipTypeTitle
        {
            get => xrLabel1.Text;
            set => xrLabel1.Text = value;
        }
    }
}
