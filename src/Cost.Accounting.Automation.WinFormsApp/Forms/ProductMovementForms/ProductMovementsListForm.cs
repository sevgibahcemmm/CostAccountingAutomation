using Cost.Accounting.Automation.Application.ProductMovements;
using Cost.Accounting.Automation.Domain.Products;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using DevExpress.XtraGrid.Columns;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ProductMovementForms
{
    public sealed partial class ProductMovementsListForm : CrudListFormBase<ProductMovementGetAllQuery, ProductMovementListDto, ProductMovementEditForm>
    {
        private readonly ProductMovementType? _targetType;
        private readonly Guid? _productId;

        public ProductMovementsListForm() : this(null, null)
        {
        }

        public ProductMovementsListForm(ProductMovementType? targetType, Guid? productId = null)
            : base(targetType switch
            {
                ProductMovementType.Input => "Stok Girişleri",
                ProductMovementType.Output => "Stok Çıkışları",
                _ => "Stok Hareketleri"
            })
        {
            _targetType = targetType;
            _productId = productId;
        }

        protected override SvgImage ModuleIcon => _targetType switch
        {
            ProductMovementType.Input => SvgIcons.MenuIcons[11],
            ProductMovementType.Output => SvgIcons.MenuIcons[12],
            _ => SvgIcons.Modules[2]
        };

        protected override string[] SearchFieldNames =>
        [
            nameof(ProductMovementListDto.ProductName),
            nameof(ProductMovementListDto.ProductCode),
            nameof(ProductMovementListDto.Barcode),
            nameof(ProductMovementListDto.WarehouseName),
            nameof(ProductMovementListDto.ReferenceNo),
            nameof(ProductMovementListDto.Description)
        ];

        protected override void ConfigureColumns()
        {
            View.Columns.Clear();

            GridColumn[] columns =
            [
                new() { Caption = "Tarih", FieldName = nameof(ProductMovementListDto.Date), Visible = true, Width = 95, DisplayFormat = { FormatType = FormatType.DateTime, FormatString = "dd.MM.yyyy" } },
                new() { Caption = "Hareket Türü", FieldName = nameof(ProductMovementListDto.MovementTypeName), Visible = !_targetType.HasValue, Width = 85 },
                new() { Caption = "Ürün Kodu", FieldName = nameof(ProductMovementListDto.ProductCode), Visible = true, Width = 110 },
                new() { Caption = "Ürün Adı", FieldName = nameof(ProductMovementListDto.ProductName), Visible = true, Width = 220 },
                new() { Caption = "Depo", FieldName = nameof(ProductMovementListDto.WarehouseName), Visible = true, Width = 120 },
                new() { Caption = "Miktar", FieldName = nameof(ProductMovementListDto.Quantity), Visible = true, Width = 80, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Birim", FieldName = nameof(ProductMovementListDto.UnitTypeName), Visible = true, Width = 65 },
                new() { Caption = "Birim Fiyat", FieldName = nameof(ProductMovementListDto.UnitPrice), Visible = true, Width = 95, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Toplam Tutar", FieldName = nameof(ProductMovementListDto.TotalPrice), Visible = true, Width = 105, DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" } },
                new() { Caption = "Belge / Ref No", FieldName = nameof(ProductMovementListDto.ReferenceNo), Visible = true, Width = 110 },
                new() { Caption = "Açıklama", FieldName = nameof(ProductMovementListDto.Description), Visible = true, Width = 180 }
            ];

            View.Columns.AddRange(columns);
            AddColumnsFromAttributes();
        }

        protected override ProductMovementGetAllQuery BuildListQuery()
            => new(ProductId: _productId, MovementType: _targetType, OnlyDeleted: ShowDeleted);

        protected override IRequest<Result<string>> BuildDeleteCommand(ProductMovementListDto item)
            => new ProductMovementDeleteCommand(item.Id);

        protected override string GetDeleteSummary(ProductMovementListDto item)
            => $"{item.ProductName} ({item.MovementTypeName} - {item.Quantity:n2})";

        protected override bool SupportsRestore => true;

        protected override IRequest<Result<string>> BuildRestoreCommand(ProductMovementListDto item)
            => new ProductMovementRestoreCommand(item.Id);
    }
}
