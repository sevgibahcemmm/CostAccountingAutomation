using System.ComponentModel;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.Application.StockIssues;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Products;
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
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;

public sealed partial class RecipeEditForm : XtraForm
{
    private readonly RecipeListDto? _editing;
    private readonly BindingList<RecipeItemDto> _lines = [];
    private List<ProductLookUpDto> _mamulProducts = [];
    private List<ChartOfAccountLookUpDto> _workshops = [];
    private RepositoryItemSearchLookUpEdit riProduct = null!;

    public RecipeEditForm() : this(null) { }

    public RecipeEditForm(RecipeListDto? existing)
    {
        _editing = existing;
        InitializeComponent();

        Text = _editing is null ? "Yeni Reçete" : "Reçete Düzenle";
        lblTitle.Text = Text;
        lblSubtitle.Text = _editing is null
            ? "Mamül ürün için bir reçete oluşturun."
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
            await LoadRecipeItemsAsync();
        }
        else
        {
            _lines.Add(new RecipeItemDto());
        }
    }

    private void SetupWorkshopLookup()
    {
        lookUpWorkshop.Properties.DataSource = _workshops;
        lookUpWorkshop.Properties.ValueMember = nameof(ChartOfAccountLookUpDto.Id);
        lookUpWorkshop.Properties.DisplayMember = nameof(ChartOfAccountLookUpDto.Display);
        lookUpWorkshop.Properties.BestFitMode = BestFitMode.BestFit;
        lookUpWorkshop.Properties.PopupFilterMode = PopupFilterMode.Contains;
    }

    private void SetupMamulLookup()
    {
        lookUpProduct.Properties.DataSource = _mamulProducts;
        lookUpProduct.Properties.ValueMember = nameof(ProductLookUpDto.Id);
        lookUpProduct.Properties.DisplayMember = nameof(ProductLookUpDto.Display);
        lookUpProduct.Properties.BestFitMode = BestFitMode.BestFit;
        lookUpProduct.Properties.PopupFilterMode = PopupFilterMode.Contains;
    }

    private void SetupGrid()
    {
        gridLinesView.OptionsBehavior.AutoPopulateColumns = false;
        gridLinesView.OptionsView.ColumnAutoWidth = false;
        gridLinesView.OptionsView.ShowGroupPanel = false;
        gridLinesView.RowHeight = 26;
        gridLinesControl.DataSource = _lines;

        riProduct = new RepositoryItemSearchLookUpEdit
        {
            ValueMember = nameof(ProductLookUpDto.Id),
            DisplayMember = nameof(ProductLookUpDto.Display),
            NullText = "Malzeme seçiniz...",
            PopupFilterMode = PopupFilterMode.Contains,
            DataSource = GetFilteredProducts()
        };

        RepositoryItemTextEdit riUnitName = new() { ReadOnly = true };
        RepositoryItemSpinEdit riQuantity = new()
        {
            MinValue = 0.0001m,
            MaxValue = 999999999,
            Increment = 1,
            DisplayFormat = { FormatType = FormatType.Numeric, FormatString = "n4" }
        };

        gridLinesControl.RepositoryItems.AddRange([riProduct, riUnitName, riQuantity]);

        GridColumn productColumn = new()
        {
            Caption = "Malzeme",
            FieldName = nameof(RecipeItemDto.ProductId),
            ColumnEdit = riProduct,
            Visible = true,
            Width = 340
        };
        GridColumn unitColumn = new()
        {
            Caption = "Birim",
            FieldName = nameof(RecipeItemDto.ProductUnitTypeName),
            ColumnEdit = riUnitName,
            Visible = true,
            Width = 80,
            OptionsColumn = { AllowEdit = false }
        };
        GridColumn quantityColumn = new()
        {
            Caption = "Miktar (1 birim için)",
            FieldName = nameof(RecipeItemDto.Quantity),
            ColumnEdit = riQuantity,
            Visible = true,
            Width = 150,
            AppearanceCell = { TextOptions = { HAlignment = HorzAlignment.Far } }
        };

        gridLinesView.Columns.AddRange([productColumn, unitColumn, quantityColumn]);

        foreach (GridColumn col in gridLinesView.Columns)
        {
            col.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
        }
    }

    private List<ProductLookUpDto> GetFilteredProducts()
    {
        if (SelectedWorkshopId is Guid wid)
        {
            return _mamulProducts.Where(p => p.WarehouseCode.StartsWith(wid.ToString()[..2])).ToList();
        }
        return _mamulProducts.Where(p => p.WarehouseCode.StartsWith("150")).ToList();
    }

    private void LookUpProduct_EditValueChanged(object? sender, EventArgs e)
    {
        if (SelectedWorkshopId is not Guid workshopId || workshopId == Guid.Empty)
        {
            ToastHelper.Show("Önce atölye seçmelisiniz.", ToastType.Warning);
            lookUpWorkshop.Focus();
            return;
        }
        LoadRecipeItemsForProduct();
    }

    private async void LookUpWorkshop_EditValueChanged(object? sender, EventArgs e)
    {
        await FilterLookupsByWorkshop();
    }

    private async Task FilterLookupsByWorkshop()
    {
        if (SelectedWorkshopId is not Guid workshopId || workshopId == Guid.Empty)
        {
            lookUpProduct.Properties.DataSource = _mamulProducts;
            if (riProduct is not null) riProduct.DataSource = _mamulProducts;
            return;
        }
        List<ProductLookUpDto> filtered = await GetWorkshopMaterialsAsync(workshopId);
        lookUpProduct.Properties.DataSource = filtered;
        if (riProduct is not null) riProduct.DataSource = filtered;
    }

    private async void LoadRecipeItemsForProduct()
    {
        if (SelectedWorkshopId is not Guid workshopId || workshopId == Guid.Empty)
        {
            ToastHelper.Show("Önce atölye seçmelisiniz.", ToastType.Warning);
            return;
        }

        Guid productId = _editing is not null ? _editing.ProductId : Guid.Empty;
        if (productId == Guid.Empty)
        {
            ToastHelper.Show("Mamül ürün seçilmelidir.", ToastType.Warning);
            lookUpProduct.Focus();
            return;
        }

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
                    if (!await IsProductInWorkshopAsync(item.ProductId, workshopId))
                    {
                        ToastHelper.Show($"{item.ProductName} ({item.ProductUnitTypeName}) atölyeye ait değil.", ToastType.Warning);
                        continue;
                    }
                    _lines.Add(new RecipeItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductUnitTypeName = item.ProductUnitTypeName,
                        Quantity = item.Quantity
                    });
                }
            }

            if (_lines.Count == 0)
            {
                _lines.Add(new RecipeItemDto());
            }
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Reçete yüklenemedi: " + ex.Message, ToastType.Warning);
            _lines.Add(new RecipeItemDto());
        }
    }

    private async Task LoadRecipeItemsAsync()
    {
        if (_editing is null) return;

        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            RecipeDto? recipe = await mediator.Send(new RecipeGetByProductQuery(_editing.ProductId), CancellationToken.None);

            _lines.Clear();
            if (recipe is not null && recipe.Items.Count > 0)
            {
                foreach (RecipeItemDto item in recipe.Items)
                {
                    if (!await IsProductInWorkshopAsync(item.ProductId, SelectedWorkshopId ?? Guid.Empty))
                    {
                        continue;
                    }
                    _lines.Add(new RecipeItemDto
                    {
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
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Reçete yüklenemedi: " + ex.Message, ToastType.Warning);
            _lines.Add(new RecipeItemDto());
        }
    }

    private async Task<List<ProductLookUpDto>> GetWorkshopMaterialsAsync(Guid workshopId)
    {
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            List<AtelierTransferProductDto> transferred =
                (await mediator.Send(new AtelierTransferProductsQuery(workshopId), CancellationToken.None)).Data ?? new List<AtelierTransferProductDto>();
            HashSet<Guid> ids = transferred.Select(t => t.ProductId).ToHashSet();
            return _mamulProducts.Where(p => ids.Contains(p.Id)).ToList();
        }
        catch
        {
            return new List<ProductLookUpDto>();
        }
    }

    private async void GridLinesView_CellValueChanged(object? sender, CellValueChangedEventArgs e)
    {
        if (e.RowHandle < 0) return;

        RecipeItemDto? line = gridLinesView.GetRow(e.RowHandle) as RecipeItemDto;
        if (line is null) return;

        if (e.Column?.FieldName == nameof(RecipeItemDto.ProductId))
        {
            Guid newProductId = line.ProductId;
            ProductLookUpDto? product = _mamulProducts.FirstOrDefault(p => p.Id == newProductId);
            if (product is null)
            {
                line.ProductId = Guid.Empty;
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            if (_lines.Any(l => l.ProductId == newProductId && l != line))
            {
                ToastHelper.Show("Bu malzeme zaten reçetedeki başka bir satırdadır.", ToastType.Warning);
                line.ProductId = Guid.Empty;
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            if (SelectedWorkshopId is Guid wid && !await IsProductInWorkshopAsync(newProductId, wid))
            {
                ToastHelper.Show("Bu malzeme seçili atölyeye ait değil.", ToastType.Warning);
                line.ProductId = Guid.Empty;
                line.ProductName = string.Empty;
                line.ProductUnitTypeName = string.Empty;
                return;
            }

            line.ProductId = newProductId;
            line.ProductName = product.Name;
            line.ProductUnitTypeName = product.ProductUnitTypeName;
        }
        else if (e.Column?.FieldName == nameof(RecipeItemDto.Quantity))
        {
            if (line.Quantity < 0)
            {
                line.Quantity = 0;
            }
        }
    }

    private static async Task<bool> IsProductInWorkshopAsync(Guid productId, Guid workshopId)
    {
        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            List<AtelierTransferProductDto> transferred =
                (await mediator.Send(new AtelierTransferProductsQuery(workshopId), CancellationToken.None)).Data ?? [];
            return transferred.Any(t => t.ProductId == productId);
        }
        catch { return false; }
    }

    private void AddEmptyLine()
    {
        _lines.Add(new RecipeItemDto());
        int lastIdx = _lines.Count - 1;
        gridLinesView.FocusedRowHandle = lastIdx;
        gridLinesView.FocusedColumn = gridLinesView.Columns[nameof(RecipeItemDto.ProductId)];
    }

    private void DeleteSelectedLine()
    {
        int[] rows = gridLinesView.GetSelectedRows();
        if (rows.Length == 0) return;

        if (MsgBox.Confirm("Seçili malzeme satırı silinecek. Emin misiniz?", "Silme Onayı") != DialogResult.Yes)
            return;

        foreach (int row in rows.OrderByDescending(r => r))
        {
            RecipeItemDto? line = gridLinesView.GetRow(row) as RecipeItemDto;
            if (line is not null)
            {
                _lines.Remove(line);
            }
        }

        if (_lines.Count == 0)
        {
            _lines.Add(new RecipeItemDto());
        }
    }

    private Guid? SelectedWorkshopId
        => lookUpWorkshop.EditValue is Guid id && id != Guid.Empty ? id : null;

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        List<RecipeItemDto> materialLines = _lines.Where(l => l.ProductId != Guid.Empty && l.Quantity > 0).ToList();
        if (materialLines.Count == 0)
        {
            ToastHelper.Show("Reçetede en az bir malzeme satırı bulunmalıdır.", ToastType.Warning);
            return;
        }

        Guid productId = _editing is not null ? _editing.ProductId : Guid.Empty;
        if (productId == Guid.Empty)
        {
            ToastHelper.Show("Mamül ürün seçilmelidir.", ToastType.Warning);
            lookUpProduct.Focus();
            return;
        }

        Guid? workshopId = SelectedWorkshopId;
        if (workshopId is null)
        {
            ToastHelper.Show("Atölye seçilmelidir.", ToastType.Warning);
            lookUpWorkshop.Focus();
            return;
        }

        foreach (RecipeItemDto line in materialLines)
        {
            if (!await IsProductInWorkshopAsync(line.ProductId, workshopId.Value))
            {
                ToastHelper.Show($"Seçilen malzemeten bazıları {lookUpWorkshop.Text} atölyesine ait değil.", ToastType.Warning);
                return;
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
            bool ok = await CrudExecutor.ExecuteAsync(new RecipeSaveCommand(productId, true, items, workshopId.Value));
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