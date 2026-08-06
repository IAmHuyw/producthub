namespace ProductHub.Application.Categories;

// Command diễn tả ý định "tạo Category" của caller.
// API map CreateCategoryRequest sang command này; các adapter khác cũng có thể dùng command mà không phụ thuộc HTTP.
public sealed record CreateCategoryCommand(string Name);
