namespace ProductHub.Application.Products;

// DTO/read model Product do Application trả về.
// API có thể serialize trực tiếp mà không để client thấy Entity đang được EF Core theo dõi.
public sealed record ProductDto(
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
