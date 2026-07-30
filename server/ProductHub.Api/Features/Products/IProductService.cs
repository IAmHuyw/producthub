namespace ProductHub.Api.Features.Products;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(
        ProductQuery query,
        CancellationToken cancellationToken);

    Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken);
}