using System.ComponentModel;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Recipes;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.Result;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;

public sealed partial class RecipeEditForm : XtraForm
{
    private readonly RecipeListDto? _editing;
    private readonly BindingList<RecipeItemDto> _lines = [];
    private readonly Dictionary<Guid, List<AtelierTransferProductDto>> _transferredByWorkshop = [];
    private List<ProductLookUpDto> _mamulProducts = [];
    private List<ChartOfAccountLookUpDto> _workshops = [];
    private RepositoryItemSearchLookUpEdit riProduct = null!;
    private bool _suppressProductLoad;

    private sealed record GridProductItem(Guid Id, string Name, string Code, string UnitTypeName)
    {
        public string Display => $"{Name} ({Code}) [{UnitTypeName}]";
    }

    public RecipeEditForm() : this(null) { }

    public RecipeEditForm(RecipeListDto? existing)
    {
        _editing = existing;
        InitializeComponent();

        Text = _editing is null ? "Yeni Reçete" : "Reçete Düzenle";
        lblTitle.Text = Text;
        lblSubtitle.Text = _editing is null
            ? "Atölyeyi seçin, mamül ürünü belirleyin ve reçete kalemlerini girin."
            : $"{_editing.ProductName} üretim reçetesini güncelleyin.";

        WireEvents();
    }

    private void WireEvents()
    {
        Load += RecipeEditForm_Load;
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (_, _) => Close();
        btnAddLine.Click += (_, _) => AddEmptyLine();
        btnDeleteLine.Click += (_, _) => DeleteSelectedLine();
        gridLinesView.CellValueChanged += GridLinesView_CellValueChanged;
        lookUpWorkshop.EditValueChanged += LookUpWorkshop_EditValueChanged;
        lookUpProduct.EditValueChanged += LookUpProduct_EditValueChanged;
    }

    private async void RecipeEditForm_Load(object? sender, EventArgs e)
    {
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            _workshops = ((await mediator.Send(new ChartOfAccountLookUpQuery(), CancellationToken.None)).Data ?? [])
                .Where(a => a.Type == ChartOfAccountType.Workshop)
                .ToList();
            _mamulProducts = await mediator.Send(new ProductLookUpQuery("152"), CancellationToken.None);
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Veriler yüklenemedi: " + ex.Message, ToastType.Warning);
        }

        SetupWorkshopLookup();
        SetupMamulLookup();
        SetupGrid();

        if (_editing is not null)
        {
            _suppressProductLoad = true;
            lookUpProduct.EditValue = _editing.ProductId;
            _suppressProductLoad = false;
            await LoadRecipeItemsAsync();
        }
        else
        {
            _lines.Add(new RecipeItemDto());
        }

