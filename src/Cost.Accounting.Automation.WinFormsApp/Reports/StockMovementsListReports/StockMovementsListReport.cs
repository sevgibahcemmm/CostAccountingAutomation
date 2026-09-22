using Cost.Accounting.Automation.Application.StockMovements;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.StockMovementsListReports
{
    public partial class StockMovementsListReport : DevExpress.XtraReports.UI.XtraReport
    {
        public StockMovementsListReport()
        {
            InitializeComponent();

           
        }

        private void GroupHeader2_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string? subGroupCode = GetCurrentColumnValue("SubGroupCode") as string;
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            List<StockMovementReportRowDto> rows,
            string companyName = "")
        {
            
            List<StockMovementReportRowDto> sortedRows = rows
                .Select(r => new StockMovementReportRowDto
                {
                    AccountType = r.AccountType,
                    LocationCode = r.LocationCode?.Trim() ?? string.Empty,
                    LocationName = r.LocationName?.Trim() ?? string.Empty,
                    SubGroupCode = r.SubGroupCode?.Trim() ?? string.Empty,
                    SubGroupName = r.SubGroupName?.Trim() ?? string.Empty,
                    ProductName = r.ProductName,
                    ProductCode = r.ProductCode,
                    UnitTypeName = r.UnitTypeName,
                    TotalInQuantity = r.TotalInQuantity,
                    TotalOutQuantity = r.TotalOutQuantity,
                    BalanceQuantity = r.BalanceQuantity,
                    UnitCost = r.UnitCost,
                    TotalInAmount = r.TotalInAmount,
                    TotalOutAmount = r.TotalOutAmount,
                    BalanceAmount = r.BalanceAmount,
                    SalesQuantity = r.SalesQuantity,
                    SalesAmount = r.SalesAmount,
                })
                .OrderBy(r => r.LocationCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubGroupCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProductName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (IGrouping<string, StockMovementReportRowDto> group in sortedRows
                .GroupBy(r => r.LocationCode))
            {
                decimal totalInQuantity = group.Sum(r => r.TotalInQuantity);
                decimal totalOutQuantity = group.Sum(r => r.TotalOutQuantity);
                decimal balanceQuantity = group.Sum(r => r.BalanceQuantity);
                decimal totalInAmount = group.Sum(r => r.TotalInAmount);
                decimal totalOutAmount = group.Sum(r => r.TotalOutAmount);
                decimal balanceAmount = group.Sum(r => r.BalanceAmount);
                decimal salesQuantity = group.Sum(r => r.SalesQuantity);
                decimal salesAmount = group.Sum(r => r.SalesAmount);

                foreach (StockMovementReportRowDto row in group)
                {
                    row.GroupTotalInQuantity = totalInQuantity;
                    row.GroupTotalOutQuantity = totalOutQuantity;
                    row.GroupBalanceQuantity = balanceQuantity;
                    row.GroupAvgUnitCost = group.Average(r => r.UnitCost);
                    row.GroupTotalInAmount = totalInAmount;
                    row.GroupTotalOutAmount = totalOutAmount;
                    row.GroupBalanceAmount = balanceAmount;
                    row.GroupSalesQuantity = salesQuantity;
                    row.GroupSalesAmount = salesAmount;
                }
            }

            DataSource = sortedRows;

            Parameters["parameterTitleText"].Value = BuildTitle(startDate, endDate);
            Parameters["parameterCompanyName"].Value = companyName;
            Parameters["parameterEndDateText"].Value = endDate.ToString("dd.MM.yyyy");
            Parameters["parameterTotalInQuantity"].Value = rows.Sum(r => r.TotalInQuantity);
            Parameters["parameterTotalOutQuantity"].Value = rows.Sum(r => r.TotalOutQuantity);
            Parameters["parameterTotalQuantity"].Value = rows.Sum(r => r.BalanceQuantity);
            Parameters["parameterTotalInAmount"].Value = rows.Sum(r => r.TotalInAmount);
            Parameters["parameterTotalOutAmount"].Value = rows.Sum(r => r.TotalOutAmount);
            Parameters["parameterGrandTotal"].Value = rows.Sum(r => r.BalanceAmount);
            Parameters["parameterSalesQuantity"].Value = rows.Sum(r => r.SalesQuantity);
            Parameters["parameterSalesAmount"].Value = rows.Sum(r => r.SalesAmount);
        }

        private static string BuildTitle(DateOnly startDate, DateOnly endDate)
        {
            string period = startDate.Year == endDate.Year && startDate.Month == endDate.Month
                ? $"{startDate.Year} -{TurkishMonth(startDate.Month)}"
                : $"{startDate:MM.yyyy} - {endDate:MM.yyyy}";

            return $"STOK HAREKET LİSTESİ ( {period} )";
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