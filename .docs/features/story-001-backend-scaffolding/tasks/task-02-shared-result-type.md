# Task 02: Shared Result&lt;T&gt; Type

## Status

pending

## Wave

2

## Description

Implements the `Result<T>` return type and the `TypedResultHelper` extension used by every handler and endpoint in the system. `Result<T>` is the universal envelope all Application-layer handlers return; it carries either a success payload or a list of error messages, plus an HTTP status code. `TypedResultHelper` converts a `Result<T>` to an `IResult` at the Minimal API boundary.

## Dependencies

**Depends on:** task-01-solution-structure.md
**Blocks:** All stories that implement handlers or endpoints (STORY-005+)

**Context from dependencies:** task-01 created `server/src/Shared/Shared.csproj` and `server/src/Api/Api.csproj` as empty class libraries. This task populates the Shared project with the cross-cutting type and the Api project with the HTTP mapping helper.

## Files to Create

- `server/src/Shared/Result.cs` — `Result<T>` with factory methods
- `server/src/Api/Helpers/TypedResultHelper.cs` — maps Result to IResult

## Technical Details

### Implementation Steps

1. Create `Result<T>` as a generic record/class in `server/src/Shared/Result.cs`. It must have `Data` (of type `T?`), `Errors` (`IReadOnlyList<string>`), `StatusCode` (`int`), and `IsSuccess` (`bool`) properties.

2. Add static factory methods for common outcomes.

3. Create `TypedResultHelper` in `server/src/Api/Helpers/` to map any `Result<T>` to the appropriate `IResult` based on `StatusCode`.

### Code Snippets

```csharp
// server/src/Shared/Result.cs
namespace TableNow.Shared;

public class Result<T>
{
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public int StatusCode { get; init; }
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    public static Result<T> Success(T data, int statusCode = 200) =>
        new() { Data = data, StatusCode = statusCode };

    public static Result<T> Failure(IEnumerable<string> errors, int statusCode) =>
        new() { Errors = errors.ToList(), StatusCode = statusCode };

    public static Result<T> Failure(string error, int statusCode) =>
        Failure([error], statusCode);
}
```

```csharp
// server/src/Api/Helpers/TypedResultHelper.cs
namespace TableNow.Api.Helpers;

public static class TypedResultHelper
{
    public static IResult ToResult<T>(Result<T> result) => result.StatusCode switch
    {
        200 => Results.Ok(result.Data),
        201 => Results.Created(string.Empty, result.Data),
        400 => Results.BadRequest(new { errors = result.Errors }),
        401 => Results.Unauthorized(),
        403 => Results.Forbid(),
        404 => Results.NotFound(new { errors = result.Errors }),
        409 => Results.Conflict(new { errors = result.Errors }),
        _   => Results.StatusCode(result.StatusCode)
    };
}
```

## Acceptance Criteria

- [ ] `Result<T>` has `Data`, `Errors`, `StatusCode`, `IsSuccess` properties
- [ ] `Result<T>.Success(data)` returns `IsSuccess = true` and correct StatusCode
- [ ] `Result<T>.Failure(errors, 409)` returns `IsSuccess = false` with the provided errors
- [ ] `TypedResultHelper.ToResult` maps 200→Ok, 201→Created, 400→BadRequest, 401→Unauthorized, 403→Forbid, 404→NotFound, 409→Conflict
- [ ] Both files are in file-scoped namespaces and compile without errors
