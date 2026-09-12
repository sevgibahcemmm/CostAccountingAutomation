using DevExpress.Utils.Svg;
using System.IO;
using System.Text;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    public static class SvgIcons
    {
        private static readonly string[] _ModuleSvg =
        {
            _SvgDashboard,
            _SvgCalculator,
            _SvgBox,
            _SvgCart,
            _SvgReceipt,
            _SvgPeople,
            _SvgChart,
            _SvgWallet,
            _SvgGear,
            _SvgLogout
        };

        public static SvgImage[] Modules { get; } = CreateModules();

        private static readonly string[] _MenuSvg =
        {
            _SvgDashboard,      // 0  Ana Sayfa
            _SvgCalculator,     // 1  Maliyet
            _SvgBox,            // 2  Stok
            _SvgCart,           // 3  Satın Alma
            _SvgReceipt,        // 4  Satış
            _SvgPeople,         // 5  Cari
            _SvgChart,          // 6  Muhasebe
            _SvgWallet,         // 7  Finans
            _SvgGear,           // 8  Sistem
            _SvgLogout,         // 9  Çıkış
            _SvgTarget,         // 10 Maliyet Merkezleri
            _SvgArrowDown,      // 11 Stok Girişi
            _SvgArrowUp,        // 12 Stok Çıkışı
            _SvgLedger,         // 13 Hesap Planı
            _SvgTruckWhite,     // 14 Tedarikçiler
            _SvgSheet,          // 15 Mizan
            _SvgBank,           // 16 Banka İşlemleri
            _SvgPayCard,        // 17 Ödeme/Tahsilat
            _SvgCash,           // 18 Kasa
            _SvgTrend,          // 19 Gelir/Gider
            _SvgUserWhite,      // 20 Kullanıcılar
            _SvgKeyWhite,       // 21 Roller
            _SvgBuildingWhite,  // 22 Şirket Ayarları
            _SvgBag             // 23 Satış Siparişleri
        };

        public static SvgImage[] MenuIcons { get; } = CreateModulesFrom(_MenuSvg);

        public static SvgImage UserIcon { get; } = Svg(_SvgUser);
        public static SvgImage HeaderUserIcon { get; } = Svg(_SvgUserBlue);
        public static SvgImage AtIcon { get; } = Svg(_SvgAt);
        public static SvgImage MailIcon { get; } = Svg(_SvgMail);
        public static SvgImage IdCardIcon { get; } = Svg(_SvgIdCard);
        public static SvgImage BuildingIcon { get; } = Svg(_SvgBuilding);
        public static SvgImage KeyIcon { get; } = Svg(_SvgKey);
        public static SvgImage ShieldIcon { get; } = Svg(_SvgShield);
        public static SvgImage PhotoIcon { get; } = Svg(_SvgPhoto);
        public static SvgImage PlusIcon { get; } = Svg(_SvgPlus);
        public static SvgImage StarIcon { get; } = Svg(_SvgStar);
        public static SvgImage TrashIcon { get; } = Svg(_SvgTrash);
        public static SvgImage CheckIcon { get; } = Svg(_SvgCheck);
        public static SvgImage CloseIcon { get; } = Svg(_SvgClose);
        public static SvgImage BuildingWhiteIcon { get; } = Svg(_SvgBuildingWhite);
        public static SvgImage TruckIcon { get; } = Svg(_SvgTruck);
        public static SvgImage EditIcon { get; } = Svg(_SvgEdit);
        public static SvgImage RefreshIcon { get; } = Svg(_SvgRefresh);
        public static SvgImage RestoreIcon { get; } = Svg(_SvgRestore);
        public static SvgImage ImportIcon { get; } = Svg(_SvgImport);
        public static SvgImage ExpandAllIcon { get; } = Svg(_SvgExpandAll);
        public static SvgImage CollapseAllIcon { get; } = Svg(_SvgCollapseAll);
        public static SvgImage KeyWhiteIcon { get; } = Svg(_SvgKeyWhite);
        public static SvgImage RefreshWhiteIcon { get; } = Svg(_SvgRefreshWhite);
        public static SvgImage NextIcon { get; } = Svg(_SvgArrowRight);

        public static SvgImage CheckAllIcon { get; } = Svg(_SvgCheckAll);

        public static SvgImage UncheckIcon { get; } = Svg(_SvgUncheck);

        public static SvgImage BarcodeIcon { get; } = Svg(_SvgBarcode);
        public static SvgImage QRIcon { get; } = Svg(_SvgQr);
        public static SvgImage TagIcon { get; } = Svg(_SvgTag);
        public static SvgImage TrendBlueIcon { get; } = Svg(_SvgTrendBlue);
        public static SvgImage PhoneIcon { get; } = Svg(_SvgPhone);
        public static SvgImage PinIcon { get; } = Svg(_SvgPin);
        public static SvgImage ReceiptGrayIcon { get; } = Svg(_SvgReceiptGray);

        private static SvgImage[] CreateModules()
        {
            var images = new SvgImage[_ModuleSvg.Length];
            for (int i = 0; i < _ModuleSvg.Length; i++)
            {
                images[i] = Svg(_ModuleSvg[i]);
            }
            return images;
        }

        private static SvgImage[] CreateModulesFrom(string[] contents)
        {
            var images = new SvgImage[contents.Length];
            for (int i = 0; i < contents.Length; i++)
            {
                images[i] = Svg(contents[i]);
            }
            return images;
        }

        private static SvgImage Svg(string content)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return SvgImage.FromStream(stream);
        }

        private const string _SvgDashboard = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='3' width='7' height='9' rx='1' fill='none' stroke='white' stroke-width='1.6'/><rect x='14' y='3' width='7' height='5' rx='1' fill='none' stroke='white' stroke-width='1.6'/><rect x='14' y='12' width='7' height='9' rx='1' fill='none' stroke='white' stroke-width='1.6'/><rect x='3' y='16' width='7' height='5' rx='1' fill='none' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgCalculator = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='5' y='2' width='14' height='20' rx='2' fill='none' stroke='white' stroke-width='1.6'/><rect x='8' y='5' width='8' height='4' rx='0.8' fill='none' stroke='white' stroke-width='1.4'/><circle cx='8' cy='12' r='1' fill='white'/><circle cx='12' cy='12' r='1' fill='white'/><circle cx='16' cy='12' r='1' fill='white'/><circle cx='8' cy='16' r='1' fill='white'/><circle cx='12' cy='16' r='1' fill='white'/><circle cx='16' cy='16' r='1' fill='white'/><circle cx='8' cy='20' r='1' fill='white'/><circle cx='12' cy='20' r='1' fill='white'/><circle cx='16' cy='20' r='1' fill='white'/></svg>";

        private const string _SvgBox = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 2L21 6v12l-9 4-9-4V6l9-4z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><line x1='12' y1='12' x2='21' y2='6' stroke='white' stroke-width='1.6'/><line x1='12' y1='12' x2='3' y2='6' stroke='white' stroke-width='1.6'/><line x1='12' y1='12' x2='12' y2='22' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgCart = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='9' cy='20' r='1.5' fill='white'/><circle cx='17' cy='20' r='1.5' fill='white'/><path d='M2 3h3l3.4 12h9l2.6-9H6' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgReceipt = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6 2h12v20l-3-2-3 2-3-2-3 2V2z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><line x1='9' y1='7' x2='15' y2='7' stroke='white' stroke-width='1.6'/><line x1='9' y1='11' x2='15' y2='11' stroke='white' stroke-width='1.6'/><line x1='9' y1='15' x2='13' y2='15' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgPeople = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='9' cy='8' r='3' fill='none' stroke='white' stroke-width='1.6'/><path d='M3 20c0-3.3 2.7-6 6-6s6 2.7 6 6' fill='none' stroke='white' stroke-width='1.6'/><circle cx='17' cy='9' r='2.5' fill='none' stroke='white' stroke-width='1.6'/><path d='M15.5 14.5c2.7.7 4.5 3 4.5 5.5' fill='none' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgChart = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='11' width='4' height='10' rx='0.5' fill='none' stroke='white' stroke-width='1.6'/><rect x='10' y='5' width='4' height='16' rx='0.5' fill='none' stroke='white' stroke-width='1.6'/><rect x='17' y='8' width='4' height='13' rx='0.5' fill='none' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgWallet = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='2' y='6' width='20' height='14' rx='2' fill='none' stroke='white' stroke-width='1.6'/><path d='M2 9h20' stroke='white' stroke-width='1.6'/><path d='M16 14.5h3' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgGear = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='12' r='3' fill='none' stroke='white' stroke-width='1.6'/><path d='M12 2v3M12 19v3M2 12h3M19 12h3M4.9 4.9l2.1 2.1M17 17l2.1 2.1M19.1 4.9L17 7M7 17l-2.1 2.1' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgLogout = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h7' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round' stroke-linejoin='round'/><path d='M20 12H10M20 12l-3 3M20 12l-3-3' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgUser = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='8' r='3.2' fill='none' stroke='#64748b' stroke-width='1.7'/><path d='M4.5 20c0-3.9 3.4-6.6 7.5-6.6s7.5 2.7 7.5 6.6' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgUserBlue = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='8' r='3.2' fill='none' stroke='#2563eb' stroke-width='1.8'/><path d='M4.5 20c0-3.9 3.4-6.6 7.5-6.6s7.5 2.7 7.5 6.6' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round'/></svg>";

        private const string _SvgAt = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='12' r='4' fill='none' stroke='#64748b' stroke-width='1.7'/><path d='M16.5 11.5V15a2.25 2.25 0 0 0 4.5 0v-.5M21 12a9 9 0 1 0-3.4 7' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgMail = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='5' width='18' height='14' rx='2' fill='none' stroke='#64748b' stroke-width='1.7'/><path d='M3 7l9 6 9-6' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linejoin='round'/></svg>";

        private const string _SvgIdCard = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='5' width='18' height='14' rx='2' fill='none' stroke='#64748b' stroke-width='1.7'/><line x1='7' y1='9' x2='12' y2='9' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/><circle cx='15' cy='11' r='1.6' fill='none' stroke='#64748b' stroke-width='1.7'/><line x1='10' y1='15.5' x2='17' y2='15.5' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgBuilding = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M4 21V5a1 1 0 0 1 1-1h9a1 1 0 0 1 1 1v16' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linejoin='round'/><path d='M15 9h4a1 1 0 0 1 1 1v11' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linejoin='round'/><line x1='4' y1='21' x2='21' y2='21' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/><line x1='8' y1='8' x2='10' y2='8' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/><line x1='8' y1='12' x2='10' y2='12' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/><line x1='8' y1='16' x2='10' y2='16' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgKey = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='8' cy='14' r='4' fill='none' stroke='#64748b' stroke-width='1.7'/><path d='M11 11l8-8' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/><path d='M16 6l2.5 2.5' fill='none' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgShield = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 3l8 3v6c0 4.6-3.5 7.9-8 9-4.5-1.1-8-4.4-8-9V6l8-3z' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linejoin='round'/><path d='M9 12l2 2 4-4' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgPhoto = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='5' width='18' height='14' rx='2' fill='none' stroke='#2563eb' stroke-width='1.8'/><circle cx='8.5' cy='10' r='1.5' fill='none' stroke='#2563eb' stroke-width='1.8'/><path d='M3 16l5-4 4 3 3-2 6 5' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linejoin='round'/></svg>";

        private const string _SvgPlus = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 5v14M5 12h14' stroke='#2563eb' stroke-width='2' stroke-linecap='round'/></svg>";

        private const string _SvgStar = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 3l2.6 5.4 5.9.8-4.3 4.1 1 5.8L12 16.6 6.8 19.1l1-5.8L3.5 9.2l5.9-.8z' fill='none' stroke='#d97706' stroke-width='1.8' stroke-linejoin='round'/></svg>";

        private const string _SvgTrash = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M4 7h16' stroke='#dc2626' stroke-width='1.8' stroke-linecap='round'/><path d='M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2' fill='none' stroke='#dc2626' stroke-width='1.8' stroke-linecap='round'/><path d='M6 7l1 13a1 1 0 0 0 1 .9h8a1 1 0 0 0 1-.9L18 7' fill='none' stroke='#dc2626' stroke-width='1.8' stroke-linecap='round'/><line x1='10' y1='11' x2='10' y2='17' stroke='#dc2626' stroke-width='1.8' stroke-linecap='round'/><line x1='14' y1='11' x2='14' y2='17' stroke='#dc2626' stroke-width='1.8' stroke-linecap='round'/></svg>";

        private const string _SvgCheck = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M5 12.5l4.5 4.5L19 7' fill='none' stroke='white' stroke-width='2.2' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgClose = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6 6l12 12M18 6L6 18' stroke='#64748b' stroke-width='1.8' stroke-linecap='round'/></svg>";

        private const string _SvgBuildingWhite = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M4 21V5a1 1 0 0 1 1-1h8.8a1.2 1.2 0 0 1 1.2 1.2V21' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><path d='M15 9h4a1 1 0 0 1 1 1v11' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><line x1='4' y1='21' x2='21' y2='21' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='8' x2='10' y2='8' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='12' x2='10' y2='12' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='16' x2='10' y2='16' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgTruck = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M2 5h13v12H2z' fill='none' stroke='#2563eb' stroke-width='1.7' stroke-linejoin='round'/><path d='M15 9h4.2L21 12.4V17h-6z' fill='none' stroke='#2563eb' stroke-width='1.7' stroke-linejoin='round'/><circle cx='7' cy='18' r='1.8' fill='none' stroke='#2563eb' stroke-width='1.7'/><circle cx='17' cy='18' r='1.8' fill='none' stroke='#2563eb' stroke-width='1.7'/><line x1='1' y1='8' x2='4' y2='8' stroke='#2563eb' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgTarget = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='12' r='7' fill='none' stroke='white' stroke-width='1.6'/><circle cx='12' cy='12' r='1.6' fill='white'/><path d='M12 2v3M12 19v3M2 12h3M19 12h3' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgArrowDown = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 4v15M6 13l6 6 6-6' fill='none' stroke='white' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgArrowUp = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 20V5M6 11l6-6 6 6' fill='none' stroke='white' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgLedger = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M5 4h13a2 2 0 0 1 2 2v14H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><line x1='8' y1='8' x2='16' y2='8' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='12' x2='16' y2='12' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='16' x2='13' y2='16' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgTruckWhite = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M2 5h13v12H2z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><path d='M15 9h4.2L21 12.4V17h-6z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><circle cx='7' cy='18' r='1.8' fill='none' stroke='white' stroke-width='1.6'/><circle cx='17' cy='18' r='1.8' fill='none' stroke='white' stroke-width='1.6'/><line x1='1' y1='8' x2='4' y2='8' stroke='white' stroke-width='1.5' stroke-linecap='round'/></svg>";

        private const string _SvgSheet = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='4' y='4' width='16' height='16' rx='1' fill='none' stroke='white' stroke-width='1.6'/><line x1='8' y1='8' x2='16' y2='8' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='12' x2='16' y2='12' stroke='white' stroke-width='1.6' stroke-linecap='round'/><line x1='8' y1='16' x2='13' y2='16' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgBank = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M3 10L12 4l9 6' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><line x1='3' y1='10' x2='21' y2='10' stroke='white' stroke-width='1.6'/><rect x='5' y='14' width='3.4' height='6' fill='none' stroke='white' stroke-width='1.5'/><rect x='10.3' y='14' width='3.4' height='6' fill='none' stroke='white' stroke-width='1.5'/><rect x='15.6' y='14' width='3.4' height='6' fill='none' stroke='white' stroke-width='1.5'/><line x1='2' y1='20' x2='22' y2='20' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgPayCard = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='2' y='5' width='20' height='14' rx='2' fill='none' stroke='white' stroke-width='1.6'/><line x1='2' y1='10' x2='22' y2='10' stroke='white' stroke-width='1.6'/><path d='M6 13h.01M9 13h.01M6 16h4' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgCash = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='4' y='7' width='16' height='10' rx='1.5' fill='none' stroke='white' stroke-width='1.6'/><circle cx='12' cy='12' r='2.2' fill='none' stroke='white' stroke-width='1.6'/><line x1='2' y1='10' x2='4' y2='10' stroke='white' stroke-width='1.6'/><line x1='20' y1='10' x2='22' y2='10' stroke='white' stroke-width='1.6'/></svg>";

        private const string _SvgTrend = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M3 17l6-6 4 3 7-8' fill='none' stroke='white' stroke-width='1.7' stroke-linecap='round' stroke-linejoin='round'/><path d='M14 6h6v6' fill='none' stroke='white' stroke-width='1.7' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgUserWhite = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='12' cy='8' r='3.2' fill='none' stroke='white' stroke-width='1.6'/><path d='M4.5 20c0-3.9 3.4-6.6 7.5-6.6s7.5 2.7 7.5 6.6' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgKeyWhite = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><circle cx='8' cy='14' r='4' fill='none' stroke='white' stroke-width='1.6'/><path d='M11 11l8-8' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round'/><path d='M16 6l2.5 2.5' fill='none' stroke='white' stroke-width='1.6' stroke-linecap='round'/></svg>";

        private const string _SvgBag = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6 8h12l1.5 12h-15z' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/><path d='M9 8a3 3 0 0 1 6 0' fill='none' stroke='white' stroke-width='1.6' stroke-linejoin='round'/></svg>";

        private const string _SvgEdit = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M4 20l.8-4L16.5 4.3 19.7 7.5 8 19.2z' fill='none' stroke='#2563eb' stroke-width='1.7' stroke-linejoin='round'/><line x1='14' y1='6' x2='18' y2='10' stroke='#2563eb' stroke-width='1.7'/></svg>";

        private const string _SvgRefresh = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M23 4v6h-6' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/><path d='M20.49 15a9 9 0 1 1-2.12-9.36L23 10' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgRestore = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M3 12a9 9 0 1 0 3-6.7' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/><path d='M3 3v6h6' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgImport = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 16V4M7 9l5-5 5 5' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/><path d='M4 16v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgExpandAll = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3.5' y='3.5' width='17' height='17' rx='2.5' fill='none' stroke='#16a34a' stroke-width='1.7'/><path d='M12 8v8M8 12h8' stroke='#16a34a' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgCollapseAll = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3.5' y='3.5' width='17' height='17' rx='2.5' fill='none' stroke='#16a34a' stroke-width='1.7'/><path d='M8 12h8' stroke='#16a34a' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgRefreshWhite = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M23 4v6h-6' fill='none' stroke='white' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/><path d='M20.49 15a9 9 0 1 1-2.12-9.36L23 10' fill='none' stroke='white' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgArrowRight = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M5 12h14M13 6l6 6-6 6' fill='none' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgCheckAll = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6 6l5 5L20 4' fill='none' stroke='#16a34a' stroke-width='1.9' stroke-linecap='round' stroke-linejoin='round'/><path d='M6 15l5 5L20 12' fill='none' stroke='#16a34a' stroke-width='1.9' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgUncheck = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3.5' y='3.5' width='17' height='17' rx='2.5' fill='none' stroke='#64748b' stroke-width='1.7'/><path d='M7 12h10' stroke='#64748b' stroke-width='1.7' stroke-linecap='round'/></svg>";

        private const string _SvgBarcode = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='2.5' y='5' width='2.4' height='14' rx='0.5' fill='#2563eb'/><rect x='6' y='5' width='1.6' height='14' rx='0.5' fill='#2563eb'/><rect x='8.6' y='5' width='3.2' height='14' rx='0.5' fill='#2563eb'/><rect x='12.8' y='5' width='1.6' height='14' rx='0.5' fill='#2563eb'/><rect x='15.4' y='5' width='2.4' height='14' rx='0.5' fill='#2563eb'/><rect x='18.8' y='5' width='1.6' height='14' rx='0.5' fill='#2563eb'/><rect x='21.4' y='5' width='0.8' height='14' rx='0.5' fill='#2563eb'/></svg>";

        private const string _SvgQr = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><rect x='3' y='3' width='7' height='7' rx='1.2' fill='none' stroke='#2563eb' stroke-width='1.8'/><rect x='14' y='3' width='7' height='7' rx='1.2' fill='none' stroke='#2563eb' stroke-width='1.8'/><rect x='3' y='14' width='7' height='7' rx='1.2' fill='none' stroke='#2563eb' stroke-width='1.8'/><path d='M14.5 14.5h3v3h-3zM19.5 14.5h1.5M14.5 19.5h1.5M19.5 19.5h1.5' fill='none' stroke='#2563eb' stroke-width='1.6' stroke-linejoin='round'/></svg>";

        private const string _SvgTag = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12.6 3H21v8.4L12.8 19.6a2 2 0 0 1-2.8 0l-5.6-5.6a2 2 0 0 1 0-2.8z' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linejoin='round'/><circle cx='15.5' cy='8.5' r='1.4' fill='none' stroke='#2563eb' stroke-width='1.8'/></svg>";

        private const string _SvgTrendBlue = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M3 17l6-6 4 3 7-8' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/><path d='M14 6h6v6' fill='none' stroke='#2563eb' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

        private const string _SvgPhone = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6.6 10.8c1.4 2.8 3.8 5.2 6.6 6.6l2.2-2.2c.3-.3.7-.4 1.1-.3 1.2.4 2.5.6 3.8.6.6 0 1 .4 1 1V20c0 .6-.4 1-1 1C10.6 21 3 13.4 3 4c0-.6.4-1 1-1h3.5c.6 0 1 .4 1 1 0 1.3.2 2.6.6 3.8.1.4 0 .8-.3 1.1z' fill='none' stroke='#64748b' stroke-width='1.6' stroke-linejoin='round'/></svg>";

        private const string _SvgPin = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M12 22s7-6.6 7-12a7 7 0 1 0-14 0c0 5.4 7 12 7 12z' fill='none' stroke='#64748b' stroke-width='1.6' stroke-linejoin='round'/><circle cx='12' cy='10' r='2.4' fill='none' stroke='#64748b' stroke-width='1.6'/></svg>";

        private const string _SvgReceiptGray = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'><path d='M6 2h12v20l-3-2-3 2-3-2-3 2V2z' fill='none' stroke='#64748b' stroke-width='1.6' stroke-linejoin='round'/><line x1='9' y1='7' x2='15' y2='7' stroke='#64748b' stroke-width='1.6'/><line x1='9' y1='11' x2='15' y2='11' stroke='#64748b' stroke-width='1.6'/><line x1='9' y1='15' x2='13' y2='15' stroke='#64748b' stroke-width='1.6'/></svg>";
    }
}