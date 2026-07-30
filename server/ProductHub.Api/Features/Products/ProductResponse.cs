namespace ProductHub.Api.Features.Products;

// không cho kiểu khác kế thừa
public sealed record ProductResponse(
    int Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

//Cái này là class dùng để trả về thông tin sản phẩm, có các thuộc tính như Id, Name, Sku, Description, Price, StockQuantity, IsActive, CategoryId, CategoryName và CreatedAtUtc.