using Microsoft.AspNetCore.Diagnostics;
using PresentationApi.Core.Exeptions;

namespace PresentationApi;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            GenerationsEndedException => (StatusCodes.Status402PaymentRequired, "Payment Required"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request") 
        };

        httpContext.Response.StatusCode = statusCode;

        var response = new 
        {
            Title = title,
            Status = statusCode,
            Detail = exception.Message
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true; 
    }
}