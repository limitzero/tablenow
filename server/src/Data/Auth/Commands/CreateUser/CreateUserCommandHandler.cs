using CM.TableNow.Auth.Domain.Entities;
using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public sealed class CreateUserCommandHandler(AuthDbContext db)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
            return Result<Guid>.Conflict("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            PasswordHash = command.PasswordHash,
            Role = "Diner",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
