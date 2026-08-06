using ProductHub.Application.Common.Models;

namespace ProductHub.Application.Products;

// Contract use case Product. Controller gọi interface này và không được inject repository/AppDbContext.
public interface IProductService
{
    // Lấy Product active có phân trang, search, filter category và sort.
    Task<PagedResult<ProductDto>> GetAllAsync(
        ProductListQuery query,
        CancellationToken cancellationToken);

    // Lấy Product theo Id; ném NotFoundException nếu không tồn tại.
    Task<ProductDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    // Tạo Product; kiểm tra Category và SKU trước khi commit.
    Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken);

    // Cập nhật các field cho phép sửa; không thay đổi SKU.
    Task UpdateAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken);

    // Soft delete bằng cách chuyển IsActive thành false.
    Task DeactivateAsync(int id, CancellationToken cancellationToken);
}
