namespace CareLinkAPI.Common.Exceptions;

/// <summary>
/// Base class for expected (business) errors. The exception handling middleware
/// converts these into RFC 7807 <c>ProblemDetails</c> responses.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message, int statusCode, string title) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }

    public string Title { get; }
}

public sealed class BadRequestException(string message)
    : AppException(message, StatusCodes.Status400BadRequest, "Bad Request");

public sealed class UnauthorizedException(string message)
    : AppException(message, StatusCodes.Status401Unauthorized, "Unauthorized");

public sealed class ForbiddenException(string message)
    : AppException(message, StatusCodes.Status403Forbidden, "Forbidden");

public sealed class NotFoundException(string message)
    : AppException(message, StatusCodes.Status404NotFound, "Not Found");

/// <summary>The request conflicts with the current state of the resource (e.g. invalid state transition).</summary>
public sealed class ConflictException(string message)
    : AppException(message, StatusCodes.Status409Conflict, "Conflict");

/// <summary>The request is well-formed but violates a business rule.</summary>
public sealed class BusinessRuleException(string message)
    : AppException(message, StatusCodes.Status422UnprocessableEntity, "Business Rule Violation");
