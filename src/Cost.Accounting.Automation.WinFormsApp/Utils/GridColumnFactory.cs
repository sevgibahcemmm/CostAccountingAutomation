using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Cost.Accounting.Automation.Domain.Abstractions;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// DTO / satır tipindeki <see cref="ColumnAttribute"/> tanımlarından DevExpress GridView kolonları üretir.
/// Tüm formlardaki grid kolon başlıklarının tek kaynağı bu özniteliktir.
/// </summary>
public static class GridColumnFactory
{
    private sealed class ViewState
    {
        public RepositoryItemCheckEdit? Check { get; set; }
        public RepositoryItemTextEdit? BoolText { get; set; }
        public Dictionary<string, ColumnAttribute> BoolTextColumns { get; } = [];
        public Dictionary<string, string> StringFormatColumns { get; } = [];
    }

    private static readonly ConditionalWeakTable<GridView, ViewState> _states = new();

    /// <summary>
    /// <paramref name="rowType"/> üzerindeki [Column] attribute'larını okuyarak
    /// view'daki tüm kolonları yeniden oluşturur (sıralama, genişlik, hizalama,
    /// format, bool metin/checkbox ve string maskesi desteklenir).
    /// </summary>
    public static void ConfigureFromAttributes(GridView view, Type rowType)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(rowType);

        view.OptionsBehavior.AutoPopulateColumns = false;
        view.Columns.Clear();

        ViewState state = _states.GetOrCreateValue(view);
        if (state.Check is null || state.BoolText is null)
        {
            state.Check = new RepositoryItemCheckEdit { ReadOnly = true };
            state.BoolText = new RepositoryItemTextEdit { ReadOnly = true };
            if (view.GridControl is { } gridControl)
            {
                gridControl.RepositoryItems.Add(state.Check);
                gridControl.RepositoryItems.Add(state.BoolText);
            }
            view.CustomColumnDisplayText += (_, e) => RenderDisplayText(view, e);
        }

        state.BoolTextColumns.Clear();
        state.StringFormatColumns.Clear();

        var built = new List<(GridColumn Column, int Order)>();

        foreach (PropertyInfo property in rowType.GetProperties())
        {
            ColumnAttribute? attr = property.GetCustomAttribute<ColumnAttribute>();
            if (attr is null || !attr.IsVisible)
            {
                continue;
            }

            GridColumn column = new()
            {
                Caption = attr.Title,
                FieldName = property.Name,
                ToolTip = attr.Tip,
                Visible = true,
                Width = attr.Width
            };

            ApplyAlignment(column, attr.Alignment);

            if (property.PropertyType == typeof(bool))
            {
                if (attr.TrueText is null && attr.FalseText is null)
                {
                    column.ColumnEdit = state.Check;
                }
                else
                {
                    column.ColumnEdit = state.BoolText;
                    state.BoolTextColumns[property.Name] = attr;
                }
            }
            else if (property.PropertyType == typeof(string))
            {
                if (!string.IsNullOrWhiteSpace(attr.Format) && attr.Format != "G")
                {
                    state.StringFormatColumns[property.Name] = attr.Format;
                }
            }
            else if (!string.IsNullOrWhiteSpace(attr.Format) && attr.Format != "G")
            {
                column.DisplayFormat.FormatType = FormatType.Custom;
                column.DisplayFormat.FormatString = attr.Format;
            }

            built.Add((column, attr.Order));
        }

        foreach (GridColumn column in built.OrderBy(c => c.Order).Select(c => c.Column))
        {
            view.Columns.Add(column);
        }
    }

    private static void ApplyAlignment(GridColumn column, string alignment)
    {
        switch (alignment?.ToLowerInvariant())
        {
            case "center":
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Center;
                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Center;
                break;
            case "right":
                column.AppearanceCell.TextOptions.HAlignment = HorzAlignment.Far;
                column.AppearanceHeader.TextOptions.HAlignment = HorzAlignment.Far;
                break;
        }
    }

    private static void RenderDisplayText(GridView view, CustomColumnDisplayTextEventArgs e)
    {
        if (e.Column is null || !_states.TryGetValue(view, out ViewState? state))
        {
            return;
        }

        if (state.BoolTextColumns.TryGetValue(e.Column.FieldName, out ColumnAttribute? boolAttr))
        {
            e.DisplayText = e.Value is bool value && value ? boolAttr.TrueText : boolAttr.FalseText;
            return;
        }

        if (state.StringFormatColumns.TryGetValue(e.Column.FieldName, out string? format) && e.Value is string raw)
        {
            e.DisplayText = ApplyStringMask(raw, format);
        }
    }

    private static string ApplyStringMask(string value, string format)
    {
        StringBuilder sb = new(value.Length + 4);
        int valueIndex = 0;

        foreach (char c in format)
        {
            if (c == '#')
            {
                if (valueIndex < value.Length)
                {
                    sb.Append(value[valueIndex++]);
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}