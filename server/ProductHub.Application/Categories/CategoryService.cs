// Các interface persistence: Application chỉ gọi port, không gọi DbContext/EF Core.
using ProductHub.Application.Abstractions.Persistence;
// Clock được inject để timestamp dễ test và luôn dùng UTC.
using ProductHub.Application.Abstractions.Time;
// Exception mức Application; API sẽ chuyển chúng thành HTTP ProblemDetails.
using ProductHub.Application.Common.Exceptions;
// Category là Entity nghiệp vụ được tạo/đổi state trong service này.
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Categories;

// File này chứa các use case của Category.
// Luồng chung: Controller tạo Command -> CategoryService kiểm tra rule -> Repository/UnitOfWork -> Infrastructure/EF Core.
// Service phụ thuộc interface thay vì AppDbContext, vì vậy business logic không bị khóa vào PostgreSQL hay EF Core.
public sealed class CategoryService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    IClock clock)
    : ICategoryService
{
    // Use case GET /api/categories.
    // Bước 1: lấy Entity từ repository. Bước 2: map sang DTO. Bước 3: trả DTO, không trả EF Entity cho API.
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var categoryEntities = await categories.GetAllAsync(cancellationToken);

        // Select chỉ tạo read model; không làm thay đổi Entity hay gọi SaveChanges.
        return categoryEntities
            .Select(MapToDto)
            .ToList();
    }

    // Use case GET /api/categories/{id}.
    // Nếu repository trả null, ném exception để GlobalExceptionHandler tạo HTTP 404 thống nhất.
    public async Task<CategoryDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken);

        return category is null
            ? throw new NotFoundException("Category was not found.")
            : MapToDto(category);
    }

    // Use case POST /api/categories.
    // Luồng: normalize input -> kiểm tra trùng tên -> tạo Domain Entity -> Add vào UnitOfWork -> commit -> trả DTO.
    public async Task<CategoryDto> CreateAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // Domain chuẩn hóa (trim + validate) để mọi loại caller dùng đúng một luật dữ liệu.
        var name = Category.NormalizeName(command.Name);

        // Kiểm tra sớm để trả lỗi dễ hiểu; unique index NormalizedName vẫn xử lý race condition ở database.
        if (await categories.ExistsByNormalizedNameAsync(
                Category.NormalizeForComparison(name),
                excludingId: null,
                cancellationToken))
        {
            throw new ConflictException("Category name already exists.");
        }

        // Domain Entity tự validate tên rỗng/whitespace trước khi nhận state mới.
        var category = Category.Create(name, clock.UtcNow);
        categories.Add(category);

        // Add chỉ track Entity; SaveChangesAsync mới thực sự INSERT database.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    // Use case PUT /api/categories/{id}.
    // Luồng: lấy Entity tracked -> kiểm tra tồn tại/trùng -> gọi Entity.Rename -> commit.
    public async Task UpdateAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // Repository command trả Entity được tracking để Rename được EF Core phát hiện khi commit.
        var category = await categories.GetByIdAsync(command.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        var name = Category.NormalizeName(command.Name);

        // excludingId giúp chính Category đang update không bị coi là duplicate.
        if (await categories.ExistsByNormalizedNameAsync(
                Category.NormalizeForComparison(name),
                command.Id,
                cancellationToken))
        {
            throw new ConflictException("Category name already exists.");
        }

        // Không gán category.Name trực tiếp vì setter private; Entity chịu trách nhiệm kiểm tra state.
        category.Rename(name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Use case DELETE /api/categories/{id}.
    // Luồng: tìm Category -> kiểm tra còn Product không -> đánh dấu xóa -> commit.
    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        // Luật nghiệp vụ: không xóa Category còn Product để giữ dữ liệu product luôn có category hợp lệ.
        if (await categories.HasProductsAsync(id, cancellationToken))
        {
            throw new BusinessRuleException(
                "A category containing products cannot be deleted.");
        }

        // Foreign key Restrict ở database là lớp bảo vệ cuối cùng nếu có request đồng thời tạo Product.
        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Chỉ map field cho output; không expose collection Products hay Entity nội bộ ra API.
    private static CategoryDto MapToDto(Category category)
        => new(category.Id, category.Name, category.CreatedAtUtc);

}
