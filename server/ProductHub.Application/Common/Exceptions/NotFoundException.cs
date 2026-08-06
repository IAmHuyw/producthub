namespace ProductHub.Application.Common.Exceptions;

// Use case ném lỗi này khi không tìm thấy resource cần thiết.
// API bắt lỗi và đổi thành HTTP 404 ProblemDetails.
public sealed class NotFoundException(string message)
    : Exception(message);
