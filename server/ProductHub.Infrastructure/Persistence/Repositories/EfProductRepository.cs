// Extension LINQ async, EF.Functions và AsNoTracking của EF Core.
using Microsoft.EntityFrameworkCore;
// Command/query ports do Application định nghĩa.
using ProductHub.Application.Abstractions.Persistence;
// Output phân trang và Product DTO/query model thuộc Application.
using ProductHub.Application.Common.Models;
using ProductHub.Application.Products;
// Product là Entity Domain được dùng trong luồng command.
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence.Repositories;

// Một implementation EF Core cho cả command port và read port của Product.
// Tách interface ở Application vẫn có lợi: command làm việc với Entity tracked, query trả DTO no-tracking.
public sealed class EfProductRepository(AppDbContext dbContext)
    : IProductRepository, IProductReadRepository
{
    // Command query: lấy Entity tracked để ProductService có thể gọi UpdateDetails/Deactivate.
    public Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return dbContext.Products.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
    }

    // Kiểm tra SKU đã chuẩn hóa. ProductService chuyển SKU thành uppercase trước khi gọi.
    public Task<bool> ExistsBySkuAsync(
        string normalizedSku,
        CancellationToken cancellationToken)
    {
        return dbContext.Products.AnyAsync(
            x => x.Sku == normalizedSku,
            cancellationToken);
    }

    // Đánh dấu Product mới là Added trong change tracker. INSERT xảy ra khi EfUnitOfWork.SaveChangesAsync.
    public void Add(Product product) => dbContext.Products.Add(product);

    // Read use case GET /api/products.
    // Luồng SQL: tạo IQueryable -> thêm filter có điều kiện -> sort ổn định -> Count -> Skip/Take -> project thẳng DTO.
    public async Task<PagedResult<ProductDto>> GetPageAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        // Query bắt đầu chỉ gồm Product active. List public sẽ không hiển thị soft-deleted Product.
        IQueryable<Product> productsQuery = dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            // ILike là PostgreSQL case-insensitive LIKE; EF Core parameterize giá trị search nên không SQL injection.
            productsQuery = productsQuery.Where(x =>
                EF.Functions.ILike(x.Name, $"%{search}%") ||
                EF.Functions.ILike(x.Sku, $"%{search}%"));
        }

        // Filter Category chỉ được thêm khi caller truyền CategoryId.
        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(
                x => x.CategoryId == query.CategoryId.Value);
        }

        // Application đã validate Sort bằng whitelist. ThenBy(Id) làm paging ổn định khi nhiều Product cùng name/price.
        var orderedProducts = query.Sort switch
        {
            "price_asc" => productsQuery.OrderBy(x => x.Price).ThenBy(x => x.Id),
            "price_desc" => productsQuery.OrderByDescending(x => x.Price).ThenBy(x => x.Id),
            "name_desc" => productsQuery.OrderByDescending(x => x.Name).ThenBy(x => x.Id),
            _ => productsQuery.OrderBy(x => x.Name).ThenBy(x => x.Id)
        };

        // Count và query Items là hai SQL query riêng: count dùng để frontend biết tổng số trang.
        var totalCount = await orderedProducts.CountAsync(cancellationToken);

        // Skip bỏ record của các trang trước; Take giới hạn số record của trang hiện tại.
        // Select projection giúp EF tạo JOIN Category và chỉ lấy cột cần cho ProductDto, không load Product Entity đầy đủ.
        var items = await orderedProducts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProductDto(
                x.Id,
                x.Name,
                x.Sku,
                x.Description,
                x.Price,
                x.StockQuantity,
                x.IsActive,
                x.CategoryId,
                x.Category.Name,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }

    // Read use case GET /api/products/{id} hoặc đọc lại Product sau Create.
    // includeInactive cho phép Application quyết định policy soft delete mà repository không tự áp rule nghiệp vụ.
    public Task<ProductDto?> GetByIdAsync(
        int id,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        // Detail cũng no-tracking vì chỉ trả response, không thay đổi Entity.
        IQueryable<Product> productsQuery = dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == id);

        if (!includeInactive)
        {
            // Khi policy yêu cầu, loại Product inactive khỏi kết quả detail.
            productsQuery = productsQuery.Where(x => x.IsActive);
        }

        // Projection navigation x.Category.Name khiến EF Core tạo JOIN; không cần Include vì không materialize Entity.
        return productsQuery
            .Select(x => new ProductDto(
                x.Id,
                x.Name,
                x.Sku,
                x.Description,
                x.Price,
                x.StockQuantity,
                x.IsActive,
                x.CategoryId,
                x.Category.Name,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
