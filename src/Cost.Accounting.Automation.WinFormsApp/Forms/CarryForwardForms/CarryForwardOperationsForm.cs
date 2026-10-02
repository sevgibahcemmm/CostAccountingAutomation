using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.CarryForwards;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraGrid.Columns;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CarryForwardForms;

/// <summary>
/// Devir işlemleri: aynı şirketin bir önceki mali yılından seçili yıla hesap
/// planı ve açılış bakiyelerini aktarır.
///
/// Ekranın amacı yıl sonunda "yeni yıla ne devredilecek?" sorusunu tek bakışta
/// cevaplamaktır: üstte dört özet kartı (stok devri, alacak, borç, hesap planı),
/// ortada devredilecek kayıtların dökümü, altında aktarılacak veri grupları.
/// </summary>
public sealed partial class CarryForwardOperationsForm : XtraFormMdiBase
{
    /// <summary>Onay diyaloğunda tutarları göstermek için son başarılı ön izleme.</summary>
    private CarryForwardPreviewResult? _lastPreview;

    /// <summary>Skin değişiminde renkleri yeniden çözümlemek için abonelik.</summary>
    private readonly IDisposable? _skinBinding;

    public CarryForwardOperationsForm() : base("Devir İşlemleri")
    {
        InitializeComponent();

        IconOptions.SvgImage = DxIcon.Restore;

        chkAccounts.Checked = true;
        chkTaxUnits.Checked = true;
        chkCurrent.Checked = true;
        chkProducts.Checked = true;
        chkPrices.Checked = true;
        chkRecipes.Checked = true;
        chkChartBalances.Checked = true;

        btnClose.Click += (_, _) => Close();
        btnStart.Click += BtnStart_Click;
        btnRefresh.Click += async (_, _) => await RefreshPreviewAsync();

        ApplySkin();
        _skinBinding = SkinTheme.Bind(ApplySkin);

        ShowStatus("Devir ön izlemesi hazırlanıyor...", false);
        ClearSummaryCards();

        Shown += async (_, _) => await RefreshPreviewAsync();
    }

    /// <summary>
    /// Panellerin kendi zemin rengi atanmaz; yalnızca renk vurgusu gereken ince
    /// şeritler ve metin renkleri aktif skinden çözülür. Böylece form, seçili
    /// DevExpress skin'inin şablonuyla birebir uyumlu kalır ve skin değiştiğinde
    /// kendiliğinden güncellenir.
    /// </summary>
    private void ApplySkin()
    {
        Color surface = SkinTheme.SurfaceOf(this);
        Color primary = SkinTheme.Text;
        Color muted = SkinTheme.MutedText(surface);

        SetStrip(accentBar, SkinTheme.Primary);
        SetStrip(cardStockStrip, SkinTheme.Primary);
        SetStrip(cardReceivableStrip, SkinTheme.Warning);
        SetStrip(cardPayableStrip, SkinTheme.Danger);
        SetStrip(cardChartStrip, SkinTheme.Success);
        SetStrip(headerDivider, SkinTheme.BorderMuted(surface));

        SetForeColor(lblHeaderTitle, primary);
        SetForeColor(lblHeaderSub, muted);
        SetForeColor(lblPreviewTitle, SkinTheme.Primary);
        SetForeColor(lblStockTitle, muted);
        SetForeColor(lblReceivableTitle, muted);
        SetForeColor(lblPayableTitle, muted);
        SetForeColor(lblChartTitle, muted);
        SetForeColor(lblStockNote, muted);
        SetForeColor(lblReceivableNote, muted);
        SetForeColor(lblPayableNote, muted);
        SetForeColor(lblChartNote, muted);
        SetForeColor(lblStockValue, primary);
        SetForeColor(lblReceivableValue, primary);
        SetForeColor(lblPayableValue, primary);
        SetForeColor(lblChartValue, primary);
    }

