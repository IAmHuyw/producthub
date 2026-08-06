// ASP.NET Core MVC types và attributes routing/model binding.
using Microsoft.AspNetCore.Mvc;
// HTTP request/query contracts.
using ProductHub.Api.Contracts.Products;
// Application output/input cho Product use case.
using ProductHub.Application.Common.Models;
using ProductHub.Application.Products;

namespace ProductHub.Api.Controllers;

// HTTP adapter của Product.
// Controller tuyệt đối không truy cập AppDbContext/repository; trách nhiệm của nó chỉ là HTTP <-> Application mapping.
[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService productService)
    : ControllerBase
{
    // GET /api/products?page=1&pageSize=10&search=&categoryId=&sort=name_asc.
    // [FromQuery] yêu cầu ASP.NET Core bind query string vào ProductQuery thay vì request body.
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] ProductQuery query,
        CancellationToken cancellationToken)
    {
        // Chuyển contract của HTTP sang transport-neutral ProductListQuery trước khi gọi Application.
        var result = await productService.GetAllAsync(
            new ProductListQuery(
                query.Page,
                query.PageSize,
                query.Search,
                query.CategoryId,
                query.Sort),
            cancellationToken);

        return Ok(result);
    }

    // GET /api/products/{id}: service quyết định policy inactive và ném 404 nếu không có.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    // POST /api/products: ApiController validate DataAnnotations rồi controller map request thành command.
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(
            new CreateProductCommand(
                request.Name,
                request.Sku,
                request.Description,
                request.Price,
                request.StockQuantity,
                request.CategoryId),
            cancellationToken);

        // CreatedAtAction tạo HTTP 201, response body và header Location tới GET-by-id.
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /api/products/{id}: endpoint full update cho các field editable, SKU không đổi.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await productService.UpdateAsync(
            new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity,
                request.CategoryId),
            cancellationToken);

        return NoContent();
    }

    // DELETE /api/products/{id}: đây là soft delete, service chuyển IsActive=false và trả 204.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        await productService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