        RefreshMaterialLookup();
    }

    private void SetupWorkshopLookup()
    {
        lookUpWorkshop.Properties.DataSource = _workshops;
        lookUpWorkshop.Properties.ValueMember = nameof(ChartOfAccountLookUpDto.Id);
        lookUpWorkshop.Properties.DisplayMember = nameof(ChartOfAccountLookUpDto.Display);
        lookUpWorkshop.Properties.BestFitMode = BestFitMode.BestFit;
        lookUpWorkshop.Properties.PopupFilterMode = PopupFilterMode.Contains;

        lookUpWorkshopView.Columns.Clear();
        lookUpWorkshopView.OptionsBehavior.AutoPopulateColumns = false;
        GridColumn workshopColumn = lookUpWorkshopView.Columns.AddField(nameof(ChartOfAccountLookUpDto.Display));
        workshopColumn.Caption = "Atölye";
        workshopColumn.VisibleIndex = 0;
        workshopColumn.Width = 320;
        lookUpWorkshopView.BestFitColumns();
    }

    private void SetupMamulLookup()
    {
        lookUpProduct.Properties.DataSource = _mamulProducts;
        lookUpProduct.Properties.ValueMember = nameof(ProductLookUpDto.Id);
        lookUpProduct.Properties.DisplayMember = nameof(ProductLookUpDto.Display);
        lookUpProduct.Properties.BestFitMode = BestFitMode.BestFit;
        lookUpProduct.Properties.PopupFilterMode = PopupFilterMode.Contains;

        lookUpProductView.Columns.Clear();
        lookUpProductView.OptionsBehavior.AutoPopulateColumns = false;
        GridColumn nameColumn = lookUpProductView.Columns.AddField(nameof(ProductLookUpDto.Name));
        nameColumn.Caption = "Mamül";
        nameColumn.VisibleIndex = 0;
        nameColumn.Width = 260;
        GridColumn codeColumn = lookUpProductView.Columns.AddField(nameof(ProductLookUpDto.ProductCode));
        codeColumn.Caption = "Ürün Kodu";
        codeColumn.VisibleIndex = 1;
        codeColumn.Width = 100;
        GridColumn unitColumn = lookUpProductView.Columns.AddField(nameof(ProductLookUpDto.ProductUnitTypeName));
        unitColumn.Caption = "Birim";
        unitColumn.VisibleIndex = 2;
        unitColumn.Width = 70;
        GridColumn warehouseColumn = lookUpProductView.Columns.AddField(nameof(ProductLookUpDto.WarehouseName));
        warehouseColumn.Caption = "Depo";
        warehouseColumn.VisibleIndex = 3;
        warehouseColumn.Width = 120;
        lookUpProductView.BestFitColumns();
    }

    private void SetupGrid()
    {
        gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
        gridLinesView.OptionsView.ColumnAutoWidth = false;
        gridLinesView.OptionsView.ShowGroupPanel = false;
        gridLinesView.OptionsView.ShowFooter = true;
        gridLinesView.RowHeight = 26;
        gridLinesControl.DataSource = _lines;

        riProduct = new RepositoryItemSearchLookUpEdit
        {
            ValueMember = nameof(GridProductItem.Id),
            DisplayMember = nameof(GridProductItem.Display),
            NullText = "Malzeme seçiniz...",
            PopupFilterMode = PopupFilterMode.Contains,
            DataSource = new List<GridProductItem>()
        };
        riProduct.View.Columns.Clear();
        riProduct.View.OptionsBehavior.AutoPopulateColumns = false;
        GridColumn textColumn = riProduct.View.Columns.AddField(nameof(GridProductItem.Display));
        textColumn.Caption = "Malzeme";
        textColumn.VisibleIndex = 0;
        textColumn.Width = 320;
        riProduct.View.BestFitColumns();

        RepositoryItemTextEdit riUnitName = new() { ReadOnly = true };
        RepositoryItemSpinEdit riQuantity = new()
        {
            MinValue = 0.0001m,
            MaxValue = 999999999,
            Increment = 1,
            DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n2" }
        };

        gridLinesControl.RepositoryItems.AddRange([riProduct, riUnitName, riQuantity]);

        GridColumn noColumn = new()
        {
            Caption = "No",
            FieldName = "RowNo",
            UnboundType = DevExpress.Data.UnboundColumnType.Integer,
            Visible = true,
            Width = 45,
            OptionsColumn = { AllowEdit = false, ShowInCustomizationForm = false },
            AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Center } }
        };
        GridColumn productColumn = new()
        {
            Caption = "Malzeme",
            FieldName = nameof(RecipeItemDto.ProductId),
            ColumnEdit = riProduct,
            Visible = true,
            Width = 380
        };
        GridColumn unitColumn = new()
        {
            Caption = "Birim",
            FieldName = nameof(RecipeItemDto.ProductUnitTypeName),
            ColumnEdit = riUnitName,
            Visible = true,
            Width = 90,
            OptionsColumn = { AllowEdit = false },
            AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Center } }
        };
        GridColumn quantityColumn = new()
        {
            Caption = "Miktar (1 birim için)",
            FieldName = nameof(RecipeItemDto.Quantity),
            ColumnEdit = riQuantity,
            Visible = true,
            Width = 170,
            AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
        };