    private static void SetStrip(DevExpress.XtraEditors.PanelControl panel, Color color)
    {
        panel.Appearance.BackColor = color;
        panel.Appearance.Options.UseBackColor = true;
    }

    private static void SetForeColor(DevExpress.XtraEditors.LabelControl label, Color color)
    {
        label.Appearance.ForeColor = color;
        label.Appearance.Options.UseForeColor = true;
    }

    /// <summary>Özet kartlarını "hesaplanıyor" durumuna döndürür.</summary>
    private void ClearSummaryCards()
    {
        lblStockValue.Text = "-";
        lblStockNote.Text = "Stok bakiyesi olan ürün";
        lblReceivableValue.Text = "-";
        lblReceivableNote.Text = "Tahsil edilmemiş alacak";
        lblPayableValue.Text = "-";
        lblPayableNote.Text = "Ödenmemiş borç";
        lblChartValue.Text = "-";
        lblChartNote.Text = "Hesap planı / mizan";
    }

    /// <summary>Önizleme sonucunu dört özet kartına ve başlık satırına yazar.</summary>
    private void ApplySummary(CarryForwardPreviewResult preview)
    {
        lblHeaderSub.Text =
            $"Kaynak yıl {preview.SourceYear} ({preview.SourceDatabaseName})  →  " +
            $"Hedef yıl {preview.TargetYear} ({preview.TargetDatabaseName})";

        // Stok devri: bakiyesi olan ürün adedi ve toplam değer.
        lblStockValue.Text = preview.SourceStockValue.ToString("N2") + " ₺";
        lblStockNote.Text = $"{preview.SourceStockBalanceCount:N0} ürün · " +
                            $"{preview.SourceStockQuantity:N2} adet";

        // Alacak: müşterilerden tahsil edilmemiş tutar.
        lblReceivableValue.Text = preview.SourceReceivableTotal.ToString("N2") + " ₺";
        lblReceivableNote.Text = $"{preview.SourceReceivableCount:N0} cari · " +
                                 $"{preview.SourceCustomerCount:N0} müşteri kartı";

        // Borç: tedarikçilere ödenmemiş tutar.
        lblPayableValue.Text = preview.SourcePayableTotal.ToString("N2") + " ₺";
        lblPayableNote.Text = $"{preview.SourcePayableCount:N0} cari · " +
                              $"{preview.SourceSupplierCount:N0} tedarikçi kartı";

        // Hesap planı / mizan: hedefte zaten bulunan hesaplar ayrıca gösterilir.
        lblChartValue.Text = $"{preview.SourceChartBalanceCount:N0} / {preview.SourceChartOfAccountCount:N0}";
        lblChartNote.Text = "bakiyeli hesap / toplam hesap";
    }

    // ---------------------------------------------------------------- önizleme

    private async Task RefreshPreviewAsync()
    {
        btnStart.Enabled = false;
        btnRefresh.Enabled = false;

        try
        {
            CarryForwardPreviewResult preview = await LoadingHelper.RunAsync(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new CarryForwardPreviewQuery(), CancellationToken.None);

                return result.IsSuccessful && result.Data is not null
                    ? result.Data
                    : throw new InvalidOperationException(
                        AuthFormStyles.GetErrorText(result.ErrorMessages));
            }, caption: "Devir bilgileri hazırlanıyor...", description: "Kaynak yıl taranıyor...");

