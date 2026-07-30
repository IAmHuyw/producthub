using Microsoft.AspNetCore.Mvc;

namespace ProductHub.Api.Features.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService productService)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<PagedResult<ProductResponse>>>
        GetAll(
            [FromQuery] ProductQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await productService.GetAllAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>>
        GetById(
            int id,
            CancellationToken cancellationToken)
    {
        var product =
            await productService.GetByIdAsync(
                id,
                cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>>
        Create(
            CreateProductRequest request,
            CancellationToken cancellationToken)
    {
        var product =
            await productService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var updated =
            await productService.UpdateAsync(
                id,
                request,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var deactivated =
            await productService.DeactivateAsync(
                id,
                cancellationToken);

        return deactivated
            ? NoContent()
            : NotFound();
    }
}