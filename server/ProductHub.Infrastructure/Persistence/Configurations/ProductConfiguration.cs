// Fluent API EF Core dùng để mô tả table, column, index và foreign key.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// Product là Entity Domain, không chứa persistence annotation.
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence.Configurations;

// File này mô tả Product được lưu vào PostgreSQL như thế nào.
// Các constraint ở đây là lớp bảo vệ database; chúng không thay thế validation ở API/Domain.
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // EF Core gọi method này trong AppDbContext.OnModelCreating.
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(x => x.Id);

        // Các max length phải khớp DataAnnotations/API để database không nhận dữ liệu vượt giới hạn từ nguồn khác.
        builder.Property(x => x.Name)
            .HasMaxLength(Product.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(Product.MaxSkuLength)
            .IsRequired();

        // SKU có unique index. Application check trước để message dễ hiểu; DB giữ tính đúng khi có race condition.
        builder.HasIndex(x => x.Sku)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(Product.MaxDescriptionLength);

        // numeric(18,2) phù hợp tiền tệ cơ bản và tránh sai số của float/double.
        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        // Relationship 1 Category - N Product. Restrict là lớp bảo vệ cuối cùng:
        // database từ chối DELETE Category nếu vẫn còn Product tham chiếu đến nó.
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // CHECK là lớp bảo vệ khi dữ liệu được import/ghi trực tiếp mà không đi qua Domain Entity.
        // Các rule này phải đồng nhất với Product.Validate để lỗi không chỉ được phát hiện khi đọc dữ liệu.
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_products_price_positive", "\"Price\" > 0");
            table.HasCheckConstraint("CK_products_stock_quantity_non_negative", "\"StockQuantity\" >= 0");
            table.HasCheckConstraint("CK_products_name_not_blank", "char_length(btrim(\"Name\")) > 0");
            table.HasCheckConstraint("CK_products_sku_not_blank", "char_length(btrim(\"Sku\")) > 0");
        });
    }
}
