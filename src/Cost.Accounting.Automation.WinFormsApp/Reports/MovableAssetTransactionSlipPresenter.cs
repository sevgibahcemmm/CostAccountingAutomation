using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Reports
{
    internal static class MovableAssetTransactionSlipPresenter
    {
        public static async Task<CompanyDto> LoadCompanyAsync()
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
                CrashLog.WriteException("SlipReport.Company", ex);
            }

            return new CompanyDto();
        }

        public static async Task<List<ChartOfAccountLookUpDto>> LoadAccountsAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            return (await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [];
        }

        public static async Task<Dictionary<Guid, ProductDto>> LoadProductsByIdAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            List<ProductDto> products = (await mediator.Send(new ProductGetAllQuery(), CancellationToken.None)).ToList();
            return products.ToDictionary(p => p.Id);
        }

        public static Dictionary<string, string> BuildAccountNameMap(IEnumerable<ChartOfAccountLookUpDto> accounts)
        {
            Dictionary<string, string> map = new(StringComparer.Ordinal);
            foreach (ChartOfAccountLookUpDto account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.Code))
                {
                    map[account.Code] = account.Name;
                }
            }

            return map;
        }

        /// <summary>
        /// TIF kaleminde gösterilecek ve gruplanacak hesap kodunu belirler.
        /// Öncelik sırası: ürün kategorisi (4. düzey), hesap planı kodu, ürün kodu.
        /// </summary>
        public static string ResolveItemCode(ProductDto? product, string fallbackCode)
        {
            if (product is not null)
            {
                if (!string.IsNullOrWhiteSpace(product.CategoryCode))
                {
                    return product.CategoryCode;
                }

                if (!string.IsNullOrWhiteSpace(product.ChartOfAccountCode))
                {
                    return product.ChartOfAccountCode;
                }

                if (!string.IsNullOrWhiteSpace(product.ProductCode))
                {
                    return product.ProductCode;
                }
            }

            return fallbackCode;
        }

        public static async Task ShowAsync(MovableAssetTransactionSlipData data)
        {
            WaitForm? waitForm = null;
            try
            {
                MovableAssetTransactionSlipReport report = new(data);

                waitForm = WaitFormHelper.Show<WaitForm>("Rapor hazırlanıyor...", "Lütfen bekleyin...");
                await report.CreateDocumentAsync(CancellationToken.None);

                waitForm.Close();
                waitForm.Dispose();
                waitForm = null;

                using ReportPrintTool tool = new(report);
                tool.PreviewRibbonForm.PrintControl.UseDirectXPaint = DefaultBoolean.True;
                tool.ShowRibbonPreviewDialog();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Taşınır işlem fişi açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
            finally
            {
                waitForm?.Close();
                waitForm?.Dispose();
            }
        }
    }
}
