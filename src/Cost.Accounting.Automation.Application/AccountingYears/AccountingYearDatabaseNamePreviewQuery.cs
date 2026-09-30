using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Services;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.AccountingYears;

/// <summary>
/// Yıl veritabanı oluşturma formunda gösterilecek önerilen adı üretir.
/// Kullanıcı bu adı formda değiştirebilir; değişiklik yalnızca
/// <see cref="AccountingYearCreateCommand"/> ile kalıcılaşır.
/// </summary>
[Permission("company:view")]
public sealed record AccountingYearDatabaseNamePreviewQuery(
    string CompanyName,
    int Year) : IRequest<Result<AccountingYearDatabaseNamePreviewDto>>;

public sealed record AccountingYearDatabaseNamePreviewDto
{
    public required string SuggestedDatabaseName { get; init; }
    public bool IsAvailable { get; init; }
    public string? Warning { get; init; }
}

internal sealed class AccountingYearDatabaseNamePreviewQueryHandler(
    IDatabaseNameBuilder databaseNameBuilder,
    IAccountingDbSelector dbSelector) : IRequestHandler<AccountingYearDatabaseNamePreviewQuery, Result<AccountingYearDatabaseNamePreviewDto>>
{
    public async Task<Result<AccountingYearDatabaseNamePreviewDto>> Handle(
        AccountingYearDatabaseNamePreviewQuery request,
        CancellationToken cancellationToken)
    {
        string suggestion = await databaseNameBuilder.SuggestAvailableAsync(
            request.CompanyName,
            request.Year,
            cancellationToken);

        bool isTaken = !string.IsNullOrWhiteSpace(dbSelector.MasterDatabaseName)
            && string.Equals(
                dbSelector.MasterDatabaseName,
                suggestion,
                StringComparison.OrdinalIgnoreCase);

        return Result<AccountingYearDatabaseNamePreviewDto>.Succeed(new AccountingYearDatabaseNamePreviewDto
        {
            SuggestedDatabaseName = suggestion,
            IsAvailable = !isTaken,
            Warning = isTaken
                ? "Bu ad master veritabanıyla aynı. Başka bir ad girin."
                : null
        });
    }
}
