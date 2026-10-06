using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Companies;
using Cost.Accounting.Automation.Application.CostSlips;
using Cost.Accounting.Automation.Application.CurrentAccountMovements;
using Cost.Accounting.Automation.Application.Customers;
using Cost.Accounting.Automation.Application.Employees;
using Cost.Accounting.Automation.Application.Employees.SigningRoles;
using Cost.Accounting.Automation.Application.Invoices;
using Cost.Accounting.Automation.Application.ProductMovements;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Products.ProductUnitTypes;
using Cost.Accounting.Automation.Application.Products.TaxRates;
using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Application.Roles;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Application.Suppliers;
using Cost.Accounting.Automation.Application.Users;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Bir modülün CRUD akışı için gereken yetki anahtarları.
/// Değerler, Application katmanındaki komut/sorgu <c>[Permission]</c>
/// özniteliklerindeki anahtarlarla birebir eşleşmelidir.
/// </summary>
/// <param name="View">Listeyi görüntüleme yetkisi.</param>
/// <param name="Create">Yeni kayıt (oluşturma) yetkisi.</param>
/// <param name="Update">Kayıt düzenleme yetkisi.</param>
/// <param name="Delete">Kayıt silme yetkisi.</param>
/// <param name="Approve">Onay akışı olan modüllerde onaylama yetkisi; yoksa <c>null</c>.</param>
/// <param name="Restore">Silinen kaydı geri yükleme yetkisi; yoksa <c>null</c>.</param>
public readonly record struct FormPermissionSet(
    string View,
    string Create,
    string Update,
    string Delete,
    string? Approve = null,
    string? Restore = null);

/// <summary>
/// Arayüzün modül yetkilerinin tek kaynağı.
/// </summary>
/// <remarks>
/// <para>
/// Güvenlik burada değil, her zaman sunucu tarafındaki
    /// <c>PermissionBehavior</c> ile sağlanır. Bu katalog
    /// yalnızca arayüzün "sayfaya hiç girilmemeli" kararını verir: ribbon menü
/// öğesi tıklanınca ya da liste formunun araç çubuğu kurulurken yetki buradan
/// sorgulanır ve yetkisiz kullanıcıya yetki mesajı gösterilir, sayfa açılmaz.
/// </para>
/// <para>
/// Katalog, yetki anahtarlarını her formda elle tekrarlamak yerine iki
/// eşlemeyle tek noktadan yönetir: menü öğesi (elm*) → görüntüleme yetkisi ve
/// liste sorgu türü → CRUD yetkileri.
/// </para>
/// </remarks>
public static class ModulePermissionCatalog
{
    /// <summary>
    /// Ribbon menü öğesi (Designer'daki <c>elm*</c> adı) → görüntüleme yetkisi.
    /// Menüye eklenecek her sayfa bir satırla buraya tanımlanır; aksi hâlde o
    /// sayfa yetki denetiminden geçerek açılır.
    /// </summary>
    private static readonly Dictionary<string, string> MenuViewPermissions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Stok Yönetimi - tanımlar
            ["elmProducts"] = "product:view",
            ["elmUnitTypes"] = "product:view",
            ["elmTaxRates"] = "product:view",
            ["elmConsumptionUnits"] = "chartofaccount:view",

            // Stok İşlemleri
            ["elmStockInput"] = "stock_movement:view",
            ["elmStockOutput"] = "stock_movement:view",
            ["elmConsumption"] = "stock_issue:view",
            ["elmWorkshopTransfer"] = "stock_issue:view",

            // Faturalar
            ["elmInvoices"] = "invoice:view",
            ["elmInvoiceApproval"] = "invoice:view",

            // Cari Yönetimi
            ["elmCustomers"] = "customer:view",
            ["elmSuppliers"] = "supplier:view",
            ["elmCurrentAccountMovements"] = "current_account_movement:view",
            ["elmPayments"] = "current_account_movement:view",
            ["elmCurrentAccountBalance"] = "current_account_movement:view",

            // Maliyet & Üretim
            ["elmCostSlips"] = "costslip:view",
            ["elmRecipes"] = "recipe:view",
            ["elmWorkshopAnalysis"] = "costslip:view",

            // Muhasebe
            ["elmChartOfAccounts"] = "chartofaccount:view",
            ["elmCarryForwardOperations"] = "devir:view",

            // Raporlar
            ["elmStockMovements"] = "stock_movement:view",
            ["elmPriceStockList"] = "product:view",
            ["elmWorkshopStockReport"] = "stock_issue:view",

            // Sistem Yönetimi
            ["elmCompanies"] = "company:view",
            ["elmUsers"] = "user:view",
            ["elmRoles"] = "role:view",
            ["elmEmployees"] = "employee:view",
            ["elmSigningRoles"] = "employee:view",

