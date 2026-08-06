// Interface cấu hình Entity và builder Fluent API của EF Core.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// Entity thuần của Domain được ánh xạ ở đây, thay vì tự mang attribute database.
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence.Configurations;

// File này mô tả cách Category được lưu xuống PostgreSQL.
// Thay đổi schema ở đây cần đi kèm migration mới, không sửa migration đã áp dụng.
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // EF Core gọi Configure khi dựng model. builder là Fluent API đại diện cho bảng categories.
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Đặt tên bảng theo snake_case thay vì dùng mặc định Categories.
        builder.ToTable("categories");

        // Id là primary key và mặc định được PostgreSQL sinh theo strategy hiện có trong migration.
        builder.HasKey(x => x.Id);

        // Name bắt buộc, tối đa 100 ký tự; đây là hàng rào database bổ sung cho Domain/API validation.
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Unique index chặn hai request đồng thời cùng tạo đúng một tên giống hệt nhau.
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
