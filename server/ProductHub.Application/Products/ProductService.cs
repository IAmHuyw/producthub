// Các port persistence cho command và query. Application không import Microsoft.EntityFrameworkCore.
using ProductHub.Application.Abstractions.Persistence;
// Clock giúp tạo audit timestamp có thể test được.
using ProductHub.Application.Abstractions.Time;
// Các lỗi nghiệp vụ sẽ được API map sang mã HTTP.
using ProductHub.Application.Common.Exceptions;
// PagedResult là output phân trang chung của Application.
using ProductHub.Application.Common.Models;
// Product là Domain Entity; method Create/UpdateDetails/Deactivate bảo vệ invariant.
using ProductHub.Domain.Entities;

namespace ProductHub.Application.Products;

// File này điều phối các use case Product.
// Luồng command: Controller -> command -> service kiểm tra rule giữa Entity -> Domain Entity đổi state -> UnitOfWork commit.
// Luồng query: Controller -> query -> service validate -> read repository trả DTO; không cần tải Entity tracked.
public sealed class ProductService(
    IProductRepository products,
    IProductReadRepository productReads,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IProductService
{
    // Chặn page quá lớn để tránh phép tính Skip overflow và query OFFSET quá tốn tài nguyên.
    private const int MaximumPage = 10_000;

    // Whitelist sort được chấp nhận. Không truyền tên cột/biểu thức sort tự do từ client xuống database.
    private static readonly HashSet<string> SupportedSorts =
        new(StringComparer.Ordinal)
        {
            "name_asc",
            "name_desc",
            "price_asc",
            "price_desc"
        };

    // Use case GET /api/products.
    // Bước 1: validate phân trang/sort ở Application. Bước 2: giao read repository tạo SQL. Bước 3: trả DTO phân trang.
    public Task<PagedResult<ProductDto>> GetAllAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        // Đây là validation nghiệp vụ/transport-neutral; API DataAnnotations vẫn là hàng rào 400 sớm hơn.
        ValidateListQuery(query);
        return productReads.GetPageAsync(query, cancellationToken);
    }

    // Use case GET /api/products/{id}.
    // Giữ behavior cũ: detail có thể xem Product inactive; list công khai chỉ trả Product active.
    public async Task<ProductDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await productReads.GetByIdAsync(
            id,
            includeInactive: true,
            cancellationToken);

        return product
            ?? throw new NotFoundException("Product was not found.");
    }

    // Use case POST /api/products.
    // Luồng: normalize input -> check Category -> check SKU -> Product.Create -> track -> commit -> query lại DTO đầy đủ.
    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        // Name/SKU được trim; SKU còn uppercase để tránh tạo "sku-01" và "SKU-01" khác nhau qua API.
        var name = NormalizeRequired(command.Name);
        var sku = NormalizeRequired(command.Sku).ToUpperInvariant();

        // Kiểm tra Category trước khi tạo Product để trả 404 có ý nghĩa thay vì chờ foreign-key lỗi từ DB.
        var category = await categories.GetByIdAsync(command.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        // Check trước giúp trả message rõ; unique index DB vẫn chống race condition ở EfUnitOfWork.
        if (await products.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new ConflictException("SKU already exists.");
        }

        // Domain kiểm tra invariant nội tại: name/SKU không rỗng, price > 0, stock >= 0, categoryId > 0.
        var product = Product.Create(
            name,
            sku,
            NormalizeOptional(command.Description),
            command.Price,
            command.StockQuantity,
            command.CategoryId,
            clock.UtcNow);

        // Add mới chỉ gắn Entity vào change tracker; Id có giá trị sau khi SaveChangesAsync hoàn thành.
        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Entity chỉ có CategoryId, còn response cần CategoryName nên query lại qua read port với projection DTO.
        return await productReads.GetByIdAsync(
                   product.Id,
                   includeInactive: true,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "Created product could not be loaded.");
    }

    // Use case PUT /api/products/{id}.
    // Luồng: lấy Product tracked -> check Product/Category -> gọi Entity.UpdateDetails -> UnitOfWork commit.
    public async Task UpdateAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        // Không dùng read repository ở đây vì cần Entity tracked để EF biết field nào đã đổi.
        var product = await products.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product was not found.");
        }

        // Product phải luôn trỏ tới Category tồn tại trước khi thay đổi foreign key.
        var category = await categories.GetByIdAsync(command.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        // Domain method là điểm duy nhất đổi state chỉnh sửa; SKU không thể bị đổi trong luồng này.
        product.UpdateDetails(
            NormalizeRequired(command.Name),
            NormalizeOptional(command.Description),
            command.Price,
            command.StockQuantity,
            command.CategoryId,
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Use case DELETE /api/products/{id}.
    // Đây là soft delete: Entity chuyển IsActive=false, record vẫn còn cho audit/quan hệ dữ liệu.
    public async Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product was not found.");
        }

        // Deactivate idempotent: nếu Product đã inactive thì Entity không đổi state lần nữa.
        product.Deactivate(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Validate query ở Application để use case vẫn an toàn nếu được gọi từ worker/gRPC, không chỉ từ HTTP API.
    private static void ValidateListQuery(ProductListQuery query)
    {
        if (query.Page < 1 || query.Page > MaximumPage)
        {
            throw new BusinessRuleException(
                $"Page must be between 1 and {MaximumPage}.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new BusinessRuleException("Page size must be between 1 and 100.");
        }

        // Chỉ cho phép các giá trị enum-like trong whitelist để repository có switch an toàn/dễ đoán.
        if (!SupportedSorts.Contains(query.Sort))
        {
            throw new BusinessRuleException("The supplied sort value is not supported.");
        }
    }

    // Trim input bắt buộc. Nếu input null thì trả "" để Domain.Create/UpdateDetails ném DomainException rõ ràng.
    private static string NormalizeRequired(string? value)
        => value?.Trim() ?? string.Empty;

    // Description rỗng sau Trim được chuyển thành null, tránh lưu chuỗi "" không có ý nghĩa nghiệp vụ.
    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
