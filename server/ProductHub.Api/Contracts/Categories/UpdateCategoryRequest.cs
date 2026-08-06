// Data annotations được ASP.NET Core chạy tự động nhờ [ApiController].
using System.ComponentModel.DataAnnotations;

namespace ProductHub.Api.Contracts.Categories;

// HTTP request contract của PUT /api/categories/{id}.
// PUT hiện thay toàn bộ các field Category có thể sửa; hiện tại chỉ có Name.
public sealed class UpdateCategoryRequest
{
    // Validation HTTP ban đầu; Domain vẫn kiểm tra lại whitespace để không phụ thuộc duy nhất vào API.
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}
