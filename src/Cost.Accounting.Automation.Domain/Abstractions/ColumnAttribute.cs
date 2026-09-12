namespace Cost.Accounting.Automation.Domain.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public string Title { get; set; }
    public int Width { get; set; } = 100;
    public string Alignment { get; set; } = "Left"; // Left, Center, Right
    public string Format { get; set; } = "G";
    public int Order { get; set; } = 100;
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// bool property'ler için: true değerinde gösterilecek metin (örn. "Aktif", "Evet").
    /// Null ise checkbox olarak gösterilmeye devam eder.
    /// </summary>
    public string? TrueText { get; set; }

    /// <summary>
    /// bool property'ler için: false değerinde gösterilecek metin (örn. "Pasif", "Hayır").
    /// TrueText veya FalseText'ten biri set edilirse checkbox yerine metin gösterilir.
    /// </summary>
    public string? FalseText { get; set; }

    public ColumnAttribute(string title)
    {
        Title = title;
    }
}