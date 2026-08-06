// EF Core cung cấp DbContext, DbSet và ModelBuilder.
using Microsoft.EntityFrameworkCore;
// Import Domain Entity để EF Core biết các bảng nào cần ánh xạ.
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence;

// File này là cổng làm việc của EF Core với database.
// DbContext (change tracker + Unit of Work của EF) chỉ sống ở Infrastructure; Application không được inject class này.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    // DbSet đại diện cho bảng products; repository dùng property này để tạo LINQ query/track Entity.
    public DbSet<Product> Products => Set<Product>();

    // DbSet đại diện cho bảng categories.
    public DbSet<Category> Categories => Set<Category>();

    // EF gọi method này khi khởi tạo model để biết schema/table/index/relationship.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tự quét các class IEntityTypeConfiguration trong assembly Infrastructure.
        // Nhờ đó Domain Entity không cần dùng attribute EF Core như [Table] hay [Key].
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
