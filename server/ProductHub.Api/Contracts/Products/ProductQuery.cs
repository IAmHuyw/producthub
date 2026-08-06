// Validation cho query string được ASP.NET Core thực hiện trước action controller.
using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Contracts.Products;

// Query-string contract của GET /api/products.
// Controller map model này sang ProductListQuery; Application kiểm tra whitelist cho Sort.
public sealed class ProductQuery
{
    // Giới hạn page để phép tính Skip không overflow và OFFSET không quá lớn.
    [Range(1, 10_000)]
    public int Page { get; init; } = 1;

    // Mỗi trang tối đa 100 record để tránh client yêu cầu response quá lớn.
    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    // Search tối đa 100 ký tự, dùng cho Name/SKU.
    [StringLength(100)]
    public string? Search { get; init; }

    // Filter optional, nhưng khi có phải là Id dương.
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    // Giá trị mặc định. Sort không dùng enum vì query string cần dễ đọc; Application sẽ whitelist giá trị hợp lệ.
    public string Sort { get; init; } = "name_asc";
}
