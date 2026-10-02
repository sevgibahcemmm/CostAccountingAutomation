using Cost.Accounting.Automation.Application.Recipes;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.RecipeForms;

public sealed partial class RecipeMaterialSelectionForm : XtraForm
{
    private static readonly Color AccentColor = Color.FromArgb(64, 120, 200);
    private static readonly Color DangerColor = Color.FromArgb(200, 60, 60);

    public List<RecipeItemRow> SelectedItems { get; private set; } = [];

    /// <summary>
    /// Yalnızca Visual Studio tasarım yüzeyi içindir; gerçek açılışta malzeme
    /// listesiyle açılan yapıcı kullanılır.
    /// </summary>
    public RecipeMaterialSelectionForm()
    {
        InitializeComponent();
        DesignTime.Guard(typeof(RecipeMaterialSelectionForm));
    }

    public RecipeMaterialSelectionForm(
        string producedProductName,
        int producedQuantity,
        IReadOnlyCollection<RecipeMaterialCandidate> materials)
    {
        InitializeComponent();

        lblHeaderTitle.Text = "Reçeteye Eklenecek Malzemeler";
        Text = "Reçeteye Eklenecek Malzemeler";
        lblHeaderSub.Text = $"'{producedProductName}' üretim reçetesine hangi malzemeler eklensin?";

        foreach (RecipeMaterialCandidate material in materials)
        {
            decimal perUnitQuantity = producedQuantity > 0
                ? Math.Round(material.TotalQuantity / producedQuantity, 2)
                : 0m;
            checkedList.Items.Add(new RecipeMaterialsListItem(
                material.ProductId,
                material.ProductName,
                material.ProductUnitTypeName,
                perUnitQuantity), true);
        }

        checkedList.ItemCheck += (_, _) => UpdateBadge();
        btnSelectAll.Click += (_, _) => SetAllChecked(true);
        btnClear.Click += (_, _) => SetAllChecked(false);
        btnOk.Click += BtnOk_Click;
        btnCancel.Click += (_, _) => Close();

        UpdateBadge();
    }

    private void SetAllChecked(bool value)
    {
        for (int i = 0; i < checkedList.Items.Count; i++)
        {
            checkedList.SetItemChecked(i, value);
        }

        UpdateBadge();
    }

    private void UpdateBadge()
    {
        List<RecipeMaterialsListItem> selected = GetSelected();

        lblBadge.Text = selected.Count == 0
            ? "Hiçbir malzeme seçilmedi"
            : selected.Count == checkedList.Items.Count
                ? $"Tümü seçildi ({selected.Count})"
                : $"{selected.Count} malzeme seçildi";

        badgeAccentStrip.Appearance.BackColor = selected.Count == 0 ? DangerColor : AccentColor;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        List<RecipeMaterialsListItem> selected = GetSelected();

        if (selected.Count == 0)
        {
            ToastHelper.Show("Reçeteye en az bir malzeme seçmelisiniz.", ToastType.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        SelectedItems = selected
            .Select(s => new RecipeItemRow(s.ProductId, s.PerUnitQuantity))
            .ToList();
        DialogResult = DialogResult.OK;
    }

    private List<RecipeMaterialsListItem> GetSelected()
    {
        List<RecipeMaterialsListItem> result = [];

        for (int i = 0; i < checkedList.Items.Count; i++)
        {
            if (checkedList.GetItemChecked(i) && checkedList.Items[i].Value is RecipeMaterialsListItem item)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private sealed class RecipeMaterialsListItem
    {
        public RecipeMaterialsListItem(Guid productId, string productName, string unitTypeName, decimal perUnitQuantity)
        {
            ProductId = productId;
            ProductName = productName;
            ProductUnitTypeName = unitTypeName;
            PerUnitQuantity = perUnitQuantity;
        }

        public Guid ProductId { get; }
        public string ProductName { get; }
        public string ProductUnitTypeName { get; }
        public decimal PerUnitQuantity { get; }

        public override string ToString()
            => $"{ProductName}  ({ProductUnitTypeName})  —  Birim Miktar: {PerUnitQuantity:n2}";
    }
}

public sealed record RecipeMaterialCandidate(
    Guid ProductId,
    string ProductName,
    string ProductUnitTypeName,
    decimal TotalQuantity);