using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    /// <summary>Ürün detay formundan açılan salt okunur fiyat geçmişi penceresi.</summary>
    public sealed partial class ProductPriceHistoryForm : XtraForm
    {
        private readonly ProductDetailDto _detail;
        private readonly IDisposable? skinBinding;

        public ProductPriceHistoryForm(ProductDetailDto detail)
        {
            _detail = detail;

            InitializeComponent();
            ApplyLayout();
            ApplySkin();
            skinBinding = SkinTheme.Bind(ApplySkin);

            lblTitle.Text = detail.Prices.Count == 0
                ? $"{detail.Name} — fiyat kaydı tanımlı değil"
                : $"{detail.Name} — tanımlı {detail.Prices.Count} fiyat kaydı";
            lblCurrent.Text =
                $"Güncel alış fiyatı: {Format(detail.PurchasePrice ?? detail.CostPrice)}   •   "
                + $"Güncel satış fiyatı: {Format(detail.SalePrice)}   •   "
                + $"Maliyet fiyatı: {Format(detail.CostPrice)}";
            lblEmpty.Visible = detail.Prices.Count == 0;

            ConfigureGrid();
            gridPrices.DataSource = detail.Prices;

            btnClose.Click += (_, _) => Close();
        }

        /// <summary>
        /// Etiket renkleri aktif skinden çözülür; skin değişiminde yeniden uygulanır. Buton
        /// rengi atanmaz, DevExpress skin'inin kendi buton stili geçerli kalır.
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);

            lblTitle.Appearance.ForeColor = SkinTheme.Text;
            lblCurrent.Appearance.ForeColor = SkinTheme.SecondaryText;
            lblEmpty.Appearance.ForeColor = SkinTheme.MutedText(surface);
        }

        /// <summary>
        /// DevExpress kontrolleri <c>Appearance</c> atamaları sonrası yeniden ölçtüğü için
        /// InitializeComponent içindeki ölçüleri ezebiliyor. Tüm konum/boyutlar bu metotta,
        /// kurucudan hemen sonra bir kez uygulanır (bkz. ProductDetailForm.ApplyLayout).
        /// </summary>
        private void ApplyLayout()
        {
            const int margin = 20;
            const int formWidth = 780;
            const int formHeight = 430;
            const int contentWidth = formWidth - (2 * margin);

            ClientSize = new Size(formWidth, formHeight);
            MinimumSize = new Size(600, 320);

            lblTitle.SetBounds(margin, 14, contentWidth, 22);
            lblCurrent.SetBounds(margin, 40, contentWidth, 18);
            gridPrices.SetBounds(margin, 64, contentWidth, 314);
            lblEmpty.SetBounds(margin, 190, contentWidth, 46);
            btnClose.SetBounds(formWidth - margin - 180, 386, 180, 34);
        }

        private void ConfigureGrid()
        {
            GridColumnFactory.ConfigureFromAttributes(viewPrices, typeof(ProductPriceDto));
            viewPrices.OptionsBehavior.Editable = false;
            viewPrices.OptionsView.ShowGroupPanel = false;
            viewPrices.OptionsView.ShowIndicator = false;
            viewPrices.Columns[nameof(ProductPriceDto.UnitPrice)]!.Caption = "Birim Fiyat (Kdv Hariç)";
            viewPrices.Columns[nameof(ProductPriceDto.StartDate)]!.Caption = "Fiyat Başlangıç";
            viewPrices.Columns[nameof(ProductPriceDto.StartDate)]!.ToolTip =
                "Bu fiyatın geçerli olmaya başladığı tarih.";
            viewPrices.Columns[nameof(ProductPriceDto.EndDate)]!.Caption = "Fiyat Bitiş";
            viewPrices.Columns[nameof(ProductPriceDto.EndDate)]!.ToolTip =
                "Fiyatın geçerliliğinin bittiği tarih. Boş bırakılırsa süresiz geçerlidir.";
            viewPrices.BestFitColumns();
        }

        private static string Format(decimal? value)
            => value is null ? "—" : $"{value.Value:n2} ₺";
    }
}
