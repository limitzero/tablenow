# Task 02: Shared Result<T> Type

## Status

pending

## Wave

1

## Description

Implement the cross-cutting `Result<T>` type and the `TypedResultHelper` in the `Shared` project. Every Application and Data handler in TableNow returns `Result<T>` instead of throwing for business-logic failures, and every minimal-API endpoint converts that result into a typed HTTP response via `TypedResultHelper`. Getting this type right now means all later stories (auth, restaurants, reservations) have a consistent success/failure contract.

## Dependencies

**Depends on:** None (Wave 1)
**Blocks:** task-03-module-registration

**Context from dependencies:** None. This task runs in parallel with task-01. The `Shared` project (`CM.TableNow.Shared`) is created by task-01, but the `Result<T>` and `TypedResultHelper` source files are self-contained and can be authored independently; if the project file does not yet exist when this task runs, create the file paths under `server/src/Shared/` as specified — task-01 will reference the same project.

## Files to Create

- `server/src/Shared/Results/Result.cs` — the generic `Result<T>` type plus a non-generic `Result` companion for void operations.
- `server/src/Shared/Results/Error.cs` — an error descriptor record used in `Result.Errors`.
- `server/src/Shared/Results/TypedResultHelper.cs` — static helper that maps a `Result<T>` to an ASP.NET Core minimal-API `IResult` / typed result.

## Files to Modify

- `server/src/Shared/CM.TableNow.Shared.csproj` — add a reference/package for `Microsoft.AspNetCore.Http.Abstractions` (or `FrameworkReference Microsoft.AspNetCore.App`) so `TypedResultHelper` can return `IResult` / `Results<...>` typed results. Prefer `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.

## Technical Details

### Public shape (required members)

`Result<T>` must expose:
- `T? Data` — the success payload (null on failure).
- `IReadOnlyList<Error> Errors` — error descriptors (empty on success).
- `int StatusCode` — HTTP status code (e.g., 200, 201, 400, 401, 409).
- `bool IsSuccess` — derived: `StatusCode is >= 200 and <= 299`.

### Implementation Steps

1. Create the `Error` record: `public sealed record Error(string Code, string Message);`.
2. Create `Result<T>` with a private constructor and static factory methods. Make it immutable.
3. Add success factories: `Success(T data, int statusCode = 200)` and `Created(T data)` (status 201).
4. Add failure factories: `Failure(int statusCode, params Error[] errors)`, plus convenience helpers `BadRequest`, `Unauthorized`, `Forbidden`, `Conflict`, `NotFound` that set the canonical status codes.
5. Add a non-generic `Result` for operations without a payload (mirror the same factories over `Result<Unit>` or a dedicated type).
6. Implement `TypedResultHelper.ToHttpResult<T>(Result<T> result)` that switches on `StatusCode` and returns the matching `TypedResults.*` value, embedding `Data` for success and a `ProblemDetails`-style payload for failures.

### Code Snippets

```csharp
namespace CM.TableNow.Shared.Results;

public sealed record Error(string Code, string Message);

public sealed class Result<T>
{
    private Result(T? data, IReadOnlyList<Error> errors, int statusCode)
    {
        Data = data;
        Errors = errors;
        StatusCode = statusCode;
    }

    public T? Data { get; }
    public IReadOnlyList<Error> Errors { get; }
    public int StatusCode { get; }
    public bool IsSuccess => StatusCode is >= 200 and <= 299;

    public static Result<T> Success(T data, int statusCode = StatusCodes.Status200OK)
        => new(data, [], statusCode);

    public static Result<T> Created(T data)
        => new(data, [], StatusCodes.Status201Created);

    public static Result<T> Failure(int statusCode, params Error[] errors)
        => new(default, errors, statusCode);

    public static Result<T> Conflict(string message)
        => Failure(StatusCodes.Status409Conflict, new Error("conflict", message));

    public static Result<T> BadRequest(params Error[] errors)
        => Failure(StatusCodes.Status400BadRequest, errors);

    public static Result<T> Unauthorized()
        => Failure(StatusCodes.Status401Unauthorized, new Error("unauthorized", "Authentication required."));

    public static Result<T> Forbidden()
        => Failure(StatusCodes.Status403Forbidden, new Error("forbidden", "You do not have access to this resource."));

    public static Result<T> NotFound(string message)
        => Failure(StatusCodes.Status404NotFound, new Error("not_found", message));
}
```

```csharp
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
```

## Acceptance Criteria

- [ ] `Result<T>` exposes `Data`, `Errors`, `StatusCode`, and `IsSuccess` with the semantics above.
- [ ] `IsSuccess` returns true only for status codes 200–299.
- [ ] Static factory methods exist for success (200/201) and the common failure codes (400/401/403/404/409).
- [ ] A non-generic `Result` exists for payload-less operations.
- [ ] `TypedResultHelper.ToHttpResult` returns `TypedResults.Ok` for 200, `TypedResults.Created` for 201, and a problem result for failures.
- [ ] The `Shared` project references `Microsoft.AspNetCore.App` (framework reference) so the helper compiles.
- [ ] `dotnet build` succeeds.

## Notes

- Keep `Result<T>` immutable and free of any context-specific or EF dependencies — it lives in `Shared` and must be safe to reference everywhere.
- The failure payload uses `HttpValidationProblemDetails` so validation errors (STORY-005) map cleanly to RFC 9457 problem responses.
- Do not throw exceptions for business failures anywhere in the codebase; always return a failed `Result<T>`.
