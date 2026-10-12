using Cost.Accounting.Automation.Application.ChartOfAccounts;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

public static class ChartOfAccountFileReader
{
    public static List<ChartOfAccountImportRow> Read(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        List<string[]> cells = extension switch
        {
            ".xlsx" or ".xlsm" => ReadXlsx(filePath),
            ".csv" or ".txt" => ReadCsv(filePath),
            _ => throw new NotSupportedException("Desteklenen dosya formatları: .xlsx, .csv")
        };

        int nameColumn = DetectNameColumn(cells);

        List<ChartOfAccountImportRow> rows = new();

        foreach (string[] cell in cells)
        {
            if (IsHeader(cell))
            {
                continue;
            }

            string code = BuildCode(cell, nameColumn);
            string name = nameColumn >= 0 && nameColumn < cell.Length
                ? (cell[nameColumn] ?? string.Empty).Trim()
                : string.Empty;

            if (code.Length == 0 || name.Length == 0)
            {
                continue;
            }

            rows.Add(new ChartOfAccountImportRow(code, name));
        }

        return rows;
    }

    private static bool IsHeader(string[] cell)
    {
        string first = cell.Length > 0 ? (cell[0] ?? string.Empty).Trim().ToLowerInvariant() : string.Empty;
        if (first is "heskod" or "kod" or "hesap kodu" or "kodu" or "no" or "nosu")
        {
            return true;
        }

        foreach (string value in cell)
        {
            string text = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (text is "hesap adı" or "hesap adi" or "adi" or "ad" or "isim" or "name"
                or "açıklama" or "aciklama" or "description")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Başlık satırındaki "HESKOD / YARDIMCI n" sütunlarını sayıp hesap adının
    /// bulunduğu sütun indeksini bulur. Böylece plan düzeyi (yardımcı hesap
    /// sayısı) değiştiğinde sabit sütun varsayımı kırılmaz.
    /// </summary>
    private static int DetectNameColumn(List<string[]> rows)
    {
        foreach (string[] cells in rows.Take(10))
        {
            if (!IsHeader(cells))
            {
                continue;
            }

            int codeColumns = 0;

            for (int i = 0; i < cells.Length; i++)
            {
                string text = (cells[i] ?? string.Empty).Trim().ToLowerInvariant();
                if (text.StartsWith("heskod", StringComparison.Ordinal)
                    || text.StartsWith("yardimci", StringComparison.Ordinal)
                    || text.StartsWith("yardımcı", StringComparison.Ordinal))
                {
                    codeColumns = i + 1;
                }
            }

            if (codeColumns > 0)
            {
                return codeColumns;
            }
        }

        return -1;
    }

    private static string BuildCode(string[] cell, int nameColumn)
    {
        string first = cell.Length > 0 ? (cell[0] ?? string.Empty).Trim() : string.Empty;

        if (first.Length > 0 && (first.Contains('.') || first.Contains('-')))
        {
            return string.Join(".", SplitCode(first));
        }

        List<string> parts = new();

        if (first.Length > 0)
        {
            parts.AddRange(SplitPart(first));
        }

        int codeColumns = nameColumn > 0 ? nameColumn : cell.Length;

        for (int i = 1; i < codeColumns && i < cell.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(cell[i]))
            {
                continue;
            }

            parts.AddRange(SplitPart(cell[i]));
        }

        return string.Join(".", parts);
    }

    private static string[] SplitPart(string value)
        => value.Trim().Split(new[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries);

    private static string[] SplitCode(string value)
    {
        string raw = value.Trim().Replace('-', '.');
        return raw.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();
    }

    private static List<string[]> ReadCsv(string filePath)
    {
        List<string[]> rows = new();

        using StreamReader reader = new(filePath);

        while (reader.ReadLine() is string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] fields = SplitCsvLine(line);
            rows.Add(fields);
        }

        return rows;
    }

    private static string[] SplitCsvLine(string line)
    {
        bool hasSemicolon = line.Contains(';');
        char separator = hasSemicolon ? ';' : ',';

        List<string> fields = new();
        StringBuilder current = new();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == separator && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());

        return fields.Select(f => f.Trim()).ToArray();
    }

    private static List<string[]> ReadXlsx(string filePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(filePath);

        string[] sharedStrings = ReadSharedStrings(archive);

        ZipArchiveEntry? sheet = GetFirstSheetEntry(archive);
        if (sheet is null)
        {
            throw new InvalidDataException("Excel dosyasında çalışma sayfası bulunamadı");
        }

        List<string[]> rows = new();

        using Stream stream = sheet.Open();
        XDocument doc = XDocument.Load(stream);
        XElement? sheetData = doc.Root?.Element(XName.Get("sheetData", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"));
        if (sheetData is null)
        {
            return rows;
        }

        foreach (XElement row in sheetData.Elements(XName.Get("row", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")))
        {
            string[] cells = new string[20];

            foreach (XElement cell in row.Elements(XName.Get("c", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")))
            {
                string? reference = cell.Attribute("r")?.Value;
                if (reference is null || reference.Length < 2)
                {
                    continue;
                }

                char column = reference[0];
                int index = char.ToUpperInvariant(column) - 'A';
                if (index < 0 || index >= cells.Length)
                {
                    continue;
                }

                string? type = cell.Attribute("t")?.Value;
                XElement? value = cell.Element(XName.Get("v", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"));

                if (type == "s" && value is not null && int.TryParse(value.Value, out int stringIndex))
                {
                    cells[index] = stringIndex >= 0 && stringIndex < sharedStrings.Length ? sharedStrings[stringIndex] : string.Empty;
                }
                else if (type == "inlineStr")
                {
                    XElement? text = cell
                        .Element(XName.Get("is", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))?
                        .Element(XName.Get("t", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"));
                    cells[index] = text?.Value ?? string.Empty;
                }
                else
                {
                    cells[index] = value?.Value ?? string.Empty;
                }
            }

            if (cells.Any(c => !string.IsNullOrWhiteSpace(c)))
            {
                rows.Add(cells);
            }
        }

        return rows;
    }

    private static string[] ReadSharedStrings(ZipArchive archive)
    {
        ZipArchiveEntry? entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        List<string> strings = new();

        using Stream stream = entry.Open();
        XDocument doc = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        foreach (XElement si in doc.Root?.Elements(ns + "si") ?? Enumerable.Empty<XElement>())
        {
            StringBuilder builder = new();

            foreach (XElement text in si.Descendants(ns + "t"))
            {
                builder.Append(text.Value);
            }

            strings.Add(builder.ToString());
        }

        return strings.ToArray();
    }

    private static ZipArchiveEntry? GetFirstSheetEntry(ZipArchive archive)
    {
        ZipArchiveEntry? sheet1 = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheet1 is not null)
        {
            return sheet1;
        }

        return archive.Entries
            .Where(e => e.FullName.Contains("/worksheets/") && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }
}