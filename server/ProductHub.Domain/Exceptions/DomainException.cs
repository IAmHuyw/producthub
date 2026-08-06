namespace ProductHub.Domain.Exceptions;

// File này định nghĩa lỗi của tầng Domain.
// DomainException được ném khi một Entity sắp rơi vào trạng thái không hợp lệ,
// ví dụ Product có giá <= 0. API sẽ bắt lỗi này ở GlobalExceptionHandler và trả HTTP 400.
public sealed class DomainException(string message)
    : Exception(message);