gridLinesView.Columns.AddRange([noColumn, productColumn, unitColumn, quantityColumn]);
            GridColumnFactory.RegisterManualNumericColumns(gridLinesView);
            gridLinesView.CustomUnboundColumnData += GridLinesView_CustomUnboundColumnData;

        noColumn.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Count;
        noColumn.SummaryItem.DisplayFormat = "Kalem: {0}";

        foreach (GridColumn col in gridLinesView.Columns)
        {
            col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
        }
    }

    private void GridLinesView_CustomUnboundColumnData(object? sender, CustomColumnDataEventArgs e)
    {
        if (e.IsGetData && e.Column.FieldName == "RowNo")
        {
            e.Value = e.ListSourceRowIndex + 1;
        }
    }

    private async void LookUpWorkshop_EditValueChanged(object? sender, EventArgs e)
    {
        await FilterMamulByWorkshopAsync();
        await LoadTransferredProductsForWorkshopAsync();
        RefreshMaterialLookup();

        if (_editing is not null)
        {
            return;
        }

        Guid productId = lookUpProduct.EditValue is Guid pid && pid != Guid.Empty ? pid : Guid.Empty;
        if (productId != Guid.Empty && !IsMamulVisible(productId))
        {
            lookUpProduct.EditValue = null;
            _lines.Clear();
            _lines.Add(new RecipeItemDto());
        }
    }

    private async Task FilterMamulByWorkshopAsync()
    {
        List<ProductLookUpDto> filtered;
        if (SelectedWorkshopId is Guid workshopId)
        {
            ChartOfAccountLookUpDto? workshop = _workshops.FirstOrDefault(w => w.Id == workshopId);
            Guid? linkId = workshop?.FinishedAccountId;
            string? workshopName = workshop?.Name?.Trim();
            filtered = _mamulProducts
                .Where(p => p.WarehouseCode.StartsWith("152", StringComparison.OrdinalIgnoreCase)
                            && (linkId is Guid lid
                                ? p.CategoryId == lid
                                : string.Equals(p.CategoryName?.Trim(), workshopName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        else
        {
            filtered = _mamulProducts;
        }

        if (_editing is not null && _editing.ProductId != Guid.Empty
            && filtered.All(p => p.Id != _editing.ProductId))
        {
            ProductLookUpDto? editingProduct = _mamulProducts.FirstOrDefault(p => p.Id == _editing.ProductId);
            if (editingProduct is not null)
            {
                filtered.Insert(0, editingProduct);
            }
        }

        lookUpProduct.Properties.DataSource = filtered;
        lookUpProduct.Properties.NullText = filtered.Count == 0
            ? "Bu atölye için 152 mamül ürünü tanımlı değil"
            : "Mamül ürün seçiniz...";
    }

    private bool IsMamulVisible(Guid productId)
    {
        if (lookUpProduct.Properties.DataSource is not System.Collections.IEnumerable items)
        {
            return false;
        }

        foreach (object item in items)
        {
            if (item is ProductLookUpDto p && p.Id == productId)
            {
                return true;
            }
        }

        return false;
    }

    private async Task LoadTransferredProductsForWorkshopAsync()
    {
        if (SelectedWorkshopId is not Guid workshopId || _transferredByWorkshop.ContainsKey(workshopId))
        {
            return;
        }

        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            List<AtelierTransferProductDto> transferred =
                (await mediator.Send(new AtelierTransferProductsQuery(workshopId), CancellationToken.None)).Data ?? [];
            _transferredByWorkshop[workshopId] = transferred;
        }
        catch
        {
            _transferredByWorkshop[workshopId] = [];
        }
    }

    private void RefreshMaterialLookup()
    {
        List<GridProductItem> items = [];

        if (SelectedWorkshopId is Guid workshopId
            && _transferredByWorkshop.TryGetValue(workshopId, out List<AtelierTransferProductDto>? transferred))
        {
            items.AddRange(transferred.Select(t => new GridProductItem(
                t.ProductId, t.ProductName, t.ProductCode, t.UnitTypeName)));
        }

        foreach (RecipeItemDto line in _lines)
        {
            if (line.ProductId != Guid.Empty && items.All(i => i.Id != line.ProductId))
            {
                items.Add(new GridProductItem(line.ProductId, line.ProductName, "", line.ProductUnitTypeName));
            }
        }

        if (riProduct is not null)
        {
            riProduct.DataSource = items;
            riProduct.NullText = SelectedWorkshopId is null
                ? "Önce atölye seçiniz..."
                : items.Count == 0
                    ? "Bu atölyeye transfer edilen ürün bulunamadı"
                    : "Malzeme seçiniz...";
        }

        gridLinesView.RefreshData();
    }

    private void LookUpProduct_EditValueChanged(object? sender, EventArgs e)
    {
        if (_suppressProductLoad)
        {
            return;
        }

        if (_editing is not null)
        {
            LoadRecipeItemsForProduct();
        }
        else if (SelectedWorkshopId is Guid workshopId)
        {
            _ = LoadRecipeItemsForWorkshopProductAsync(workshopId);
        }
    }

    private void LoadRecipeItemsForProduct()
    {
        if (lookUpProduct.EditValue is Guid productId && productId != Guid.Empty)
        {
            _ = RefreshLinesAsync(productId);
        }
    }

    private async Task LoadRecipeItemsForWorkshopProductAsync(Guid workshopId)
    {
        if (lookUpProduct.EditValue is not Guid productId || productId == Guid.Empty)
        {
            _lines.Clear();
            _lines.Add(new RecipeItemDto());
            RefreshMaterialLookup();
            return;
        }

        await LoadTransferredProductsForWorkshopAsync();
        await RefreshLinesAsync(productId);
        RefreshMaterialLookup();
    }

    private async Task LoadRecipeItemsAsync()
    {
        if (_editing is null)
        {
            return;
        }

        await RefreshLinesAsync(_editing.ProductId);
        RefreshMaterialLookup();
    }

    private async Task RefreshLinesAsync(Guid productId)
    {
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            RecipeDto? recipe = await mediator.Send(new RecipeGetByProductQuery(productId), CancellationToken.None);

            _lines.Clear();
            if (recipe is not null && recipe.Items.Count > 0)
            {
                foreach (RecipeItemDto item in recipe.Items)
                {
                    _lines.Add(new RecipeItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductUnitTypeName = item.ProductUnitTypeName,
                        Quantity = item.Quantity
                    });
                }
            }
            else
            {
                _lines.Add(new RecipeItemDto());
            }

            RefreshMaterialLookup();
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Reçete yüklenemedi: " + ex.Message, ToastType.Warning);
            _lines.Clear();
            _lines.Add(new RecipeItemDto());
        }
    }

    private void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
    {
        if (e.RowHandle < 0)
        {
            return;
        }

        RecipeItemDto? line = gridLinesView.GetRow(e.RowHandle) as RecipeItemDto;
        if (line is null)
        {
            return;
        }

        if (e.Column?.FieldName == nameof(RecipeItemDto.ProductId))
        {
            Guid newProductId = line.ProductId;
            if (newProductId == Guid.Empty)
            {
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            GridProductItem? item = (riProduct.DataSource as System.Collections.IEnumerable)?.Cast<object?>()
                .OfType<GridProductItem>()
                .FirstOrDefault(x => x.Id == newProductId);

            if (item is null)
            {
                line.ProductId = Guid.Empty;
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            if (_lines.Any(l => l.ProductId == newProductId && !ReferenceEquals(l, line)))
            {
                ToastHelper.Show("Bu malzeme zaten reçetedeki başka bir satırdadır.", ToastType.Warning);
                line.ProductId = Guid.Empty;
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            line.ProductName = item.Name;
            line.ProductUnitTypeName = item.UnitTypeName;
        }
        else if (e.Column?.FieldName == nameof(RecipeItemDto.Quantity))
        {
            if (line.Quantity < 0)
            {
                line.Quantity = 0;
            }
        }
    }

    private void AddEmptyLine()
    {
        _lines.Add(new RecipeItemDto());
        int lastIdx = _lines.Count - 1;
        gridLinesView.FocusedRowHandle = lastIdx;
        gridLinesView.FocusedColumn = gridLinesView.Columns[nameof(RecipeItemDto.ProductId)];
        RefreshMaterialLookup();
    }

    private void DeleteSelectedLine()
    {
        int[] rows = gridLinesView.GetSelectedRows();
        if (rows.Length == 0)
        {
            rows = [gridLinesView.FocusedRowHandle];
        }

        if (rows.Length == 0)
        {
            return;
        }

        if (MsgBox.ConfirmRowDelete(rows.Length, "malzeme") != DialogResult.Yes)
        {
            return;
        }

        foreach (int row in rows.OrderByDescending(r => r))
        {
            if (gridLinesView.GetRow(row) is RecipeItemDto line)
            {
                _lines.Remove(line);
            }
        }

        if (_lines.Count == 0)
        {
            _lines.Add(new RecipeItemDto());
        }

        RefreshMaterialLookup();
    }

    private Guid? SelectedWorkshopId
        => lookUpWorkshop.EditValue is Guid id && id != Guid.Empty ? id : null;

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (SelectedWorkshopId is not Guid workshopId)
        {
            ToastHelper.Show("Atölye seçilmelidir.", ToastType.Warning);
            lookUpWorkshop.Focus();
            return;
        }

        Guid productId = lookUpProduct.EditValue is Guid pid && pid != Guid.Empty
            ? pid
            : _editing?.ProductId ?? Guid.Empty;
        if (productId == Guid.Empty)
        {
            ToastHelper.Show("Mamül ürün seçilmelidir.", ToastType.Warning);
            lookUpProduct.Focus();
            return;
        }

        List<RecipeItemDto> materialLines = _lines.Where(l => l.ProductId != Guid.Empty && l.Quantity > 0).ToList();
        if (materialLines.Count == 0)
        {
            ToastHelper.Show("Reçetede en az bir malzeme satırı bulunmalıdır.", ToastType.Warning);
            return;
        }

        await LoadTransferredProductsForWorkshopAsync();

        if (_transferredByWorkshop.TryGetValue(workshopId, out List<AtelierTransferProductDto>? transferred)
            && transferred.Count > 0)
        {
            HashSet<Guid> transferredIds = transferred.Select(t => t.ProductId).ToHashSet();
            List<RecipeItemDto> incompatibleLines =
                materialLines.Where(l => !transferredIds.Contains(l.ProductId)).ToList();

            if (incompatibleLines.Count > 0)
            {
                string incompatibleItemNames = string.Join(
                    Environment.NewLine, incompatibleLines.Select(l => $"• {l.ProductName}"));

                if (MsgBox.Confirm(
                    "Aşağıdaki kalem(ler) atölyeye transfer edilen ürünler arasında bulunamadı:" +
                    Environment.NewLine + Environment.NewLine + incompatibleItemNames +
                    Environment.NewLine + Environment.NewLine + "Reçeteyi yine de kaydetmek istiyor musunuz?",
                    "Reçete Uyumsuzluk Uyarısı") != DialogResult.Yes)
                {
                    return;
                }
            }
        }

        List<RecipeItemRow> items = materialLines
            .Select(l => new RecipeItemRow(l.ProductId, l.Quantity))
            .ToList();

        btnSave.Enabled = false;
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            bool ok = await CrudExecutor.ExecuteAsync(new RecipeSaveCommand(productId, true, items, workshopId));
            if (ok)
            {
                ToastHelper.Show("Reçete kaydedildi.", ToastType.Success);
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