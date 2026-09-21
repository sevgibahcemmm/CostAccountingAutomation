using Cost.Accounting.Automation.Application.StockMovements;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.StockMovementsListReports
{
    public partial class StockMovementsListReport : DevExpress.XtraReports.UI.XtraReport
    {
        public StockMovementsListReport()
        {
            InitializeComponent();
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            List<StockMovementReportRowDto> rows,
            string companyName = "")
        {
            DataSource = rows;

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