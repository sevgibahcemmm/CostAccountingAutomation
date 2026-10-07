using Cost.Accounting.Automation.Application.WorkshopAnalysis;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Drawing;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.WorkshopAnalysisForms
{
    /// <summary>
    /// Atölye gelir/gider analizi.
    ///
    /// Varsayılan olarak TÜM atölyeleri kapsayan genel sayfa açılır. Lookup'tan
    /// bir atölye seçildiğinde tüm KPI'lar ve grafikler o atölyenin
    /// verilerine döner; lookup boşaltılırsa genel sayfaya dönülür.
    /// </summary>
public partial class WorkshopAnalysisForm : DevExpress.XtraEditors.XtraForm
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>Karşılaştırma grafiğinde gösterilecek en çok atölye sayısı.</summary>
        private const int MaxCompareRows = 15;

        public WorkshopAnalysisForm()
        {
            InitializeComponent();

            // Görsel tanımların tamamı Designer dosyasındadır; burada yalnızca
            // olay bağlantıları kurulur.
            Load += Form_Load;
            lookUpWorkshop.EditValueChanged += (_, _) => _ = ReloadAsync();
            dtFrom.EditValueChanged += (_, _) => _ = ReloadAsync();
            dtTo.EditValueChanged += (_, _) => _ = ReloadAsync();
        }

        private async void Form_Load(object? sender, EventArgs e)
        {
            // Varsayılan dönem: içinde bulunulan yılın başı -> bugün.
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            dtFrom.EditValue = new DateTime(today.Year, 1, 1);
            dtTo.EditValue = today.ToDateTime(TimeOnly.MinValue);

            await LoadWorkshopOptionsAsync();
            await ReloadAsync();
        }

        /// <summary>Lookup'ı atölye hesaplarıyla doldurur.</summary>
        private async Task LoadWorkshopOptionsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<IReadOnlyList<WorkshopOption>> result =
                    await mediator.Send(new WorkshopOptionsQuery(), CancellationToken.None);

                lookUpWorkshop.Properties.DataSource = result.Data ?? [];
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("WorkshopAnalysis.Options", ex);
                ToastHelper.Show("Atölye listesi yüklenemedi: " + ex.Message, ToastType.Error, 6000);
            }
        }

private void BtnRefresh_Click(object? sender, EventArgs e)
            => _ = ReloadAsync();

private async Task ReloadAsync()
        {
            if (dtFrom.EditValue is not DateTime fromDate || dtTo.EditValue is not DateTime toDate)
            {
                return;
            }

            DateOnly from = DateOnly.FromDateTime(fromDate);
            DateOnly to = DateOnly.FromDateTime(toDate);

            // Lookup boşsa genel sayfa; doluysa tek atölye.
            Guid? workshopId = lookUpWorkshop.EditValue is Guid id ? id : null;

            btnRefresh.Enabled = false;

            try
            {
                await LoadingHelper.RunAsync(
                    () => LoadDataAsync(from, to, workshopId),
                    caption: "Analiz hazırlanıyor...",
                    description: "Atölye verileri hesaplanıyor...");
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private async Task LoadDataAsync(DateOnly from, DateOnly to, Guid? workshopId)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<WorkshopAnalysisSnapshot> result =
                await mediator.Send(new WorkshopIncomeExpenseQuery(from, to, workshopId), CancellationToken.None);

            if (result.Data is not WorkshopAnalysisSnapshot snapshot)
            {
                ToastHelper.Show(
                    AuthFormStyles.GetErrorText(result.ErrorMessages),
                    ToastType.Error,
                    6000);
                return;
            }

            // Lookup her zaman tüm atölyeleri gösterir; seçim korunur.
            if (!ReferenceEquals(lookUpWorkshop.Properties.DataSource, snapshot.WorkshopOptions)
                && (lookUpWorkshop.Properties.DataSource as IReadOnlyList<WorkshopOption>)?.Count != snapshot.WorkshopOptions.Count)
            {
                lookUpWorkshop.Properties.DataSource = snapshot.WorkshopOptions;
            }

            ApplyKpis(snapshot);
            ApplyCharts(snapshot);
        }

        private void ApplyKpis(WorkshopAnalysisSnapshot snapshot)
        {
            lblRevenueValue.Text = snapshot.TotalRevenue.ToString("n2", Tr);
            lblExpenseValue.Text = snapshot.TotalExpense.ToString("n2", Tr);
            lblNetValue.Text = snapshot.Net.ToString("n2", Tr);
            lblRatioValue.Text = snapshot.ExpenseRatio.ToString("n1", Tr) + " %";

// Fark negatifse (gider > gelir) renk kırmızıya döner; bu normal bir
            // durumdur, atölye kâr zarar etmemiş olabilir. Rengin kendisi
            // Designer'da tanımlıdır, burada yalnızca koşula göre seçilir.
            lblNetValue.Appearance.ForeColor =
                snapshot.Net < 0m ? ExpenseColor : RevenueColor;

            lblSummary.Text = snapshot.IsAllWorkshops
                ? $"{snapshot.From:dd.MM.yyyy} - {snapshot.To:dd.MM.yyyy}   |   Tüm Atölyeler   |   {snapshot.Workshops.Count} atölyede hareket var"
                : $"{snapshot.From:dd.MM.yyyy} - {snapshot.To:dd.MM.yyyy}   |   {snapshot.ScopeTitle}";

            // Atölyeye bağlanamayan gelir varsa gizlenmez; aksi hâlde grafikler
            // boş görünür ve kullanıcı hatayı arar.
            lblUnattributed.Text = snapshot.UnattributedRevenue > 0m
                ? $"Not: {snapshot.UnattributedRevenue:n2} ₺ satış geliri hiçbir atölyeye bağlanamadı "
                  + "(atölyede üretilmemiş ürün). Bu tutar yukarıdaki gelir ve fark hesaplarına dahil edilmedi."
                : " ";
        }

        private void ApplyCharts(WorkshopAnalysisSnapshot snapshot)
        {
            // 1) Aylık fark
            List<(string Label, decimal Value)> netPoints =
            [
                .. snapshot.MonthlyTrend.Select(p => (p.Label, p.Net))
            ];

            DashboardChartLoader.LoadHorizontalBarValues(chartNet, netPoints, NetColor);

            // 2) Aylık gelir / gider
            List<(string Label, decimal First, decimal Second)> trendPoints =
            [
                .. snapshot.MonthlyTrend.Select(p => (p.Label, p.Revenue, p.Expense))
            ];

            DashboardChartLoader.LoadDualAreaValues(
                chartTrend,
                trendPoints,
                "Gelir",
                "Gider",
                RevenueColor,
                ExpenseColor);

            // 3) Atölye karşılaştırması
            List<(string Label, decimal Value)> comparePoints =
            [
                .. snapshot.Workshops
                    .OrderByDescending(w => w.Expense)
                    .Take(MaxCompareRows)
                    .Select(w => ($"{w.Code} {Truncate(w.Name, 28)}", w.Expense))
            ];

            DashboardChartLoader.LoadRankedBarValues(chartCompare, comparePoints);

            // 4) Gider dağılımı (hesap tipi)
            List<(string Label, decimal Value)> expensePoints =
            [
                .. snapshot.ExpenseBreakdown.Select(p => (Truncate(p.Label, 34), p.Amount))
            ];

DashboardChartLoader.LoadDoughnutValues(chartExpense, expensePoints);
        }

private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "-";
            }

            return value.Length <= max ? value : value[..(max - 1)] + "…";
        }
    }
}