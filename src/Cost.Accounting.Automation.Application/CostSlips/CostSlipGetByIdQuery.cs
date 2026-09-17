using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.CostSlips;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.CostSlips;

[Permission("costslip:view")]
public sealed record CostSlipGetByIdQuery(Guid Id) : IRequest<Result<CostSlipDto>>;

internal sealed class CostSlipGetByIdQueryHandler(
    ICostSlipRepository costSlipRepository) : IRequestHandler<CostSlipGetByIdQuery, Result<CostSlipDto>>
{
    public async Task<Result<CostSlipDto>> Handle(CostSlipGetByIdQuery request, CancellationToken cancellationToken)
    {
        var slip = await costSlipRepository.GetWithDetailsAsync(new Domain.Abstractions.IdentityId(request.Id), cancellationToken);

        if (slip is null || slip.IsDeleted)
        {
            return Result<CostSlipDto>.Failure("Maliyet pusulası bulunamadı.");
        }

        return Result<CostSlipDto>.Succeed(slip.ToDto());
    }
}