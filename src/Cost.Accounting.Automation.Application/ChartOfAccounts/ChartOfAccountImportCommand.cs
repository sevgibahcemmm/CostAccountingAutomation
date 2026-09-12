using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.Shared;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

public sealed record ChartOfAccountImportRow(string Code, string Name, bool IsActive = true);

[Permission("chartofaccount:import")]
public sealed record ChartOfAccountImportCommand(
    IReadOnlyCollection<ChartOfAccountImportRow> Rows) : IRequest<Result<string>>;

public sealed class ChartOfAccountImportCommandValidator : AbstractValidator<ChartOfAccountImportCommand>
{
    public ChartOfAccountImportCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("Hesap planı için hiç satır bulunamadı");

        RuleForEach(x => x.Rows)
            .Must(r => !string.IsNullOrWhiteSpace(r.Code))
            .WithMessage("Hesap kodu boş olamaz")
            .Must(r => !string.IsNullOrWhiteSpace(r.Name))
            .WithMessage("Hesap adı boş olamaz");
    }
}

internal sealed class ChartOfAccountImportCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountImportCommand, Result<string>>
{
    private static readonly HashSet<string> WarehouseCodes =
        new(StringComparer.OrdinalIgnoreCase) { "150", "150.98", "151", "152" };

    private readonly Dictionary<string, ChartOfAccount> _nodes =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, List<string>> _children =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<Result<string>> Handle(ChartOfAccountImportCommand request, CancellationToken cancellationToken)
    {
        List<ChartOfAccountImportRow> rows = request.Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Code))
            .GroupBy(r => NormalizeCode(r.Code), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();

        if (rows.Count == 0)
        {
            return Result<string>.Failure("Hesap planı için geçerli bir satır bulunamadı");
        }

        List<ChartOfAccount> existing = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
        Dictionary<string, ChartOfAccount> existingByCode = existing.ToDictionary(
            e => NormalizeCode(e.Code.Value),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> importedCodes = new(StringComparer.OrdinalIgnoreCase);
        List<ChartOfAccount> toAdd = new();

        foreach (ChartOfAccountImportRow row in rows)
        {
            string fullCode = NormalizeCode(row.Code);
            importedCodes.Add(fullCode);
            int level = CountLevel(fullCode);
            string name = row.Name?.Trim() ?? string.Empty;

if (existingByCode.TryGetValue(fullCode, out ChartOfAccount? account))
                {
                    if (account.IsDeleted)
                    {
                        account.Restore();
                    }

                    if (!string.Equals(account.Code.Value, fullCode, StringComparison.OrdinalIgnoreCase))
                    {
                        account.SetCode(new AccountCode(fullCode));
                    }

                    account.SetName(new Name(name));
                account.SetLevel(level);
                account.SetStatus(row.IsActive);
                _nodes[fullCode] = account;
            }
            else
            {
                ChartOfAccount created = new(
                    new AccountCode(fullCode),
                    new Name(name),
                    level,
                    ChartOfAccountType.MainGroup);

                created.SetStatus(row.IsActive);
                _nodes[fullCode] = created;
                toAdd.Add(created);
            }
        }

        LinkParents(_nodes);
        Classify();

        foreach (ChartOfAccount existingAccount in existing)
        {
            if (!importedCodes.Contains(existingAccount.Code.Value) && !existingAccount.IsDeleted)
            {
                existingAccount.Delete();
            }
        }

        foreach (ChartOfAccount account in toAdd)
        {
            await chartOfAccountRepository.AddAsync(account, cancellationToken);
        }

        int warehouses = _nodes.Values.Count(n => n.Type == ChartOfAccountType.Warehouse);
        int workshops = _nodes.Values.Count(n => n.Type == ChartOfAccountType.Workshop);
        int categories = _nodes.Values.Count(n => n.Type == ChartOfAccountType.Category);
        int mainGroups = _nodes.Values.Count(n => n.Type == ChartOfAccountType.MainGroup);
        int linked = _nodes.Values.Count(n => n.SemiFinishedAccountId is not null || n.FinishedAccountId is not null);

        return $"Hesap planı başarıyla içe aktarıldı: {_nodes.Count} hesap " +
            $"({mainGroups} anagrup, {warehouses} depo, {categories} kategori, {workshops} atölye, {linked} atölye 151/152 bağlantısı).";
    }

    private void LinkParents(Dictionary<string, ChartOfAccount> nodes)
    {
        _children.Clear();

        foreach (KeyValuePair<string, ChartOfAccount> pair in nodes)
        {
            string? parentKey = GetParentKey(pair.Key);
            if (parentKey is not null && nodes.TryGetValue(parentKey, out ChartOfAccount? parent))
            {
                pair.Value.SetParent(parent.Id);

                if (!_children.TryGetValue(parentKey, out List<string>? children))
                {
                    children = new List<string>();
                    _children[parentKey] = children;
                }

                children.Add(pair.Key);
            }
        }
    }

    private void Classify()
    {
        // Başlangıç: tümü Anagrup.
        foreach (ChartOfAccount account in _nodes.Values)
        {
            account.SetType(ChartOfAccountType.MainGroup);
        }

        // Depo: 150 / 150.98 / 151 / 152.
        foreach (ChartOfAccount account in _nodes.Values)
        {
            if (WarehouseCodes.Contains(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Warehouse);
            }
        }

        // Atölye: 150.55 altındaki en son düzey (yaprak) hesaplar.
        foreach (ChartOfAccount account in _nodes.Values)
        {
            if (IsUnderWorkshopRoot(account.Code.Value) && !_children.ContainsKey(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Workshop);
            }
        }

        // Kategori: depo kökleri (150 / 150.98 / 151 / 152) altındaki
        // en son düzey (yaprak) hesaplar; Atölye dalı hariç.
        foreach (ChartOfAccount account in _nodes.Values)
        {
            if (account.Type != ChartOfAccountType.MainGroup)
            {
                continue;
            }

            if (_children.ContainsKey(account.Code.Value))
            {
                continue;
            }

            if (IsUnderWorkshopRoot(account.Code.Value))
            {
                continue;
            }

            if (IsUnderAnyDepotRoot(account.Code.Value))
            {
                account.SetType(ChartOfAccountType.Category);
            }
        }

        LinkWorkshops();
    }

    private void LinkWorkshops()
    {
        Dictionary<string, List<ChartOfAccount>> semiFinished =
            GetLeafCandidatesByRoot(_nodes, "151");
        Dictionary<string, List<ChartOfAccount>> finished =
            GetLeafCandidatesByRoot(_nodes, "152");

        foreach (ChartOfAccount account in _nodes.Values)
        {
            if (account.Type != ChartOfAccountType.Workshop)
            {
                continue;
            }

            string name = account.Name.Value.Trim();
            if (name.Length == 0)
            {
                continue;
            }

            IdentityId? semiId = ResolveUniqueLeaf(semiFinished, name);
            IdentityId? finishedId = ResolveUniqueLeaf(finished, name);
            account.SetProductionLinks(semiId, finishedId);
        }
    }

    private static Dictionary<string, List<ChartOfAccount>> GetLeafCandidatesByRoot(
        Dictionary<string, ChartOfAccount> nodes,
        string rootKey)
    {
        Dictionary<string, List<ChartOfAccount>> result = new(StringComparer.OrdinalIgnoreCase);

        foreach (ChartOfAccount account in nodes.Values)
        {
            if (!IsUnderRoot(account.Code.Value, rootKey))
            {
                continue;
            }

            if (account.ParentId is null)
            {
                continue;
            }

            bool isLeaf = !nodes.Values.Any(c => c.ParentId == account.Id);
            if (!isLeaf)
            {
                continue;
            }

            string key = account.Name.Value.Trim();

            if (!result.TryGetValue(key, out List<ChartOfAccount>? candidates))
            {
                candidates = new List<ChartOfAccount>();
                result[key] = candidates;
            }

            candidates.Add(account);
        }

        return result;
    }

    private static IdentityId? ResolveUniqueLeaf(
        Dictionary<string, List<ChartOfAccount>> candidates,
        string name)
    {
        if (!candidates.TryGetValue(name, out List<ChartOfAccount>? list) || list.Count != 1)
        {
            return null;
        }

        return list[0].Id;
    }

    private static bool IsUnderAnyDepotRoot(string code)
    {
        return WarehouseCodes.Any(root => IsUnderRoot(code, root));
    }

    private static bool IsUnderWorkshopRoot(string code)
    {
        string[] segments = code.Split('.');
        return segments.Length >= 3 && segments[0] == "150" && segments[1] == "55";
    }

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
            if (segments[i] != rootSegments[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeCode(string code)
    {
        string[] segments = (code ?? string.Empty)
            .Replace('-', '.')
            .Replace(',', '.')
            .Split('.', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(".", segments.Select(s => NormalizeSegment(s)).Where(s => s.Length > 0));
    }

    private static string NormalizeSegment(string segment)
    {
        segment = segment.Trim();
        if (segment.Length > 0 && segment.All(char.IsDigit) && segment.Length < 2)
        {
            return segment.PadLeft(2, '0');
        }

        return segment;
    }

    private static int CountLevel(string code)
    {
        int count = code.Split('.').Length;
        return Math.Max(count - 1, 0);
    }

    private static string? GetParentKey(string code)
    {
        int lastDot = code.LastIndexOf('.');
        if (lastDot <= 0)
        {
            return null;
        }

        return code[..lastDot];
    }
}