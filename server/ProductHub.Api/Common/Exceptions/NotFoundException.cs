namespace ProductHub.Api.Common.Exceptions;

public sealed class NotFoundException(string message)
    : Exception(message);
// Đây là một lớp ngoại lệ tùy chỉnh trong C# có tên là NotFoundException, được sử dụng để biểu thị rằng một tài nguyên hoặc đối tượng không được tìm thấy trong ứng dụng. Lớp này kế thừa từ lớp Exception cơ bản và có một constructor nhận một thông điệp (message) làm tham số. Khi một tài nguyên không tồn tại hoặc không thể tìm thấy, bạn có thể ném ngoại lệ này với thông điệp thích hợp để thông báo cho người dùng hoặc hệ thống về vấn đề đó.
// Ví dụ, nếu bạn đang tìm kiếm một sản phẩm theo ID trong cơ sở dữ liệu và không tìm thấy sản phẩm đó, bạn có thể ném NotFoundException với thông điệp mô tả rằng sản phẩm không tồn tại. Điều này giúp cải thiện khả năng xử lý lỗi trong ứng dụng của bạn và cung cấp thông tin rõ ràng hơn cho người dùng hoặc các nhà phát triển khác về lý do tại sao yêu cầu của họ không thành công.