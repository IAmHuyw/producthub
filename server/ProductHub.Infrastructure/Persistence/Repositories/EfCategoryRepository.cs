// Extension async LINQ của EF Core như ToListAsync/AnyAsync/AsNoTracking.
using Microsoft.EntityFrameworkCore;
// Interface persistence do Application định nghĩa; Infrastructure implement interface này.
using ProductHub.Application.Abstractions.Persistence;
// Category là Entity Domain được EF Core materialize/track.
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence.Repositories;

// Repository EF Core của Category.
// File này chuyển các method abstract của ICategoryRepository thành LINQ được EF/Npgsql dịch sang SQL PostgreSQL.
public sealed class EfCategoryRepository(AppDbContext dbContext)
    : ICategoryRepository
{
    // Query danh sách chỉ đọc. AsNoTracking giảm memory vì EF không cần theo dõi Entity để update.
    public async Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    // Query cho command. Không dùng AsNoTracking vì Application có thể Rename/Remove Entity rồi UnitOfWork.SaveChanges.
    public Task<Category?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return dbContext.Categories.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
    }

    // Kiểm tra duplicate theo tên đã normalize. Query này được chạy trước Create/Update để trả lỗi dễ hiểu.
    // Database unique index vẫn cần thiết vì hai request có thể cùng vượt qua check này.
    public Task<bool> ExistsByNameAsync(
        string normalizedName,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        return dbContext.Categories.AnyAsync(
            x => (!excludingId.HasValue || x.Id != excludingId.Value) &&
                 x.Name.ToUpper() == normalizedName,
            cancellationToken);
    }

    // Dùng AnyAsync thay vì CountAsync vì database có thể dừng ngay khi tìm thấy Product đầu tiên.
    public Task<bool> HasProductsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            x => x.CategoryId == id,
            cancellationToken);
    }

    // Add chỉ bắt đầu tracking Entity ở trạng thái Added; SaveChangesAsync mới INSERT.
    public void Add(Category category) => dbContext.Categories.Add(category);

    // Remove đánh dấu trạng thái Deleted; database chỉ xóa khi UnitOfWork commit.
    public void Remove(Category category) => dbContext.Categories.Remove(category);
}
