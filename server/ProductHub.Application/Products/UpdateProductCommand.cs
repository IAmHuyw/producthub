namespace ProductHub.Application.Products;

// Command diễn tả các field được phép sửa của Product. SKU không nằm ở đây vì đang là immutable.
public sealed record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);
