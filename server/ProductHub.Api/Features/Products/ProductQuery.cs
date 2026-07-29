using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Features.Products;

public sealed class ProductQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [StringLength(100)]
    public string? Search { get; init; }

    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    public string Sort { get; init; } = "name_asc";
}
// Cái này là class dùng để query sản phẩm, có các thuộc tính như Page, PageSize, Search, CategoryId và Sort.