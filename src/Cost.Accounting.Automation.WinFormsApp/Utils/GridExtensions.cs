using System.Reflection;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// DevExpress GridView için extension metodlar.
/// ConfigureColumns → ana tablo,  ConfigureDetailColumns → alt tablo yapılandırması.
/// </summary>
public static class GridExtensions
{
    public static void ConfigureColumns<T>(this GridView view, params string[] excludeColumns)
    {
        view.OptionsBehavior.Editable = false;
        view.OptionsBehavior.AllowAddRows = DefaultBoolean.False;
        view.OptionsBehavior.AllowDeleteRows = DefaultBoolean.False;

        view.OptionsSelection.EnableAppearanceFocusedCell = false;
        view.OptionsSelection.EnableAppearanceFocusedRow = false;
        view.OptionsSelection.MultiSelect = true;
        view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.CheckBoxRowSelect;
        view.OptionsSelection.CheckBoxSelectorColumnWidth = 40;
        view.OptionsSelection.ShowCheckBoxSelectorInColumnHeader = DefaultBoolean.True;
        view.OptionsSelection.ShowCheckBoxSelectorInGroupRow = DefaultBoolean.True;

        view.OptionsView.ColumnAutoWidth = false;
        view.OptionsView.ShowGroupPanel = true;
        view.OptionsView.ShowFooter = true;
        view.OptionsView.RowAutoHeight = true;
        view.OptionsView.EnableAppearanceOddRow = true;
        view.OptionsView.ShowHorizontalLines = DefaultBoolean.True;

        view.Appearance.OddRow.Options.UseBackColor = true;
        view.Appearance.OddRow.BackColor = Color.FromArgb(225, 230, 235);
        view.Appearance.EvenRow.Options.UseBackColor = true;
        view.Appearance.EvenRow.BackColor = Color.White;
        view.Appearance.HorzLine.BackColor = Color.FromArgb(210, 210, 210);

        view.GridControl.ForceInitialize();
        var checkCol = view.VisibleColumns.FirstOrDefault(c => c.VisibleIndex == 1);
        if (checkCol is not null)
            checkCol.Fixed = FixedStyle.Left;

        ConfigureFindPanel(view);
        view.SetupRowIndicatorNumbers();

        var properties = typeof(T)
            .GetProperties()
            .Select(p => new
            {
                Property = p,
                Attr = p.GetCustomAttribute<ColumnAttribute>(inherit: true)
            })
            .Where(x => x.Attr is { IsVisible: true })
            .Where(x => excludeColumns is null || !excludeColumns.Contains(x.Property.Name))
            .OrderBy(x => x.Attr!.Order)
            .ToList();

        var memoEdit = new RepositoryItemMemoEdit
        {
            WordWrap = true,
            AutoHeight = true,
            ScrollBars = ScrollBars.None
        };
        view.GridControl.RepositoryItems.Add(memoEdit);

        int visibleIndex = 1;
        foreach (var item in properties)
        {
            var col = view.Columns[item.Property.Name]
                      ?? view.Columns.AddField(item.Property.Name);

            col.Caption = item.Attr!.Title;
            col.Width = item.Attr.Width;
            col.Visible = true;
            col.VisibleIndex = visibleIndex++;

            ApplyColumnEditor(col, item.Property, memoEdit);
            ApplyAlignment(col, item.Attr.Alignment);
            ApplyFormat(col, item.Property, item.Attr.Format);

            col.AppearanceCell.TextOptions.VAlignment = VertAlignment.Center;
        }

        foreach (GridColumn col in view.Columns)
        {
            if (!properties.Any(x => x.Property.Name == col.FieldName))
                col.Visible = false;
        }

        view.SetupCustomDisplayText();
        view.SetupBaseStyles();
        DevExpressLocalizers.SetupFilterPopupTranslation(view.GridControl);
    }

