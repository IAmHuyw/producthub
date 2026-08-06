// Data annotations được model binding của ASP.NET Core dùng để tạo ValidationProblemDetails HTTP 400.
using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Contracts.Products;

// HTTP request contract của POST /api/products.
// Đây không phải Domain Entity: client không được bind trực tiếp vào Entity có state/logic nghiệp vụ.
public sealed class CreateProductRequest
{
    // Tên có độ dài 2-120. Application/Domain còn trim và chặn chuỗi chỉ có khoảng trắng.
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    // SKU là mã định danh nghiệp vụ, bắt buộc và tối đa 50 ký tự.
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Sku { get; init; } = string.Empty;

    // Description optional; Application chuyển chuỗi rỗng sau trim thành null.
    [StringLength(1000)]
    public string? Description { get; init; }

    // decimal phù hợp tiền tệ hơn double; Range chặn giá <= 0 ngay tại API.
    [Range(0.01, 999_999_999)]
    public decimal Price { get; init; }

    // Tồn kho không được âm.
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    // CategoryId phải dương; Application vẫn query database để chắc chắn Category thật sự tồn tại.
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
