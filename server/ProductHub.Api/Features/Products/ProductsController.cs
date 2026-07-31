using Microsoft.AspNetCore.Mvc;

namespace ProductHub.Api.Features.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService productService)
    : ControllerBase
{
    // async/await là cơ chế bất đồng bộ trong C# giúp xử lý các tác vụ tốn thời gian (như truy vấn database, gọi API) mà không làm block luồng chính.
    // Khi một phương thức được đánh dấu là async, nó có thể sử dụng từ khóa await để chờ kết quả của một tác vụ mà không làm treo ứng dụng. 
    // Điều này giúp cải thiện hiệu suất và khả năng phản hồi của ứng dụng, đặc biệt là trong các ứng dụng web nơi có nhiều yêu cầu đồng thời.
    // ActionResult<T> là một kiểu trả về trong ASP.NET Core, cho phép controller trả về dữ liệu cùng với mã trạng thái HTTP.
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