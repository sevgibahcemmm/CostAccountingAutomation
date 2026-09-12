using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Companies;
using Cost.Accounting.Automation.Domain.Customers;
using Cost.Accounting.Automation.Domain.LoginTokens;
using Cost.Accounting.Automation.Domain.Photos;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Suppliers;
using Cost.Accounting.Automation.Domain.Users;
using Cost.Accounting.Automation.Infrastructure.Context;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.Utils.Colors;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class DashboardMdiForm : XtraFormMdiBase
    {
        private sealed record ChartPoint(string Label, int Count);

        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, Label> _kpiValues = new();

        private Color SkinPrimaryColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.FillColors.Primary,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinSuccessColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.FillColors.Success,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinWarningColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.FillColors.Warning,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinDangerColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.FillColors.Danger,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinQuestionColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.FillColors.Question,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinTextColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.ForeColors.WindowText,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        private Color SkinSecondaryTextColor =>
            DXSkinColorHelper.GetDXSkinColor(
                DXSkinColors.ForeColors.DisabledText,
                LookAndFeel.ActiveSkinName,
                LookAndFeel.ActiveSvgPaletteName);

        public DashboardMdiForm() : base("Dashboard")
        {
            _session =
                Program.Services
                    .GetRequiredService<SessionClaimContext>();

            Size =
                new Size(1280, 720);

            MinimumSize =
                new Size(1024, 640);

            IconOptions.SvgImage =
                SvgIcons.Modules[0];

            InitializeComponent();

            _kpiValues[1] = lblKpi1Value;
            _kpiValues[2] = lblKpi2Value;
            _kpiValues[3] = lblKpi3Value;
            _kpiValues[4] = lblKpi4Value;
            _kpiValues[5] = lblKpi5Value;
            _kpiValues[6] = lblKpi6Value;
            _kpiValues[7] = lblKpi7Value;
            _kpiValues[8] = lblKpi8Value;

            LookAndFeel.StyleChanged += LookAndFeel_StyleChanged;

            BuildDashboardAppearance();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BuildDashboardAppearance();

            LoadSessionInfo();

            _ = LoadDashboardDataAsync();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LookAndFeel.StyleChanged -= LookAndFeel_StyleChanged;

            base.OnFormClosed(e);
        }

        private void LookAndFeel_StyleChanged(object? sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (!IsHandleCreated)
                return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || Disposing)
                    return;

                BuildDashboardAppearance();
            }));
        }

        private void BuildDashboardAppearance()
        {
            Color primary =
                SkinPrimaryColor;

            Color success =
                SkinSuccessColor;

            Color warning =
                SkinWarningColor;

            Color danger =
                SkinDangerColor;

            Color question =
                SkinQuestionColor;

            Color textColor =
                SkinTextColor;

            Color secondaryTextColor =
                SkinSecondaryTextColor;

            pnlHeader.BackColor =
                primary;

            pnlBody.BackColor =
                Color.Transparent;

            lblWelcome.ForeColor =
                Color.White;

            lblSub.ForeColor =
                Color.White;

            lblDate.ForeColor =
                Color.White;

            Color[] accents =
            {
                primary,
                success,
                warning,
                primary,
                question,
                danger,
                success,
                warning
            };

            PanelControl[] cards =
            {
                kpi1,
                kpi2,
                kpi3,
                kpi4,
                kpi5,
                kpi6,
                kpi7,
                kpi8
            };

            Panel[] accentPanels =
            {
                acc1,
                acc2,
                acc3,
                acc4,
                acc5,
                acc6,
                acc7,
                acc8
            };

            Panel[] iconBadges =
            {
                pnlKpi1IconBadge,
                pnlKpi2IconBadge,
                pnlKpi3IconBadge,
                pnlKpi4IconBadge,
                pnlKpi5IconBadge,
                pnlKpi6IconBadge,
                pnlKpi7IconBadge,
                pnlKpi8IconBadge
            };

            Label[] valueLabels =
            {
                lblKpi1Value,
                lblKpi2Value,
                lblKpi3Value,
                lblKpi4Value,
                lblKpi5Value,
                lblKpi6Value,
                lblKpi7Value,
                lblKpi8Value
            };

            Label[] titleLabels =
            {
                lblKpi1Title,
                lblKpi2Title,
                lblKpi3Title,
                lblKpi4Title,
                lblKpi5Title,
                lblKpi6Title,
                lblKpi7Title,
                lblKpi8Title
            };

            for (int i = 0; i < cards.Length; i++)
            {
                Color accent =
                    accents[i];

                // Kartları belirgin kılmak için Simple border ve aralarında boşluk için Margin
                cards[i].BorderStyle =
                    DevExpress.XtraEditors.Controls.BorderStyles.Simple;

                cards[i].Margin =
                    new Padding(8);

                cards[i].Appearance.Options.UseBackColor =
                    false;

                cards[i].Appearance.Options.UseForeColor =
                    false;

                accentPanels[i].BackColor =
                    accent;

                iconBadges[i].BackColor =
                    CreateTransparentColor(
                        accent,
                        32);

                valueLabels[i].ForeColor =
                    textColor;

                titleLabels[i].ForeColor =
                    secondaryTextColor;
            }

            ConfigureChartAppearance(
                pnlChartRoles,
                lblChartRolesTitle,
                chartRoles,
                "Rol Dağılımı");

            ConfigureChartAppearance(
                pnlChartCompanies,
                lblChartCompaniesTitle,
                chartCompanies,
                "Şirket Bazlı Kullanıcılar");

            ConfigureChartAppearance(
                pnlChartAccounts,
                lblChartAccountsTitle,
                chartAccounts,
                "Hesap Planı Türleri");

            Refresh();
        }

        private static Color CreateTransparentColor(
            Color color,
            int alpha)
        {
            return Color.FromArgb(
                alpha,
                color.R,
                color.G,
                color.B);
        }

        private void ConfigureChartAppearance(
            PanelControl panel,
            Label title,
            ChartControl chart,
            string caption)
        {
            var textColor =
                DXSkinColorHelper.GetDXSkinColor(
                    DXSkinColors.ForeColors.WindowText,
                    this.LookAndFeel.ActiveSkinName,
                    this.LookAndFeel.ActiveSvgPaletteName);

            // Grafik panellerini de belirgin kartlar haline getirip aralarına boşluk ekledik
            panel.BorderStyle = BorderStyles.Simple;
            panel.Margin = new Padding(8);
            panel.Appearance.Options.UseBackColor = false;
            panel.Appearance.Options.UseForeColor = false;

            title.Text = caption;
            title.ForeColor = textColor;
            title.BackColor = Color.Transparent;
            title.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            chart.BackColor = Color.Transparent;
            chart.BorderOptions.Visibility = DefaultBoolean.False;
            chart.Legend.EnableAntialiasing = DefaultBoolean.True;
        }

        private void LoadSessionInfo()
        {
            string roleName;

            try
            {
                roleName =
                    _session.GetRoleName();
            }
            catch
            {
                roleName =
                    "Kullanıcı";
            }

            string companyName =
                "-";

            try
            {
                string? token =
                    _session.Token;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    JwtSecurityToken jwt =
                        new JwtSecurityTokenHandler()
                            .ReadJwtToken(token);

                    companyName =
                        jwt.Claims
                            .FirstOrDefault(
                                c => c.Type == "company")
                            ?.Value
                        ?? "-";
                }
            }
            catch
            {
                companyName =
                    "-";
            }

            lblSub.Text =
                $"Rol: {roleName}   •   Kurum: {companyName}";

            lblDate.Text =
                DateTime.Now.ToString(
                    "dddd, dd MMMM yyyy",
                    CultureInfo.GetCultureInfo("tr-TR"));
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                using IServiceScope scope =
                    Program.Services.CreateScope();

                ApplicationDbContext db =
                    scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                int companyCount =
                    await db.Set<Company>()
                        .CountAsync();

                int userCount =
                    await db.Set<User>()
                        .CountAsync();

                int roleCount =
                    await db.Set<Role>()
                        .CountAsync();

                int activeSessionCount =
                    await db.Set<LoginToken>()
                        .CountAsync(
                            t => t.IsActive.Value);

                int customerCount =
                    await db.Set<Customer>()
                        .CountAsync();

                int supplierCount =
                    await db.Set<Supplier>()
                        .CountAsync();

                int accountCount =
                    await db.Set<ChartOfAccount>()
                        .CountAsync();

                int photoCount =
                    await db.Set<Photo>()
                        .CountAsync();

                SetKpi(1, companyCount);
                SetKpi(2, userCount);
                SetKpi(3, roleCount);
                SetKpi(4, activeSessionCount);
                SetKpi(5, customerCount);
                SetKpi(6, supplierCount);
                SetKpi(7, accountCount);
                SetKpi(8, photoCount);

                List<ChartPoint> roleData =
                    await LoadRoleDistributionAsync(db);

                List<ChartPoint> companyData =
                    await LoadCompanyDistributionAsync(db);

                List<ChartPoint> accountData =
                    await LoadAccountTypeDistributionAsync(db);

                LoadDoughnut(chartRoles, roleData);
                LoadBar(chartCompanies, companyData);
                LoadDoughnut(chartAccounts, accountData);

                BuildDashboardAppearance();
            }
            catch
            {
                for (int i = 1; i <= 8; i++)
                {
                    SetKpi(i, null);
                }
            }
        }

        private void SetKpi(
            int index,
            int? value)
        {
            if (_kpiValues.TryGetValue(
                    index,
                    out Label? label))
            {
                label.Text =
                    value?.ToString("N0")
                    ?? "-";
            }
        }

        private static async Task<List<ChartPoint>>
            LoadRoleDistributionAsync(
                ApplicationDbContext db)
        {
            var rows =
                await
                (
                    from u in db.Set<User>()
                    join r in db.Set<Role>()
                        on u.RoleId equals r.Id
                    group u by r.Name.Value
                    into g
                    select new
                    {
                        Label = g.Key,
                        Count = g.Count()
                    }
                ).ToListAsync();

            return rows
                .Select(
                    x =>
                        new ChartPoint(
                            x.Label,
                            x.Count))
                .ToList();
        }

        private static async Task<List<ChartPoint>>
            LoadCompanyDistributionAsync(
                ApplicationDbContext db)
        {
            var rows =
                await
                (
                    from u in db.Set<User>()
                    join c in db.Set<Company>()
                        on u.CompanyId equals c.Id
                    group u by c.Name.Value
                    into g
                    orderby g.Count() descending
                    select new
                    {
                        Label = g.Key,
                        Count = g.Count()
                    }
                ).ToListAsync();

            return rows
                .Select(
                    x =>
                        new ChartPoint(
                            x.Label,
                            x.Count))
                .ToList();
        }

        private static async Task<List<ChartPoint>>
            LoadAccountTypeDistributionAsync(
                ApplicationDbContext db)
        {
            var rows =
                await
                (
                    from a in db.Set<ChartOfAccount>()
                    group a by a.Type
                    into g
                    select new
                    {
                        Type = g.Key,
                        Count = g.Count()
                    }
                ).ToListAsync();

            return rows
                .Select(
                    x =>
                        new ChartPoint(
                            AccountTypeText(x.Type),
                            x.Count))
                .ToList();
        }

        private static string AccountTypeText(
            ChartOfAccountType type)
        {
            return type switch
            {
                ChartOfAccountType.MainGroup =>
                    "Ana Grup",

                ChartOfAccountType.Warehouse =>
                    "Depo",

                ChartOfAccountType.Category =>
                    "Kategori",

                ChartOfAccountType.Workshop =>
                    "Atölye",

                _ =>
                    type.ToString()
            };
        }

        private static void LoadDoughnut(
            ChartControl chart,
            List<ChartPoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series series =
                new Series(
                    "Dağılım",
                    ViewType.Doughnut)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(ChartPoint.Label)
                };

            series.ValueDataMembers.AddRange(
                nameof(ChartPoint.Count));

            series.LabelsVisibility =
                DefaultBoolean.False;

            if (series.View is DoughnutSeriesView view)
            {
                view.HoleRadiusPercent =
                    65;
            }

            chart.Series.Add(series);

            chart.Legend.Visibility =
                DefaultBoolean.True;

            chart.Legend.AlignmentHorizontal =
                LegendAlignmentHorizontal.Center;

            chart.Legend.AlignmentVertical =
                LegendAlignmentVertical.Bottom;

            if (chart.Diagram is SimpleDiagram diagram)
            {
                chart.Legend.EnableAntialiasing = DefaultBoolean.True;
            }
        }

        private static void LoadBar(
            ChartControl chart,
            List<ChartPoint> data)
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility =
                    DefaultBoolean.False;

                return;
            }

            Series series =
                new Series(
                    "Kullanıcı",
                    ViewType.Bar)
                {
                    DataSource = data,
                    ArgumentDataMember =
                        nameof(ChartPoint.Label)
                };

            series.ValueDataMembers.AddRange(
                nameof(ChartPoint.Count));

            chart.Series.Add(series);

            chart.Legend.Visibility =
                DefaultBoolean.False;

            if (chart.Diagram is XYDiagram diagram)
            {
                diagram.Rotated =
                    false;

                diagram.AxisX.Title.Visibility =
                    DefaultBoolean.False;

                diagram.AxisY.Title.Visibility =
                    DefaultBoolean.False;

                diagram.AxisX.Label.TextPattern =
                    "{A}";

                diagram.AxisY.Label.TextPattern =
                    "{V:N0}";

                diagram.AxisY.WholeRange.Auto =
                    true;
            }
        }
    }
}