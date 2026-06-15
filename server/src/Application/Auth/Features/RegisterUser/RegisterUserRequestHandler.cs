using CM.TableNow.Auth.Data.Commands.CreateUser;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Application.Features.RegisterUser;

public sealed class RegisterUserRequestHandler(IMediator mediator)
    : IRequestHandler<RegisterUserRequest, Result<RegisterUserResponse>>
{
    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<RegisterUserResponse>.BadRequest(new Error("name", "Name is required"));

        if (!request.Email.Contains('@'))
            return Result<RegisterUserResponse>.BadRequest(new Error("email", "Invalid email format"));

        if (request.Password.Length < 8)
            return Result<RegisterUserResponse>.BadRequest(new Error("password", "Password must be at least 8 characters"));

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
        var command = new CreateUserCommand(request.Name, request.Email, passwordHash);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return Result<RegisterUserResponse>.Failure(result.StatusCode, [.. result.Errors]);

        return Result<RegisterUserResponse>.Created(
            new RegisterUserResponse(result.Data!, request.Name, request.Email));
    }
}
