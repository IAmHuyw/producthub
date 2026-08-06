namespace ProductHub.Application.Products;

// Dữ liệu lọc/phân trang của use case danh sách Product.
// API map query string vào record này; record không phụ thuộc ASP.NET Core nên tái dùng được ở adapter khác.
public sealed record ProductListQuery(
    int Page,
    int PageSize,
    string? Search,
    int? CategoryId,
    string Sort);
