using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.Auth;

/// <summary>
/// Kullanıcının tek kutucuğa "sonuç kod" biçiminde (örn. "29 5424")
/// yazdığı girişi doğrular. ChallengeId, soru oluşturulurken verilir.
/// </summary>
public sealed record ValidateCaptchaCommand(
    Guid ChallengeId,
    string UserInput) : IRequest<Result<bool>>;

internal sealed class ValidateCaptchaCommandHandler(ICaptchaService captchaService)
    : IRequestHandler<ValidateCaptchaCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(ValidateCaptchaCommand request, CancellationToken cancellationToken)
    {
        string[] parts = request.UserInput.Trim().Split(
            new[] { ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 || !int.TryParse(parts[0], out int answer))
        {
            return Task.FromResult(Result<bool>.Failure("Sonucu ve kodu boşluk bırakarak girin"));
        }

        bool isValid = captchaService.Validate(request.ChallengeId, answer, parts[1]);
        return Task.FromResult<Result<bool>>(isValid);
    }
}