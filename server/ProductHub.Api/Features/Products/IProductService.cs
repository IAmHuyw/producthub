namespace ProductHub.Api.Features.Products;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(
        ProductQuery query,
        CancellationToken cancellationToken);

    Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken);
}

// interface này định nghĩa các phương thức mà service sẽ cung cấp, bao gồm các phương thức để lấy danh sách sản phẩm, lấy sản phẩm theo id, tạo mới sản phẩm, cập nhật sản phẩm và vô hiệu hóa sản phẩm.