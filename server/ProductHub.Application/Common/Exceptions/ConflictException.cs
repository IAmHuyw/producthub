namespace ProductHub.Application.Common.Exceptions;

// Use case/repository ném lỗi này khi thay đổi xung đột với state hiện tại,
// ví dụ SKU trùng hoặc database từ chối unique/foreign-key constraint. API trả HTTP 409.
public sealed class ConflictException(string message)
    : Exception(message);
