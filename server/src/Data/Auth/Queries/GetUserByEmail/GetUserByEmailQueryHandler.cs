using CM.TableNow.Shared.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CM.TableNow.Auth.Data.Queries.GetUserByEmail;

public sealed class GetUserByEmailQueryHandler(AuthDbContext db)
    : IQueryHandler<GetUserByEmailQuery, Result<UserData?>>
{
    public async ValueTask<Result<UserData?>> Handle(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken)
    {
        var data = await db.Users
            .AsNoTracking()
            .Where(u => u.Email == query.Email)
            .Select(u => new UserData(u.Id, u.Email, u.PasswordHash, u.Role))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<UserData?>.Success(data);
    }
}
