using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductHub.Infrastructure.Persistence.Migrations;

// Migration này chỉ gia cố Catalog hiện tại. Nó phải được chạy trước migration lớn
// chuyển Product thành aggregate chứa Variant ở checkpoint sau.
public partial class AddCatalogIntegrity : Migration
{
    // Up là luồng nâng cấp database đang tồn tại.
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Dừng migration có chủ đích nếu dữ liệu cũ đã có tên trùng không phân biệt hoa/thường.
        // Nếu không chặn sớm, create unique index sẽ báo lỗi khó hiểu và DBA khó biết nguyên nhân.
        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT upper(btrim(\"Name\"))
                    FROM categories
                    GROUP BY upper(btrim(\"Name\"))
                    HAVING COUNT(*) > 1
                ) THEN
                    RAISE EXCEPTION 'Cannot add case-insensitive category uniqueness because duplicate category names exist.';
                END IF;
            END $$;
            """);

        // Cột mới bắt buộc cần default tạm để PostgreSQL có thể thêm vào table đã có dữ liệu.
        // Ngay bên dưới mọi row được backfill, sau đó default bị xóa để tránh che giấu dữ liệu thiếu.
        migrationBuilder.AddColumn<string>(
            name: "NormalizedName",
            table: "categories",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.Sql("""
            UPDATE categories
            SET \"NormalizedName\" = upper(btrim(\"Name\"));

            ALTER TABLE categories
            ALTER COLUMN \"NormalizedName\" DROP DEFAULT;
            """);

        // Index cũ chỉ phân biệt trùng tuyệt đối. Thay nó bằng index của khóa chuẩn hóa.
        migrationBuilder.DropIndex(
            name: "IX_categories_Name",
            table: "categories");

        migrationBuilder.CreateIndex(
            name: "IX_categories_NormalizedName",
            table: "categories",
            column: "NormalizedName",
            unique: true);

        // CHECK bảo vệ database nếu import/script bên ngoài bỏ qua Domain validation.
        migrationBuilder.AddCheckConstraint(
            name: "CK_categories_name_not_blank",
            table: "categories",
            sql: "char_length(btrim(\"Name\")) > 0");

        migrationBuilder.AddCheckConstraint(
            name: "CK_products_price_positive",
            table: "products",
            sql: "\"Price\" > 0");

        migrationBuilder.AddCheckConstraint(
            name: "CK_products_stock_quantity_non_negative",
            table: "products",
            sql: "\"StockQuantity\" >= 0");

        migrationBuilder.AddCheckConstraint(
            name: "CK_products_name_not_blank",
            table: "products",
            sql: "char_length(btrim(\"Name\")) > 0");

        migrationBuilder.AddCheckConstraint(
            name: "CK_products_sku_not_blank",
            table: "products",
            sql: "char_length(btrim(\"Sku\")) > 0");
    }

    // Down chỉ gỡ những thay đổi do migration này tạo ra.
    // Lưu ý: Down không khôi phục biến thể chữ hoa/thường của Name, vì Name gốc không bị thay đổi ở Up.
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_products_sku_not_blank",
            table: "products");

        migrationBuilder.DropCheckConstraint(
            name: "CK_products_name_not_blank",
            table: "products");

        migrationBuilder.DropCheckConstraint(
            name: "CK_products_stock_quantity_non_negative",
            table: "products");

        migrationBuilder.DropCheckConstraint(
            name: "CK_products_price_positive",
            table: "products");

        migrationBuilder.DropCheckConstraint(
            name: "CK_categories_name_not_blank",
            table: "categories");

        migrationBuilder.DropIndex(
            name: "IX_categories_NormalizedName",
            table: "categories");

        migrationBuilder.CreateIndex(
            name: "IX_categories_Name",
            table: "categories",
            column: "Name",
            unique: true);

        migrationBuilder.DropColumn(
            name: "NormalizedName",
            table: "categories");
    }
}
