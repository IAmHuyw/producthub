namespace ProductHub.Application.Common.Exceptions;

// Request có thể đúng kiểu dữ liệu nhưng vẫn vi phạm luật nghiệp vụ,
// ví dụ không được xóa Category đang chứa Product. API trả HTTP 409.
public sealed class BusinessRuleException(string message)
    : Exception(message);
