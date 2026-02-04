using shot_reminder_2.Application.Commons;

namespace shot_reminder_2.Api.Middleware;

public sealed class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case NotFoundException:
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                case ArgumentOutOfRangeException:
                case ArgumentException:
                case InvalidOperationException:
                case BadRequestException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case ConflictException:
                case InsufficientInventoryException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    ex = new Exception("An unexpected error occurred.");
                    break;
            }

            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
    }
}
