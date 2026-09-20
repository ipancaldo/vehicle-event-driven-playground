using AutoAuctionPlayground.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.API.Middleware
{
    // Central place mapping exception types to HTTP responses, so command/query handlers can just
    // throw plain, meaningful exceptions (KeyNotFoundException, InvalidOperationException, ...)
    // without knowing anything about HTTP. Without this, every domain rejection reaches the client
    // as a raw 500 with a full stack trace attached.
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // The response body may already be partially written (e.g. streaming); there's
                // nothing safe left to do but abort rather than try to write a second response.
                if (context.Response.HasStarted)
                {
                    logger.LogError(ex, "Exception after the response had already started; aborting.");
                    context.Abort();
                    return;
                }

                var (statusCode, title) = MapException(ex);

                if (statusCode == StatusCodes.Status500InternalServerError)
                    logger.LogError(ex, "Unhandled exception handling {Method} {Path}", context.Request.Method, context.Request.Path);
                else
                    logger.LogWarning(ex, "{StatusCode} handling {Method} {Path}: {Message}", statusCode, context.Request.Method, context.Request.Path, ex.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    // Domain/validation messages are safe to return as-is — they're written for a
                    // caller. Anything unexpected (a real bug) must not leak internals.
                    Detail = statusCode == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : ex.Message,
                    Type = $"https://httpstatuses.com/{statusCode}",
                    Instance = context.Request.Path
                };
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }

        private static (int StatusCode, string Title) MapException(Exception ex) => ex switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            UniqueConstraintViolationException => (StatusCodes.Status409Conflict, "Conflict"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
}
