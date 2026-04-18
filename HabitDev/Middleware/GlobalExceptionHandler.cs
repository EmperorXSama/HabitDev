using Microsoft.AspNetCore.Diagnostics;

namespace HabitDev.Middleware;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Internal server error",
                Detail = "an error occurred while processing the request .. please try again"
            }
        });
    }
}
