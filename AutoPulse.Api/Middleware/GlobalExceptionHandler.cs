using AutoPulse.Domain.Common.Exceptions;
using AutoPulse.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security;

namespace AutoPulse.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;
        public GlobalExceptionHandler
        (
            ILogger<GlobalExceptionHandler> logger,
            IWebHostEnvironment environment
        )
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync
        (
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            // 1. Log the internal error for debugging purposes
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            // 2. Log the internal error for debugging purposes
            _logger.LogError(
                exception,
                "An unhandled exception occurred. [TraceId: {TraceId}] -> {Message}",
                traceId,
                exception.Message
            );

            // 3. Determine the appropriate HTTP status code based on the exception type
            var (statusCode, title, messageOverride) = exception switch
            {
                // Handle database transient or connection failures as 500 Internal Server Error
                Exception ex when IsTransientOrConnectionException(ex) =>
                    (StatusCodes.Status500InternalServerError, "Database Connection Error", "A transient database or connection error occurred. Please try again later."),

                // 3.a Domain Exceptions are custom exceptions that represent business rule violations or domain-specific errors.
                DomainException domainEx =>
                (domainEx.StatusCode, domainEx.Title, domainEx.Message),

                // 3.b Structural Exceptions are standard .NET exceptions that indicate issues with the structure or state of the application.
                ArgumentNullException =>
                    (StatusCodes.Status400BadRequest, "Bad Request", "A required argument was null."),

                ArgumentException or InvalidOperationException =>
                    (StatusCodes.Status400BadRequest, "Bad Request", exception.Message ?? "An invalid argument was provided."),

                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, "Not Found", "The requested resource was not found."),

                ApplicationException when exception.Message.Contains("concurrency") =>
                    (StatusCodes.Status409Conflict, "Concurrency Conflict", "Concurrency Conflict"),

                UnauthorizedAccessException =>
                    (StatusCodes.Status401Unauthorized, "Unauthorized", "Unauthorized access."),

                // 3.c Database Exceptions are exceptions that occur during database operations, such as concurrency conflicts or foreign key violations.
                DbUpdateConcurrencyException =>
                    (StatusCodes.Status409Conflict, "Concurrency Conflict", "The record was updated by another user. Please reload the data."),

                DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("violates foreign key") ?? false =>
                    (StatusCodes.Status400BadRequest, "Related Record Not Found", "The operation failed because a related required record does not exist."),

                DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("violates unique constraint") ?? false =>
                    (StatusCodes.Status409Conflict, "Duplicate Record", "A record with the same unique identifier or key already exists."),

                // 3.d Security Exceptions
                Exception secEx when secEx is SecurityException || secEx is TokenCompromisedException =>
                    (StatusCodes.Status403Forbidden, "Access Denied", "The security token provided is invalid or has been compromised."),

                // 3.e Fallback for unhandled exceptions
                _ =>
                    (StatusCodes.Status500InternalServerError, "Internal Server Error", exception.Message)
            };

            // 4. Determine the error message based on the environment (development or production)
            string detailMessage = messageOverride;
            if (statusCode == StatusCodes.Status500InternalServerError && _environment.IsProduction())
            {
                detailMessage = "An unexpected error occurred on our server. Please provide the traceId to technical support.";
            }

            // 5. Build the error response under RFC 7807 (Problem Details for HTTP APIs)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detailMessage,
                Instance = httpContext.Request.Path
            };

            // 6. Inject the traceId into the response for easier debugging
            problemDetails.Extensions.Add("traceId", traceId);

            // 7. Configure the http response
            httpContext.Response.StatusCode = statusCode;

            // 8. Return the error response asynchronously as JSON
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // 9. Indicate that the exception has been handled
            return true;
        }

        private static bool IsTransientOrConnectionException(Exception? exception)
        {
            while (exception != null)
            {
                var typeName = exception.GetType().Name;
                if (typeName == "NpgsqlException" ||
                    typeName == "SocketException" ||
                    exception.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase) ||
                    exception.Message.Contains("Failed to connect", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                exception = exception.InnerException;
            }
            return false;
        }
    }
}
