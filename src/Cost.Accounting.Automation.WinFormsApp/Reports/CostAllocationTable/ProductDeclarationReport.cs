using System.ComponentModel;
using Cost.Accounting.Automation.Application.CostSlips;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public partial class ProductDeclarationReport : DevExpress.XtraReports.UI.XtraReport
    {
        private Dictionary<string, (decimal Quantity, decimal Total)> _workshopTotals = new();
        private string _currentWorkshop = "";

        public ProductDeclarationReport()
        {
            InitializeComponent();

            Detail.BeforePrint += Detail_BeforePrint;
            GroupFooter.BeforePrint += GroupFooter_BeforePrint;
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            List<ProductDeclarationRowDto> rows,
            string companyName = "")
        {
            DataSource = rows;

            _workshopTotals = rows
                .GroupBy(r => r.WorkshopName)
                .ToDictionary(
                    g => g.Key,
                    g => (Quantity: (decimal)g.Sum(r => r.Quantity), Total: g.Sum(r => r.Total)));

            string period = BuildPeriod(startDate, endDate);
            Parameters["parameterPeriodText"].Value = period;
            Parameters["parameterCompanyName"].Value = companyName;
            Parameters["parameterEndDateText"].Value = endDate.ToString("dd.MM.yyyy");
        }

        private void Detail_BeforePrint(object? sender, CancelEventArgs e)
        {
            if (GetCurrentRow() is ProductDeclarationRowDto row)
            {
                _currentWorkshop = row.WorkshopName;
            }
        }

        private void GroupFooter_BeforePrint(object? sender, CancelEventArgs e)
        {
            if (_workshopTotals.TryGetValue(_currentWorkshop, out var totals))
            {
                xrTableCell12.Text = totals.Quantity.ToString("N0");
                xrTableCell14.Text = totals.Total.ToString("N2");
            }
        }

        private static string BuildPeriod(DateOnly startDate, DateOnly endDate)
        {
            return startDate.Year == endDate.Year && startDate.Month == endDate.Month
                ? $"{startDate.Year} -{TurkishMonth(startDate.Month)}"
                : $"{startDate:MM.yyyy} - {endDate:MM.yyyy}";
        }

        private static string TurkishMonth(int month)
        {
            return month switch
            {
                1 => "OCAK",
                2 => "ŞUBAT",
                3 => "MART",
                4 => "NİSAN",
                5 => "MAYIS",
                6 => "HAZİRAN",
                7 => "TEMMUZ",
                8 => "AĞUSTOS",
                9 => "EYLÜL",
                10 => "EKİM",
                11 => "KASIM",
                _ => "ARALIK",
            };
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