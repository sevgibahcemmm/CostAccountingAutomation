using Cost.Accounting.Automation.Domain.Invoices;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.ConsumptionUnitForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CostSlipForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CurrentAccountForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CustomerForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.ProductMovementForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.RoleForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.StockIssueForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.UserForms;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraTab;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class RibbonMainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, string> _groupNames = new();
        private readonly SvgImage[] _moduleIcons;
        private readonly Dictionary<string, Action> _menuActions = new(StringComparer.OrdinalIgnoreCase);
        private System.Windows.Forms.Timer? _clockTimer;
        private DateTime _tokenExpiry;

        private static readonly Dictionary<string, SvgImage> _menuIcons = new(StringComparer.OrdinalIgnoreCase)
        {
            ["elmUrunler"] = DxIcon.Products,
            ["elmBirimCinsleri"] = DxIcon.Tag,
            ["elmKdvOranlari"] = DxIcon.Percent,
            ["elmStokGirisi"] = DxIcon.StockInput,
            ["elmStokCikisi"] = DxIcon.StockOutput,
            ["elmStokHareketleri"] = DxIcon.StockMovements,
            ["elmFiyatStokListesi"] = DxIcon.PriceStock,
            ["elmTuketim"] = DxIcon.StockIssue,
            ["elmAtolyeTransferi"] = DxIcon.AtelierTransfer,
            ["elmAtolyeStokRaporu"] = DxIcon.PriceStock,
            ["elmTuketimBirimleri"] = DxIcon.Tag,
            ["elmFaturalar"] = DxIcon.Invoices,
            ["elmFaturaOnaylama"] = DxIcon.Check,
            ["elmMusteriler"] = DxIcon.Customers,
            ["elmTedarikciler"] = DxIcon.Suppliers,
            ["elmCariHareketler"] = DxIcon.CurrentAccounts,
            ["elmCariBorcAlacakOzeti"] = DxIcon.Balance,
            ["elmHesapPlani"] = DxIcon.ChartAccounts,
            ["elmOdemeTahsilat"] = DxIcon.Payments,
            ["elmMaliyetPusulasi"] = DxIcon.Balance,
            ["elmSirketAyarlari"] = DxIcon.Company,
            ["elmKullanicilar"] = DxIcon.Users,
            ["elmRoller"] = DxIcon.Roles
        };

        public RibbonMainForm()
        {
            InitializeComponent();

            _session = Program.Services.GetRequiredService<SessionClaimContext>();
            MdiFormManager.Instance.Initialize(xtraTabbedMdiManager);
            _moduleIcons =
            [
                DxIcon.Home,              // 0 Ana Sayfa
                DxIcon.Module,            // 1 (kullanılmıyor)
                DxIcon.Products,          // 2 Stok Yönetimi
                DxIcon.Invoices,          // 3 Fatura Yönetimi
                DxIcon.Sales,             // 4 Satış Yönetimi
                DxIcon.CurrentAccounts,   // 5 Cari Yönetimi
                DxIcon.ChartAccounts,     // 6 Muhasebe Yönetimi
                DxIcon.AppIcon,           // 7 Sol üst uygulama ikonu
                DxIcon.Security,          // 8 Sistem Yönetimi
                DxIcon.Exit               // 9 Çıkış
            ];

            InitializeAccordion();
            BuildMenuActions();
            IconOptions.SvgImage = _moduleIcons[7];
            accordionControl.OptionsMinimizing.State = AccordionControlState.Minimized;
            accordionControl.OptionsMinimizing.NormalWidth = 260;
            accordionControl.AnimationType = DevExpress.XtraBars.Navigation.AnimationType.None;

            Load += RibbonMainForm_Load;
            FormClosing += RibbonMainForm_FormClosing;
            FormClosed += RibbonMainForm_FormClosed;
        }

        private void RibbonMainForm_Load(object? sender, EventArgs e)
        {
            xtraTabbedMdiManager.MdiParent = this;
            xtraTabbedMdiManager.ClosePageButtonShowMode = ClosePageButtonShowMode.InTabControlHeader;
            LoadSessionInfoToStatusBar();
            StartClock();
            OpenDashboard();
        }

        private static Image NormalizeIcon(SvgImage svg, int canvasSize, int innerSize)
        {
            Bitmap result = new Bitmap(canvasSize, canvasSize);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                SvgBitmap svgBitmap = SvgBitmap.Create(svg);
                RectangleF bounds = svgBitmap.GetBounds(false);

                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    using Image fallback = svgBitmap.Render(new Size(canvasSize, canvasSize), null, DevExpress.Utils.DefaultBoolean.False, DevExpress.Utils.DefaultBoolean.False);
                    g.DrawImage(fallback, 0, 0, canvasSize, canvasSize);
                    return result;
                }

                double scale = Math.Min((double)innerSize / bounds.Width, (double)innerSize / bounds.Height);
                int renderWidth = Math.Max(1, (int)Math.Round(svgBitmap.Width * scale));
                int renderHeight = Math.Max(1, (int)Math.Round(svgBitmap.Height * scale));

                using Image rendered = svgBitmap.Render(new Size(renderWidth, renderHeight), null, DevExpress.Utils.DefaultBoolean.False, DevExpress.Utils.DefaultBoolean.False);
                g.DrawImage(rendered, (canvasSize - renderWidth) / 2, (canvasSize - renderHeight) / 2, renderWidth, renderHeight);
            }

            return result;
        }

        private void InitializeAccordion()
        {
            foreach (AccordionControlElement element in accordionControl.Elements)
            {
                if (element.Tag is not int tag)
                {
                    continue;
                }

                if (tag >= 1 && tag <= 8)
                {
                    element.ImageOptions.SvgImage = null;
                    element.ImageOptions.Image = NormalizeIcon(_moduleIcons[Math.Min(tag, _moduleIcons.Length - 1)], 34, 30);
                    element.Appearance.Normal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    _groupNames[tag] = element.Text;
                }
                else if (tag == 0)
                {
                    element.ImageOptions.SvgImage = null;
                    element.ImageOptions.Image = NormalizeIcon(_moduleIcons[0], 30, 26);
                }
                else if (tag == 99)
                {
                    element.ImageOptions.SvgImage = null;
                    element.ImageOptions.Image = NormalizeIcon(_moduleIcons[9], 30, 26);
                }

                element.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;

                WireElementClick(element);

                foreach (AccordionControlElement child in element.Elements)
                {
                    if (_menuIcons.TryGetValue(child.Name, out SvgImage? icon))
                    {
                        child.ImageOptions.SvgImage = icon;
                        child.ImageOptions.SvgImageSize = new Size(22, 22);
                        child.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.False;
                    }

                    WireElementClick(child);
                }
            }
        }

        private void WireElementClick(AccordionControlElement element)
        {
            if (element.Style != ElementStyle.Item)
            {
                return;
            }

            element.Click += (s, e) => HandleMenuClick((AccordionControlElement)s!);
        }

        private void BuildMenuActions()
        {
            _menuActions["elmKullanicilar"] = () => MdiFormManager.Instance.OpenForm<UsersListForm>(this, "Kullanıcılar");
            _menuActions["elmRoller"] = () => MdiFormManager.Instance.OpenForm<RolesListForm>(this, "Roller ve Yetkiler");
            _menuActions["elmSirketAyarlari"] = () => MdiFormManager.Instance.OpenForm<CompaniesListForm>(this, "Şirketler");
            _menuActions["elmMusteriler"] = () => MdiFormManager.Instance.OpenForm<CustomersListForm>(this, "Müşteriler");
            _menuActions["elmTedarikciler"] = () => MdiFormManager.Instance.OpenForm<SuppliersListForm>(this, "Tedarikçiler");
            _menuActions["elmCariHareketler"] = () => MdiFormManager.Instance.OpenForm<CurrentAccountMovementsListForm>(this, "Cari Hareketler");
            _menuActions["elmCariBorcAlacakOzeti"] = () => MdiFormManager.Instance.OpenForm<CurrentAccountBalanceForm>(this, "Cari Borç/Alacak Özeti");
            _menuActions["elmOdemeTahsilat"] = () => MdiFormManager.Instance.OpenForm<PaymentCollectionListForm>(this, "Ödeme / Tahsilat");
            _menuActions["elmFaturalar"] = () => OpenInvoices(null);
            _menuActions["elmFaturaOnaylama"] = () => MdiFormManager.Instance.OpenForm<InvoiceApprovalForm>(this, "Fatura Onaylama");
            _menuActions["elmStokGirisi"] = () => OpenProductMovements(ProductMovementType.Input);
            _menuActions["elmStokCikisi"] = () => OpenProductMovements(ProductMovementType.Output);
            _menuActions["elmStokHareketleri"] = () => OpenProductMovements(null);
            _menuActions["elmFiyatStokListesi"] = () => MdiFormManager.Instance.OpenForm<ProductPriceStockListForm>(this, "Fiyat & Stok Listesi");
            _menuActions["elmTuketim"] = () => MdiFormManager.Instance.OpenForm<ConsumptionsListForm>(this, "Tüketim");
            _menuActions["elmAtolyeTransferi"] = () => MdiFormManager.Instance.OpenForm<AtelierTransfersListForm>(this, "Atölye Transferi");
            _menuActions["elmAtolyeStokRaporu"] = () => MdiFormManager.Instance.OpenForm<AtelierTransferStockForm>(this, "Atölye Stok Raporu");
            _menuActions["elmTuketimBirimleri"] = () => MdiFormManager.Instance.OpenForm<ConsumptionUnitsListForm>(this, "Tüketim Birimleri");
            _menuActions["elmHesapPlani"] = () => MdiFormManager.Instance.OpenForm<ChartOfAccountsListForm>(this, "Hesap Planı");
            _menuActions["elmMaliyetPusulasi"] = () => MdiFormManager.Instance.OpenForm<CostSlipsListForm>(this, "Maliyet Pusulası");
            _menuActions["elmRecete"] = () => MdiFormManager.Instance.OpenForm<RecipeListForm>(this, "Reçeteler");
            _menuActions["elmUrunler"] = () => MdiFormManager.Instance.OpenForm<ProductsListForm>(this, "Ürünler");
            _menuActions["elmBirimCinsleri"] = () => MdiFormManager.Instance.OpenForm<ProductUnitTypesListForm>(this, "Birim Cinsleri");
            _menuActions["elmKdvOranlari"] = () => MdiFormManager.Instance.OpenForm<TaxRatesListForm>(this, "KDV Oranları");
        }

        private void HandleMenuClick(AccordionControlElement element)
        {
            if (element.Tag is not int index)
            {
                return;
            }

            switch (index)
            {
                case 99:
                    Close();
                    return;
                case 0:
                    OpenDashboard();
                    return;
            }

            if (!string.IsNullOrEmpty(element.Name) && _menuActions.TryGetValue(element.Name, out Action? open))
            {
                open();
                return;
            }

            if (index >= 1 && index <= 8)
            {
                string groupName = _groupNames.TryGetValue(index, out string? name) ? name : "Modül";
                OpenModule(element.Text, groupName, index);
            }
        }

        private void OpenInvoices(InvoiceType? type)
        {
            string title = type switch
            {
                InvoiceType.Purchase => "Alış Faturaları",
                InvoiceType.PurchaseReturn => "Alış İade Faturaları",
                InvoiceType.Sales => "Satış Faturaları",
                InvoiceType.SalesReturn => "Satış İade Faturaları",
                _ => "Faturalar"
            };
            InvoiceType? target = type;
            MdiFormManager.Instance.OpenForm<InvoicesListForm>(this, title, () => new InvoicesListForm(target));
        }

        private void OpenProductMovements(ProductMovementType? type)
        {
            string title = type switch
            {
                ProductMovementType.Input => "Stok Girişleri",
                ProductMovementType.Output => "Stok Çıkışları",
                _ => "Stok Hareketleri"
            };
            MdiFormManager.Instance.OpenForm<ProductMovementsListForm>(this, title, () => new ProductMovementsListForm(type));
        }

        private void OpenDashboard()
        {
            MdiFormManager.Instance.OpenForm<DashboardMdiForm>(this);
        }

        private void OpenModule(string moduleTitle, string groupName, int iconIndex)
        {
            MdiFormManager.Instance.OpenForm<ModulePlaceholderMdiForm>(
                this,
                moduleTitle,
                () => new ModulePlaceholderMdiForm(moduleTitle, groupName, _moduleIcons[iconIndex]));
        }

        private void LoadSessionInfoToStatusBar()
        {
            string companyName = "-";
            string fullName = "-";
            string roleName = "-";

            try
            {
                if (!string.IsNullOrEmpty(_session.Token))
                {
                    JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(_session.Token);
                    companyName = jwt.Claims.FirstOrDefault(c => c.Type == "company")?.Value ?? "-";
                    fullName = jwt.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value ?? "-";
                    roleName = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "-";
                    _tokenExpiry = jwt.ValidTo;
                }
            }
            catch
            {
            }

            try
            {
                if (_tokenExpiry == default)
                {
                    roleName = _session.GetRoleName();
                }
            }
            catch
            {
            }

            barButtonItemCompanyName.Caption = $"🏢  {companyName}";
            barButtonItemUserName.Caption = $"👤  {fullName}";
            barButtonItemRoleName.Caption = $"🔑  Rol: {roleName}";
        }

        private void StartClock()
        {
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();
            UpdateClock();
        }

        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            CultureInfo tr = CultureInfo.GetCultureInfo("tr-TR");

            string timeText = $"🕐  {now:HH:mm:ss}";
            string dateText = $"📅  {now.ToString("dddd, dd MMMM yyyy", tr)}";

            if (barButtonItemTime.Caption != timeText)
            {
                barButtonItemTime.Caption = timeText;
            }

            if (barButtonItemLongDate.Caption != dateText)
            {
                barButtonItemLongDate.Caption = dateText;
            }

            if (_tokenExpiry != default)
            {
                TimeSpan remaining = _tokenExpiry - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                {
                    string expiredText = "⏳  Oturum süresi doldu";
                    if (barButtonItemExpTime.Caption != expiredText)
                    {
                        barButtonItemExpTime.Caption = expiredText;
                    }

                    RestartToLogin();
                    return;
                }

                string expText = $"⏳  Kalan Süre: {remaining:hh\\:mm\\:ss}";
                if (barButtonItemExpTime.Caption != expText)
                {
                    barButtonItemExpTime.Caption = expText;
                }
            }
        }

        private bool _restartingToLogin;

        private void RestartToLogin()
        {
            if (_restartingToLogin)
            {
                return;
            }

            _restartingToLogin = true;
            _clockTimer?.Stop();
            _session.Clear();

            XtraLoginForm login = Program.Services.GetRequiredService<XtraLoginForm>();
            login.Show();

            foreach (Form form in System.Windows.Forms.Application.OpenForms.Cast<Form>().Reverse().ToList())
            {
                if (form is XtraLoginForm)
                {
                    continue;
                }

                try
                {
                    form.Close();
                }
                catch
                {
                }
            }

            foreach (Form form in System.Windows.Forms.Application.OpenForms.Cast<Form>().Reverse().ToList())
            {
                if (form is XtraLoginForm)
                {
                    continue;
                }

                try
                {
                    form.Dispose();
                }
                catch
                {
                }
            }
        }

        private void RibbonMainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_restartingToLogin)
            {
                return;
            }

            if (MsgBox.Confirm("Programı kapatmak istediğinize emin misiniz?", "Çıkış Onayı") != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void RibbonMainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_restartingToLogin)
            {
                return;
            }

            System.Windows.Forms.Application.Exit();
        }
    }
}