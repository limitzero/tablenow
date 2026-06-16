using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.Login;

public sealed record LoginRequest(string Email, string Password)
    : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);
