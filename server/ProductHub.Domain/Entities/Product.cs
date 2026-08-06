// Import exception của Domain để Product có thể báo lỗi invariant mà không biết HTTP là gì.
using ProductHub.Domain.Exceptions;

namespace ProductHub.Domain.Entities;

// File này là Entity Product - nơi giữ state và các invariant cốt lõi của sản phẩm.
// Application không được public set property trực tiếp; mọi thay đổi phải qua method bên dưới.
// Nhờ vậy, giá luôn > 0 và số lượng tồn luôn >= 0, bất kể Entity được gọi từ API, worker hay test.
public sealed class Product
{
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
        Validate(name, sku, price, stockQuantity, categoryId);

        return new Product(
            name,
            sku,
            description,
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
        Validate(name, Sku, price, stockQuantity, categoryId);

        Name = name;
        Description = description;
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

    private static void Validate(
        string name,
        string sku,
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        // Các điều kiện này là invariant - lớp phòng thủ cuối cùng trước khi Entity nhận state mới.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Product SKU is required.");
        }

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
}
