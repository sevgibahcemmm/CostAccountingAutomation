namespace Cost.Accounting.Automation.Infrastructure.Options;
public sealed class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 480;
}
