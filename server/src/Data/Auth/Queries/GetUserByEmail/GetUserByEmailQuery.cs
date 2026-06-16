using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public sealed record UserData(Guid Id, string Email, string PasswordHash, string Role);

public sealed record GetUserByEmailQuery(string Email) : IQuery<Result<UserData?>>;
