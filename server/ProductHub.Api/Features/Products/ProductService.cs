using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductHub.Api.Common.Exceptions;
using ProductHub.Api.Data;
using ProductHub.Api.Domain;

namespace ProductHub.Api.Features.Products;

public sealed class ProductService(
    AppDbContext dbContext,
    ILogger<ProductService> logger)
    : IProductService
{
    // Phương thức GetAllAsync sẽ truy vấn danh sách sản phẩm từ cơ sở dữ liệu dựa trên các tiêu chí trong ProductQuery, bao gồm tìm kiếm theo tên hoặc SKU, lọc theo categoryId, sắp xếp theo giá hoặc tên, và phân trang. Kết quả trả về là một PagedResult<ProductResponse> chứa danh sách sản phẩm và thông tin phân trang.
    public async Task<PagedResult<ProductResponse>>
        GetAllAsync(
            ProductQuery query,
            CancellationToken cancellationToken)
    {
        var productsQuery = dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            productsQuery = productsQuery.Where(x =>
                EF.Functions.ILike(
                    x.Name,
                    $"%{search}%") ||
                EF.Functions.ILike(
                    x.Sku,
                    $"%{search}%"));
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(x =>
                x.CategoryId == query.CategoryId.Value);
        }

        productsQuery = query.Sort switch
        {
            "price_asc" =>
                productsQuery.OrderBy(x => x.Price),

            "price_desc" =>
                productsQuery.OrderByDescending(
                    x => x.Price),

            "name_desc" =>
                productsQuery.OrderByDescending(
                    x => x.Name),

            _ =>
                productsQuery.OrderBy(x => x.Name)
        };

        var totalCount =
            await productsQuery.CountAsync(
                cancellationToken);

        var items = await productsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProductResponse(
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

        return new PagedResult<ProductResponse>(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }
    // Phương thức GetByIdAsync sẽ tìm kiếm sản phẩm theo id, nếu tìm thấy thì trả về ProductResponse, nếu không tìm thấy thì trả về null.
    public async Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductResponse(
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
    // Phương thức CreateAsync sẽ kiểm tra xem categoryId có tồn tại hay không, nếu không tồn tại thì ném ra NotFoundException. Sau đó kiểm tra xem SKU đã tồn tại hay chưa, nếu đã tồn tại thì ném ra ConflictException. Nếu tất cả đều hợp lệ, nó sẽ tạo một đối tượng Product mới, thêm vào DbContext và lưu thay đổi vào cơ sở dữ liệu. Cuối cùng ghi log thông tin và trả về ProductResponse của sản phẩm vừa tạo.
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var normalizedSku = request.Sku
            .Trim()
            .ToUpperInvariant();

        var categoryExists =
            await dbContext.Categories
                .AnyAsync(
                    x => x.Id == request.CategoryId,
                    cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException(
                "Category was not found.");
        }

        var skuExists =
            await dbContext.Products
                .AnyAsync(
                    x => x.Sku == normalizedSku,
                    cancellationToken);

        if (skuExists)
        {
            throw new ConflictException(
                "SKU already exists.");
        }

        var product = new Product
        {
            Name = normalizedName,
            Sku = normalizedSku,
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Products.Add(product);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsUniqueViolation(exception))
        {
            throw new ConflictException(
                "SKU already exists.");
        }

        logger.LogInformation(
            "Product {ProductId} with SKU {Sku} was created.",
            product.Id,
            product.Sku);

        return await GetByIdAsync(
            product.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Created product could not be loaded.");
    }
    // Phương thức UpdateAsync sẽ tìm kiếm sản phẩm theo id, nếu không tìm thấy thì trả về false. Nếu tìm thấy, nó sẽ kiểm tra xem categoryId mới có tồn tại hay không, nếu không tồn tại thì ném ra NotFoundException. Sau đó cập nhật các thuộc tính của sản phẩm và lưu thay đổi vào cơ sở dữ liệu. Cuối cùng ghi log thông tin và trả về true để xác nhận rằng sản phẩm đã được cập nhật thành công.
    public async Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        var categoryExists =
            await dbContext.Categories
                .AnyAsync(
                    x => x.Id == request.CategoryId,
                    cancellationToken);

        if (!categoryExists)
        {
            throw new NotFoundException(
                "Category was not found.");
        }

        product.Name = request.Name.Trim();
        product.Description =
            request.Description?.Trim();
        product.Price = request.Price;
        product.StockQuantity =
            request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Product {ProductId} was updated.",
            product.Id);

        return true;
    }
    // Phương thức DeactivateAsync sẽ tìm kiếm sản phẩm theo id, nếu không tìm thấy thì trả về false. Nếu sản phẩm đã bị vô hiệu hóa, trả về true. Nếu sản phẩm còn hoạt động, nó sẽ đặt IsActive thành false, cập nhật thời gian UpdatedAtUtc, lưu thay đổi vào cơ sở dữ liệu và ghi log thông tin. Cuối cùng trả về true để xác nhận rằng sản phẩm đã được vô hiệu hóa thành công.
    public async Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        if (!product.IsActive)
        {
            return true;
        }

        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Product {ProductId} was deactivated.",
            product.Id);

        return true;
    }
    // Phương thức này kiểm tra xem một ngoại lệ DbUpdateException có phải là do vi phạm ràng buộc duy nhất trong cơ sở dữ liệu hay không.
    private static bool IsUniqueViolation(
        DbUpdateException exception)
    {
        return exception.InnerException
            is PostgresException
            {
                SqlState:
                    PostgresErrorCodes.UniqueViolation
            };
    }
}
// service này sẽ kế thừa interface IProductService, và implement các phương thức để xử lý nghiệp vụ liên quan đến sản phẩm, như GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeactivateAsync. Service này sẽ tương tác với database thông qua AppDbContext để thực hiện các thao tác CRUD trên bảng Products.