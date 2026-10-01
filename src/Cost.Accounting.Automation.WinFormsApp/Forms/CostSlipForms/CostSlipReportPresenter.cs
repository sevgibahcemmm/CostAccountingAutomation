using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using ReportItemDto = Cost.Accounting.Automation.WinFormsApp.Reports.CostSlipReport.CostSlipItemDto;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public static class CostSlipReportPresenter
    {
        public static async Task ShowAsync(CostSlipListDto slip)
        {
            bool isService = slip.CostSlipType == CostSlipType.Service;
            XtraReport report = isService
                ? new CostSlipServiceReport()
                : new CostSlipProductReport();
            report.RequestParameters = false;

            if (report is CostSlipProductReport productReport)
            {
                productReport.SlipTypeTitle = slip.CostSlipType == CostSlipType.SemiFinishedProduct
                    ? "YARI MAMÜL MALİYET PUSULASI"
                    : "MAMÜL MALİYET PUSULASI";
            }
            else if (report is CostSlipServiceReport serviceReport)
            {
                serviceReport.SlipTypeTitle = "HİZMET MALİYET PUSULASI";
            }
            foreach (DevExpress.XtraReports.Parameters.Parameter parameter in report.Parameters)
            {
                parameter.Visible = false;
            }

            report.DataSource = slip.CostSlipItems
                .Where(l => l.Quantity > 0 && l.ProductId is not null)
                .Select(l => new ReportItemDto
                {
                    ProductName = string.IsNullOrWhiteSpace(l.ProductName)
                        ? l.Description
                        : l.ProductName,
                    ProductUnitTypeName = l.ProductUnitTypeName,
                    UnitPrice = l.UnitPrice,
                    Quantity = l.Quantity,
                    TotalAmount = l.TotalAmount,
                    ExpenseAccountType = l.ExpenseAccountType
                })
                .ToList();

            Dictionary<string, decimal> totals = slip.CostSlipItems
                .GroupBy(i => GetReportParam(i.ExpenseAccountType))
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Sum(i => i.TotalAmount), 2),
                    StringComparer.Ordinal);

            CompanyDto company = await LoadCompanyAsync();

            SetReportParam(report, "MamulAdi", slip.ProducedProductName);
            SetReportParam(report, "Miktari", slip.Quantity);
            SetReportParam(report, "Tarih", slip.CostDate);
            SetReportParam(report, "Donem", slip.CostDate.ToString(
                "MM'. Ay - 'MMMM'-'yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
            SetReportParam(report, "Isyurdu", company.Name);
            SetReportParam(report, "Atolye", slip.WorkshopName);
            SetReportParam(report, "Antet", company.Name);

            SetReportParam(report, "CiltNo", slip.CostDate.Year);
            SetReportParam(report, "SeriNo", slip.SlipNumber);
            SetReportParam(report, "SiparisNo", slip.SlipNumber);

            string[] moneyParams = ["M710", "M720", "M730", "M740", "M750", "M760", "M770", "M780", "M151"];
            foreach (string param in moneyParams)
            {
                decimal value = totals.TryGetValue(param, out decimal sum) ? Math.Round(sum, 2) : 0;
                SetReportParam(report, param, value);
            }

            decimal grandTotal = slip.CostSlipItems.Sum(i => i.TotalAmount);
            SetReportParam(report, "Toplam", Math.Round(grandTotal, 2));

            // Yalnızca belge üretimi bekleme penceresinin kapsamında; önizleme
            // penceresi modal olduğu için bekleme kapandıktan sonra açılır.
            await LoadingHelper.RunAsync(
                () => report.CreateDocumentAsync(CancellationToken.None),
                caption: "Pusula hazırlanıyor...",
                description: "Lütfen bekleyin...");

            ReportPrintTool tool = new(report);
            tool.ShowRibbonPreviewDialog();
        }

        private static string GetReportParam(ExpenseAccountType account)
        {
            byte value = (byte)account;
            return value switch
            {
                1 => "M710",
                2 or 3 => "M720",
                >= 4 and <= 10 => "M730",
                >= 11 and <= 18 => "M740",
                19 => "M750",
                20 => "M760",
                21 => "M770",
                22 => "M780",

                // Yarı mamul (151) tüketimi kendi kovasına gider ve YALNIZCA
                // "Maliyet Bedeli" satırına eklenir; 730/740 kovalarına ve
                // ara toplamlara karışmaz. Kovaya karıştırılırsa (örn hep M730)
                // ya 730 satırında yanlış görünür ya da hizmet raporunda M730
                // parametresi olmadığı için hiç görünmez.
                (byte)ExpenseAccountHelper.SemiFinishedAccount => "M151",
                _ => string.Empty
            };
        }

        private static void SetReportParam(XtraReport report, string name, object value)
        {
            if (report.Parameters[name] is { } parameter)
            {
                parameter.Value = value;
            }
        }

        private static async Task<CompanyDto> LoadCompanyAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                SessionClaimContext session = Program.Services.GetRequiredService<SessionClaimContext>();

                var result = await mediator.Send(new CompanyGetQuery(session.GetCompanyId()), CancellationToken.None);
                if (result.IsSuccessful && result.Data is not null)
                {
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CostSlipReport.Company", ex);
            }

            return new CompanyDto();
        }
    }
}