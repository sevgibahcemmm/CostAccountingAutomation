using Cost.Accounting.Automation.Application.Services;

namespace Cost.Accounting.Automation.Infrastructure.Context;

/// <summary>
/// Migration üretimi gibi tasarım zamanı işlemlerinde oturum olmadığı için
/// kullanılan, audit alanlarını doldurmayan <see cref="IClaimContext"/>.
/// </summary>
internal sealed class DesignTimeClaimContext : IClaimContext
{
    public Guid GetUserId() => Guid.Empty;

    public Guid GetCompanyId() => Guid.Empty;

    public string GetRoleName() => string.Empty;

    public Guid? GetUserIdOrDefault() => null;
}
