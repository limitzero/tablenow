using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public sealed record RegisterUserRequest(string Name, string Email, string Password)
    : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(Guid UserId, string Name, string Email);
