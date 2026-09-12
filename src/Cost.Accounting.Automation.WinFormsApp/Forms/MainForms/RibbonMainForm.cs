using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CompanyForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.CustomerForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.RoleForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.SupplierForms;
using Cost.Accounting.Automation.WinFormsApp.Forms.UserForms;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraTab;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class RibbonMainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, string> _groupNames = new();
        private readonly SvgImage[] _moduleIcons = SvgIcons.Modules;
        private System.Windows.Forms.Timer? _clockTimer;
        private DateTime _tokenExpiry;

        private static readonly Dictionary<string, int> _menuIconIndex = new(StringComparer.OrdinalIgnoreCase)
        {
            ["elmMaliyetMerkezleri"] = 10,
            ["elmMaliyetHesaplama"] = 1,
            ["elmMaliyetRaporlari"] = 6,
            ["elmUrunler"] = 2,
            ["elmBirimCinsleri"] = 12,
            ["elmStokGirisi"] = 11,
            ["elmStokCikisi"] = 12,
            ["elmSatinAlmaSiparisleri"] = 3,
            ["elmSatinAlmaFaturalari"] = 4,
            ["elmSatisSiparisleri"] = 23,
            ["elmSatisFaturalari"] = 4,
            ["elmMusteriler"] = 5,
            ["elmTedarikciler"] = 14,
            ["elmHesapPlani"] = 13,
            ["elmOdemeTahsilat"] = 17,
            ["elmBankaIslemleri"] = 16,
            ["elmMizan"] = 15,
            ["elmGelirGider"] = 19,
            ["elmKasa"] = 18,
            ["elmRaporlar"] = 6,
            ["elmSirketAyarlari"] = 22,
            ["elmKullanicilar"] = 20,
            ["elmRoller"] = 21
        };

        public RibbonMainForm()
        {
            InitializeComponent();

            _session = Program.Services.GetRequiredService<SessionClaimContext>();
            MdiFormManager.Instance.Initialize(xtraTabbedMdiManager);

            InitializeAccordion();
            IconOptions.SvgImage = _moduleIcons[7];
            accordionControl.OptionsMinimizing.State = AccordionControlState.Minimized;

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

        private void InitializeAccordion()
        {
            foreach (AccordionControlElement element in accordionControl.Elements)
            {
                if (element.Tag is int tag)
                {
                    if (tag >= 1 && tag <= 8)
                    {
                        element.ImageOptions.SvgImage = SvgIcons.MenuIcons[tag];
                        element.ImageOptions.SvgImageSize = new Size(24, 24);
                        _groupNames[tag] = element.Text;
                    }
                    else if (tag == 0)
                    {
                        element.ImageOptions.SvgImage = SvgIcons.MenuIcons[0];
                        element.ImageOptions.SvgImageSize = new Size(20, 20);
                    }
                    else if (tag == 99)
                    {
                        element.ImageOptions.SvgImage = SvgIcons.MenuIcons[9];
                        element.ImageOptions.SvgImageSize = new Size(20, 20);
                    }

                    WireElementClick(element);

                    foreach (AccordionControlElement child in element.Elements)
                    {
                        if (_menuIconIndex.TryGetValue(child.Name, out int iconIndex))
                        {
                            child.ImageOptions.SvgImage = SvgIcons.MenuIcons[iconIndex];
                            child.ImageOptions.SvgImageSize = new Size(16, 16);
                        }

                        WireElementClick(child);
                    }
                }
            }
        }

        private void WireElementClick(AccordionControlElement element)
        {
            if (element.Style == ElementStyle.Item)
            {
                element.Click += (s, e) => HandleMenuClick((AccordionControlElement)s!);
            }

            foreach (AccordionControlElement child in element.Elements)
            {
                WireElementClick(child);
            }
        }

        private void HandleMenuClick(AccordionControlElement element)
        {
            if (element.Tag is not int index)
            {
                return;
            }

            if (index == 99)
            {
                Close();
                return;
            }

            if (index == 0)
            {
                OpenDashboard();
                return;
            }

            if (index >= 1 && index <= 8)
            {
                string groupName = _groupNames.TryGetValue(index, out string? name) ? name : "Modül";

                if (element.Text == "Kullanıcılar")
                {
                    OpenUsers();
                    return;
                }

                if (element.Text == "Roller ve Yetkiler")
                {
                    OpenRoles();
                    return;
                }

                if (element.Text == "Şirket Ayarları")
                {
                    OpenCompanies();
                    return;
                }

                if (element.Text == "Müşteriler")
                {
                    OpenCustomers();
                    return;
                }

                if (element.Text == "Tedarikçiler")
                {
                    OpenSuppliers();
                    return;
                }

                if (element.Text == "Hesap Planı")
                {
                    OpenChartOfAccounts();
                    return;
                }

                if (element.Text == "Ürünler")
                {
                    OpenProducts();
                    return;
                }

                if (element.Text == "Birim Cinsleri")
                {
                    OpenUnitTypes();
                    return;
                }

                OpenModule(element.Text, groupName, index);
            }
        }

        private void OpenUsers()
        {
            MdiFormManager.Instance.OpenForm<UsersListForm>(this, "Kullanıcılar");
        }

        private void OpenRoles()
        {
            MdiFormManager.Instance.OpenForm<RolesListForm>(this, "Roller ve Yetkiler");
        }

        private void OpenCompanies()
        {
            MdiFormManager.Instance.OpenForm<CompaniesListForm>(this, "Şirketler");
        }

        private void OpenCustomers()
        {
            MdiFormManager.Instance.OpenForm<CustomersListForm>(this, "Müşteriler");
        }

        private void OpenSuppliers()
        {
            MdiFormManager.Instance.OpenForm<SuppliersListForm>(this, "Tedarikçiler");
        }

        private void OpenChartOfAccounts()
        {
            MdiFormManager.Instance.OpenForm<ChartOfAccountsListForm>(this, "Hesap Planı");
        }

        private void OpenProducts()
        {
            MdiFormManager.Instance.OpenForm<ProductsListForm>(this, "Ürünler");
        }

        private void OpenUnitTypes()
        {
            MdiFormManager.Instance.OpenForm<ProductUnitTypesListForm>(this, "Birim Cinsleri");
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

            barButtonItemTime.Caption = $"🕐  {now:HH:mm:ss}";
            barButtonItemLongDate.Caption = $"📅  {now.ToString("dddd, dd MMMM yyyy", tr)}";

            if (_tokenExpiry != default)
            {
                TimeSpan remaining = _tokenExpiry - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 0)
                {
                    barButtonItemExpTime.Caption = "⏳  Oturum süresi doldu";
                    RestartToLogin();
                    return;
                }

                barButtonItemExpTime.Caption = $"⏳  Kalan Süre: {remaining:hh\\:mm\\:ss}";
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