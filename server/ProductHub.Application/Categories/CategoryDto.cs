namespace ProductHub.Application.Categories;

// DTO/read model của Category do Application trả về.
// Nó không phải EF Entity và không phụ thuộc JSON/HTTP, nên không làm lộ persistence model ra bên ngoài.
public sealed record CategoryDto(
    int Id,
    string Name,
    DateTime CreatedAtUtc);
