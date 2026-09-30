using Cost.Accounting.Automation.Application.Dashboards;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraCharts;
using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;

/// <summary>
/// Dashboard grafiklerini besleyen yükleyiciler.
///
/// Tüm metotlar grafiği <c>BeginInit/EndInit</c> çifti içinde tek seferde yeniden
/// kurar; boyama sırasında denetim yapılmasının aksine çok daha hızlı çalışır.
/// Renkler <see cref="DashboardChartPalette"/> üzerinden skin'e göre normalize edilir.
/// </summary>
internal static class DashboardChartLoader
{
    private const string CurrencyFormat = "n0";
    private const string QuantityFormat = "n0";

    // ------------------------------------------------------------------ bar

    /// <summary>Yatay bar: her çubuk ayrı renk alır (kategori bazlı okunurluk).</summary>
    public static void LoadRankedBar(
        ChartControl chart,
        IReadOnlyList<DashboardRankPoint> data)
    {
        Render(chart, "Miktar", series =>
        {
            ApplyBarFill(
                (BarSeriesView)series.View,
                DashboardChartPalette.Primary);

            series.Points.AddRange(
                data
                    .Select(d => new SeriesPoint(d.Label, (double)d.Quantity))
                    .ToArray());

            // Sıralamaya göre her çubuğa farklı canlı renk veriyoruz.
            Color[] palette = DashboardChartPalette.Categorical(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                series.Points[i].Color = palette[i];
            }
        }, horizontal: true, valueFormat: QuantityFormat);
    }

    /// <summary>Tek serili yatay bar (alacak / borç / kategori değeri listeleri).</summary>
    public static void LoadHorizontalBar(
        ChartControl chart,
        IReadOnlyList<DashboardBalancePoint> data,
        Color color)
    {
        Render(chart, "Tutar", series =>
        {
            ApplyBarFill(
                (BarSeriesView)series.View,
                color);

            series.Points.AddRange(
                data
                    .Select(d => new SeriesPoint(d.Label, (double)d.Amount))
                    .ToArray());
        }, horizontal: true, valueFormat: CurrencyFormat);
    }

    /// <summary>İki serili gruplu bar (giriş/çıkış).</summary>
    public static void LoadGroupedBar(
        ChartControl chart,
        IReadOnlyList<DashboardDualPoint> data,
        string firstCaption,
        string secondCaption,
        Color firstColor,
        Color secondColor)
    {
        chart.BeginInit();

        try
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                return;
            }

            chart.Series.Add(
                CreateBarSeries(
                    data,
                    firstCaption,
                    nameof(DashboardDualPoint.First),
                    firstColor));

            chart.Series.Add(
                CreateBarSeries(
                    data,
                    secondCaption,
                    nameof(DashboardDualPoint.Second),
                    secondColor));

