using Microsoft.AspNetCore.Http;

namespace CM.TableNow.Shared.Results;

public static class TypedResultHelper
{
    public static IResult ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                StatusCodes.Status201Created => TypedResults.Created((string?)null, result.Data),
                StatusCodes.Status204NoContent => TypedResults.NoContent(),
                _ => TypedResults.Ok(result.Data),
            };
        }

        var problem = new HttpValidationProblemDetails(
            result.Errors.ToDictionary(e => e.Code, e => new[] { e.Message }))
        {
            Status = result.StatusCode,
        };

        return TypedResults.Problem(problem);
    }
}
