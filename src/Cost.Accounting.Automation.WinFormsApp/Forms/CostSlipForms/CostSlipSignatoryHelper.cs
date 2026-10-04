using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms
{
    /// <summary>
    /// Maliyet pusulasının altındaki üç imza kutusunu personel kayıtlarından
    /// doldurur: işyurdu müdürü, atölye şefi ve taşınır kayıt yetkilisi.
    ///
    /// <para>
    /// Yetkiler <c>EmployeeSigningRoles</c> tablosundaki görev tanımlarıdır;
    /// burada yalnızca adıyla aranır. Görev tanımı silinmiş veya yeniden
    /// adlandırılmışsa o kutu boş kalır ve olay günlüğüne yazılır —
    /// belgenin imza bölümü bozulmaz.
    /// </para>
    ///
    /// <para>
    /// Atölye şefi maliyet pusulasının atölyesine özeldir: önce o atölyeye
    /// bağlı görev, bulunamazsa kurum genelindeki kayıt kullanılır. Bu
    /// çözümleme <see cref="ReportSignatoryQuery"/> içinde yapılır.
    /// </para>
    ///
    /// <para>
    /// Değerler rapor parametresi olarak yazılır; şablonlardaki değer
    /// etiketleri bu parametrelere bağlıdır. Bu yüzden iki rapor şablonu
    /// (ürün ve hizmet) aynı parametre adlarını taşımak zorundadır.
    /// </para>
    /// </summary>
    internal static class CostSlipSignatoryHelper
    {
        private const string LogCategory = "CostSlipReport.Signatory";

        private const string PlantManagerRoleName = "İşyurdu Müdürü";
        public const string WorkshopChiefRoleName = "Atölye Şefi";
        private const string RegistryOfficerRoleName = "Taşınır Kayıt Yetkilisi";
        public const string AccountingOfficerRoleName = "Muhasebe Yetkilisi";
        public const string AccountingClerkRoleName = "Muhasebe Memuru";

        public static async Task<IReadOnlyDictionary<Guid, string>> ResolveWorkshopNamesAsync(
            IReadOnlyCollection<Guid> workshopIds,
            string roleName)
        {
            var names = new Dictionary<Guid, string>();

            foreach (Guid workshopId in workshopIds)
            {
                string name = await ResolveNameAsync(workshopId, roleName);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names[workshopId] = name;
                }
            }

            return names;
        }

        /// <summary>Kurum geneli iki muhasebe imza adını tek turda çözer.</summary>
        public static async Task<IReadOnlyDictionary<string, string>> ResolveAccountingSignatoriesAsync()
        {
            var names = new Dictionary<string, string>
            {
                [AccountingOfficerRoleName] = await ResolveNameAsync(null, AccountingOfficerRoleName),
                [AccountingClerkRoleName] = await ResolveNameAsync(null, AccountingClerkRoleName)
            };

            return names;
        }

        /// <summary>
        /// Çözülemeyen görevleri tek metinde birleştirir. Rapor imza kutusu
        /// sessizce boş kaldığında kullanıcı nedenini göremez; bu liste
        /// uyarı metninde kullanılır.
        /// </summary>
        public static string DescribeMissing(params (string Role, string Name)[] resolved)
            => string.Join(", ", resolved
                .Where(r => string.IsNullOrWhiteSpace(r.Name))
                .Select(r => r.Role));

        public static async Task<string> ResolveNameAsync(Guid? workshopId, string roleName)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                List<EmployeeSigningRoleOption> roles =
                    (await mediator.Send(new EmployeeSigningRoleLookUpQuery(), CancellationToken.None))
                    .ToList();

                EmployeeSigningRoleOption? role = FindRole(roles, roleName);
                if (role is null)
                {
                    return string.Empty;
                }

                var signatories = await mediator.Send(
                    new ReportSignatoryQuery(
                        WorkshopId: workshopId,
                        Slots: [new SignatorySlot(role.Id, roleName)]),
                    CancellationToken.None);

                if (!signatories.IsSuccessful
                    || signatories.Data is null
                    || !signatories.Data.TryGetValue(role.Id, out ReportSignatory? signatory))
                {
                    CrashLog.Write(
                        LogCategory,
                        $"'{roleName}' görevini taşıyan aktif personel bulunamadı.");
                    return string.Empty;
                }

                return signatory.FullName;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException(LogCategory, ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Raporun imza parametrelerini doldurur. Hata durumunda rapor yine de
        /// basılır; yalnızca imza satırları boş kalır.
        /// </summary>
        /// <param name="report">Maliyet pusulası raporu (ürün veya hizmet).</param>
        /// <param name="workshopId">
        /// Pusulanın atölyesi. Atölye seçilmemişse atölye şefi kutusu boş kalır;
        /// yanlış şef basmaktansa boş basmak yeğdir.
        /// </param>
        public static async Task ApplySignatoriesAsync(XtraReport report, Guid? workshopId)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                List<EmployeeSigningRoleOption> roles =
                    (await mediator.Send(new EmployeeSigningRoleLookUpQuery(), CancellationToken.None))
                    .ToList();

                EmployeeSigningRoleOption? manager = FindRole(roles, PlantManagerRoleName);
                EmployeeSigningRoleOption? chief = FindRole(roles, WorkshopChiefRoleName);
                EmployeeSigningRoleOption? registry = FindRole(roles, RegistryOfficerRoleName);

                List<SignatorySlot> slots = [];

                if (manager is not null)
                {
                    slots.Add(new SignatorySlot(manager.Id, PlantManagerRoleName));
                }

                if (chief is not null)
                {
                    slots.Add(new SignatorySlot(chief.Id, WorkshopChiefRoleName));
                }

                if (registry is not null)
                {
                    slots.Add(new SignatorySlot(registry.Id, RegistryOfficerRoleName));
                }

                if (slots.Count == 0)
                {
                    return;
                }

                var signatories = await mediator.Send(
                    new ReportSignatoryQuery(WorkshopId: workshopId, Slots: slots),
                    CancellationToken.None);

                if (!signatories.IsSuccessful || signatories.Data is null)
                {
                    return;
                }

                if (manager is not null)
                {
                    ApplySignatory(
                        report, signatories.Data, manager.Id, PlantManagerRoleName,
                        "MudurAdi", "MudurUnvani");
                }

                if (chief is not null)
                {
                    ApplySignatory(
                        report, signatories.Data, chief.Id, WorkshopChiefRoleName,
                        "SefAdi", "SefUnvani");
                }

                if (registry is not null)
                {
                    ApplySignatory(
                        report, signatories.Data, registry.Id, RegistryOfficerRoleName,
                        "KayitYetkilisiAdi", "KayitYetkilisiUnvani");
                }
            }
            catch (Exception ex)
            {
                // İmza bilgisi belgeyi bozmaz; yalnızca alanlar boş kalır.
                CrashLog.WriteException(LogCategory, ex);
            }
        }

        /// <summary>
        /// Bir kutunun ad ve ünvan satırlarını rapor parametrelerine yazar.
        /// Yetkili bulunamazsa kutu boş bırakılır ve günlüğe yazılır; geçmiş
        /// belgelerde imzası basılan kişi başka biriyle değiştirilmez.
        /// </summary>
        private static void ApplySignatory(
            XtraReport report,
            IReadOnlyDictionary<Guid, ReportSignatory> signatories,
            Guid roleId,
            string roleName,
            string nameParameter,
            string titleParameter)
        {
            if (!signatories.TryGetValue(roleId, out ReportSignatory? signatory))
            {
                CrashLog.Write(
                    LogCategory,
                    $"'{roleName}' görevini taşıyan aktif personel bulunamadı.");
                return;
            }

            SetParameter(report, nameParameter, signatory.FullName);
            SetParameter(report, titleParameter, signatory.Title);
        }

        private static void SetParameter(XtraReport report, string name, string? value)
        {
            if (report.Parameters[name] is { } parameter)
            {
                parameter.Value = value ?? string.Empty;
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
                CrashLog.Write(LogCategory, $"Görev tanımı bulunamadı: {roleName}");
            }

            return role;
        }
    }
}