    public static void ConfigureDetailColumns<TDetail>(
        this GridView mainView,
        string viewCaption,
        params string[] excludeColumns)
    {
        mainView.OptionsDetail.ShowDetailTabs = false;
        mainView.MasterRowExpanded += (s, e) =>
        {
            if (s is not GridView master) return;
            if (master.GetDetailView(e.RowHandle, e.RelationIndex) is not GridView detail) return;

            detail.OptionsView.ColumnAutoWidth = false;
            detail.ConfigureColumns<TDetail>(excludeColumns);

            foreach (GridColumn col in detail.Columns)
            {
                var attr = typeof(TDetail)
                    .GetProperty(col.FieldName)
                    ?.GetCustomAttribute<ColumnAttribute>();

                if (attr is null) continue;
                col.Width = attr.Width;
                col.MinWidth = attr.Width;
            }

            detail.ViewCaption = viewCaption;
            detail.OptionsView.ShowViewCaption = true;
            detail.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
            detail.OptionsView.ShowGroupPanel = false;
            detail.GridControl.ForceInitialize();
        };
    }

    public static void SetupRowIndicatorNumbers(this GridView view)
    {
        view.IndicatorWidth = 70;
        view.CustomDrawRowIndicator += (s, e) =>
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
                e.Info.DisplayText = (e.RowHandle + 1).ToString("00000");
        };
    }

    public static void SetupCustomDisplayText(this GridView view)
    {
        view.CustomColumnDisplayText += (sender, e) =>
        {
            if (e.Value is not bool boolValue) return;
            e.DisplayText = e.Column.FieldName switch
            {
                "IsActive" => boolValue ? "Aktif" : "Pasif",
                "IsDeleted" => boolValue ? "Silindi" : "Mevcut",
                _ => e.DisplayText
            };
        };
    }

    public static void SetupBaseStyles(this GridView view)
    {
        view.RowStyle += (sender, e) =>
        {
            if (e.RowHandle < 0 || sender is not GridView gv) return;

            if (gv.IsRowSelected(e.RowHandle))
            {
                e.Appearance.BeginUpdate();
                e.Appearance.BackColor = Color.FromArgb(255, 165, 0);
                e.Appearance.BackColor2 = Color.FromArgb(255, 140, 0);
                e.Appearance.Options.UseBackColor = true;
                e.Appearance.ForeColor = Color.Black;
                e.Appearance.Font = new Font(gv.Appearance.Row.Font, FontStyle.Bold);
                e.Appearance.EndUpdate();
                e.HighPriority = true;
                return;
            }

            if (gv.GetRow(e.RowHandle) is not { } row) return;

            var props = row.GetType().GetProperties();
            bool isDeleted = (bool?)props.FirstOrDefault(p => p.Name == "IsDeleted")?.GetValue(row) ?? false;
            bool isActive = (bool?)props.FirstOrDefault(p => p.Name == "IsActive")?.GetValue(row) ?? true;

            if (!isDeleted && isActive) return;

            e.Appearance.ForeColor = Color.Gray;
            e.Appearance.Font = new Font(gv.Appearance.Row.Font, FontStyle.Italic);
            e.HighPriority = true;
        };
    }

    private static void ConfigureFindPanel(GridView view)
    {
        view.OptionsFind.AlwaysVisible = true;
        view.OptionsFind.ShowClearButton = true;
        view.OptionsFind.ShowFindButton = true;
    }

    private static void ApplyColumnEditor(
        GridColumn col,
        PropertyInfo property,
        RepositoryItemMemoEdit memoEdit)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (type == typeof(string))
        {
            col.ColumnEdit = memoEdit;
            col.AppearanceCell.TextOptions.WordWrap = WordWrap.Wrap;
        }
        else if (type == typeof(bool))
        {
            col.ColumnEdit = new RepositoryItemTextEdit();
        }
    }

    private static void ApplyAlignment(GridColumn col, string? alignment)
    {
        if (string.IsNullOrWhiteSpace(alignment)) return;
        if (!Enum.TryParse(alignment, out HorzAlignment align)) return;
        col.AppearanceCell.TextOptions.HAlignment = align;
        col.AppearanceHeader.TextOptions.HAlignment = align;
    }

    private static void ApplyFormat(GridColumn col, PropertyInfo property, string? format)
    {
        if (string.IsNullOrWhiteSpace(format)) return;
        var typeName = property.PropertyType.Name;
        bool isDate = typeName.Contains("Date", StringComparison.OrdinalIgnoreCase)
                   || typeName.Contains("Offset", StringComparison.OrdinalIgnoreCase);
        col.DisplayFormat.FormatType = isDate ? FormatType.DateTime : FormatType.Numeric;
        col.DisplayFormat.FormatString = format;
    }
}
