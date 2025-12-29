using System.Net;
using SocialMap.WebAPI.Models;

namespace SocialMap.WebAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            Message = "Internal Server Error"
        };

        switch (exception)
        {
            case KeyNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = ex.Message;
                break;
            case UnauthorizedAccessException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized";
                break;
            case ArgumentException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = ex.Message;
                break;
            case FluentValidation.ValidationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.Details = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
                break;
            case InvalidOperationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // Often used for domain logic errors
                response.Message = ex.Message;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
                break;
        }

        response.StatusCode = context.Response.StatusCode;

        // In Development, include stack trace for 500 errors or others if needed
        if (_env.IsDevelopment() && context.Response.StatusCode == (int)HttpStatusCode.InternalServerError && response.Details == null)
        {
           response.Details = exception.StackTrace;
        }

        await context.Response.WriteAsync(response.ToString());
    }
}
