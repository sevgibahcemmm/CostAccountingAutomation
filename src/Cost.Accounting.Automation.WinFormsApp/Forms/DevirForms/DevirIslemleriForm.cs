using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Devirs;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DevirForms;

/// <summary>
/// Devir işlemleri: aynı şirketin bir önceki mali yılından seçili yıla hesap
/// planı ve açılış bakiyelerini aktarır.
/// </summary>
public sealed class DevirIslemleriForm : XtraFormMdiBase
{
    private readonly LabelControl _lblHeader = new();
    private readonly LabelControl _lblStatus = new();
    private readonly GridControl _grid = new();
    private readonly GridView _view = new();

    private readonly CheckEdit _chkAccounts = new();
    private readonly CheckEdit _chkTaxUnits = new();
    private readonly CheckEdit _chkCurrent = new();
    private readonly CheckEdit _chkProducts = new();
    private readonly CheckEdit _chkPrices = new();
    private readonly CheckEdit _chkRecipes = new();
    private readonly CheckEdit _chkChartBalances = new();

    private readonly SimpleButton _btnStart = new();
    private readonly SimpleButton _btnRefresh = new();

    public DevirIslemleriForm() : base("Devir İşlemleri")
    {
        BuildLayout();
        Shown += async (_, _) => await RefreshPreviewAsync();
    }

    // ------------------------------------------------------------------ düzen

    private void BuildLayout()
    {
        Size = new Size(900, 640);
        MinimumSize = new Size(760, 520);

        _lblHeader.Dock = DockStyle.Top;
        _lblHeader.Height = 116;
        _lblHeader.Padding = new Padding(12, 10, 12, 6);
        _lblHeader.AutoSizeMode = LabelAutoSizeMode.None;
        _lblHeader.Appearance.TextOptions.VAlignment = VertAlignment.Top;
        _lblHeader.Text = "Yükleniyor...";

        _lblStatus.Dock = DockStyle.Bottom;
        _lblStatus.Height = 56;
        _lblStatus.Padding = new Padding(12, 6, 12, 6);
        _lblStatus.Appearance.TextOptions.VAlignment = VertAlignment.Center;

        _grid.Dock = DockStyle.Fill;
        _view.OptionsBehavior.Editable = false;
        _view.OptionsView.ShowGroupPanel = false;
        _view.OptionsView.ColumnAutoWidth = true;
        _view.OptionsSelection.MultiSelect = false;
        _grid.ViewCollection.AddRange([_view]);

        BuildGroupBox();

        // Dock sırası: önce Fill, sonra kenar panelleri.
        Controls.Add(_grid);
        Controls.Add(_lblHeader);
        Controls.Add(_lblStatus);
        Controls.Add(_grpOptions);
    }

    private GroupControl _grpOptions = null!;

    private void BuildGroupBox()
    {
        _grpOptions = new GroupControl
        {
            Dock = DockStyle.Bottom,
            Height = 188,
            Text = "Aktarılacak Veriler"
        };

        var optionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 108,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = true,
            Padding = new Padding(12, 6, 12, 0)
        };

        Configure(_chkAccounts, "Hesap planı (hesap kodu, ad, üst hesap)");
        Configure(_chkTaxUnits, "Birim cinsi ve KDV oranları");
        Configure(_chkCurrent, "Cari kartlar ve cari devir bakiyeleri");
        Configure(_chkProducts, "Stok kartları ve stok devir miktarı/değeri");
        Configure(_chkPrices, "Ürün fiyatları (FIFO/LIFO giriş katmanı)");
        Configure(_chkRecipes, "Reçeteler");
        Configure(_chkChartBalances, "Hesap bakiyeleri (mizan açılışı)");

        optionsPanel.Controls.AddRange(
        [
            _chkAccounts,
            _chkTaxUnits,
            _chkCurrent,
            _chkProducts,
            _chkPrices,
            _chkRecipes,
            _chkChartBalances
        ]);

        _btnStart.Text = "Devir İşlemlerini Başlat";
        _btnStart.Size = new Size(220, 38);
        _btnStart.Click += BtnStart_Click;

