using CM.TableNow.Api.Mappers;
using CM.TableNow.Contracts;
using CM.TableNow.Shared.Results;
using Mediator;

namespace CM.TableNow.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth");

        auth.MapPost("/register", async (RegisterRequest body, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(AuthMapper.ToRequest(body), ct);
            return TypedResultHelper.ToHttpResult(result);
        });

        return group;
    }
}
