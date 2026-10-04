using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.Helpers;
using Cost.Accounting.Automation.Domain.CostSlips;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.CostAllocationTable
{
    public partial class ProductCostAllocationTableReport : DevExpress.XtraReports.UI.XtraReport, ICostAllocationTableReport
    {
        public ProductCostAllocationTableReport()
        {
            InitializeComponent();
            PrintingSystem.ShowMarginsWarning = false;
        }

        public void SetData(
            DateOnly startDate,
            DateOnly endDate,
            ExpenseDistributionReportResult result,
            string companyName = "",
            CostSlipType type = CostSlipType.Product)
        {
            List<ProductCostAllocationReportRow> rows = result.Rows
                .Select(ProductCostAllocationReportRow.FromExpenseDistributionRow)
                .Where(r => r.Total > 0)   // sadece bu aralıkta hareketi olan atölyeler
                .ToList();

            DataSource = rows;

            ApplyColumnHeaders();

            string title = type == CostSlipType.SemiFinishedProduct
                ? "YARI MAMÜL MALİYET GİDER DAĞITIM TABLOSU"
                : "MAMÜL MALİYET GİDER DAĞITIM TABLOSU";
            xrLabel2.Text = CostAllocationHeaderFormatter.SpacedTitle(title);

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
            Parameters["parameterTotal750_780"].Value = rows.Sum(r => r.Account750_780);
            Parameters["parameterGrandTotal"].Value = rows.Sum(r => r.Total);
        }

        public void SetSignatoryNames(string accountingOfficer, string accountingClerk)
        {
            SetParameter("MuhasebeYetkilisiAdi", accountingOfficer);
            SetParameter("MuhasebeMemuruAdi", accountingClerk);
        }

        private void SetParameter(string name, string value)
        {
            if (Parameters[name] is { } parameter)
            {
                parameter.Value = value;
            }
        }

        private void ApplyColumnHeaders()
        {
            var headers = new (XRTableCell Cell, ExpenseAccountType Type)[]
            {
                (xrTableCell1, ExpenseAccountType.Account710),
                (xrTableCell5, ExpenseAccountType.Account720_1),
                (xrTableCell2, ExpenseAccountType.Account720_2),
                (xrTableCell6, ExpenseAccountType.Account730_01),
                (xrTableCell7, ExpenseAccountType.Account730_02),
                (xrTableCell8, ExpenseAccountType.Account730_03),
                (xrTableCell9, ExpenseAccountType.Account730_04),
                (xrTableCell10, ExpenseAccountType.Account730_05),
                (xrTableCell3, ExpenseAccountType.Account730_06),
                (xrTableCell12, ExpenseAccountType.Account730_07),
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