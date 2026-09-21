using Cost.Accounting.Automation.Application.StockCounts;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.StockCountListReports
{
    public partial class StockCountListReport : DevExpress.XtraReports.UI.XtraReport
    {
        public StockCountListReport()
        {
            InitializeComponent();
        }

        public void SetData(
            StockCountGroupMode mode,
            List<StockCountReportRowDto> rows,
            string companyName = "")
        {
            DataSource = rows;

            Parameters["parameterCompanyName"].Value = companyName;
            Parameters["parameterTitleText"].Value = "STOK SAYIM LİSTESİ";
            Parameters["parameterSubTitleText"].Value = mode == StockCountGroupMode.Warehouse
                ? "( DEPO BAZINDA )"
                : "( ATÖLYE BAZINDA )";
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