            // Mesajlaşma
            ["elmMessages"] = "message:view"
        };

    /// <summary>
    /// Liste sorgu türü → modülün CRUD yetkileri. Anahtar, listede hangi
    /// formun soruşturduğunu değil, veriyi çeken sorgu türünü temsil eder;
    /// böylece aynı sorguyu kullanan tüm liste formları tek satırla korunur.
    /// </summary>
    private static readonly Dictionary<Type, FormPermissionSet> ListQueryPermissions =
        new()
        {
            [typeof(UserGetAllQuery)] = new FormPermissionSet(
                "user:view", "user:create", "user:update", "user:delete",
                Restore: "user:delete"),
            [typeof(RoleGetAllQuery)] = new FormPermissionSet(
                "role:view", "role:create", "role:edit", "role:delete"),
            [typeof(EmployeeGetAllQuery)] = new FormPermissionSet(
                "employee:view", "employee:create", "employee:update", "employee:delete",
                Restore: "employee:delete"),
            [typeof(EmployeeSigningRoleGetAllQuery)] = new FormPermissionSet(
                "employee:view", "employee:create", "employee:update", "employee:delete"),
            [typeof(CompanyGetAllQuery)] = new FormPermissionSet(
                "company:view", "company:create", "company:update", "company:delete",
                Restore: "company:delete"),
            [typeof(CustomerGetAllQuery)] = new FormPermissionSet(
                "customer:view", "customer:create", "customer:update", "customer:delete",
                Restore: "customer:delete"),
            [typeof(SupplierGetAllQuery)] = new FormPermissionSet(
                "supplier:view", "supplier:create", "supplier:update", "supplier:delete",
                Restore: "supplier:delete"),
            [typeof(ProductGetAllQuery)] = new FormPermissionSet(
                "product:view", "product:create", "product:update", "product:delete",
                Restore: "product:delete"),
            [typeof(ProductCatalogListQuery)] = new FormPermissionSet(
                "product:view", "product:create", "product:update", "product:delete"),
            [typeof(ProductUnitTypeGetAllQuery)] = new FormPermissionSet(
                "product:view", "product:create", "product:update", "product:delete",
                Restore: "product:delete"),
            [typeof(TaxRateGetAllQuery)] = new FormPermissionSet(
                "product:view", "product:create", "product:update", "product:delete",
                Restore: "product:delete"),
            [typeof(RecipeGetAllQuery)] = new FormPermissionSet(
                "recipe:view", "recipe:manage", "recipe:manage", "recipe:manage",
                Restore: "recipe:manage"),
            [typeof(CostSlipGetAllQuery)] = new FormPermissionSet(
                "costslip:view", "costslip:create", "costslip:update", "costslip:delete",
                Approve: "costslip:approve", Restore: "costslip:restore"),
            [typeof(ProductMovementGetAllQuery)] = new FormPermissionSet(
                "stock_movement:view", "stock_movement:create", "stock_movement:update", "stock_movement:delete"),
            [typeof(StockIssueGetAllQuery)] = new FormPermissionSet(
                "stock_issue:view", "stock_issue:create", "stock_issue:update", "stock_issue:delete",
                Approve: "stockissue:approve"),
            [typeof(ConsumptionUnitGetAllQuery)] = new FormPermissionSet(
                "chartofaccount:view", "chartofaccount:create", "chartofaccount:update", "chartofaccount:delete"),
            [typeof(InvoiceGetAllQuery)] = new FormPermissionSet(
                "invoice:view", "invoice:create", "invoice:update", "invoice:delete",
                Approve: "invoice:approve", Restore: "invoice:restore"),
            [typeof(CurrentAccountMovementGetAllQuery)] = new FormPermissionSet(
                "current_account_movement:view",
                "current_account_movement:create",
                "current_account_movement:update",
                "current_account_movement:delete",
                Restore: "current_account_movement:restore")
        };

    /// <summary>
    /// Ribbon menü öğesinin gerektirdiği görüntüleme yetkisini döner.
    /// Menüde tanımlı olmayan öğeler için <c>false</c> döner; bu öğeler
    /// (grup başlıkları, Dashboard, Çıkış) yetki denetimine takılmaz.
    /// </summary>
    public static bool TryGetMenuViewPermission(string elementName, out string permission)
        => MenuViewPermissions.TryGetValue(elementName, out permission!);

    /// <summary>
    /// Bir liste sorgu türünün CRUD yetki kümesini döner. Katalogda tanımlı
    /// olmayan (eski/özel) listeler için <c>false</c> döner; bu listeler
    /// bugüne kadarki davranışını korur (sunucu denetimi devrededir).
    /// </summary>
    public static bool TryGetListPermissions(Type listQueryType, out FormPermissionSet permissions)
        => ListQueryPermissions.TryGetValue(listQueryType, out permissions!);
}