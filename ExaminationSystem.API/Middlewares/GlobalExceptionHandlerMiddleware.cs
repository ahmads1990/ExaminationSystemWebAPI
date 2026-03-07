using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Models.Responses;
using FluentValidation;
using System.Diagnostics;

namespace ExaminationSystem.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            if (Debugger.IsAttached) Debugger.Break();
            await HandleValidationException(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (Debugger.IsAttached) Debugger.Break();
            await HandleUnauthorizedException(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            if (Debugger.IsAttached) Debugger.Break();
            await HandleNotFoundException(context, ex);
        }
        catch (Exception ex)
        {
            if (Debugger.IsAttached) Debugger.Break();
            await HandleGenericException(context, ex);
        }
    }

    private async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        _logger.LogWarning(ex, "Validation error occurred");

        // FluentValidation provides detailed error messages
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            Success = false,
            Data = (object?)null,
            Message = ApiErrorCode.ValidationFailed.GetErrorMessage(),
            ErrorCode = ApiErrorCode.ValidationFailed,
            ValidationErrors = errors,
            Timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleUnauthorizedException(HttpContext context, UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access attempt");

        var response = new ErrorResponse<object>(ApiErrorCode.Unauthorized);

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleNotFoundException(HttpContext context, KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Resource not found");

        var response = new ErrorResponse<object>(ApiErrorCode.ResourceNotFound);

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleGenericException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        // Hide details in production, show in development
        var message = _env.IsDevelopment()
            ? $"{ex.Message}\n{ex.StackTrace}"
            : null;

        var response = new ErrorResponse<object>(ApiErrorCode.InternalServerError, message);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
