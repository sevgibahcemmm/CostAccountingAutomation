namespace Cost.Accounting.Automation.Application.Services;


public interface IClaimContext
{
    Guid GetUserId();
    Guid GetCompanyId();
    string GetRoleName();
    Guid? GetUserIdOrDefault();
}