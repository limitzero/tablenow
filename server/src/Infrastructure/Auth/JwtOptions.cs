namespace CM.TableNow.Auth.Infrastructure;

public sealed record JwtOptions
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryHours { get; init; } = 24;
}
