namespace ProductHub.Application.Products;

// Command diễn tả toàn bộ input cần thiết để tạo Product.
public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);
