using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips
{
    internal static class MovableAssetTransactionSlipPresenter
    {
        /// <summary>
        /// Fişin imza bloğunda imza alacak görev tanımının adı.
        ///
        /// <para>
        /// Görev tanımları veritabanında tutulur ve kullanıcı tarafından
        /// düzenlenebilir; bu yüzden kimlik (GUID) sabitlenmez, ad üzerinden
        /// çözülür. Ad tanımı yoksa kurulum kaydına düşülür ve imza alanı boş
        /// bırakılır — belge yine de basılır, çünkü eksik yetkili belgesi
        /// hiçbasılmamış belgeden iyidir.
        /// </para>
        /// </summary>
        private const string SignatoryRoleName = "Taşınır Kayıt Yetkilisi";

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

        public static async Task<Dictionary<Guid, ProductCatalogDto>> LoadProductsByIdAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            List<ProductCatalogDto> products = await mediator.Send(new ProductCatalogGetAllQuery(), CancellationToken.None);
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

        /// <summary>Katalog projeksiyonu (<see cref="ProductCatalogDto"/>) ile aynı kodu çözer.</summary>
        public static string ResolveItemCode(ProductCatalogDto? product, string fallbackCode)
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

        /// <summary>
        /// İmza bloğundaki "Taşınır Kayıt ve Yetkilisi" satırını personel
        /// kayıtlarından doldurur: görev tanımı bulunur, o görevi taşıyan
        /// aktif personelin adı soyadı ve ünvanı yazılır.
        /// </summary>
        public static async Task ApplySignatoryAsync(MovableAssetTransactionSlipData data)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                List<EmployeeSigningRoleOption> roles =
                    (await mediator.Send(new EmployeeSigningRoleLookUpQuery(), CancellationToken.None))
                    .ToList();

                EmployeeSigningRoleOption? role = roles.FirstOrDefault(
                    r => string.Equals(r.Name, SignatoryRoleName, StringComparison.OrdinalIgnoreCase));

                if (role is null)
                {
                    CrashLog.Write(
                        "SlipReport.Signatory",
                        $"Görev tanımı bulunamadı: {SignatoryRoleName}");

                    return;
                }

                // Taşınır fişi bir atölyeye ait değildir; görev kurum geneli
                // tanımlandığı için atölye bilgisi verilmez.
                var signatories = await mediator.Send(
                    new ReportSignatoryQuery(
                        WorkshopId: null,
                        Slots: [new SignatorySlot(role.Id, "Taşınır Kayıt ve Yetkilisi")]),
                    CancellationToken.None);

                if (signatories.IsSuccessful
                    && signatories.Data is not null
                    && signatories.Data.TryGetValue(role.Id, out ReportSignatory? signatory))
                {
                    data.SignatoryFullName = signatory.FullName;
                    data.SignatoryTitle = signatory.Title;
                }
                else
                {
                    CrashLog.Write(
                        "SlipReport.Signatory",
                        $"'{role.Name}' görevini taşıyan aktif personel bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                // İmza bilgisi belgeyi bozmaz; yalnızca alan boş kalır.
                CrashLog.WriteException("SlipReport.Signatory", ex);
            }
        }

        public static async Task ShowAsync(MovableAssetTransactionSlipData data)
        {
            try
            {
                MovableAssetTransactionSlipReport report = new();

                // Yetkili çözümlemesi veritabanına gider ve belge üretimi
                // eşzamanlıdır; ikisi de bekleme penceresinin kapsamında olmalı.
                // Aksi hâlde ekran saniyelerce donup kullanıcı hiçbir geri
                // bildirim almaz.
                await LoadingHelper.RunAsync(
                    async () =>
                    {
                        await ApplySignatoryAsync(data);
                        report = new(data);
                        await Task.Run(report.CreateDocument);
                    },
                    caption: "Fiş hazırlanıyor...",
                    description: "Lütfen bekleyin...");

                using ReportPrintTool tool = new(report);
                tool.PreviewRibbonForm.PrintControl.UseDirectXPaint = DefaultBoolean.True;
                tool.ShowRibbonPreviewDialog();
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Taşınır işlem fişi açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }
    }
}
