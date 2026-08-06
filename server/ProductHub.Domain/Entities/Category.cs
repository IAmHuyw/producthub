// Import exception của Domain để Entity tự bảo vệ dữ liệu mà không phụ thuộc API/EF Core.
using ProductHub.Domain.Exceptions;

namespace ProductHub.Domain.Entities;

// File này là Entity Category - mô hình nghiệp vụ cốt lõi của danh mục.
// Entity chỉ cho phép đổi trạng thái qua Create/Rename để mọi nơi đều áp dụng cùng một luật dữ liệu.
// Tầng Domain không import EF Core hoặc ASP.NET Core để giữ dependency hướng vào trong.
public sealed class Category
{
    private Category()
    {
        // EF Core cần constructor không tham số này để tạo object khi đọc record từ database.
        // Constructor là private để code nghiệp vụ không thể tạo Category thiếu dữ liệu hợp lệ.
    }

    private Category(string name, DateTime createdAtUtc)
    {
        Name = name;
        CreatedAtUtc = createdAtUtc;
    }

    // Id do database sinh. private set ngăn code bên ngoài tự gán Id.
    public int Id { get; private set; }

    // Tên chỉ được thay đổi bằng Rename để luôn đi qua ValidateName.
    public string Name { get; private set; } = string.Empty;

    // Thời điểm tạo theo UTC để không phụ thuộc múi giờ của server.
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation cho quan hệ 1 Category - nhiều Product. EF Core sẽ nạp khi cần.
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // Luồng tạo Category: validate tên -> khởi tạo Entity ở trạng thái hợp lệ -> trả Entity cho Application lưu.
    public static Category Create(string name, DateTime createdAtUtc)
        => new(ValidateName(name), createdAtUtc);

    // Luồng đổi tên: validate trước, chỉ gán state khi dữ liệu hợp lệ.
    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    private static string ValidateName(string name)
    {
        // String.IsNullOrWhiteSpace chặn cả null, "" và chuỗi chỉ chứa khoảng trắng.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        return name;
    }
}
