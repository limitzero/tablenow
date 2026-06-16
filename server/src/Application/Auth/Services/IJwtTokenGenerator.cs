namespace CM.TableNow.Auth.Application.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string email, string role);
}
