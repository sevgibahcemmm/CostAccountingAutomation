using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Domain.CostSlips;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    public static class GiderDagitimReportPresenter
    {
        public static async Task ShowAsync(CostSlipListDto slip)
        {
            List<GiderDagitimRow> rows = slip.CostSlipItems
                .Where(l => l.Quantity > 0)
                .GroupBy(l => l.ExpenseAccountType)
                .OrderBy(g => g.Key)
                .Select((g, index) => new GiderDagitimRow
                {
                    SiraNo = index + 1,
                    HesapAdi = CostSlipDto.GetDisplayName(g.Key),
                    Tutar = Math.Round(g.Sum(l => l.TotalAmount), 2),
                    Oran = 0
                })
                .ToList();

            decimal grandTotal = rows.Sum(r => r.Tutar);
            decimal unitCost = slip.Quantity > 0 && grandTotal > 0
                ? Math.Round(grandTotal / slip.Quantity, 2)
                : 0;

            foreach (GiderDagitimRow row in rows)
            {
                row.Oran = grandTotal > 0 ? Math.Round(row.Tutar / grandTotal * 100, 2) : 0;
            }

            CompanyDto company = await LoadCompanyAsync();

            GiderDagitimReport report = new() { DataSource = rows };
            report.RequestParameters = false;

            SetReportParam(report, "PusulaNo", slip.SlipNumber);
            SetReportParam(report, "Tarih", slip.CostDate.ToString("dd.MM.yyyy"));
            SetReportParam(report, "Turu", slip.CostSlipType == CostSlipType.Service ? "Hizmet" : "Mamul");
            SetReportParam(report, "Isyurdu", company.Name);
            SetReportParam(report, "Atolye", slip.WorkshopName);
            SetReportParam(report, "MamulAdi", slip.ProducedProductName);
            SetReportParam(report, "Miktari", slip.Quantity.ToString("N0"));
            SetReportParam(report, "CiltNo", slip.CostDate.Year.ToString());
            SetReportParam(report, "SayfaNo", slip.SlipNumber);
            SetReportParam(report, "SiparisNo", slip.SlipNumber);
            SetReportParam(report, "GenelToplam", grandTotal.ToString("N2"));
            SetReportParam(report, "BirimMaliyet", unitCost.ToString("N2"));

            WaitForm waitForm = WaitFormHelper.Show<WaitForm>("Gider dağıtım tablosu hazırlanıyor...", "Lütfen bekleyin...");
            try
            {
                await report.CreateDocumentAsync(CancellationToken.None);
            }
            finally
            {
                waitForm.Close();
                waitForm.Dispose();
            }

            ReportPrintTool tool = new(report);
            tool.ShowRibbonPreviewDialog();
        }

        private static void SetReportParam(XtraReport report, string name, string value)
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
                CrashLog.WriteException("GiderDagitimReport.Company", ex);
            }

            return new CompanyDto();
        }
    }
}