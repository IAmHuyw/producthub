// Attribute validation cho request body trước khi controller gọi Application service.
using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Contracts.Products;

// HTTP request contract của PUT /api/products/{id}.
// SKU cố tình không có ở đây: đổi SKU là use case riêng để tránh vô tình thay mã định danh nghiệp vụ.
public sealed class UpdateProductRequest
{
    // Các constraint này cần khớp schema trong ProductConfiguration.
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