            ApplyPreview(preview);
        }
        catch (AuthorizationException ex)
        {
            lblHeaderSub.Text = ex.Message;
            ShowStatus("Yetkiniz yok.", true);
        }
        catch (Exception ex)
        {
            lblHeaderSub.Text = "Devir bilgisi alınamadı.";
            ShowStatus(ex.Message, true);
        }
        finally
        {
            btnRefresh.Enabled = true;

            // Başlat butonunun durumu ApplyPreview tarafından belirlenir
            // (kaynak yıl yoksa / hedef yıl boş değilse kapalı kalır). Burada
            // koşulsuz açmak, engellenen bir devri yeniden tetiklemeye yol
            // açardı; yalnızca önizleme hiç uygulanamadıysa açılır.
            if (gridControl.DataSource is null)
            {
                btnStart.Enabled = true;
            }
        }
    }

    private void ApplyPreview(CarryForwardPreviewResult preview)
    {
        _lastPreview = preview;
        ApplySummary(preview);
        ConfigurePreviewColumns();

        if (!preview.HasSource)
        {
            ClearSummaryCards();
            lblHeaderSub.Text = "Hedef yıldan önceki bir mali yıl bulunamadı.";

            gridControl.DataSource = new List<PreviewRow>
            {
                new("Kaynak mali yıl", "-", "Bu şirket için hedef yıldan eski bir mali yıl bulunamadı.")
            };

            ShowStatus("Devir alınacak bir önceki mali yıl bulunamadı.", true);
            btnStart.Enabled = false;
            return;
        }

        gridControl.DataSource = new List<PreviewRow>
        {
            new("Stok devri", $"{preview.SourceStockBalanceCount:N0} ürün",
                $"{preview.SourceStockQuantity:N2} adet / {preview.SourceStockValue:N2} TL "
                + "— yeni yılın açılış stoğu olarak yazılır."),
            new("Alacak (tahsil edilmemiş)", $"{preview.SourceReceivableTotal:N2} TL",
                $"{preview.SourceReceivableCount:N0} müşteri cari bakiyesi açılış alacağı olarak yazılır."),
            new("Borç (ödenmemiş)", $"{preview.SourcePayableTotal:N2} TL",
                $"{preview.SourcePayableCount:N0} tedarikçi cari bakiyesi açılış borcu olarak yazılır."),
            new("Hesap planı", $"{preview.SourceChartOfAccountCount:N0} hesap",
                $"Eksik kodlar eklenir; hedefteki {preview.TargetChartOfAccountCount:N0} mevcut hesaba dokunulmaz."),
            new("Hesap bakiyeleri (mizan)", $"{preview.SourceChartBalanceCount:N0} hesap",
                $"Borç {preview.SourceChartDebit:N2} / Alacak {preview.SourceChartCredit:N2} TL"),
            new("Cari kartlar", $"{preview.SourceCustomerCount:N0} müşteri, {preview.SourceSupplierCount:N0} tedarikçi",
                "Vergi numarasıyla eşleşen kartlar yeniden kullanılır."),
            new("Ürün kartları", $"{preview.SourceProductCount:N0} ürün", "Ürün koduyla eşleşenler korunur."),
            new("Ürün fiyatları", $"{preview.SourcePriceCount:N0} fiyat",
                "Atlanırsa yeni yılda FIFO/LIFO maliyeti 0 olur."),
            new("Reçeteler", $"{preview.SourceRecipeCount:N0} reçete", "Yalnızca eklenen ürünlere ait reçeteler."),
            new("Hesap bakiyeleri (yevmiye)", $"{preview.SourceChartBalanceCount:N0} hesap",
                $"Borç {preview.SourceChartDebit:N2} / Alacak {preview.SourceChartCredit:N2}")
        };

        if (preview.BlockingReason is not null)
        {
            ShowStatus(preview.BlockingReason, true);
            btnStart.Enabled = false;
            return;
        }

        if (preview.PreviousCarryForward is { } done)
        {
            string by = string.IsNullOrWhiteSpace(done.CreatedByName) ? string.Empty : $" / {done.CreatedByName}";

            ShowStatus(
                $"Bu mali yıl {done.SourceYear} mali yılından zaten devredilmiş "
                + $"({done.CreatedAt:d.MM.yyyy HH:mm}{by}, {done.TotalAdded:N0} kayıt). "
                + "Mükerrer devir yapılamaz.", true);

            gridControl.DataSource = new List<PreviewRow>
            {
                new("Devir durumu", $"{done.SourceYear} → {done.TargetYear}",
                    $"{done.CreatedAt:d.MM.yyyy HH:mm} tarihinde {done.TotalAdded:N0} kayıt aktarıldı.")
            };

            btnStart.Enabled = false;
            return;
        }

        ShowStatus("Devir yapılabilir. Başlamak için onaylayın.", false);
        btnStart.Enabled = true;
    }

    private void ShowStatus(string message, bool isError)
    {
        lblStatus.Text = message;

        // Durum rengi de skinden gelir; hata yeşil yeşil kalmasın diye skin'in
        // kendi Danger/Success tonları kullanılır.
        SetForeColor(lblStatus, isError ? SkinTheme.Danger : SkinTheme.Success);
        SetStrip(statusAccentStrip, isError ? SkinTheme.Danger : SkinTheme.Primary);
    }

    // ------------------------------------------------------------------ devir

    /// <summary>
    /// Onay diyaloğunda yalnızca seçilen kalemlerin tutarlarını gösterir; kullanıcı
    /// neyin devredileceğini onaylamadan önce rakamları görebilmelidir.
    /// </summary>
    private string BuildConfirmationText(CarryForwardOptions options)
    {
        CarryForwardPreviewResult? p = _lastPreview;
        List<string> lines = ["Devir işlemi başlatılacak.", ""];

        if (p is not null)
        {
            lines.Add($"{p.SourceYear} → {p.TargetYear} devri");
            lines.Add("");
        }

        if (options.Products)
        {
            lines.Add(p is null
                ? "• Stok kartları ve stok devri"
                : $"• Stok devri: {p.SourceStockValue:N2} ₺ ({p.SourceStockBalanceCount:N0} ürün)");
        }

        if (options.CurrentAccounts)
        {
            lines.Add(p is null
                ? "• Cari kartlar ve devir bakiyeleri"
                : $"• Cari bakiyeleri: Alacak {p.SourceReceivableTotal:N2} ₺ / Borç {p.SourcePayableTotal:N2} ₺");
        }

        if (options.ChartOfAccounts)
        {
            lines.Add(p is null
                ? "• Hesap planı"
                : $"• Hesap planı: {p.SourceChartOfAccountCount:N0} hesap");
        }

        if (options.ChartBalances)
        {
            lines.Add(p is null
                ? "• Hesap bakiyeleri (mizan açılışı)"
                : $"• Mizan açılışı: {p.SourceChartBalanceCount:N0} hesap");
        }

        if (options.ProductPrices)
        {
            lines.Add("• Ürün fiyatları (FIFO/LIFO giriş katmanı)");
        }

        if (options.Recipes)
        {
            lines.Add("• Reçeteler");
        }

        if (options.TaxRatesAndUnits)
        {
            lines.Add("• Birim cinsi ve KDV oranları");
        }

        lines.Add("");
        lines.Add("Hedefte zaten bulunan kayıtlar korunur, üzerine yazılmaz.");
        lines.Add("Devir kayıtları açılış tarihinde \"DEVIR-<kaynak yıl>\" belgesiyle yazılır.");
        lines.Add("");
        lines.Add("Devam edilsin mi?");

        return string.Join("\r\n", lines);
    }

    private async void BtnStart_Click(object? sender, EventArgs e)
    {
        CarryForwardOptions options = BuildOptions();

        if (!options.HasAnything)
        {
            ShowStatus("En az bir veri grubu seçilmelidir.", true);
            return;
        }

        string question = BuildConfirmationText(options);

        if (MsgBox.Confirm(this, question, "Devir İşlemleri") != DialogResult.Yes)
        {
            return;
        }

        btnStart.Enabled = false;
        btnRefresh.Enabled = false;
        bool previewRefreshed = false;

        try
        {
            CarryForwardTransferResult result = await LoadingHelper.RunAsync(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var response = await mediator.Send(new CarryForwardStartCommand(options), CancellationToken.None);

                return response.IsSuccessful && response.Data is not null
                    ? response.Data
                    : throw new InvalidOperationException(
                        AuthFormStyles.GetErrorText(response.ErrorMessages));
            }, caption: "Devir işlemi yapılıyor...", description: "Kayıtlar aktarılıyor, lütfen bekleyin...");

            ToastHelper.Show(BuildResultText(result), ToastType.Success, 6000);

            // Devir sonrası hedef yıl artık boş değildir; önizleme bunu
            // yansıtıp butonu kapatacak.
            await RefreshPreviewAsync();
            previewRefreshed = true;
        }
        catch (AuthorizationException ex)
        {
            ToastHelper.Show(ex.Message, ToastType.Warning, 4000);
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("Devir.Start", ex);
            ToastHelper.Show("Devir yapılamadı: " + ex.Message, ToastType.Error, 6000);
        }
        finally
        {
            btnRefresh.Enabled = true;

            // Önizleme yenilendiyse butonun durumu onun sonucudur (başarılı
            // devirden sonra kapalı kalması gerekir). Yenilenemediyse
            // kullanıcı tekrar deneyebilsin diye açılır.
            if (!previewRefreshed)
            {
                btnStart.Enabled = true;
            }
        }
    }

    private CarryForwardOptions BuildOptions() => new()
    {
        ChartOfAccounts = chkAccounts.Checked,
        TaxRatesAndUnits = chkTaxUnits.Checked,
        CurrentAccounts = chkCurrent.Checked,
        Products = chkProducts.Checked,
        ProductPrices = chkPrices.Checked,
        Recipes = chkRecipes.Checked,
        ChartBalances = chkChartBalances.Checked
    };

    /// <summary>
    /// Önizleme grid'inin kolonlarını tanımlar. Kolonlar veri bağımlı olduğu için
    /// tasarım dosyasında değil burada kurulur; görünüm ayarları (salt okunurluk,
    /// grup paneli vb.) tasarım dosyasındadır.
    /// </summary>
    private void ConfigurePreviewColumns()
    {
        gridView.Columns.Clear();

        GridColumn group = gridView.Columns.AddField(nameof(PreviewRow.Grup));
        group.Caption = "Veri Grubu";
        group.AppearanceCell.Font = new Font("Segoe UI Semibold", 9.5F);
        group.AppearanceCell.Options.UseFont = true;
        group.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;

        GridColumn source = gridView.Columns.AddField(nameof(PreviewRow.Kaynak));
        source.Caption = "Kaynakta Bulunan";
        source.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        source.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;

        GridColumn note = gridView.Columns.AddField(nameof(PreviewRow.Not));
        note.Caption = "Aktarım Kuralı";
        note.AppearanceCell.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;

        gridView.RowHeight = 28;
    }

    private static string BuildResultText(CarryForwardTransferResult r)
    {
        List<string> lines =
        [
            $"{r.SourceYear} → {r.TargetDatabaseName} devri tamamlandı.",
            $"Hesap planı: {r.ChartOfAccountsAdded} eklendi, {r.ChartOfAccountsSkipped} mevcut korundu.",
            $"Cari kart: {r.CustomersAdded} müşteri, {r.SuppliersAdded} tedarikçi.",
            $"Ürün kartı: {r.ProductsAdded} eklendi, {r.ProductsSkipped} mevcut korundu.",
            $"Birim/KDV: {r.UnitTypesAdded} birim, {r.TaxRatesAdded} oran eklendi.",
            $"Fiyat/görsel: {r.ProductPricesAdded} fiyat, {r.ProductPhotosAdded} görsel.",
            $"Reçete: {r.RecipesAdded} reçete, {r.RecipeItemsAdded} satır.",
            $"Devir bakiyesi: {r.CurrentAccountBalancesAdded} cari, {r.StockBalancesAdded} stok, {r.ChartBalancesAdded} hesap kaydı.",
            $"Toplam {r.TotalAdded} kayıt yazıldı."
        ];

        return string.Join("\r\n", lines);
    }

    private sealed record PreviewRow(string Grup, string Kaynak, string Not);
}
