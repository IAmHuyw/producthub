// PagedResult là model phân trang độc lập HTTP.
using ProductHub.Application.Common.Models;
// ProductDto/ProductListQuery là input-output của use case đọc Product.
using ProductHub.Application.Products;

namespace ProductHub.Application.Abstractions.Persistence;

// Port phục vụ các luồng đọc Product (query side).
// Query trả ProductDto trực tiếp, không cần tải Domain Entity hay EF tracking rồi mới map.
public interface IProductReadRepository
{
    // Trả một trang Product active theo search/category/sort đã được Application validate.
    Task<PagedResult<ProductDto>> GetPageAsync(
        ProductListQuery query,
        CancellationToken cancellationToken);

    // Lấy chi tiết theo Id. includeInactive cho phép use case quyết định policy của soft delete.
    Task<ProductDto?> GetByIdAsync(
        int id,
        bool includeInactive,
        CancellationToken cancellationToken);
}
