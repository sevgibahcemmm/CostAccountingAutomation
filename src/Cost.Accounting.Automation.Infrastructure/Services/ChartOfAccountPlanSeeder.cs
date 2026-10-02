using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;
using Cost.Accounting.Automation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cost.Accounting.Automation.Infrastructure.Services;

/// <summary>
/// Yeni mali yıl veritabanı açılırken standart UFRS hesap planını gömülü
/// Excel kaynağından okuyup tohumlar.
///
/// Kod üretimi ve sınıflandırma kuralları bilinçli olarak
/// <see cref="ChartOfAccountImportCommand"/> ile aynı tutulmuştur: kullanıcı
/// hesap planını elle içe aktardığında üretilen kodlar ile tohumlanan kodlar
/// birebir aynı olmalıdır. Aksi hâlde içe aktarma, tohumlanan hesapları
/// silip yeniden ekler ve devirde hesap eşleşmesi bozulur.
/// </summary>
internal static class ChartOfAccountPlanSeeder
{
    private const string ResourceName =
        "Cost.Accounting.Automation.Infrastructure.Resources.ChartOfAccounts.xlsx";

    /// <summary>Depo kökleri; bunların altındaki yapraklar kategori olur.</summary>
    private static readonly HashSet<string> WarehouseCodes =
        new(StringComparer.OrdinalIgnoreCase) { "150", "150.98", "151", "152" };

    private const string ConsumptionRootCode = "900";

    /// <summary>
    /// Hesap planı tablosu boşsa gömülü kaynaktan tohumlar. Daha önce eklenmiş
    /// hesap varsa hiçbir şey yapmaz.
    /// </summary>
    /// <returns>Eklenen hesap sayısı.</returns>
    public static async Task<int> SeedAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        if (await context.Set<ChartOfAccount>().AnyAsync(cancellationToken))
        {
            return 0;
        }

        List<PlanRow> plan = ReadEmbeddedPlan();
        if (plan.Count == 0)
        {
            return 0;
        }

        var nodes = new Dictionary<string, ChartOfAccount>(StringComparer.OrdinalIgnoreCase);
        var children = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Üst hesaplar alt hesaplardan önce eklenmelidir; sıralama seviyeye göre.
        foreach (PlanRow row in plan.OrderBy(r => r.Code.Count(c => c == '.') + 1))
        {
            var account = new ChartOfAccount(
                new AccountCode(row.Code),
                new Name(row.Name),
                row.Level,
                ChartOfAccountType.MainGroup);

            nodes[row.Code] = account;
            context.Set<ChartOfAccount>().Add(account);
        }

        // Üst/alt ilişkisi: eşleme tamamlandıktan sonra bağlanır.
        foreach ((string code, ChartOfAccount account) in nodes)
        {
            string? parentCode = ParentOf(code);
            if (parentCode is not null && nodes.TryGetValue(parentCode, out ChartOfAccount? parent))
            {
                account.SetParent(parent.Id);
                children.Add(parentCode);
            }
        }

        Classify(nodes, children);

