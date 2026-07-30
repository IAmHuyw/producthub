using Microsoft.AspNetCore.Mvc;

namespace ProductHub.Api.Features.Categories;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(
    ICategoryService categoryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<CategoryResponse>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        var categories =
            await categoryService.GetAllAsync(
                cancellationToken);

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>>
        Create(
            CreateCategoryRequest request,
            CancellationToken cancellationToken)
    {
        var category =
            await categoryService.CreateAsync(
                request,
                cancellationToken);

        return Created(
            $"/api/categories/{category.Id}",
            category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var updated =
            await categoryService.UpdateAsync(
                id,
                request,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted =
            await categoryService.DeleteAsync(
                id,
                cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}