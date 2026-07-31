using Microsoft.AspNetCore.Diagnostics;
using ProductHub.Api.Common.Exceptions;

namespace ProductHub.Api.Common;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    // Phương thức TryHandleAsync sẽ xử lý ngoại lệ toàn cục trong ứng dụng ASP.NET Core. Nó ghi lại thông tin về ngoại lệ, xác định mã trạng thái HTTP và thông điệp lỗi phù hợp dựa trên loại ngoại lệ, và trả về phản hồi lỗi chuẩn hóa cho client.
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Request failed. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        var statusCode = exception switch
        {
            ConflictException =>
                StatusCodes.Status409Conflict,

            NotFoundException =>
                StatusCodes.Status404NotFound,

            BusinessRuleException =>
                StatusCodes.Status400BadRequest,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var title = statusCode switch
        {
            StatusCodes.Status409Conflict =>
                "A conflict occurred.",

            StatusCodes.Status404NotFound =>
                "The requested resource was not found.",

            StatusCodes.Status400BadRequest =>
                "A business rule was violated.",

            _ =>
                "An unexpected error occurred."
        };

        var detail =
            statusCode ==
            StatusCodes.Status500InternalServerError
                ? environment.IsDevelopment()
                    ? exception.Message
                    : "The server could not process the request."
                : exception.Message;

        httpContext.Response.StatusCode = statusCode;

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            extensions:
                new Dictionary<string, object?>
                {
                    ["traceId"] =
                        httpContext.TraceIdentifier
                })
            .ExecuteAsync(httpContext);

        return true;
    }
}
// Lớp này là một trình xử lý ngoại lệ toàn cục trong ứng dụng ASP.NET Core. Nó ghi lại thông tin về ngoại lệ, xác định mã trạng thái HTTP và thông điệp lỗi phù hợp dựa trên loại ngoại lệ, và trả về phản hồi lỗi chuẩn hóa cho client.