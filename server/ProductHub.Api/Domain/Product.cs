namespace ProductHub.Api.Domain;

public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    // thêm isactive để quản lý trạng thái sản phẩm, nó có thế ngừng bán chứ không xóa vì ảnh hưởng đến quan hệ
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}