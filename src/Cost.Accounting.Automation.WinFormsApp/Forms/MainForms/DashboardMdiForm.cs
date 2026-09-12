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
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    public partial class DashboardMdiForm : XtraFormMdiBase
    {
        private sealed record ChartPoint(string Label, int Count);

        private static readonly (string Title, string Icon, Color Accent)[] KpiDefs =
        {
            ("Şirket", "🏢", Color.FromArgb(37, 99, 235)),
            ("Kullanıcı", "👤", Color.FromArgb(22, 163, 74)),
            ("Rol", "🔑", Color.FromArgb(217, 119, 6)),
            ("Aktif Oturum", "🕐", Color.FromArgb(8, 145, 178)),
            ("Müşteri", "🤝", Color.FromArgb(13, 148, 136)),
            ("Tedarikçi", "🚚", Color.FromArgb(124, 58, 237)),
            ("Hesap Planı", "📒", Color.FromArgb(234, 88, 12)),
            ("Fotoğraf", "📷", Color.FromArgb(219, 39, 119))
        };

        private readonly SessionClaimContext _session;
        private readonly Dictionary<int, LabelControl> _kpiValues = new();
        private LabelControl _lblSub = null!;
        private LabelControl _lblDate = null!;
        private ChartControl _chartRoles = null!;
        private ChartControl _chartCompanies = null!;
        private ChartControl _chartAccounts = null!;

        public DashboardMdiForm() : base("Dashboard")
        {
            _session = Program.Services.GetRequiredService<SessionClaimContext>();
            Size = new Size(1280, 720);
            MinimumSize = new Size(1024, 640);
            IconOptions.SvgImage = SvgIcons.Modules[0];
            BuildLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSessionInfo();
            _ = LoadDashboardDataAsync();
        }

        private void BuildLayout()
        {
            Panel pnlHeader = new() { Dock = DockStyle.Top, Height = 118 };
            pnlHeader.Paint += PnlHeader_Paint;

            LabelControl lblWelcome = new()
            {
                AutoSize = false,
                Location = new Point(34, 22),
                Size = new Size(560, 48),
                Text = "Hoş Geldiniz!",
                Appearance = { Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Color.White }
            };

            _lblSub = new LabelControl
            {
                AutoSize = false,
                Location = new Point(36, 82),
                Size = new Size(700, 26),
                Text = "-",
                Appearance = { Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(226, 232, 240) }
            };

            _lblDate = new LabelControl
            {
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 46),
                Size = new Size(700, 28),
                Text = "-",
                Appearance = { Font = new Font("Segoe UI", 11F), ForeColor = Color.FromArgb(226, 232, 240) }
            };
            _lblDate.Appearance.TextOptions.HAlignment = HorzAlignment.Far;

            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(_lblSub);
            pnlHeader.Controls.Add(_lblDate);

            Panel pnlBody = new() { Dock = DockStyle.Fill, Padding = new Padding(24) };

            TableLayoutPanel tblLayout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));

            TableLayoutPanel tblKpi = CreateKpiGrid();
            TableLayoutPanel tblCharts = CreateChartsGrid();

            tblLayout.Controls.Add(tblKpi, 0, 0);
            tblLayout.Controls.Add(tblCharts, 0, 1);

            pnlBody.Controls.Add(tblLayout);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
        }

        private TableLayoutPanel CreateKpiGrid()
        {
            TableLayoutPanel tbl = new() { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Margin = new Padding(0) };
            for (int i = 0; i < 4; i++)
            {
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }
            for (int i = 0; i < 2; i++)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            }

            int index = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    (string title, string icon, Color accent) = KpiDefs[index];
                    tbl.Controls.Add(CreateKpiCard(title, icon, accent, index + 1), col, row);
                    index++;
                }
            }

            return tbl;
        }

        private PanelControl CreateKpiCard(string title, string icon, Color accent, int index)
        {
            PanelControl card = new() { Dock = DockStyle.Fill, Margin = new Padding(3), Padding = new Padding(0, 0, 0, 4) };

            Panel acc = new() { Dock = DockStyle.Left, Width = 6, BackColor = accent };

            LabelControl lblValue = new()
            {
                AutoSize = false,
                Location = new Point(22, 14),
                Size = new Size(200, 42),
                Text = "-",
                Appearance = { Font = new Font("Segoe UI", 21F, FontStyle.Bold) }
            };

            LabelControl lblTitle = new()
            {
                AutoSize = false,
                Location = new Point(24, 60),
                Size = new Size(200, 20),
                Text = title,
                Appearance = { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) }
            };

            LabelControl lblIcon = new()
            {
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 18),
                Size = new Size(44, 40),
                Text = icon,
                Font = new Font("Segoe UI Emoji", 15F)
            };
            lblIcon.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            lblIcon.Appearance.TextOptions.VAlignment = VertAlignment.Center;

            card.Controls.Add(acc);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblIcon);

            _kpiValues[index] = lblValue;
            return card;
        }

        private TableLayoutPanel CreateChartsGrid()
        {
            TableLayoutPanel tbl = new() { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 3, 0, 0) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            tbl.Controls.Add(CreateChartPanel("Rol Dağılımı", out _chartRoles), 0, 0);
            tbl.Controls.Add(CreateChartPanel("Şirket Bazlı Kullanıcılar", out _chartCompanies), 1, 0);
            tbl.Controls.Add(CreateChartPanel("Hesap Planı Türleri", out _chartAccounts), 2, 0);

            return tbl;
        }

        private static Panel CreateChartPanel(string title, out ChartControl chart)
        {
            Panel panel = new() { Dock = DockStyle.Fill, Margin = new Padding(3), Padding = new Padding(16, 38, 16, 12) };

            LabelControl lblTitle = new()
            {
                AutoSize = false,
                Location = new Point(18, 12),
                Size = new Size(300, 24),
                Text = title,
                Appearance = { Font = new Font("Segoe UI", 11F, FontStyle.Bold) }
            };

            chart = new ChartControl { Dock = DockStyle.Fill };

            panel.Controls.Add(chart);
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private void LoadSessionInfo()
        {
            string roleName;
            try
            {
                roleName = _session.GetRoleName();
            }
            catch
            {
                roleName = "Kullanıcı";
            }

            string companyName = "-";
            try
            {
                string? token = _session.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    companyName = jwt.Claims.FirstOrDefault(c => c.Type == "company")?.Value ?? "-";
                }
            }
            catch
            {
            }

            _lblSub.Text = $"Rol: {roleName}   •   Kurum: {companyName}";
            _lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", CultureInfo.GetCultureInfo("tr-TR"));
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                using IServiceScope scope = Program.Services.CreateScope();
                ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                int companyCount = await db.Set<Company>().CountAsync();
                int userCount = await db.Set<User>().CountAsync();
                int roleCount = await db.Set<Role>().CountAsync();
                int activeSessionCount = await db.Set<LoginToken>().CountAsync(t => t.IsActive.Value);
                int customerCount = await db.Set<Customer>().CountAsync();
                int supplierCount = await db.Set<Supplier>().CountAsync();
                int accountCount = await db.Set<ChartOfAccount>().CountAsync();
                int photoCount = await db.Set<Photo>().CountAsync();

                SetKpi(1, companyCount);
                SetKpi(2, userCount);
                SetKpi(3, roleCount);
                SetKpi(4, activeSessionCount);
                SetKpi(5, customerCount);
                SetKpi(6, supplierCount);
                SetKpi(7, accountCount);
                SetKpi(8, photoCount);

                List<ChartPoint> roleData = await LoadRoleDistributionAsync(db);
                List<ChartPoint> companyData = await LoadCompanyDistributionAsync(db);
                List<ChartPoint> accountData = await LoadAccountTypeDistributionAsync(db);

                LoadDoughnut(_chartRoles, roleData);
                LoadBar(_chartCompanies, companyData);
                LoadDoughnut(_chartAccounts, accountData);
            }
            catch
            {
                for (int i = 1; i <= 8; i++)
                {
                    SetKpi(i, null);
                }
            }
        }

        private void SetKpi(int index, int? value)
        {
            if (_kpiValues.TryGetValue(index, out LabelControl? label))
            {
                label.Text = value?.ToString("N0") ?? "-";
            }
        }

        private static async Task<List<ChartPoint>> LoadRoleDistributionAsync(ApplicationDbContext db)
        {
            var rows = await (from u in db.Set<User>()
                              join r in db.Set<Role>() on u.RoleId equals r.Id
                              group u by r.Name.Value into g
                              select new { Label = g.Key, Count = g.Count() })
                             .ToListAsync();

            return rows.Select(x => new ChartPoint(x.Label, x.Count)).ToList();
        }

        private static async Task<List<ChartPoint>> LoadCompanyDistributionAsync(ApplicationDbContext db)
        {
            var rows = await (from u in db.Set<User>()
                              join c in db.Set<Company>() on u.CompanyId equals c.Id
                              group u by c.Name.Value into g
                              orderby g.Count() descending
                              select new { Label = g.Key, Count = g.Count() })
                             .ToListAsync();

            return rows.Select(x => new ChartPoint(x.Label, x.Count)).ToList();
        }

        private static async Task<List<ChartPoint>> LoadAccountTypeDistributionAsync(ApplicationDbContext db)
        {
            var rows = await (from a in db.Set<ChartOfAccount>()
                              group a by a.Type into g
                              select new { Type = g.Key, Count = g.Count() })
                             .ToListAsync();

            return rows
                .Select(x => new ChartPoint(AccountTypeText(x.Type), x.Count))
                .ToList();
        }

        private static string AccountTypeText(ChartOfAccountType type) => type switch
        {
            ChartOfAccountType.MainGroup => "Ana Grup",
            ChartOfAccountType.Warehouse => "Depo",
            ChartOfAccountType.Category => "Kategori",
            ChartOfAccountType.Workshop => "Atölye",
            _ => type.ToString()
        };

        private static void LoadDoughnut(ChartControl chart, List<ChartPoint> data)
        {
            chart.Series.Clear();

            Series series = new("Dağılım", ViewType.Doughnut)
            {
                DataSource = data,
                ArgumentDataMember = nameof(ChartPoint.Label)
            };
            series.ValueDataMembers.AddRange(nameof(ChartPoint.Count));
            series.LabelsVisibility = DefaultBoolean.False;

            chart.Series.Add(series);
            chart.Legend.Visibility = DefaultBoolean.True;
        }

        private static void LoadBar(ChartControl chart, List<ChartPoint> data)
        {
            chart.Series.Clear();

            Series series = new("Kullanıcı", ViewType.Bar)
            {
                DataSource = data,
                ArgumentDataMember = nameof(ChartPoint.Label)
            };
            series.ValueDataMembers.AddRange(nameof(ChartPoint.Count));

            chart.Series.Add(series);
            chart.Legend.Visibility = DefaultBoolean.False;

            if (chart.Diagram is XYDiagram diagram)
            {
                diagram.Rotated = false;
                diagram.AxisX.Title.Visibility = DefaultBoolean.True;
                diagram.AxisX.Title.Text = "Şirket";
                diagram.AxisY.Title.Visibility = DefaultBoolean.True;
                diagram.AxisY.Title.Text = "Kullanıcı Sayısı";
            }
        }
    }
}