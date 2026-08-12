// Import exception của Domain để Entity tự bảo vệ dữ liệu mà không phụ thuộc API/EF Core.
using ProductHub.Domain.Exceptions;

namespace ProductHub.Domain.Entities;

// File này là Entity Category - mô hình nghiệp vụ cốt lõi của danh mục.
// Entity chỉ cho phép đổi trạng thái qua Create/Rename để mọi nơi đều áp dụng cùng một luật dữ liệu.
// Tầng Domain không import EF Core hoặc ASP.NET Core để giữ dependency hướng vào trong.
public sealed class Category
{
    // Giới hạn này là luật nghiệp vụ dùng chung. API và EF Core sẽ dùng cùng giá trị
    // để không xảy ra tình trạng một tầng nhận dữ liệu nhưng tầng khác lại từ chối.
    public const int MaxNameLength = 100;

    private Category()
    {
        // EF Core cần constructor không tham số này để tạo object khi đọc record từ database.
        // Constructor là private để code nghiệp vụ không thể tạo Category thiếu dữ liệu hợp lệ.
    }

    private Category(
        string name,
        string normalizedName,
        DateTime createdAtUtc)
    {
        Name = name;
        NormalizedName = normalizedName;
        CreatedAtUtc = createdAtUtc;
    }

    // Id do database sinh. private set ngăn code bên ngoài tự gán Id.
    public int Id { get; private set; }

    // Tên chỉ được thay đổi bằng Rename để luôn đi qua ValidateName.
    public string Name { get; private set; } = string.Empty;

    // Bản tên chuẩn hóa (trim + uppercase invariant), chỉ dùng nội bộ để so sánh/unique.
    // Không trả field này ra client vì nó không phải tên hiển thị của danh mục.
    public string NormalizedName { get; private set; } = string.Empty;

    // Thời điểm tạo theo UTC để không phụ thuộc múi giờ của server.
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation cho quan hệ 1 Category - nhiều Product. EF Core sẽ nạp khi cần.
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    // Luồng tạo Category:
    // 1. NormalizeName loại khoảng trắng đầu/cuối và kiểm tra luật độ dài.
    // 2. NormalizeForComparison tạo khóa so sánh không phân biệt hoa/thường.
    // 3. Khởi tạo Entity hợp lệ; Application mới quyết định lưu Entity này.
    public static Category Create(string name, DateTime createdAtUtc)
    {
        var normalizedDisplayName = NormalizeName(name);

        return new(
            normalizedDisplayName,
            NormalizeForComparison(normalizedDisplayName),
            createdAtUtc);
    }

    // Luồng đổi tên: validate trước, chỉ gán state khi dữ liệu hợp lệ.
    public void Rename(string name)
    {
        var normalizedDisplayName = NormalizeName(name);

        Name = normalizedDisplayName;
        NormalizedName = NormalizeForComparison(normalizedDisplayName);
    }

    // Public vì Application cần cùng một quy tắc để kiểm tra duplicate trước khi tạo/sửa.
    // Entity vẫn gọi lại method này, nên caller không thể bỏ qua validation bằng cách gọi Create/Rename trực tiếp.
    public static string NormalizeName(string? name)
    {
        // String.IsNullOrWhiteSpace chặn cả null, "" và chuỗi chỉ chứa khoảng trắng.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > MaxNameLength)
        {
            throw new DomainException(
                $"Category name cannot be longer than {MaxNameLength} characters.");
        }

        return trimmedName;
    }

    // Invariant culture tránh kết quả phụ thuộc locale của máy chủ, ví dụ quy tắc chữ i ở Turkish locale.
    // Parameter phải là tên đã được NormalizeName; method giữ public để service không tự lặp lại quy tắc này.
    public static string NormalizeForComparison(string normalizedName)
        => normalizedName.ToUpperInvariant();
}
