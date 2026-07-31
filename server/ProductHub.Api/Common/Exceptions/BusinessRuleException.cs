namespace ProductHub.Api.Common.Exceptions;

public sealed class BusinessRuleException(string message)
    : Exception(message);
// Đây là một lớp ngoại lệ tùy chỉnh trong C# có tên là BusinessRuleException, được sử dụng để biểu thị rằng một quy tắc kinh doanh đã bị vi phạm trong ứng dụng. Lớp này kế thừa từ lớp Exception cơ bản và có một constructor nhận một thông điệp (message) làm tham số. Khi một quy tắc kinh doanh không được tuân thủ, bạn có thể ném ngoại lệ này với thông điệp thích hợp để thông báo cho người dùng hoặc hệ thống về vấn đề đó.