using System;
using System.Collections.Generic;
using DevExpress.Images;
using DevExpress.Utils.Svg;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    internal static class DxIcon
    {
        private static readonly Dictionary<string, SvgImage> _cache = new(StringComparer.OrdinalIgnoreCase);

        private static SvgImage Icon(string resourceKey)
        {
            if (_cache.TryGetValue(resourceKey, out SvgImage? cached) && cached is not null)
            {
                return cached;
            }

            SvgImage image = ImageResourceCache.Default.GetSvgImage(resourceKey)
                ?? ImageResourceCache.Default.GetSvgImage("svgimages/business%20objects/bo_list.svg");

            _cache.Add(resourceKey, image);
            return image;
        }

        // Toolbar actions
        public static SvgImage Add => Icon("svgimages/icon%20builder/actions_add.svg");
        public static SvgImage Edit => Icon("svgimages/icon%20builder/actions_edit.svg");
        public static SvgImage Delete => Icon("svgimages/icon%20builder/actions_trash.svg");
        public static SvgImage Refresh => Icon("svgimages/icon%20builder/actions_refresh.svg");
        public static SvgImage Restore => Icon("svgimages/icon%20builder/actions_rollback.svg");
        public static SvgImage Check => Icon("svgimages/icon%20builder/actions_check.svg");
        public static SvgImage CheckAll => Icon("svgimages/icon%20builder/actions_checkcircled.svg");
        public static SvgImage Uncheck => Icon("svgimages/icon%20builder/actions_removecircled.svg");
        public static SvgImage Close => Icon("devav/actions/close.svg");
        public static SvgImage ExpandAll => Icon("svgimages/icon%20builder/actions_arrow1down.svg");
        public static SvgImage CollapseAll => Icon("svgimages/icon%20builder/actions_arrow1up.svg");
        public static SvgImage Import => Icon("svgimages/pdf%20viewer/import.svg");
        public static SvgImage Next => Icon("svgimages/icon%20builder/actions_next.svg");
        public static SvgImage Previous => Icon("svgimages/arrows/prev.svg");
        public static SvgImage Forward => Icon("svgimages/arrows/next.svg");
        public static SvgImage Trend => Icon("svgimages/icon%20builder/business_linearchart.svg");

        // Form / field icons
        public static SvgImage User => Icon("svgimages/icon%20builder/actions_user.svg");
        public static SvgImage At => Icon("devav/contacts/mail.svg");
        public static SvgImage Mail => Icon("devav/contacts/mail.svg");
        public static SvgImage Phone => Icon("devav/contacts/phone.svg");
        public static SvgImage Company => Icon("svgimages/business%20objects/bo_organization.svg");
        public static SvgImage IdCard => Icon("svgimages/icon%20builder/security_personalid.svg");
        public static SvgImage Barcode => Icon("svgimages/icon%20builder/shopping_barcode.svg");
        public static SvgImage Tag => Icon("svgimages/icon%20builder/actions_label.svg");
        public static SvgImage Pin => Icon("svgimages/icon%20builder/travel_mappointer.svg");
        public static SvgImage Star => Icon("svgimages/icon%20builder/actions_rating.svg");
        public static SvgImage Photo => Icon("svgimages/icon%20builder/electronics_photo.svg");
        public static SvgImage Shield => Icon("svgimages/icon%20builder/security_security.svg");
        public static SvgImage Truck => Icon("svgimages/icon%20builder/shopping_delivery.svg");
        public static SvgImage Key => Icon("svgimages/icon%20builder/security_key.svg");
        public static SvgImage Lock => Icon("svgimages/icon%20builder/security_lock.svg");
        public static SvgImage Eye => Icon("svgimages/icon%20builder/security_visibility.svg");
        public static SvgImage EyeOff => Icon("svgimages/icon%20builder/security_visibilityoff.svg");
        public static SvgImage Receipt => Icon("svgimages/business%20objects/bo_invoice.svg");

        // Actions
        public static SvgImage Save => Icon("devav/actions/save.svg");
        public static SvgImage Print => Icon("svgimages/icon%20builder/actions_print.svg");

        // Module icons
        public static SvgImage Home => Icon("svgimages/icon%20builder/actions_home.svg");
        public static SvgImage Products => Icon("svgimages/business%20objects/bo_product.svg");
        public static SvgImage Suppliers => Icon("svgimages/business%20objects/bo_vendor.svg");
        public static SvgImage Customers => Icon("svgimages/business%20objects/bo_customer.svg");
        public static SvgImage Users => Icon("svgimages/business%20objects/bo_user.svg");
        public static SvgImage Roles => Icon("svgimages/business%20objects/bo_role.svg");

        /// <summary>
        /// Personel (rapor imza yetkilileri). Kullanıcı ikonundan farklı olarak
        /// "bo_contact" kullanılır: <c>bo_user</c> oturum açan hesabı, bo_contact
        /// ise belgelerde imza atacak gerçek kişiyi temsil eder.
        /// </summary>
        public static SvgImage Employees => Icon("svgimages/business%20objects/bo_contact.svg");
        public static SvgImage Invoices => Icon("svgimages/business%20objects/bo_invoice.svg");
        public static SvgImage Sales => Icon("svgimages/business%20objects/bo_sale.svg");
        public static SvgImage PriceStock => Icon("svgimages/icon%20builder/business_dollar.svg");
        public static SvgImage StockBox => Icon("svgimages/icon%20builder/shopping_box.svg");
        public static SvgImage StockInput => Icon("svgimages/icon%20builder/shopping_box.svg");
        public static SvgImage StockOutput => Icon("svgimages/icon%20builder/shopping_shoppingbasket.svg");
        public static SvgImage StockMovements => Icon("svgimages/icon%20builder/actions_reload.svg");
        public static SvgImage StockIssue => Icon("svgimages/icon%20builder/shopping_shoppingbasket.svg");
        public static SvgImage AtelierTransfer => Icon("svgimages/icon%20builder/actions_arrow1right.svg");
        public static SvgImage CurrentAccounts => Icon("svgimages/icon%20builder/business_money.svg");
        public static SvgImage Payments => Icon("svgimages/icon%20builder/business_cash.svg");
        public static SvgImage Balance => Icon("svgimages/icon%20builder/business_dollarcircled.svg");
        public static SvgImage ChartAccounts => Icon("svgimages/icon%20builder/business_barchart.svg");
        public static SvgImage Security => Icon("svgimages/icon%20builder/security_security.svg");
        public static SvgImage Percent => Icon("svgimages/icon%20builder/shopping_percent.svg");
        public static SvgImage AppIcon => Icon("svgimages/icon%20builder/business_diagram.svg");
        public static SvgImage Exit => Icon("devav/actions/exit.svg");
        public static SvgImage Module => Icon("svgimages/business%20objects/bo_list.svg");
        public static SvgImage Recipe => Icon("svgimages/business%20objects/bo_document.svg");
        public static SvgImage RecipeMaterials => Icon("svgimages/icon%20builder/actions_edit.svg");

        // Hamburger menü grupları. Her grup ve her öğe kendi ikonunu kullanır:
        // eski haritada "Fiyat & Stok Listesi" ile "Atölye Stok Raporu" aynı
        // SVG'yi, "Birim Cinsleri" ile "Tüketim Birimleri" aynı SVG'yi paylasiyordu.
        public static SvgImage Store => Icon("svgimages/icon%20builder/shopping_store.svg");
        public static SvgImage Cart => Icon("svgimages/icon%20builder/shopping_shoppingcart.svg");
        public static SvgImage Contact => Icon("svgimages/business%20objects/bo_contact.svg");
        public static SvgImage Category => Icon("svgimages/business%20objects/bo_category.svg");
        public static SvgImage PriceItem => Icon("svgimages/business%20objects/bo_price_item.svg");
        public static SvgImage Coupon => Icon("svgimages/icon%20builder/shopping_coupon.svg");
        public static SvgImage Contract => Icon("svgimages/business%20objects/bo_contract.svg");
        public static SvgImage Calculator => Icon("svgimages/icon%20builder/business_calculator.svg");
        public static SvgImage Report => Icon("svgimages/icon%20builder/business_report.svg");
        public static SvgImage ListItems => Icon("svgimages/icon%20builder/actions_list.svg");
        public static SvgImage Swap => Icon("svgimages/icon%20builder/actions_refresh.svg");
        public static SvgImage Ledger => Icon("svgimages/business%20objects/bo_list.svg");
        public static SvgImage DoughnutChart => Icon("svgimages/icon%20builder/business_doughnutchart.svg");
    }
}