using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Data.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string PasswordHash) : ICommand<Result<Guid>>;
