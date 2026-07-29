using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Features.Products;

public sealed class UpdateProductRequest
{
    [Required]
    [StringLength(
        maximumLength: 120,
        MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Range(
        typeof(decimal),
        "0.01",
        "999999999")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
//init khác set, init chỉ có thể được gán giá trị trong quá trình khởi tạo đối tượng.
// trong khi set có thể được gán giá trị bất cứ lúc nào.