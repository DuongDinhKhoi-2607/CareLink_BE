using CareLinkAPI.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace CareLinkAPI.Middlewares;

/// <summary>Converts exceptions into RFC 7807 ProblemDetails responses.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    // PostgreSQL: serialization_failure / deadlock_detected -> client may simply retry.
    private static readonly string[] RetryableSqlStates = ["40001", "40P01"];

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        ProblemDetails problem;

        if (ex is AppException appEx)
        {
            problem = new ProblemDetails
            {
                Status = appEx.StatusCode,
                Title = appEx.Title,
                Detail = appEx.Message
            };
        }
        else if (ex.GetBaseException() is PostgresException { SqlState: var state } && RetryableSqlStates.Contains(state))
        {
            logger.LogWarning(ex, "Concurrent update conflict on {Path}", context.Request.Path);
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "The resource was modified by another request. Please try again."
            };
        }
        else
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            };
        }

        problem.Instance = context.Request.Path;
        context.Response.Clear();
        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
