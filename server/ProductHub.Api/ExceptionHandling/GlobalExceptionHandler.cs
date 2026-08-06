// IExceptionHandler là extension point của ASP.NET Core để xử lý exception tập trung.
using Microsoft.AspNetCore.Diagnostics;
// Các exception này do Application ném trong use case.
using ProductHub.Application.Common.Exceptions;
// DomainException do Entity Domain ném khi invariant bị vi phạm.
using ProductHub.Domain.Exceptions;

namespace ProductHub.Api.ExceptionHandling;

// File này là ranh giới giữa lỗi bên trong hệ thống và HTTP response.
// Domain/Application chỉ diễn đạt ý nghĩa lỗi; chỉ API mới biết HTTP status code/ProblemDetails.
// Luồng: exception từ controller/service/repository -> middleware UseExceptionHandler -> TryHandleAsync -> ProblemDetails JSON.
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    // ASP.NET Core gọi method này cho mọi exception chưa được xử lý ở pipeline phía sau.
    // Trả true để báo exception đã được handle, ngăn framework ghi response lỗi mặc định lần nữa.
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Ghi full exception vào server log; client chỉ nhận detail an toàn, đặc biệt ở Production.
        logger.LogError(
            exception,
            "Request failed. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        // Map loại lỗi nghiệp vụ sang mã HTTP. Không cần if/else trong mọi controller.
        var (statusCode, title) = exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "The requested resource was not found."),
            ConflictException or BusinessRuleException => (
                StatusCodes.Status409Conflict,
                "The requested change conflicts with the current state."),
            DomainException => (
                StatusCodes.Status400BadRequest,
                "The request would create an invalid domain state."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.")
        };

        // Lỗi 500 không được lộ message nội bộ ở Production vì có thể chứa chi tiết kỹ thuật/nhạy cảm.
        // Development giữ message để developer debug nhanh hơn.
        var detail = statusCode == StatusCodes.Status500InternalServerError &&
                     !environment.IsDevelopment()
            ? "The server could not process the request."
            : exception.Message;

        // RFC 7807 ProblemDetails giúp mọi client nhận cùng cấu trúc: status, title, detail, traceId.
        // traceId liên kết response client với log server khi cần tra lỗi.
        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            })
            .ExecuteAsync(httpContext);

        return true;
    }
}
