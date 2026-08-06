// Product là Domain Entity được dùng trong các command/create/update/deactivate.
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Abstractions.Persistence;

// Port phục vụ các luồng ghi Product (command side).
// Tách interface này khỏi read repository để query không vô tình mang EF change tracker vào Application.
public interface IProductRepository
{
    // Lấy Entity Product để gọi UpdateDetails/Deactivate. Trả null nếu không có Id tương ứng.
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // Kiểm tra SKU đã tồn tại trước khi tạo. Database unique index vẫn là lớp bảo vệ cuối cùng.
    Task<bool> ExistsBySkuAsync(string normalizedSku, CancellationToken cancellationToken);

    // Đưa Product mới vào Unit of Work; chưa thực hiện INSERT tại thời điểm gọi method này.
    void Add(Product product);
}
