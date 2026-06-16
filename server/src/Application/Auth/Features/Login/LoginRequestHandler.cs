using CM.TableNow.Auth.Application.Services;
using CM.TableNow.Auth.Data.Queries.GetUserByEmail;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.Login;

public sealed class LoginRequestHandler(IMediator mediator, IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<LoginRequest, Result<LoginResponse>>
{
    // Pre-computed at startup so the BCrypt work factor cost is paid once.
    // Compared against during unknown-email requests to prevent timing-based user enumeration.
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("__dummy__", workFactor: 12);

    public async ValueTask<Result<LoginResponse>> Handle(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var queryResult = await mediator.Send(new GetUserByEmailQuery(request.Email), cancellationToken);
        var user = queryResult.Data;

        if (user is null)
        {
            BCrypt.Net.BCrypt.Verify(request.Password, DummyHash);
            return Result<LoginResponse>.Failure(401, new Error("invalid_credentials", "Invalid credentials."));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure(401, new Error("invalid_credentials", "Invalid credentials."));

        var (token, expiresAt) = jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role);
        return Result<LoginResponse>.Success(new LoginResponse(token, expiresAt));
    }
}
