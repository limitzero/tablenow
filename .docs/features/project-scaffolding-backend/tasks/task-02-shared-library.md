# Task 02: Implement Shared Library (Result\<T\>)

## Status

pending

## Wave

2

## Description

Implements the `Result<T>` return type in the `CM.TableNow.Shared` project. Every application handler and data handler in the system returns `Result<T>`, making this the single most important cross-cutting type. This task also adds a `TypedResultHelper` utility that converts `Result<T>` into an ASP.NET Core `IResult` at the endpoint layer.

## Dependencies

**Depends on:** task-01-solution-projects.md
**Blocks:** task-03-api-startup.md (needs to reference Result\<T\> in TypedResultHelper)

**Context from dependencies:** task-01 creates the `server/src/Shared/CM.TableNow.Shared.csproj` class library. This task populates that project with the core types.

## Files to Create

- `server/src/Shared/Result.cs` — Generic `Result<T>` record with `Data`, `Errors`, `StatusCode`, `IsSuccess`
- `server/src/Shared/Error.cs` — `Error` record with `Code` and `Message`
- `server/src/Shared/TypedResultHelper.cs` — Converts `Result<T>` to `IResult` (Microsoft.AspNetCore.Http)
- `server/src/Shared/ResultExtensions.cs` — Static factory methods: `Result.Ok<T>()`, `Result.Fail<T>()`

## Files to Modify

None.

## Technical Details

### Implementation Steps

1. Create `Result<T>` as a record in `server/src/Shared/Result.cs`.
2. Create `Error` as a record with `Code` (string) and `Message` (string).
3. Create static factory helpers in `ResultExtensions.cs` for ergonomic construction.
4. Create `TypedResultHelper` to map `Result<T>` status codes to `IResult` responses.

### Code Snippets

```csharp
// Result.cs
namespace CM.TableNow.Shared;

public record Result<T>
{
    public T? Data { get; init; }
    public IReadOnlyList<Error> Errors { get; init; } = [];
    public int StatusCode { get; init; }
    public bool IsSuccess => StatusCode is >= 200 and < 300;
}
```

```csharp
// Error.cs
namespace CM.TableNow.Shared;

public record Error(string Code, string Message);
```

```csharp
// ResultExtensions.cs
namespace CM.TableNow.Shared;

public static class ResultExtensions
{
    public static Result<T> Ok<T>(T data, int statusCode = 200) =>
        new() { Data = data, StatusCode = statusCode };

    public static Result<T> Created<T>(T data) =>
        new() { Data = data, StatusCode = 201 };

    public static Result<T> Fail<T>(int statusCode, params Error[] errors) =>
        new() { StatusCode = statusCode, Errors = errors };

    public static Result<T> NotFound<T>(string message = "Not found") =>
        Fail<T>(404, new Error("NOT_FOUND", message));

    public static Result<T> Conflict<T>(string message) =>
        Fail<T>(409, new Error("CONFLICT", message));

    public static Result<T> Unauthorized<T>(string message = "Unauthorized") =>
        Fail<T>(401, new Error("UNAUTHORIZED", message));

    public static Result<T> Forbidden<T>(string message = "Forbidden") =>
        Fail<T>(403, new Error("FORBIDDEN", message));

    public static Result<T> BadRequest<T>(string message) =>
        Fail<T>(400, new Error("BAD_REQUEST", message));
}
```

```csharp
// TypedResultHelper.cs
namespace CM.TableNow.Shared;

public static class TypedResultHelper
{
    public static IResult ToResult<T>(Result<T> result) => result.StatusCode switch
    {
        200 => Results.Ok(result.Data),
        201 => Results.Created(string.Empty, result.Data),
        204 => Results.NoContent(),
        400 => Results.BadRequest(result.Errors),
        401 => Results.Unauthorized(),
        403 => Results.Forbid(),
        404 => Results.NotFound(result.Errors),
        409 => Results.Conflict(result.Errors),
        _ => Results.StatusCode(result.StatusCode)
    };
}
```

## Acceptance Criteria

- [ ] `Result<T>` record exists with `Data`, `Errors`, `StatusCode`, and `IsSuccess` members
- [ ] `ResultExtensions` provides `Ok<T>`, `Created<T>`, `Fail<T>`, `NotFound<T>`, `Conflict<T>`, `Unauthorized<T>`, `Forbidden<T>`, `BadRequest<T>` factory methods
- [ ] `TypedResultHelper.ToResult<T>()` maps all common status codes to `IResult`
- [ ] `dotnet build` still exits with code 0 after this task
- [ ] All files use file-scoped namespaces and have nullable enabled

## Notes

`TypedResultHelper` references `Microsoft.AspNetCore.Http.IResult` — the Shared project may need a reference to `Microsoft.AspNetCore.App` framework reference or keep `TypedResultHelper` in the `Api` project instead if the circular reference becomes problematic. Preferred: keep it in Shared and add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to `CM.TableNow.Shared.csproj`.
