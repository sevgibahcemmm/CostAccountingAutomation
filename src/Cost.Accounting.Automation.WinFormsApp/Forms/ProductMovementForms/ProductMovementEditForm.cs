using Cost.Accounting.Automation.Application.ProductMovements;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductMovementForms
{
    public partial class ProductMovementEditForm : SkinSensitiveForm
    {
        private readonly ProductMovementListDto? _editing;
        private readonly ProductMovementType? _preselectedType;
        private List<ProductDto> _products = [];

        public ProductMovementEditForm() : this(null, null)
        {
        }

        public ProductMovementEditForm(ProductMovementType? movementType) : this(null, movementType)
        {
        }

        public ProductMovementEditForm(ProductMovementListDto? existing, ProductMovementType? movementType = null)
        {
            InitializeComponent();
            _editing = existing;
            _preselectedType = movementType ?? existing?.MovementType;

            IconOptions.SvgImage = SvgIcons.Modules[2];
            lblHeaderIcon.ImageOptions.SvgImage = SvgIcons.Modules[2];

            Text = _editing is null ? "Yeni Stok Hareketi" : "Stok Hareketi İncele";
            lblTitle.Text = Text;
            lblSubtitle.Text = _editing is null
                ? "Yeni stok giriş veya çıkış işlemi kaydedin"
                : "Stok hareket detayları";

            InitControls();
            WireEvents();
        }

        private void InitControls()
        {
            cmbMovementType.Properties.Items.Clear();
            cmbMovementType.Properties.Items.Add("Giriş");
            cmbMovementType.Properties.Items.Add("Çıkış");

            if (_preselectedType == ProductMovementType.Output)
            {
                cmbMovementType.SelectedIndex = 1;
            }
            else
            {
                cmbMovementType.SelectedIndex = 0;
            }

            dtDate.DateTime = DateTime.Today;

            if (_editing is not null)
            {
                btnSave.Visible = false;
                lookUpProduct.ReadOnly = true;
                cmbMovementType.ReadOnly = true;
                spinQuantity.ReadOnly = true;
                spinPrice.ReadOnly = true;
                dtDate.ReadOnly = true;
                txtReferenceNo.ReadOnly = true;
                txtDescription.ReadOnly = true;
            }
        }

        private void WireEvents()
        {
            Load += ProductMovementEditForm_Load;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (_, _) => Close();
        }

        private async void ProductMovementEditForm_Load(object? sender, EventArgs e)
        {
            await LoadProductsAsync();

            if (_editing is not null)
            {
                lookUpProduct.EditValue = _editing.ProductId;
                cmbMovementType.SelectedIndex = _editing.MovementType == ProductMovementType.Input ? 0 : 1;
                spinQuantity.Value = _editing.Quantity;
                spinPrice.Value = _editing.UnitPrice ?? 0;
                dtDate.DateTime = _editing.Date.ToDateTime(TimeOnly.MinValue);
                txtReferenceNo.Text = _editing.ReferenceNo ?? string.Empty;
                txtDescription.Text = _editing.Description;
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                _products = (await mediator.Send(new ProductGetAllQuery())).ToList();

                lookUpProduct.Properties.DataSource = _products;
                lookUpProduct.Properties.ValueMember = nameof(ProductDto.Id);
                lookUpProduct.Properties.DisplayMember = nameof(ProductDto.Name);

                lookUpProductView.Columns.Clear();
                lookUpProductView.Columns.AddField(nameof(ProductDto.ProductCode)).Caption = "Ürün Kodu";
                lookUpProductView.Columns.AddField(nameof(ProductDto.Name)).Caption = "Ürün Adı";
                lookUpProductView.Columns.AddField(nameof(ProductDto.StockQuantity)).Caption = "Mevcut Stok";

                foreach (GridColumn col in lookUpProductView.Columns)
                {
                    col.Visible = true;
                }

                lookUpProduct.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFit;
            }
            catch (Exception ex)
            {
                ToastHelper.Show("Ürünler yüklenirken hata oluştu: " + ex.Message, ToastType.Error);
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (lookUpProduct.EditValue is not Guid productId || productId == Guid.Empty)
            {
                ToastHelper.Show("Lütfen bir ürün seçiniz.", ToastType.Warning);
                lookUpProduct.Focus();
                return;
            }

            if (spinQuantity.Value <= 0)
            {
                ToastHelper.Show("Miktar sıfırdan büyük olmalıdır.", ToastType.Warning);
                spinQuantity.Focus();
                return;
            }

            ProductMovementType type = cmbMovementType.SelectedIndex == 0 ? ProductMovementType.Input : ProductMovementType.Output;
            DateOnly date = DateOnly.FromDateTime(dtDate.DateTime);
            decimal? unitPrice = spinPrice.Value > 0 ? spinPrice.Value : null;

            ProductMovementCreateCommand command = new(
                ProductId: productId,
                MovementType: type,
                Quantity: spinQuantity.Value,
                UnitPrice: unitPrice,
                Date: date,
                ReferenceNo: txtReferenceNo.Text.Trim(),
                Description: txtDescription.Text.Trim());

            btnSave.Enabled = false;
            try
            {
                bool ok = await CrudExecutor.ExecuteAsync(command);
                if (ok)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
