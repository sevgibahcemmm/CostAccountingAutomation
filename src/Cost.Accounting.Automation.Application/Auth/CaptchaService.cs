namespace Cost.Accounting.Automation.Application.Auth;

public sealed record CaptchaChallengeDto(Guid ChallengeId, string Question);

public interface ICaptchaService
{
    CaptchaChallengeDto Create();
    bool Validate(Guid challengeId, int answer, string code);
}

/// <summary>
/// Matematik CAPTCHA durumunu bellek içinde tutan tek örnek (singleton) servis.
/// Challenge oluşturulduğunda cevap ve "kopyalanacak" kod saklanır; doğrulama
/// tek kullanımlıktır ve süresi dolmuş kayıtlar temizlenir.
/// </summary>
internal sealed class MathCaptchaService : ICaptchaService
{
    private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(5);

    private readonly object _lock = new();
    private readonly Dictionary<Guid, CaptchaRecord> _challenges = new();
    private readonly Random _random = new();

    public CaptchaChallengeDto Create()
    {
        lock (_lock)
        {
            CleanupExpired();

            int a = _random.Next(1, 10);
            int b = _random.Next(1, 10);
            string code = _random.Next(1000, 10000).ToString();
            var challengeId = Guid.NewGuid();

            _challenges[challengeId] = new CaptchaRecord(a + b, code, DateTimeOffset.Now.Add(LifeTime));

            return new CaptchaChallengeDto(challengeId, $"{a} + {b} = ?   •   Kod: {code}");
        }
    }

    public bool Validate(Guid challengeId, int answer, string code)
    {
        lock (_lock)
        {
            if (!_challenges.TryGetValue(challengeId, out var record))
            {
                return false;
            }

            _challenges.Remove(challengeId);

            return record.Answer == answer
                && record.Code == code
                && record.ExpiresAt > DateTimeOffset.Now;
        }
    }

    private void CleanupExpired()
    {
        var now = DateTimeOffset.Now;
        var expiredIds = _challenges
            .Where(p => p.Value.ExpiresAt < now)
            .Select(p => p.Key)
            .ToArray();

        foreach (var id in expiredIds)
        {
            _challenges.Remove(id);
        }
    }

    private sealed record CaptchaRecord(int Answer, string Code, DateTimeOffset ExpiresAt);
}