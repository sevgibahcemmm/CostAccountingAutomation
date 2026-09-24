using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.InvoiceForms;

public sealed partial class SalesPriceSuggestionForm : XtraForm
{
    public decimal SuggestedPrice { get; }

    public decimal Price
    {
        get
        {
            if (spPrice.EditValue is decimal value && value > 0)
            {
                return Math.Round(value, 2);
            }

            return SuggestedPrice;
        }
    }

    public bool SaveAsSalePrice => chkSaveAsSalePrice.Checked;

    public SalesPriceSuggestionForm(
        string productName,
        string productCode,
        decimal costPrice,
        decimal taxRateRate)
    {
        InitializeComponent();

        SuggestedPrice = SalesPriceSuggestionCalculator.SuggestedPriceOf(
            costPrice,
            SalesPriceSuggestionCalculator.KdvRateOf(taxRateRate));

        Text = "Satış Fiyatı Belirleme";
        lblHeaderTitle.Text = "Satış Fiyatı Belirleme";
        lblHeaderSub.Text = string.IsNullOrWhiteSpace(productCode)
            ? productName
            : $"{productName} ({productCode})";

        lblCostValue.Text = $"{costPrice:N2} ₺";

        decimal displayRate = taxRateRate > 0 && taxRateRate <= 1 ? taxRateRate * 100 : taxRateRate;
        lblTaxValue.Text = $"{displayRate:F0} %";

        spPrice.EditValue = SuggestedPrice;
        chkSaveAsSalePrice.Checked = true;

        btnUse.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
        };
        btnCancel.Click += (_, _) => Close();
    }
}