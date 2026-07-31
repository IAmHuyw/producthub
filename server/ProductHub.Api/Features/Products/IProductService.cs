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
// cancellationToken là một cơ chế trong .NET để quản lý việc hủy bỏ các tác vụ bất đồng bộ. Nó cho phép bạn gửi tín hiệu để dừng một tác vụ đang chạy, giúp tiết kiệm tài nguyên và cải thiện hiệu suất ứng dụng. Khi một phương thức nhận CancellationToken, nó có thể kiểm tra trạng thái của token để quyết định có tiếp tục thực hiện hay không, và nếu token được hủy, phương thức có thể dừng lại một cách an toàn.
// interface này định nghĩa các phương thức mà service sẽ cung cấp, bao gồm các phương thức để lấy danh sách sản phẩm, lấy sản phẩm theo id, tạo mới sản phẩm, cập nhật sản phẩm và vô hiệu hóa sản phẩm.