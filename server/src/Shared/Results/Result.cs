using Microsoft.AspNetCore.Http;

namespace CM.TableNow.Shared.Results;

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

    public static Result<T> BadRequest(params Error[] errors)
        => Failure(StatusCodes.Status400BadRequest, errors);

    public static Result<T> Unauthorized()
        => Failure(StatusCodes.Status401Unauthorized, new Error("unauthorized", "Authentication required."));

    public static Result<T> Forbidden()
        => Failure(StatusCodes.Status403Forbidden, new Error("forbidden", "You do not have access to this resource."));

    public static Result<T> NotFound(string message)
        => Failure(StatusCodes.Status404NotFound, new Error("not_found", message));

    public static Result<T> Conflict(string message)
        => Failure(StatusCodes.Status409Conflict, new Error("conflict", message));
}

/// <summary>Marker type for void Result operations.</summary>
public readonly struct Unit
{
    public static readonly Unit Value = default;
}

/// <summary>Convenience factory for payload-less results.</summary>
public static class Result
{
    public static Result<Unit> Success()
        => Result<Unit>.Success(Unit.Value);

    public static Result<Unit> Created()
        => Result<Unit>.Created(Unit.Value);

    public static Result<Unit> Failure(int statusCode, params Error[] errors)
        => Result<Unit>.Failure(statusCode, errors);

    public static Result<Unit> BadRequest(params Error[] errors)
        => Result<Unit>.BadRequest(errors);

    public static Result<Unit> Unauthorized()
        => Result<Unit>.Unauthorized();

    public static Result<Unit> Forbidden()
        => Result<Unit>.Forbidden();

    public static Result<Unit> NotFound(string message)
        => Result<Unit>.NotFound(message);

    public static Result<Unit> Conflict(string message)
        => Result<Unit>.Conflict(message);
}
