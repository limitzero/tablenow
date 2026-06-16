using CM.TableNow.Auth.Application.Features.RegisterUser;
using CM.TableNow.Contracts;

namespace CM.TableNow.Api.Mappers;

public static class AuthMapper
{
    public static RegisterUserRequest ToRequest(RegisterRequest request)
        => new(request.Name, request.Email, request.Password);

    public static Auth.Application.Features.Login.LoginRequest ToLoginRequest(LoginRequest request)
        => new(request.Email, request.Password);
}
