using Cost.Accounting.Automation.Application.Dashboards;

namespace Cost.Accounting.Automation.Application.Services;

public interface IDashboardDataProvider
{
    Task<DashboardSnapshot> GetOverviewAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
}