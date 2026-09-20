using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.ChartOfAccounts;

[Permission("chartofaccount:delete")]
public sealed record ChartOfAccountDeleteCommand(
    IReadOnlyCollection<Guid> AccountIds) : IRequest<Result<string>>;

internal sealed class ChartOfAccountDeleteCommandHandler(
    IChartOfAccountRepository chartOfAccountRepository) : IRequestHandler<ChartOfAccountDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ChartOfAccountDeleteCommand request, CancellationToken cancellationToken)
    {
        List<Guid> ids = request.AccountIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Result<string>.Failure("Silinecek hesap seçilmedi");
        }

        List<ChartOfAccount> all = await chartOfAccountRepository.GetAllIncludingDeletedAsync(cancellationToken);
        if (all.Count == 0)
        {
            return Result<string>.Failure("Hesap planı boş");
        }

        Dictionary<Guid, ChartOfAccount> byId = all.ToDictionary(x => x.Id.Value, x => x);

        HashSet<Guid> targets = new();
        foreach (Guid id in ids)
        {
            if (byId.TryGetValue(id, out ChartOfAccount? account) && !account.IsDeleted)
            {
                targets.Add(id);
                CollectDescendants(id, all, targets);
            }
        }

        List<ChartOfAccount> toDelete = all
            .Where(a => targets.Contains(a.Id.Value) && !a.IsDeleted)
            .ToList();

        if (toDelete.Count == 0)
        {
            return Result<string>.Failure("Seçilen hesaplar silinecek durumda değil");
        }

        AccountDeletionCheck check = await chartOfAccountRepository.GetDeletionCheckAsync(targets, cancellationToken);
        if (check.MovementAccountIds.Count > 0)
        {
            string codes = string.Join(", ", all
                .Where(a => check.MovementAccountIds.Contains(a.Id.Value))
                .Select(a => a.Code.Value)
                .Take(5));

            return Result<string>.Failure($"İşlem/hareket gören hesap(lar) silinemez: {codes}");
        }

        ClearDeletedReferences(targets, all);
        chartOfAccountRepository.SoftDeleteRange(toDelete);

        if (check.RelatedAccountIds.Count == 0)
        {
            return $"{toDelete.Count} hesap silindi (silinenler İçe Aktar ile yeniden yüklenebilir)";
        }

        string relatedCodes = string.Join(", ", all
            .Where(a => check.RelatedAccountIds.Contains(a.Id.Value))
            .Select(a => a.Code.Value)
            .Take(5));

        return DeleteWarnings.Compose(
            $"{toDelete.Count} hesap silindi. NOT: {relatedCodes} kodlu hesaplar ilişkili kayıtlarda kullanılıyor; " +
            $"hareket görmedikleri için silme gerçekleştirildi.");
    }

    private static void CollectDescendants(Guid parentId, List<ChartOfAccount> all, HashSet<Guid> targets)
    {
        foreach (ChartOfAccount child in all)
        {
            if (child.ParentId is not null && child.ParentId.Value == parentId && !targets.Contains(child.Id.Value))
            {
                targets.Add(child.Id.Value);
                CollectDescendants(child.Id.Value, all, targets);
            }
        }
    }

    private static void ClearDeletedReferences(HashSet<Guid> targets, List<ChartOfAccount> all)
    {
        foreach (ChartOfAccount account in all)
        {
            if (account.IsDeleted)
            {
                continue;
            }

            IdentityId? semi = IsInTargets(account.SemiFinishedAccountId, targets) ? null : account.SemiFinishedAccountId;
            IdentityId? finished = IsInTargets(account.FinishedAccountId, targets) ? null : account.FinishedAccountId;

            if (semi != account.SemiFinishedAccountId || finished != account.FinishedAccountId)
            {
                account.SetProductionLinks(semi, finished);
            }
        }
    }

    private static bool IsInTargets(IdentityId? id, HashSet<Guid> targets)
        => id is not null && targets.Contains(id.Value);
}