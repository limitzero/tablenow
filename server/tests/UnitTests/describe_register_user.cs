using CM.TableNow.Auth.Application.Features.RegisterUser;
using CM.TableNow.Auth.Data.Commands.CreateUser;
using CM.TableNow.Shared.Results;
using FluentAssertions;
using Mediator;
using NSubstitute;

namespace describe_register_user;

public class when_name_is_missing
{
    [Fact]
    public async Task it_should_return_bad_request()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new RegisterUserRequestHandler(mediator);

        var result = await handler.Handle(
            new RegisterUserRequest("   ", "jane@example.com", "password123"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}

public class when_email_is_invalid
{
    [Fact]
    public async Task it_should_return_bad_request()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new RegisterUserRequestHandler(mediator);

        var result = await handler.Handle(
            new RegisterUserRequest("Jane Doe", "not-an-email", "password123"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}

public class when_password_is_too_short
{
    [Fact]
    public async Task it_should_return_bad_request()
    {
        var mediator = Substitute.For<IMediator>();
        var handler = new RegisterUserRequestHandler(mediator);

        var result = await handler.Handle(
            new RegisterUserRequest("Jane Doe", "jane@example.com", "short1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}

public class when_email_is_already_taken
{
    [Fact]
    public async Task it_should_return_conflict_status()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Conflict("Email already registered"));

        var handler = new RegisterUserRequestHandler(mediator);

        var result = await handler.Handle(
            new RegisterUserRequest("Jane Doe", "jane@example.com", "password123"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }
}

public class when_request_is_valid
{
    [Fact]
    public async Task it_should_return_created_response_with_user_details()
    {
        var userId = Guid.NewGuid();
        CreateUserCommand? capturedCommand = null;

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Do<CreateUserCommand>(c => capturedCommand = c), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Success(userId));

        var handler = new RegisterUserRequestHandler(mediator);

        var result = await handler.Handle(
            new RegisterUserRequest("Jane Doe", "jane@example.com", "password123"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.UserId.Should().Be(userId);
        result.Data!.Name.Should().Be("Jane Doe");
        result.Data!.Email.Should().Be("jane@example.com");

        capturedCommand.Should().NotBeNull();
        capturedCommand!.PasswordHash.Should().NotBe("password123");
        BCrypt.Net.BCrypt.Verify("password123", capturedCommand.PasswordHash).Should().BeTrue();
    }
}
