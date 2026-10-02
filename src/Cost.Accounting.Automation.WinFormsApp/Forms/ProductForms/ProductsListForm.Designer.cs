namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductForms
{
    public sealed partial class ProductsListForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _barcodeScope?.Dispose();

                foreach (Image image in _imageCache.Values
                             .Concat(_barcodeImageCache.Values)
                             .Concat(_qrImageCache.Values)
                             .Concat(_slidePreviewCache.Values)
                             .OfType<Image>())
                {
                    image.Dispose();
                }

                _imageCache.Clear();
                _barcodeImageCache.Clear();
                _qrImageCache.Clear();
                _slidePreviewCache.Clear();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
    }
}