            ConfigureLegend(chart);
            ConfigureAxes(chart, horizontal: false, valueFormat: QuantityFormat);
        }
        finally
        {
            chart.EndInit();
        }
    }

    // ------------------------------------------------------------------ line / area

    /// <summary>İki serili alan trendi (aylık alacak ve borç seyri).</summary>
    public static void LoadDualArea(
        ChartControl chart,
        IReadOnlyList<DashboardDualPoint> data,
        string firstCaption,
        string secondCaption,
        Color firstColor,
        Color secondColor)
    {
        chart.BeginInit();

        try
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                return;
            }

            chart.Series.Add(
                CreateAreaSeries(
                    data,
                    firstCaption,
                    nameof(DashboardDualPoint.First),
                    firstColor));

            chart.Series.Add(
                CreateAreaSeries(
                    data,
                    secondCaption,
                    nameof(DashboardDualPoint.Second),
                    secondColor));

            ConfigureLegend(chart);
            ConfigureAxes(chart, horizontal: false, valueFormat: CurrencyFormat);
        }
        finally
        {
            chart.EndInit();
        }
    }

    /// <summary>Tek serili alan trendi (aylık ciro).</summary>
    public static void LoadArea(
        ChartControl chart,
        IReadOnlyList<DashboardBalancePoint> data,
        Color color)
    {
        Render(chart, "Tutar", series =>
        {
            ApplyAreaFill(
                (AreaSeriesView)series.View,
                color);

            series.DataSource = data;
            series.ArgumentDataMember = nameof(DashboardBalancePoint.Label);
            series.ValueDataMembers.AddRange(nameof(DashboardBalancePoint.Amount));
        }, horizontal: false, valueFormat: CurrencyFormat, viewType: ViewType.Area);
    }

    // ------------------------------------------------------------------ doughnut

    /// <summary>Dağılım grafiği: her dilim ayrı renk alır.</summary>
    public static void LoadDoughnut(
        ChartControl chart,
        IReadOnlyList<DashboardChartPoint> data)
    {
        chart.BeginInit();

        try
        {
            chart.Series.Clear();

            if (data.Count == 0)
            {
                chart.Legend.Visibility = DefaultBoolean.False;
                return;
            }

            Series series =
                new("Dağılım", ViewType.Doughnut)
                {
                    LabelsVisibility = DefaultBoolean.True
                };

            series.Label.TextPattern = "{A}: {V}";

            if (series.View is DoughnutSeriesView view)
            {
                view.HoleRadiusPercent = 58;
                view.Border.Visibility = DefaultBoolean.True;
                view.Border.Color = Color.White;
                view.Border.Thickness = 1;
            }

            Color[] palette = DashboardChartPalette.Categorical(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                int pointIndex =
                    series.Points.Add(
                        new SeriesPoint(
                            data[i].Label,
                            data[i].Count));

                series.Points[pointIndex].Color = palette[i];
            }

            chart.Series.Add(series);
            ConfigureLegend(chart);
        }
        finally
        {
            chart.EndInit();
        }
    }

    // ------------------------------------------------------------------ yardımcılar

    private static void Render(
        ChartControl chart,
        string seriesCaption,
        Action<Series> configure,
        bool horizontal,
        string valueFormat,
        ViewType viewType = ViewType.Bar)
    {
        chart.BeginInit();

        try
        {
            chart.Series.Clear();

            chart.Legend.Visibility = DefaultBoolean.False;

            Series series =
                new(seriesCaption, viewType)
                {
                    LabelsVisibility = DefaultBoolean.False
                };

            configure(series);

            chart.Series.Add(series);
            ConfigureAxes(chart, horizontal, valueFormat);
        }
        finally
        {
            chart.EndInit();
        }
    }

    private static Series CreateBarSeries(
        IReadOnlyList<DashboardDualPoint> data,
        string caption,
        string valueMember,
        Color color)
    {
        Series series =
            new(caption, ViewType.Bar)
            {
                DataSource = data,
                ArgumentDataMember = nameof(DashboardDualPoint.Label),
                LabelsVisibility = DefaultBoolean.False
            };

        series.ValueDataMembers.AddRange(valueMember);

        if (series.View is BarSeriesView view)
        {
            view.Border.Visibility = DefaultBoolean.False;
            view.Color = color;
        }

        return series;
    }

    private static Series CreateAreaSeries(
        IReadOnlyList<DashboardDualPoint> data,
        string caption,
        string valueMember,
        Color color)
    {
        Series series =
            new(caption, ViewType.Area)
            {
                DataSource = data,
                ArgumentDataMember = nameof(DashboardDualPoint.Label),
                LabelsVisibility = DefaultBoolean.False
            };

        series.ValueDataMembers.AddRange(valueMember);

        if (series.View is AreaSeriesView view)
        {
            ApplyAreaFill(
                view,
                color);

            // Alan serisinin üst kenarını kalınlaştırıp veri okunurluğu artırıyoruz.
            view.Border.Visibility = DefaultBoolean.True;
            view.Border.Color = color;
            view.Border.Thickness = 2;
        }

        return series;
    }

    /// <summary>
    /// Yatay çubuk dolgusu.
    ///
    /// NOT: DevExpress XtraCharts 25.2'de degrade dolgu (<c>FillStyle.Options</c>)
    /// yalnızca tasarım zamanında ayarlanabilir; çalışma zamanında
    /// "This property can't be customized at runtime" hatası verir. Bu yüzden
    /// paletin canlı rengi düz dolgu olarak uygulanır.
    /// </summary>
    private static void ApplyBarFill(
        BarSeriesView view,
        Color color)
    {
        view.Border.Visibility = DefaultBoolean.False;
        view.Color = DashboardChartPalette.Normalize(color);
    }

    private static void ApplyAreaFill(
        AreaSeriesView view,
        Color color)
    {
        // Alanda hafif saydamlık, iki seri üst üste bindiğinde hangisinin
        // ne olduğunu ayırt etmeyi kolaylaştırır.
        view.Color = DashboardChartPalette.Normalize(color);
        view.FillStyle.FillMode = FillMode.Solid;
        view.Transparency = 45;
    }

    private static void ConfigureLegend(ChartControl chart)
    {
        chart.Legend.Visibility = DefaultBoolean.True;
        chart.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
        chart.Legend.AlignmentVertical = LegendAlignmentVertical.Bottom;
        chart.Legend.EnableAntialiasing = DefaultBoolean.True;
    }

    private static void ConfigureAxes(
        ChartControl chart,
        bool horizontal,
        string valueFormat)
    {
        if (chart.Diagram is not XYDiagram diagram)
        {
            return;
        }

        diagram.Rotated = horizontal;
        diagram.AxisX.Title.Visibility = DefaultBoolean.False;
        diagram.AxisY.Title.Visibility = DefaultBoolean.False;
        diagram.AxisX.Label.TextPattern = "{A}";
        diagram.AxisY.Label.TextPattern = $"{{V:{valueFormat}}}";
        diagram.AxisY.WholeRange.Auto = true;
        diagram.AxisX.GridLines.Visible = false;
        diagram.AxisY.GridLines.Visible = false;
        diagram.AxisX.Tickmarks.Visible = false;
        diagram.AxisY.Tickmarks.Visible = false;
    }
}
