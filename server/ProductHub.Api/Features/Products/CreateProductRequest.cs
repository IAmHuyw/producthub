using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Features.Products;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Sku { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Range(0.01, 999_999_999)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
//Cái này là class dùng để tạo sản phẩm mới, có các thuộc tính như Name, Sku, Description, Price, StockQuantity và CategoryId.
//Các thuộc tính này đều có các ràng buộc dữ liệu (data annotations) để đảm bảo rằng dữ liệu nhập vào hợp lệ.
//Cái này là Dto thôi.