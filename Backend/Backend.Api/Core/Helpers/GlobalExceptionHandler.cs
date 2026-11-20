using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using System.Globalization;

namespace Backend.Api.Core.Helpers;

internal sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred.");
        LogFormMappingDetails(exception, logger);

        httpContext.Response.StatusCode = exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = "An error occurred.",
                Detail = exception.Message
            }
        });
    }
    private static void LogFormMappingDetails(
        Exception exception,
        ILogger logger)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            var errorProperty = current.GetType().GetProperty("Error");

            var error = errorProperty?.GetValue(current);

            if (error == null)
                continue;

            var errorType = error.GetType();

            var key = errorType.GetProperty("Key")?.GetValue(error);
            var message = errorType.GetProperty("Message")?.GetValue(error);
            var value = errorType.GetProperty("Value")?.GetValue(error);

            var formattedMessage = message is FormattableString fs
                ? fs.ToString(CultureInfo.InvariantCulture)
                : message?.ToString();

            logger.LogError(
                """
            Form mapping failure:
              Key: {Key}
              Message: {Message}
              Value: {Value}
            """,
                key,
                formattedMessage,
                value);
        }
    }
}
