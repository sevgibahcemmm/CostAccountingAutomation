using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    /// <summary>
    /// Fiyat &amp; Stok Listesi'nde çift tıklamada veya "Detay" butonunda açılan salt okunur ürün
    /// detayı: üstte ürün künyesi ve stok/fiyat özeti, altında belgeli stok hareketleri grid'i.
    /// </summary>
    public sealed partial class ProductDetailForm : XtraForm
    {
        /// <summary>
        /// <summary>
        /// Tasarım yüzeyi yapıcısında doldurulmaz; gerçek açılışta verilen <c>detail</c> atanır.
        /// </summary>
        private readonly ProductDetailDto _detail = default!;
        private readonly IDisposable? skinBinding;

        /// <summary>
        /// Yalnızca Visual Studio tasarım yüzeyi içindir; gerçek açılışta
        /// <see cref="ProductDetailForm(ProductDetailDto)"/> kullanılır.
        /// </summary>
        public ProductDetailForm()
        {
            InitializeComponent();
            ApplyLayout();
            DesignTime.Guard(typeof(ProductDetailForm));
        }

        public ProductDetailForm(ProductDetailDto detail)
        {
            InitializeComponent();
            _detail = detail;

            ApplyLayout();
            ApplySkin();
            skinBinding = SkinTheme.Bind(ApplySkin);
            BindAll();
        }

        /// <summary>
        /// Panel, ayırıcı ve etiket renkleri aktif skinden çözülür; skin değiştiğinde yeniden
        /// uygulanır. Butonlara hiçbir renk atanmaz: renk/zemin/hover tamamen DevExpress skin'inin
        /// kendi buton stilinden gelir, aksi halde metin ve zemin soluk görünür.
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);

            // Üst panel metinleri koyu zeminde okunacak kadar açık olmalı; hiyerarşi
            // kalınlık/boyutla kurulur, metin soluklaştırılarak değil.
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.18F);

            accentBar.Appearance.BackColor = SkinTheme.Primary;
            headerPanel.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            summaryPanel.Appearance.BackColor = SkinTheme.SurfaceAccent(surface);
            footerDivider.Appearance.BackColor = SkinTheme.BorderMuted(surface);

            lblHeaderTitle.Appearance.ForeColor = primary;
            lblHeaderSub.Appearance.ForeColor = secondary;
            lblSummary.Appearance.ForeColor = primary;
            lblInfo.Appearance.ForeColor = secondary;
            lblMovementTitle.Appearance.ForeColor = primary;
            lblEmptyMovement.Appearance.ForeColor = secondary;

            lblHeaderTitle.Appearance.Options.UseForeColor = true;
            lblHeaderSub.Appearance.Options.UseForeColor = true;
            lblSummary.Appearance.Options.UseForeColor = true;
            lblInfo.Appearance.Options.UseForeColor = true;
            lblMovementTitle.Appearance.Options.UseForeColor = true;
            lblEmptyMovement.Appearance.Options.UseForeColor = true;
        }

        /// <summary>
        /// DevExpress <see cref="LabelControl"/>/<see cref="PanelControl"/> ölçüleri, özellik
        /// <c>Appearance</c> atamalarından sonra yeniden hesaplandığı için InitializeComponent
        /// içindeki Size değerlerini ezer. Bu yüzden tüm konum/boyutlar tek yerde, kurucudan
        /// hemen sonra uygulanır.
        /// </summary>
        private void ApplyLayout()
        {
            const int formWidth = 1120;
            const int formHeight = 724;
            const int margin = 20;
            const int contentWidth = formWidth - (2 * margin);

            ClientSize = new Size(formWidth, formHeight);

            accentBar.SetBounds(0, 0, formWidth, 8);

            headerPanel.SetBounds(0, 4, formWidth, 64);
            picHeader.SetBounds(margin, 14, 34, 34);
            lblHeaderTitle.SetBounds(66, 8, formWidth - 86, 28);
            lblHeaderSub.SetBounds(66, 34, formWidth - 86, 26);

            summaryPanel.SetBounds(margin, 76, contentWidth, 40);
            lblSummary.SetBounds(6, 6, contentWidth - 12, 28);

            lblInfo.SetBounds(margin + 2, 122, contentWidth - 4, 18);
            lblMovementTitle.SetBounds(margin + 2, 142, contentWidth - 4, 20);

            gridMovements.SetBounds(margin, 164, contentWidth, 486);
            lblEmptyMovement.SetBounds(margin, 330, contentWidth, 40);

            footerDivider.SetBounds(margin, 656, contentWidth, 1);
            btnPrices.SetBounds(margin, 672, 180, 36);
            btnClose.SetBounds(formWidth - margin - 180, 672, 180, 36);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BindAll();

            btnClose.Click += (_, _) => Close();
            btnPrices.Click += (_, _) =>
            {
                using var form = new ProductPriceHistoryForm(_detail);
                form.ShowDialog(this);
            };
        }

        private void BindAll()
        {
            Text = $"Ürün Detayı — {BuildStamp()}";
            BindHeader();
            BindSummary();
            BindGrid();
        }

        /// <summary>Çalışan derlemenin zaman damgası; ekran görüntülerinde build doğrulaması için.</summary>
        internal static string BuildStamp()
        {
            try
            {
                return System.IO.File.GetLastWriteTime(System.Windows.Forms.Application.ExecutablePath).ToString("dd.MM HH:mm");
            }
            catch (Exception)
            {
                return "?";
            }
        }

        private void BindHeader()
        {
            lblHeaderTitle.Text = string.IsNullOrWhiteSpace(_detail.Name) ? "Ürün" : _detail.Name;

            lblHeaderSub.Text = string.Join("   •   ", new[]
            {
                Clean(_detail.ProductCode),
                Clean(_detail.WarehouseName),
                Clean(_detail.CategoryName),
                Clean(_detail.ProductUnitTypeName),
                _detail.TaxRateRate > 0
                    ? $"KDV %{_detail.TaxRateRate * (_detail.TaxRateRate <= 1 ? 100 : 1):0}"
                    : Clean(_detail.TaxRateName)
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            List<string> notes = [];
            AddNote(notes, _detail.WarehouseCode, "Depo kodu");
            AddNote(notes, _detail.ChartOfAccountCode, "Hesap kodu");
            AddNote(notes, _detail.SemiFinishedProductName, "Bağlı mamül");
            AddNote(notes, _detail.Barcode, "Barkod");
            if (_detail.MinimumProductLevel is { } minLevel)
            {
                notes.Add($"Min. stok: {minLevel:n2}");
            }

            notes.Add(_detail.IsActive ? "Kart aktif" : "Kart pasif");
            if (_detail.Prices.Count == 0)
            {
                notes.Add("Fiyat kaydı yok");
            }

            if (_detail.Movements.Count == 0)
            {
                notes.Add("Belgeli stok hareketi yok");
            }

            AddNote(notes, _detail.Description, null);

            lblInfo.Text = string.Join("   |   ", notes);
        }

        private void BindSummary()
        {
            lblSummary.Text =
                $"Stok: {Format(_detail.StockQuantity)}   •   Toplam Giriş: {Format(_detail.TotalInQuantity)}   •   " +
                $"Toplam Çıkış: {Format(_detail.TotalOutQuantity)}   •   Maliyet Fiyatı: {Format(_detail.CostPrice)}   •   " +
                $"Alış Fiyatı: {Format(_detail.PurchasePrice ?? _detail.CostPrice)}   •   Satış Fiyatı: {Format(_detail.SalePrice)}";

            lblMovementTitle.Text = $"Belgeli Stok Hareketleri ({_detail.Movements.Count})";
            btnPrices.Text = $"Fiyat Geçmişi ({_detail.Prices.Count})";
        }

        private void BindGrid()
        {
            lblEmptyMovement.Visible = _detail.Movements.Count == 0;

            if (gridMovements.DataSource is null)
            {
                GridColumnFactory.ConfigureFromAttributes(viewMovements, typeof(ProductStockMovementDetailDto));
                viewMovements.Columns[nameof(ProductStockMovementDetailDto.DocumentTypeName)]!.Width = 150;
                viewMovements.Columns[nameof(ProductStockMovementDetailDto.Description)]!.Width = 320;
                viewMovements.BestFitColumns();
                gridMovements.DataSource = _detail.Movements;
            }
        }

        private static string Clean(string? value) => value?.Trim() ?? string.Empty;

        private static void AddNote(List<string> notes, string? value, string? label)
        {
            string text = Clean(value);
            if (text.Length == 0)
            {
                return;
            }

            notes.Add(label is null ? text : $"{label}: {text}");
        }

        private static string Format(decimal? value)
            => value is null ? "—" : $"{value.Value:n2} ₺";
    }
}
