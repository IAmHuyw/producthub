namespace ProductHub.Api.Common.Exceptions;

public sealed class ConflictException(string message)
    : Exception(message);
// Đây là một lớp ngoại lệ tùy chỉnh trong C# có tên là ConflictException, 
// được sử dụng để biểu thị một xung đột trong ứng dụng. 
// Lớp này kế thừa từ lớp Exception cơ bản và có một constructor nhận một thông điệp (message) làm tham số. Khi một xung đột xảy ra trong ứng dụng, 
// bạn có thể ném ngoại lệ này với thông điệp thích hợp để thông báo cho người dùng hoặc hệ thống về vấn đề đó.
// Ví dụ, nếu có một xung đột trong việc tạo một tài nguyên mới mà đã tồn tại, bạn có thể ném ConflictException với thông điệp mô tả xung đột đó.
// Nếu không có class ConflictException(string message) : Exception(message); thì khi xảy ra xung đột, bạn sẽ phải sử dụng một ngoại lệ chung như Exception hoặc InvalidOperationException, điều này có thể làm giảm tính rõ ràng và khả năng xử lý lỗi trong ứng dụng của bạn.