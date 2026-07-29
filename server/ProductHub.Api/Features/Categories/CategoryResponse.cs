namespace ProductHub.Api.Features.Categories;

public sealed record CategoryResponse(
    int Id,
    string Name,
    DateTime CreatedAtUtc);