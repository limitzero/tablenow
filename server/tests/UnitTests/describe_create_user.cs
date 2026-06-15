using CM.TableNow.Auth.Data;
using CM.TableNow.Auth.Data.Commands.CreateUser;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace describe_create_user;

public class when_email_is_already_taken : IAsyncLifetime
{
    private AuthDbContext _db = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new AuthDbContext(options);

        _db.Users.Add(new CM.TableNow.Auth.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "taken@example.com",
            PasswordHash = "hashed",
            Role = "Diner",
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task it_should_return_conflict_status()
    {
        var handler = new CreateUserCommandHandler(_db);

        var result = await handler.Handle(
            new CreateUserCommand("New User", "taken@example.com", "hashed"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }
}

public class when_email_is_unique : IAsyncLifetime
{
    private AuthDbContext _db = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new AuthDbContext(options);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task it_should_create_the_user_and_return_its_id()
    {
        var handler = new CreateUserCommandHandler(_db);

        var result = await handler.Handle(
            new CreateUserCommand("New User", "new@example.com", "hashed"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        var user = await _db.Users.SingleAsync(u => u.Id == result.Data);
        user.Name.Should().Be("New User");
        user.Email.Should().Be("new@example.com");
        user.PasswordHash.Should().Be("hashed");
        user.Role.Should().Be("Diner");
    }
}
