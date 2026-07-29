using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Features.Categories;

public sealed class UpdateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}