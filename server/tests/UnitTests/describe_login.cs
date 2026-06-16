using CM.TableNow.Auth.Application.Features.Login;
using CM.TableNow.Auth.Application.Services;
using CM.TableNow.Auth.Data.Queries.GetUserByEmail;
using CM.TableNow.Shared.Results;
using FluentAssertions;
using Mediator;
using NSubstitute;

namespace describe_login;

public class when_credentials_are_invalid
{
    [Fact]
    public async Task it_should_return_unauthorized()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct-password", workFactor: 4);
        var user = new UserData(Guid.NewGuid(), "user@example.com", passwordHash, "Diner");

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserData?>.Success(user));

        var jwtGenerator = Substitute.For<IJwtTokenGenerator>();
        var handler = new LoginRequestHandler(mediator, jwtGenerator);

        var result = await handler.Handle(
            new LoginRequest("user@example.com", "wrong-password"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }
}

public class when_credentials_are_valid
{
    [Fact]
    public async Task it_should_return_ok_with_token()
    {
        const string password = "correct-password";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", passwordHash, "Diner");
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserByEmailQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<UserData?>.Success(user));

        var jwtGenerator = Substitute.For<IJwtTokenGenerator>();
        jwtGenerator.GenerateToken(userId, "user@example.com", "Diner")
            .Returns(("test-jwt-token", expiresAt));

        var handler = new LoginRequestHandler(mediator, jwtGenerator);

        var result = await handler.Handle(
            new LoginRequest("user@example.com", password),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Token.Should().Be("test-jwt-token");
        result.Data!.ExpiresAt.Should().Be(expiresAt);
    }
}
