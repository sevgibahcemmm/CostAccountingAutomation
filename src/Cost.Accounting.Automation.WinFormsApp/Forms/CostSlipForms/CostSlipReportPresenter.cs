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

            SetReportParam(report, "ProductName", slip.ProducedProductName);
            SetReportParam(report, "Quantity", slip.Quantity);
            SetReportParam(report, "Tarih", slip.CostDate);
            SetReportParam(report, "Donem", slip.CostDate.ToString(
                "MM'. Ay - 'MMMM'-'yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
            SetReportParam(report, "Isyurdu", company.Name);
            SetReportParam(report, "Workshop", slip.WorkshopName);
            SetReportParam(report, "Antet", company.Letterhead);

            SetReportParam(report, "CiltNo", slip.CostDate.Year);
            SetReportParam(report, "SerialNo", slip.SlipNumber);
            SetReportParam(report, "SiparisNo", slip.SlipNumber);

            string[] moneyParams = ["M710", "M720", "M730", "M740", "M750", "M760", "M770", "M780", "M151"];
            foreach (string param in moneyParams)
            {
                decimal value = totals.TryGetValue(param, out decimal sum) ? Math.Round(sum, 2) : 0;
                SetReportParam(report, param, value);
            }

            decimal grandTotal = slip.CostSlipItems.Sum(i => i.TotalAmount);
            SetReportParam(report, "Toplam", Math.Round(grandTotal, 2));

            // Alt bilgi cümlesindeki birim maliyet ifade içinde hesaplanırsa
            // miktar sıfırken bölme hatası boş metne yol açar; burada
            // biçimlendirilmiş metin parametre olarak verilir.
            SetReportParam(report, "BirimFiyat", FormatUnitPrice(slip.Quantity, grandTotal));

            // İmza kutuları (işyurdu müdürü, atölye şefi, taşınır kayıt
            // yetkilisi) personel görev kayıtlarından çözümlenir. Atölye şefi
            // bu pusulanın atölyesine özeldir.
            await CostSlipSignatoryHelper.ApplySignatoriesAsync(
                report,
                slip.WorkshopId == Guid.Empty ? null : slip.WorkshopId);

            // Belge üretimi arka plana alınır; bekleme penceresi yalnızca
            // üretim sırasında görünür, önizleme modal olduğu için sonra açılır.
            await ReportPreviewHelper.PrintAsync(
                report,
                caption: "Pusula hazırlanıyor...",
                description: "Lütfen bekleyin...");
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

        private static string FormatUnitPrice(int quantity, decimal total)
        {
            return quantity <= 0
                ? string.Empty
                : (total / quantity).ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
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