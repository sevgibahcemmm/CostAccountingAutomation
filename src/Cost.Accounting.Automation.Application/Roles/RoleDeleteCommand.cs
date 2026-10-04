using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Roles;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Roles;

[Permission("role:delete")]
public sealed record RoleDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

/// <summary>Role atanmış kullanıcı varsa silme sonrası not üretir.</summary>
internal sealed class RoleDeleteCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<Role>(
            roleRepository,
            (ids, token) => roleRepository.GetDeletionCheckAsync(ids, token));
        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "rol",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "rol");
    }
}