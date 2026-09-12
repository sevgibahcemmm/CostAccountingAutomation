using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

public sealed record CreateCaptchaCommand : IRequest<Result<CaptchaChallengeDto>>;

internal sealed class CreateCaptchaCommandHandler(ICaptchaService captchaService)
    : IRequestHandler<CreateCaptchaCommand, Result<CaptchaChallengeDto>>
{
    public Task<Result<CaptchaChallengeDto>> Handle(
        CreateCaptchaCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Result<CaptchaChallengeDto>>(captchaService.Create());
    }
}