        _btnRefresh.Text = "Yenile";
        _btnRefresh.Size = new Size(110, 38);
        _btnRefresh.Click += async (_, _) => await RefreshPreviewAsync();

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(12, 6, 12, 6)
        };

        buttonPanel.Controls.AddRange([_btnStart, _btnRefresh]);

        _grpOptions.Controls.Add(optionsPanel);
        _grpOptions.Controls.Add(buttonPanel);
    }

    private static void Configure(CheckEdit edit, string caption)
    {
        edit.Text = caption;
        edit.Properties.Caption = caption;
        edit.Checked = true;
        edit.AutoSize = true;
        edit.Size = new Size(340, 22);
        edit.Properties.Appearance.TextOptions.VAlignment = VertAlignment.Center;
    }

    // ---------------------------------------------------------------- önizleme

    private async Task RefreshPreviewAsync()
    {
        _btnStart.Enabled = false;
        _btnRefresh.Enabled = false;

        try
        {
            DevirPreviewResult preview = await LoadingHelper.RunAsync(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await mediator.Send(new DevirPreviewQuery(), CancellationToken.None);

                return result.IsSuccessful && result.Data is not null
                    ? result.Data
                    : throw new InvalidOperationException(
                        AuthFormStyles.GetErrorText(result.ErrorMessages));
            }, caption: "Devir bilgileri hazırlanıyor...", description: "Kaynak yıl taranıyor...");

            ApplyPreview(preview);
        }
        catch (AuthorizationException ex)
        {
            _lblHeader.Text = ex.Message;
            _lblStatus.Text = string.Empty;
            ShowStatus("Yetkiniz yok.", true);
        }
        catch (Exception ex)
        {
            _lblHeader.Text = "Devir bilgisi alınamadı.";
            ShowStatus(ex.Message, true);
        }
        finally
        {
            _btnRefresh.Enabled = true;

            // Başlat butonunun durumu ApplyPreview tarafından belirlenir
            // (kaynak yıl yoksa / hedef yıl boş değilse kapalı kalır). Burada
            // koşulsuz açmak, engellenen bir devri yeniden tetiklemeye yol
            // açardı; yalnızca önizleme hiç uygulanamadıysa açılır.
            if (_grid.DataSource is null)
            {
                _btnStart.Enabled = true;
            }
        }
    }

    private void ApplyPreview(DevirPreviewResult preview)
    {
        _lblHeader.Text =
            $"Kaynak yıl:   {preview.SourceYear?.ToString() ?? "-"}   ({preview.SourceDatabaseName ?? "-"})\r\n" +
            $"Hedef yıl:    {preview.TargetYear}   ({preview.TargetDatabaseName})\r\n" +
            $"Hesap planı:  Kaynakta {preview.SourceChartOfAccountCount} hesap, " +
            $"hedefte {preview.TargetChartOfAccountCount} hesap\r\n" +
            "Devir kayıtları, hedef yılın açılış tarihinde ve " +
            "\"DEVIR-<kaynak yıl>\" belge numarasıyla oluşturulur.";

        _view.Columns.Clear();

        if (!preview.HasSource)
        {
            _grid.DataSource = new List<PreviewRow>
            {
                new("Kaynak mali yıl", "-", "Bu şirket için hedef yıldan eski bir mali yıl bulunamadı.")
            };

            ShowStatus("Devir alınacak bir önceki mali yıl bulunamadı.", true);
            _btnStart.Enabled = false;
            return;
        }

        _grid.DataSource = new List<PreviewRow>
        {
            new("Hesap planı", $"{preview.SourceChartOfAccountCount:N0} hesap",
                "Eksik kodlar eklenir; hedefteki mevcut hesaplara dokunulmaz."),
            new("Müşteri / Tedarikçi kartları",
                $"{preview.SourceCustomerCount:N0} müşteri, {preview.SourceSupplierCount:N0} tedarikçi",
                "Vergi numarasıyla eşleşen kartlar yeniden kullanılır."),
            new("Cari devir bakiyesi", $"{preview.SourceCurrentAccountBalanceCount:N0} cari hesap",
                $"Borç {preview.SourceCurrentAccountDebit:N2} / Alacak {preview.SourceCurrentAccountCredit:N2}"),
            new("Ürün kartları", $"{preview.SourceProductCount:N0} ürün", "Ürün koduyla eşleşenler korunur."),
            new("Stok devir miktarı", $"{preview.SourceStockBalanceCount:N0} ürün",
                $"{preview.SourceStockQuantity:N2} adet / {preview.SourceStockValue:N2} TL"),
            new("Ürün fiyatları", $"{preview.SourcePriceCount:N0} fiyat",
                "Atlanırsa yeni yılda FIFO/LIFO maliyeti 0 olur."),
            new("Reçeteler", $"{preview.SourceRecipeCount:N0} reçete", "Yalnızca eklenen ürünlere ait reçeteler."),
            new("Hesap bakiyeleri (yevmiye)", $"{preview.SourceChartBalanceCount:N0} hesap",
                $"Borç {preview.SourceChartDebit:N2} / Alacak {preview.SourceChartCredit:N2}")
        };

        if (preview.BlockingReason is not null)
        {
            ShowStatus(preview.BlockingReason, true);
            _btnStart.Enabled = false;
            return;
        }

        if (preview.PreviousDevir is { } done)
        {
            string by = string.IsNullOrWhiteSpace(done.CreatedByName) ? string.Empty : $" / {done.CreatedByName}";

            ShowStatus(
                $"Bu mali yıl {done.SourceYear} mali yılından zaten devredilmiş "
                + $"({done.CreatedAt:d.MM.yyyy HH:mm}{by}, {done.TotalAdded:N0} kayıt). "
                + "Mükerrer devir yapılamaz.", true);

            _grid.DataSource = new List<PreviewRow>
            {
                new("Devir durumu", $"{done.SourceYear} → {done.TargetYear}",
                    $"{done.CreatedAt:d.MM.yyyy HH:mm} tarihinde {done.TotalAdded:N0} kayıt aktarıldı.")
            };

            _btnStart.Enabled = false;
            return;
        }

        ShowStatus("Devir yapılabilir. Başlamak için onaylayın.", false);
        _btnStart.Enabled = true;
    }

    private void ShowStatus(string message, bool isError)
    {
        _lblStatus.Text = message;
        _lblStatus.Appearance.ForeColor =
            isError ? Color.Firebrick : Color.FromArgb(0, 110, 60);
    }

    // ------------------------------------------------------------------ devir

    private async void BtnStart_Click(object? sender, EventArgs e)
    {
        DevirOptions options = BuildOptions();

        if (!options.HasAnything)
        {
            ShowStatus("En az bir veri grubu seçilmelidir.", true);
            return;
        }

        string question =
            "Devir işlemi başlatılacak.\r\n\r\n" +
            "• Seçilen kayıtlar seçili mali yılın veritabanına eklenir.\r\n" +
            "• Hedefte zaten bulunan kayıtlar korunur, üzerine yazılmaz.\r\n" +
            "• Devir bakiyeleri açılış tarihinde \"DEVIR-<kaynak yıl>\" belgesiyle yazılır.\r\n\r\n" +
            "Devam edilsin mi?";

        if (MsgBox.Confirm(this, question, "Devir İşlemleri") != DialogResult.Yes)
        {
            return;
        }

        _btnStart.Enabled = false;
        _btnRefresh.Enabled = false;
        bool previewRefreshed = false;

        try
        {
            DevirTransferResult result = await LoadingHelper.RunAsync(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                var response = await mediator.Send(new DevirStartCommand(options), CancellationToken.None);

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
            _btnRefresh.Enabled = true;

            // Önizleme yenilendiyse butonun durumu onun sonucudur (başarılı
            // devirden sonra kapalı kalması gerekir). Yenilenemediyse
            // kullanıcı tekrar deneyebilsin diye açılır.
            if (!previewRefreshed)
            {
                _btnStart.Enabled = true;
            }
        }
    }

    private DevirOptions BuildOptions() => new()
    {
        ChartOfAccounts = _chkAccounts.Checked,
        TaxRatesAndUnits = _chkTaxUnits.Checked,
        CurrentAccounts = _chkCurrent.Checked,
        Products = _chkProducts.Checked,
        ProductPrices = _chkPrices.Checked,
        Recipes = _chkRecipes.Checked,
        ChartBalances = _chkChartBalances.Checked
    };

    private static string BuildResultText(DevirTransferResult r)
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
