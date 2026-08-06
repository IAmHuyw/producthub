namespace ProductHub.Application.Categories;

// Contract của các use case Category. Controller chỉ biết interface này, không biết repository/EF Core.
public interface ICategoryService
{
    // Trả danh sách Category đã map sang DTO.
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);

    // Lấy chi tiết; ném NotFoundException nếu Id không tồn tại.
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    // Tạo Category mới; ném ConflictException khi tên trùng.
    Task<CategoryDto> CreateAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken);

    // Đổi tên Category; ném NotFoundException/ConflictException khi cần.
    Task UpdateAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken);

    // Xóa Category; ném BusinessRuleException nếu Category vẫn còn Product.
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