        await context.SaveChangesAsync(cancellationToken);
        return nodes.Count;
    }

    /// <summary>
    /// Sınıflandırma <see cref="ChartOfAccountImportCommand.Classify"/> ile
    /// aynıdır: önce tümü anagrup, sonra depo kökleri, ardından atölye ve
    /// tüketim birimi yaprakları, en son kalan depo yaprakları kategori olur.
    /// </summary>
    private static void Classify(
        Dictionary<string, ChartOfAccount> nodes,
        HashSet<string> children)
    {
        foreach (ChartOfAccount account in nodes.Values)
        {
            account.SetType(ChartOfAccountType.MainGroup);
        }

        foreach (ChartOfAccount account in nodes.Values)
        {
            if (WarehouseCodes.Contains(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Warehouse);
            }
        }

        foreach (ChartOfAccount account in nodes.Values)
        {
            if (IsUnderWorkshopRoot(account.Code.Value) && !children.Contains(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Workshop);
            }
        }

        foreach (ChartOfAccount account in nodes.Values)
        {
            if (IsConsumptionUnitCode(account.Code.Value) && !children.Contains(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.ConsumptionUnit);
            }
        }

        foreach (ChartOfAccount account in nodes.Values)
        {
            if (account.Type != ChartOfAccountType.MainGroup || children.Contains(account.Code.Value))
            {
                continue;
            }

            if (IsUnderWorkshopRoot(account.Code.Value))
            {
                continue;
            }

            if (IsUnderAnyWarehouseRoot(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Category);
            }
        }
    }

    // ---------------------------------------------------------------- okuma

    private static List<PlanRow> ReadEmbeddedPlan()
    {
        using Stream stream = typeof(ChartOfAccountPlanSeeder).GetTypeInfo().Assembly
            .GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Gömülü hesap planı bulunamadı: {ResourceName}");

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        string[] sharedStrings = ReadSharedStrings(archive);
        ZipArchiveEntry? sheet = GetFirstSheetEntry(archive);
        if (sheet is null)
        {
            return [];
        }

        var plan = new List<PlanRow>();

        foreach (string[] cells in ReadRows(sheet, sharedStrings))
        {
            if (IsHeaderRow(cells))
            {
                continue;
            }

            // A..G sütunları hesap kodunun parçaları, H sütunu hesap adıdır.
            // I sütunu boştur.
            string raw = string.Join(
                ".",
                cells.Take(7).Where(c => !string.IsNullOrWhiteSpace(c)));

            string code = ChartOfAccountCodeHelper.NormalizeCode(raw);
            string name = cells.Length > 7 ? cells[7].Trim() : string.Empty;

            if (code.Length == 0 || name.Length == 0)
            {
                continue;
            }

            plan.Add(new PlanRow(code, name, code.Count(c => c == '.') + 1));
        }

        return plan;
    }

    /// <summary>
    /// Başlık satırını atlar. Excel'in ilk satırı "HESKOD / YARDIMCI 1 / ..."
    /// biçimindedir; atlanmazsa "HESKOD.YARDIMCI 1..." geçerli bir hesap kodu
    /// sanılır ve kolon taşmasına yol açar.
    /// </summary>
    private static bool IsHeaderRow(string[] cells)
    {
        string first = cells.Length > 0 ? cells[0].Trim().ToLowerInvariant() : string.Empty;
        if (first is "heskod" or "kod" or "hesap kodu" or "kodu" or "no" or "nosu")
        {
            return true;
        }

        if (cells.Length > 7)
        {
            string last = cells[7].Trim().ToLowerInvariant();
            if (last is "hesap adı" or "adi" or "ad" or "isim" or "name"
                or "açıklama" or "aciklama" or "description")
            {
                return true;
            }
        }

        return false;
    }

    private static List<string[]> ReadRows(ZipArchiveEntry sheet, string[] sharedStrings)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        using Stream sheetStream = sheet.Open();
        XDocument doc = XDocument.Load(sheetStream);

        var rows = new List<string[]>();

        XElement? sheetData = doc.Root?.Element(ns + "sheetData");
        if (sheetData is null)
        {
            return rows;
        }

        foreach (XElement row in sheetData.Elements(ns + "row"))
        {
            var cells = new string[8];

            foreach (XElement cell in row.Elements(ns + "c"))
            {
                string? reference = cell.Attribute("r")?.Value;
                if (reference is null || reference.Length < 2)
                {
                    continue;
                }

                int index = char.ToUpperInvariant(reference[0]) - 'A';
                if (index < 0 || index >= cells.Length)
                {
                    continue;
                }

                string? type = cell.Attribute("t")?.Value;
                XElement? value = cell.Element(ns + "v");

                if (type == "s" && value is not null && int.TryParse(value.Value, out int stringIndex))
                {
                    cells[index] = stringIndex >= 0 && stringIndex < sharedStrings.Length
                        ? sharedStrings[stringIndex]
                        : string.Empty;
                }
                else if (type == "inlineStr")
                {
                    cells[index] = cell.Element(ns + "is")?.Element(ns + "t")?.Value ?? string.Empty;
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

        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        using Stream stream = entry.Open();
        XDocument doc = XDocument.Load(stream);

        var strings = new List<string>();

        foreach (XElement si in doc.Root?.Elements(ns + "si") ?? Enumerable.Empty<XElement>())
        {
            var builder = new System.Text.StringBuilder();
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
            .Where(e => e.FullName.Contains("/worksheets/", StringComparison.Ordinal)
                        && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    // ---------------------------------------------------------------- kurallar

    /// <summary>Adım sayısı 2 olan en üst kısım: "150.01.01" için "150.01".</summary>
    private static string? ParentOf(string code)
    {
        int lastDot = code.LastIndexOf('.');
        return lastDot > 0 ? code[..lastDot] : null;
    }

    private static bool IsUnderWorkshopRoot(string code)
    {
        string[] segments = code.Split('.');
        return segments.Length >= 3 && segments[0] == "150" && segments[1] == "55";
    }

    private static bool IsConsumptionUnitCode(string code)
    {
        return code.Equals(ConsumptionRootCode, StringComparison.Ordinal)
            || code.StartsWith(ConsumptionRootCode + ".", StringComparison.Ordinal);
    }

    private static bool IsUnderAnyWarehouseRoot(string code)
        => "150,150.98,151,152"
            .Split(',')
            .Any(root => IsUnderRoot(code, root));

    private static bool IsUnderRoot(string code, string rootKey)
    {
        string[] rootSegments = rootKey.Split('.');
        string[] segments = code.Split('.');

        if (segments.Length < rootSegments.Length)
        {
            return false;
        }

        for (int i = 0; i < rootSegments.Length; i++)
        {
            if (!string.Equals(segments[i], rootSegments[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private sealed record PlanRow(string Code, string Name, int Level);
}
