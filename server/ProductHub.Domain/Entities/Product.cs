// Import exception của Domain để Product có thể báo lỗi invariant mà không biết HTTP là gì.
using ProductHub.Domain.Exceptions;

namespace ProductHub.Domain.Entities;

// File này là Entity Product - nơi giữ state và các invariant cốt lõi của sản phẩm.
// Application không được public set property trực tiếp; mọi thay đổi phải qua method bên dưới.
// Nhờ vậy, giá luôn > 0 và số lượng tồn luôn >= 0, bất kể Entity được gọi từ API, worker hay test.
public sealed class Product
{
    // Các giới hạn này thuộc Domain, vì Product có thể được tạo từ API, worker hoặc test.
    // API DataAnnotations và Fluent API của EF Core tham chiếu cùng giá trị để không bị lệch rule.
    public const int MaxNameLength = 120;
    public const int MaxSkuLength = 50;
    public const int MaxDescriptionLength = 1000;

    private Product()
    {
        // EF Core gọi constructor này khi materialize Product từ database.
        // Nó private để developer không thể tạo Product rỗng bằng new Product().
    }

    private Product(
        string name,
        string sku,
        string? description,
        decimal price,
        int stockQuantity,
        int categoryId,
        DateTime createdAtUtc)
    {
        Name = name;
        Sku = sku;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    // Id được database sinh sau SaveChangesAsync.
    public int Id { get; private set; }

    // Tên hiển thị của sản phẩm.
    public string Name { get; private set; } = string.Empty;

    // SKU được xem là bất biến sau khi tạo. Nếu nghiệp vụ cho đổi SKU,
    // hãy tạo use case/command riêng thay vì thêm setter public.
    public string Sku { get; private set; } = string.Empty;

    // Description là optional, null khác với chuỗi rỗng sau khi Application normalize input.
    public string? Description { get; private set; }

    // Giá bán, luôn phải lớn hơn 0 theo Validate.
    public decimal Price { get; private set; }

    // Số lượng tồn kho, luôn không âm theo Validate.
    public int StockQuantity { get; private set; }

    // Soft delete dùng cờ này thay vì xoá vật lý record.
    public bool IsActive { get; private set; }

    // Foreign key trỏ tới Category.
    public int CategoryId { get; private set; }

    // Navigation property để EF Core join sang Category khi cần Category.Name.
    public Category Category { get; private set; } = null!;

    // Audit fields lưu UTC nhằm nhất quán giữa các môi trường.
    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    // Luồng tạo Product: validate tất cả invariant -> khởi tạo Product active -> trả Entity chưa lưu.
    // Việc kiểm tra Category có tồn tại thuộc Application vì đó là rule giữa hai Entity/repository.
    public static Product Create(
        string name,
        string sku,
        string? description,
        decimal price,
        int stockQuantity,
        int categoryId,
        DateTime createdAtUtc)
    {
        // Normalize trước rồi mới validate/lưu để state trong Entity luôn nhất quán,
        // kể cả khi Entity được tạo từ một caller không đi qua HTTP API.
        var normalizedName = NormalizeRequiredText(
            name,
            MaxNameLength,
            "Product name is required.",
            "Product name");
        var normalizedSku = NormalizeRequiredText(
            sku,
            MaxSkuLength,
            "Product SKU is required.",
            "Product SKU")
            .ToUpperInvariant();
        var normalizedDescription = NormalizeDescription(description);

        Validate(price, stockQuantity, categoryId);

        return new Product(
            normalizedName,
            normalizedSku,
            normalizedDescription,
            price,
            stockQuantity,
            categoryId,
            createdAtUtc);
    }

    // Luồng cập nhật thông tin: SKU cố tình không nhận vào để bảo toàn tính bất biến của SKU.
    // Chỉ khi Validate thành công mới cập nhật các field và mốc UpdatedAtUtc.
    public void UpdateDetails(
        string name,
        string? description,
        decimal price,
        int stockQuantity,
        int categoryId,
        DateTime updatedAtUtc)
    {
        var normalizedName = NormalizeRequiredText(
            name,
            MaxNameLength,
            "Product name is required.",
            "Product name");
        var normalizedDescription = NormalizeDescription(description);

        Validate(price, stockQuantity, categoryId);

        Name = normalizedName;
        Description = normalizedDescription;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        UpdatedAtUtc = updatedAtUtc;
    }

    // Soft delete Product. Gọi lặp lại an toàn (idempotent): Product đã inactive thì không đổi thêm.
    public void Deactivate(DateTime updatedAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    // Kiểm tra các rule số và khóa ngoại nội bộ. Rule chuỗi được kiểm tra trong các method normalize
    // để Entity không bao giờ giữ dữ liệu có khoảng trắng đầu/cuối hoặc Description rỗng.
    private static void Validate(
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        // Các điều kiện này là invariant - lớp phòng thủ cuối cùng trước khi Entity nhận state mới.
        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Product stock quantity cannot be negative.");
        }

        if (categoryId <= 0)
        {
            throw new DomainException("A product must belong to a category.");
        }
    }

    // Chuẩn hóa field bắt buộc: chặn null/whitespace, trim và áp dụng giới hạn domain.
    // `fieldLabel` chỉ phục vụ thông điệp lỗi; nó không xuất hiện trong database schema.
    private static string NormalizeRequiredText(
        string? value,
        int maxLength,
        string requiredMessage,
        string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(requiredMessage);
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException(
                $"{fieldLabel} cannot be longer than {maxLength} characters.");
        }

        return trimmedValue;
    }

    // Description không bắt buộc: null, "" hoặc chỉ khoảng trắng cùng được lưu thành null.
    // Nhờ vậy query/API không cần phân biệt hai trạng thái rỗng mang cùng ý nghĩa.
    private static string? NormalizeDescription(string? description)
    {
        var trimmedDescription = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        if (trimmedDescription is not null && trimmedDescription.Length > MaxDescriptionLength)
        {
            throw new DomainException(
                $"Product description cannot be longer than {MaxDescriptionLength} characters.");
        }

        return trimmedDescription;
    }
}
