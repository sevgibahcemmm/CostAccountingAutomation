using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;

/// <summary>Seçili rolleri tek transaction'da siler.</summary>
[Permission("role:delete")]
public sealed record BulkDeleteRolesCommand(IReadOnlyCollection<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteRolesCommandHandler(
    IRoleRepository roleRepository)
    : IRequestHandler<BulkDeleteRolesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        BulkDeleteRolesCommand request,
        CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Role>(
            roleRepository,
            (ids, token) => roleRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "rol",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "rol");
    }
}