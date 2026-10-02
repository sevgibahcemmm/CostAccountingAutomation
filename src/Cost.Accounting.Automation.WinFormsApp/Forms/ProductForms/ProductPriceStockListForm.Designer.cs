namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductPriceStockListForm
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>Stok kartının satış fiyatı ve fotoğraf için açıldığı buton.</summary>
        private DevExpress.XtraEditors.SimpleButton btnPrice;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnPrice = new DevExpress.XtraEditors.SimpleButton();

            btnPrice.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnPrice.Appearance.Options.UseFont = true;
            btnPrice.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            btnPrice.ImageOptions.SvgImage = Cost.Accounting.Automation.WinFormsApp.Utils.DxIcon.Tag;
            btnPrice.ImageOptions.SvgImageSize = new System.Drawing.Size(18, 18);
            btnPrice.Location = new System.Drawing.Point(194, 16);
            btnPrice.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnPrice.Name = "btnPrice";
            btnPrice.Size = new System.Drawing.Size(190, 36);
            btnPrice.TabIndex = 20;
            btnPrice.Text = "Satış Fiyatı / Fotoğraf";

            // Taban araç çubuğu butonlarının hemen ardına eklenir; sıra
            // RegisterDerivedToolbarButtons içinde netleştirilir.
            flpToolbar.Controls.Add(btnPrice);
        }
    }
}