using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.CostSlips;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public partial class ServiceCostAllocationTable : DevExpress.XtraReports.UI.XtraReport, ICostAllocationTableReport
    {
        public ServiceCostAllocationTable()
        {
            InitializeComponent();
            PrintingSystem.ShowMarginsWarning = false;
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            GiderDagilimReportResult result,
            string companyName = "",
            CostSlipType type = CostSlipType.Service)
        {
            List<ProductCostAllocationReportRow> rows = result.Rows
                .Select(ProductCostAllocationReportRow.FromGiderDagilimRow)
                .Where(r => r.ServiceTotal > 0)   // sadece bu aralıkta hareketi olan atölyeler
                .ToList();

            DataSource = rows;

            ApplyColumnHeaders();

            string title = type == CostSlipType.SemiFinishedService
                ? "YARI MAMÜL HİZMET MALİYET GİDER DAĞITIM TABLOSU"
                : "HİZMET MALİYET GİDER DAĞITIM TABLOSU";
            xrLabel2.Text = CostAllocationHeaderFormatter.SpacedTitle(title);

            Parameters["parameterPeriodText"].Value = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} Dönemi";
            Parameters["parameterEndDateText"].Value = endDate.ToString("dd.MM.yyyy");
            Parameters["parameterCompanyName"].Value = companyName;
            Parameters["parameterTotal740_1"].Value = rows.Sum(r => r.Account740_1);
            Parameters["parameterTotal740_2"].Value = rows.Sum(r => r.Account740_2);
            Parameters["parameterTotal740_3_01"].Value = rows.Sum(r => r.Account740_3_01);
            Parameters["parameterTotal740_3_02"].Value = rows.Sum(r => r.Account740_3_02);
            Parameters["parameterTotal740_4"].Value = rows.Sum(r => r.Account740_4);
            Parameters["parameterTotal740_5"].Value = rows.Sum(r => r.Account740_5);
            Parameters["parameterTotal740_6"].Value = rows.Sum(r => r.Account740_6);
            Parameters["parameterTotal740_7"].Value = rows.Sum(r => r.Account740_7);
            Parameters["parameterTotal750_780"].Value = rows.Sum(r => r.Account750_780);
            Parameters["parameterGrandTotal"].Value = rows.Sum(r => r.ServiceTotal);
        }

        private void ApplyColumnHeaders()
        {
            var headers = new (XRTableCell Cell, ExpenseAccountType Type)[]
            {
                (xrTableCell1, ExpenseAccountType.Account740_1),
                (xrTableCell5, ExpenseAccountType.Account740_2),
                (xrTableCell2, ExpenseAccountType.Account740_3_01),
                (xrTableCell6, ExpenseAccountType.Account740_3_02),
                (xrTableCell7, ExpenseAccountType.Account740_4),
                (xrTableCell8, ExpenseAccountType.Account740_5),
                (xrTableCell9, ExpenseAccountType.Account740_6),
                (xrTableCell10, ExpenseAccountType.Account740_7),
            };

            foreach ((XRTableCell cell, ExpenseAccountType type) in headers)
            {
                cell.Text = CostAllocationHeaderFormatter.BuildVertical(EnumDisplay.GetDisplayName(type));
                cell.Angle = 90;
                cell.WordWrap = false;
            }

            xrTableCell37.Text = CostAllocationHeaderFormatter.BuildVertical(string.Join("\n",
                EnumDisplay.GetDisplayName(ExpenseAccountType.Account750),
                EnumDisplay.GetDisplayName(ExpenseAccountType.Account760),
                EnumDisplay.GetDisplayName(ExpenseAccountType.Account770),
                EnumDisplay.GetDisplayName(ExpenseAccountType.Account780)));
            xrTableCell37.Angle = 90;
            xrTableCell37.WordWrap = false;
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