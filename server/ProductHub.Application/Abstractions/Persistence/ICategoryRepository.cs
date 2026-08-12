// Application cần Entity Category nhưng không cần biết Entity này được lưu bằng EF Core, Dapper hay API khác.
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Abstractions.Persistence;

// File này là "port" persistence cho Category.
// Application chỉ phụ thuộc interface; Infrastructure sẽ cung cấp EfCategoryRepository để thực thi thật.
public interface ICategoryRepository
{
    // Đọc toàn bộ category cho use case danh sách. Implementation nên không tracking vì chỉ đọc.
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);

    // Lấy Entity để use case có thể đọc hoặc đổi trạng thái. Trả null khi không tìm thấy.
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // Kiểm tra khóa NormalizedName đã tồn tại chưa. excludingId dùng khi update để không tự coi record đang sửa là trùng.
    // NormalizedName là field được Entity Category tính bằng trim + ToUpperInvariant và có unique index trong database.
    Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName,
        int? excludingId,
        CancellationToken cancellationToken);

    // Kiểm tra quan hệ trước khi xóa Category để trả lỗi nghiệp vụ rõ ràng.
    Task<bool> HasProductsAsync(int id, CancellationToken cancellationToken);

    // Đưa Entity mới vào Unit of Work. Chưa ghi database cho đến SaveChangesAsync.
    void Add(Category category);

    // Đánh dấu Entity cần xóa. Việc xóa thật xảy ra khi Unit of Work được save.
    void Remove(Category category);
}
