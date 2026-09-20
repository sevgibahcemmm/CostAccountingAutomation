using Cost.Accounting.Automation.Application.CostSlips;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public partial class ProductCostAllocationTable : DevExpress.XtraReports.UI.XtraReport
    {
        public ProductCostAllocationTable()
        {
            InitializeComponent();
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            GiderDagilimReportResult result,
            string companyName = "")
        {
            List<ProductCostAllocationReportRow> rows = result.Rows
                .Select(ProductCostAllocationReportRow.FromGiderDagilimRow)
                .Where(r => r.Total > 0)   // sadece bu aralıkta hareketi olan atölyeler
                .ToList();

            DataSource = rows;

            Parameters["parameterPeriodText"].Value = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} Dönemi";
            Parameters["parameterEndDateText"].Value = endDate.ToString("dd.MM.yyyy");
            Parameters["parameterCompanyName"].Value = companyName;
            Parameters["parameterTotal710"].Value = rows.Sum(r => r.Account710);
            Parameters["parameterTotal720_1"].Value = rows.Sum(r => r.Account720_1);
            Parameters["parameterTotal720_2"].Value = rows.Sum(r => r.Account720_2);
            Parameters["parameterTotal730_01"].Value = rows.Sum(r => r.Account730_01);
            Parameters["parameterTotal730_02"].Value = rows.Sum(r => r.Account730_02);
            Parameters["parameterTotal730_03"].Value = rows.Sum(r => r.Account730_03);
            Parameters["parameterTotal730_04"].Value = rows.Sum(r => r.Account730_04);
            Parameters["parameterTotal730_05"].Value = rows.Sum(r => r.Account730_05);
            Parameters["parameterTotal730_06"].Value = rows.Sum(r => r.Account730_06);
            Parameters["parameterTotal730_07"].Value = rows.Sum(r => r.Account730_07);
            Parameters["parameterGrandTotal"].Value = rows.Sum(r => r.Total);
        }

        private void SetParameter(string name, Type type, object value)
        {
            Parameter? parameter = Parameters[name];
            if (parameter is null)
            {
                parameter = new Parameter { Name = name, Type = type, Visible = false };
                Parameters.Add(parameter);
            }
            parameter.Value = value;
        }

        public void PrintReport()
        {
            using (ReportPrintTool tool = new ReportPrintTool(this))
            {
                tool.ShowRibbonPreviewDialog();
            }
        }
    }
}