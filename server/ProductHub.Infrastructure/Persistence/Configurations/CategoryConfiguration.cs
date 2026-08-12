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
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        // Lưu sẵn tên chuẩn hóa để PostgreSQL đảm bảo unique không phân biệt hoa/thường.
        // Ví dụ: "Laptop", " laptop " và "LAPTOP" đều có NormalizedName là "LAPTOP".
        builder.Property(x => x.NormalizedName)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        // Unique index này là hàng rào cuối cùng khi hai request chạy đồng thời qua duplicate check ở Application.
        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();

        // CHECK ngăn dữ liệu rỗng/chỉ có khoảng trắng nếu ai đó ghi thẳng database, bỏ qua Domain/API.
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_categories_name_not_blank",
            "char_length(btrim(\"Name\")) > 0"));
    }
}
