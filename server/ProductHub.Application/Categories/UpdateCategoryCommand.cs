namespace ProductHub.Application.Categories;

// Command diễn tả ý định đổi tên Category có Id tương ứng.
public sealed record UpdateCategoryCommand(int Id, string Name);
