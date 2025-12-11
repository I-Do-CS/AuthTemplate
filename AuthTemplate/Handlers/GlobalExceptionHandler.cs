using System.Net;
using AuthTemplate.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthTemplate.Handlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, message) = GetExceptionDetails(exception);

        // If exception is a validation exceptions contstruct the errors
        Dictionary<string, string[]>? errors = null;
        if (exception is ValidationException validationException)
        {
            errors = validationException
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key.ToLowerInvariant(),
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
        }

        logger.LogError(exception, "{ExceptionMessage}", exception.Message);

        httpContext.Response.StatusCode = (int)statusCode;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = "Something went wrong.",
                Detail = message,
                Instance = httpContext.Request.Path,
            },
        };

        if (errors is not null)
        {
            context.ProblemDetails.Extensions.Add("errors", errors);
        }

        await problemDetailsService.TryWriteAsync(context);

        return true;
    }

    private (HttpStatusCode statusCode, string message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            UnprocessableEntityException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            InternalServerErrorException => (HttpStatusCode.InternalServerError, exception.Message),

            // This one comes from FluentValidation package
            ValidationException => (
                HttpStatusCode.UnprocessableEntity,
                "One or more validation errors occured."
            ),
            _ => (HttpStatusCode.InternalServerError, exception.Message),
        };
    }
}
