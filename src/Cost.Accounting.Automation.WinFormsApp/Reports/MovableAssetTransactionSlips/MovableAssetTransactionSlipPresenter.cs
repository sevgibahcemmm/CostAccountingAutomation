using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.Reports;
using Cost.Accounting.Automation.WinFormsApp.Tools;
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

        /// <summary>
        /// Atölye transferinde teslim alan kutusunu dolduran görev.
        /// Bu görev atölyeye bağlı tanımlandığı için çözümleme
        /// <see cref="MovableAssetTransactionSlipData.RecipientWorkshopId"/>
        /// üzerinden yapılır.
        /// </summary>
        private const string WorkshopChiefRoleName = "Atölye Şefi";

        /// <summary>
        /// Tüketim fişinde teslim alan kutusunu dolduran görev: malzeme
        /// tüketim biriminden çıkıp muhasebe birimine teslim edildiği için
        /// teslimi muhasebe memuru alır — muhasebe yetkilisi değil.
        /// </summary>
        private const string AccountingClerkRoleName = "Muhasebe Memuru";

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
        /// İmza bloklarının yetkililerini personel kayıtlarından doldurur.
        ///
        /// <para>
        /// Çıkış tarafı her zaman "Taşınır Kayıt Yetkilisinin"dir: giriş
        /// fişinde kayıt (giriş kutusu), tüketim ve atölye transferinde ise
        /// çıkış kaydı ve teslim eden kutuları bu kişidir — taşınırcı
        /// malzemeyi o imzalayarak teslim eder.
        /// </para>
        ///
        /// <para>
        /// Teslim alan kutusu fişin türüne göre değişir: tüketimde malzeme
        /// muhasebe birimine teslim edildiği için teslimi oradaki
        /// <b>muhasebe memuru</b> alır, atölye transferinde ise taşınırı
        /// alan atölyenin şefi imzalar. Atölye transferinde atölye
        /// bilinmiyorsa bu satırlar boş kalır — yanlış şef basmaktansa boş
        /// kalmak yeğdir.
        /// </para>
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

                EmployeeSigningRoleOption? role = FindRole(roles, SignatoryRoleName);

                if (role is null)
                {
                    return;
                }

                // Teslim alanın görevi ve gerekiyorsa atölye bağlamı
                string? recipientRoleName = data.Kind switch
                {
                    MovableAssetTransactionSlipKind.AtelierTransfer => WorkshopChiefRoleName,
                    MovableAssetTransactionSlipKind.Exit => AccountingClerkRoleName,
                    _ => null
                };

                Guid? workshopId = data.Kind == MovableAssetTransactionSlipKind.AtelierTransfer
                    ? data.RecipientWorkshopId
                    : null;

                EmployeeSigningRoleOption? recipientRole = recipientRoleName is null
                    ? null
                    : FindRole(roles, recipientRoleName);

                List<SignatorySlot> slots = [new SignatorySlot(role.Id, "Taşınır Kayıt ve Yetkilisi")];

                if (recipientRole is not null)
                {
                    slots.Add(new SignatorySlot(recipientRole.Id, recipientRoleName!));
                }

                // Atölye transferinde teslim alan kutusu için atölyeye özgü
                // yetkili aranır; kayıt yetkilisi görevi kurum geneli
                // tanımlandığı için bu aramada yine bulunur.
                var signatories = await mediator.Send(
                    new ReportSignatoryQuery(WorkshopId: workshopId, Slots: slots),
                    CancellationToken.None);

                if (signatories.IsSuccessful && signatories.Data is not null)
                {
                    if (signatories.Data.TryGetValue(role.Id, out ReportSignatory? signatory))
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

                    if (recipientRole is not null
                        && signatories.Data.TryGetValue(recipientRole.Id, out ReportSignatory? recipient))
                    {
                        data.RecipientSignatoryFullName = recipient.FullName;
                        data.RecipientSignatoryTitle = recipient.Title;
                    }
                }
            }
            catch (Exception ex)
            {
                // İmza bilgisi belgeyi bozmaz; yalnızca alan boş kalır.
                CrashLog.WriteException("SlipReport.Signatory", ex);
            }
        }

        private static EmployeeSigningRoleOption? FindRole(
            List<EmployeeSigningRoleOption> roles,
            string roleName)
        {
            EmployeeSigningRoleOption? role = roles.FirstOrDefault(
                r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));

            if (role is null)
            {
                CrashLog.Write(
                    "SlipReport.Signatory",
                    $"Görev tanımı bulunamadı: {roleName}");
            }

            return role;
        }

        public static async Task ShowAsync(MovableAssetTransactionSlipData data)
        {
            try
            {
                // Yetkili çözümlemesi veritabanına gider ve belge üretimi
                // eşzamanlıdır; ikisi de bekleme penceresinin kapsamında olmalı.
                // Aksi hâlde ekran saniyelerce donup kullanıcı hiçbir geri
                // bildirim almaz.
                await LoadingHelper.RunAsync(
                    () => ApplySignatoryAsync(data),
                    caption: "Fiş hazırlanıyor...",
                    description: "Lütfen bekleyin...");

                // Belge üretimi ve önizleme ortak yardımcıya taşındı: DevExpress
                // sahip verilmediğinde Form.ActiveForm'u seçiyor ve TopMost bir
                // ToastForm önizlemeyi sahiplenip kendiliğinden kapatıyordu.
                await ReportPreviewHelper.PrintAsync(
                    new MovableAssetTransactionSlipReport(data),
                    caption: "Fiş hazırlanıyor...",
                    description: "Lütfen bekleyin...");
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Taşınır işlem fişi açılamadı: " + ex.Message, ToastType.Error, 6000);
            }
        }
    }
}
