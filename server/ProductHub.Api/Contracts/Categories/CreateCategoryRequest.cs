// Các attribute [Required], [StringLength] được [ApiController] dùng để tự trả HTTP 400 khi request body sai.
using System.ComponentModel.DataAnnotations;
// Dùng constant từ Domain để API validation không lệch với Entity và database mapping.
using ProductHub.Domain.Entities;

namespace ProductHub.Api.Contracts.Categories;

// File này là HTTP request contract của POST /api/categories.
// Nó chỉ thuộc API layer; controller sẽ map sang CreateCategoryCommand để Application không phụ thuộc ASP.NET Core.
public sealed class CreateCategoryRequest
{
    // Required chặn null/""; StringLength giới hạn input từ client trước khi gọi use case.
    [Required]
    [StringLength(Category.MaxNameLength, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}
