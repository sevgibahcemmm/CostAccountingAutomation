using System.Globalization;
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
    /// <summary>Tüm sayısal kolonların zorunlu görünüm biçimi.</summary>
    public const string NumberFormat = "n2";

    /// <summary>Sayısal kolonların boş/null değerinde gösterdiği metin.</summary>
    public const string EmptyNumberText = "0,00";

    /// <summary>
    /// Sayısal kabul edilen tipler. Kullanıcı talebi doğrultusunda hem ondalıklı
    /// (decimal/double/float) hem de tam sayısal (int/long/short/byte) alanlar
    /// <see cref="NumberFormat"/> biçimine sokulur; <c>Nullable&lt;T&gt;</c>
    /// karşılıkları da kapsama dâhildir.
    /// </summary>
    private static readonly HashSet<Type> NumericTypes =
    [
        typeof(decimal), typeof(double), typeof(float),
        typeof(int), typeof(long), typeof(short), typeof(byte),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte)
    ];

    private sealed class ViewState
    {
        public RepositoryItemCheckEdit? Check { get; set; }
        public RepositoryItemTextEdit? BoolText { get; set; }
        public Dictionary<string, ColumnAttribute> BoolTextColumns { get; } = [];
        public Dictionary<string, string> StringFormatColumns { get; } = [];
        public Dictionary<string, string> NumericColumns { get; } = [];
        public bool DisplayTextHandlerAttached { get; set; }
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

        ViewState state = EnsureDisplayTextHandler(view);
        if (state.Check is null || state.BoolText is null)
        {
            state.Check = new RepositoryItemCheckEdit { ReadOnly = true };
            state.BoolText = new RepositoryItemTextEdit { ReadOnly = true };
            if (view.GridControl is { } gridControl)
            {
                gridControl.RepositoryItems.Add(state.Check);
                gridControl.RepositoryItems.Add(state.BoolText);
            }
        }

        state.BoolTextColumns.Clear();
        state.StringFormatColumns.Clear();
        state.NumericColumns.Clear();

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
            else if (IsNumeric(property.PropertyType))
            {
                // Yüzde/oran gibi kasıtlı olarak farklı biçimlenen sayısal
                // alanlar (p0, p1) kendi biçimini korur.
                state.NumericColumns[property.Name] = attr.Format;

                if (string.IsNullOrWhiteSpace(attr.Format) || attr.Format == "G")
                {
                    column.DisplayFormat.FormatType = FormatType.Custom;
                    column.DisplayFormat.FormatString = NumberFormat;
                }
                else
                {
                    column.DisplayFormat.FormatType = FormatType.Custom;
                    column.DisplayFormat.FormatString = attr.Format;
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

    /// <summary>
    /// Verilen tip sayısal mı (<see cref="NumericTypes"/> veya nullable karşılığı).
    /// </summary>
    private static bool IsNumeric(Type type)
    {
        Type? underlying = Nullable.GetUnderlyingType(type);
        return NumericTypes.Contains(underlying ?? type);
    }

    /// <summary>
    /// <see cref="ConfigureFromAttributes"/> ile kurulmayan, kod içinde elle eklenen
    /// kolonlar (unbound fiyat sütunları vb.) için <c>0,00</c> kuralını uygular.
    /// Kolonun kendi DisplayFormat'ı varsa (örn. <c>p0</c> yüzde) korunur.
    ///
    /// <paramref name="view"/> daha önce <see cref="ConfigureFromAttributes"/>
    /// çağrılmamışsa gerekli dinleyici burada kurulur; böylece metot tek başına
    /// da çalışır.
    /// </summary>
    public static void RegisterManualNumericColumns(GridView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        ViewState state = EnsureDisplayTextHandler(view);

        foreach (GridColumn column in view.Columns)
        {
            // ColumnType, unbound kolonlarda UnboundDataType'ı da kapsar.
            Type? columnType = column.ColumnType;
            if (columnType is null || !IsNumeric(columnType))
            {
                continue;
            }

            // Zaten öznitelikten işlenmiş kolonları atla.
            if (state.NumericColumns.ContainsKey(column.FieldName))
            {
                continue;
            }

            string format = ReadExistingFormat(column);
            state.NumericColumns[column.FieldName] = format;

            if (format == "G")
            {
                column.DisplayFormat.FormatType = FormatType.Custom;
                column.DisplayFormat.FormatString = NumberFormat;
            }
        }
    }

    /// <summary>
    /// <see cref="RenderDisplayText"/> dinleyicisinin view'a yalnızca bir kez
    /// bağlandığını garanti eder ve state'i döndürür.
    /// </summary>
    private static ViewState EnsureDisplayTextHandler(GridView view)
    {
        ViewState state = _states.GetOrCreateValue(view);
        if (!state.DisplayTextHandlerAttached)
        {
            view.CustomColumnDisplayText += (_, e) => RenderDisplayText(view, e);
            state.DisplayTextHandlerAttached = true;
        }

        return state;
    }

    /// <summary>
    /// Kolonun halihazırda sahip olduğu biçimi döndürür; biçim yoksa "G".
    /// Hem <see cref="FormatType.Custom"/> hem <see cref="FormatType.Numeric"/>
    /// yakalanır; aksi hâlde <c>n2</c> uygulanmış kolonlar biçimsiz sayılırdı.
    /// </summary>
    private static string ReadExistingFormat(GridColumn column)
    {
        FormatType type = column.DisplayFormat.FormatType;
        if (type is FormatType.Custom or FormatType.Numeric
            && !string.IsNullOrWhiteSpace(column.DisplayFormat.FormatString))
        {
            return column.DisplayFormat.FormatString;
        }

        return "G";
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
            return;
        }

        if (state.NumericColumns.TryGetValue(e.Column.FieldName, out string? numericFormat))
        {
            e.DisplayText = RenderNumber(e.Value, numericFormat);
        }
    }

    /// <summary>
    /// Sayısal hücreyi biçimlendirir. Null/DBNull değerler de <c>0,00</c> olarak
    /// yazılır; aksi hâlde ondalıklı alanlarda boş hücre boş görünürdü.
    /// </summary>
    private static string RenderNumber(object? value, string format)
    {
        string effective = string.IsNullOrWhiteSpace(format) || format == "G" ? NumberFormat : format;

        if (value is null or DBNull)
        {
            return FormatZero(effective);
        }

        if (!IsNumericValue(value))
        {
            return Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        }

        try
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:" + effective + "}", value);
        }
        catch (FormatException)
        {
            // Geçersiz/özel biçim (örn. beklenmedik bir maske) satırı bozmamalı.
            return Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }

    /// <summary>Biçime göre sıfırın metinsel karşılığı (n2 için <c>0,00</c>).</summary>
    private static string FormatZero(string format)
    {
        try
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", 0m);
        }
        catch (FormatException)
        {
            return EmptyNumberText;
        }
    }

    private static bool IsNumericValue(object value) => IsNumeric(Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType());

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