// ControllerBase, ActionResult và HTTP attributes của ASP.NET Core MVC.
using Microsoft.AspNetCore.Mvc;
// HTTP input models chỉ thuộc API layer.
using ProductHub.Api.Contracts.Categories;
// Use case contract, command và DTO thuộc Application layer.
using ProductHub.Application.Categories;

namespace ProductHub.Api.Controllers;

// File này là HTTP adapter cho Category.
// Luồng mỗi action: HTTP request -> model binding/validation -> map Request sang Command -> gọi Application service -> trả HTTP success.
// Exception không bắt ở controller; GlobalExceptionHandler xử lý 404/409/400 theo một format chung.
[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categoryService)
    : ControllerBase
{
    // GET /api/categories: chỉ đọc list DTO; không có business logic trong controller.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);
        return Ok(categories);
    }

    // GET /api/categories/{id}: Application ném NotFoundException nếu Id không có, handler trả 404.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(id, cancellationToken);
        return Ok(category);
    }

    // POST /api/categories: request body được ApiController validate trước khi method chạy.
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        // Map HTTP request thành application command để Application không reference class Request của API.
        var category = await categoryService.CreateAsync(
            new CreateCategoryCommand(request.Name),
            cancellationToken);

        // 201 Created + Location trỏ tới GET /api/categories/{id} vừa được tạo.
        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category);
    }

    // PUT /api/categories/{id}: chỉ map input và gọi use case; business rule nằm ở CategoryService.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await categoryService.UpdateAsync(
            new UpdateCategoryCommand(id, request.Name),
            cancellationToken);

        return NoContent();
    }

    // DELETE /api/categories/{id}: service chặn Category còn Product và handler đổi lỗi thành HTTP 409.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
