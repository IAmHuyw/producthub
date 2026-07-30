using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductHub.Api.Common.Exceptions;
using ProductHub.Api.Data;
using ProductHub.Api.Domain;

namespace ProductHub.Api.Features.Categories;

public sealed class CategoryService(
    AppDbContext dbContext)
    : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryResponse(
                x.Id,
                x.Name,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var exists = await dbContext.Categories
            .AnyAsync(
                x => x.Name.ToLower() ==
                     name.ToLower(),
                cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                "Category name already exists.");
        }

        var category = new Category
        {
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Categories.Add(category);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException
                  is PostgresException
                  {
                      SqlState:
                          PostgresErrorCodes
                              .UniqueViolation
                  })
        {
            throw new ConflictException(
                "Category name already exists.");
        }

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.CreatedAtUtc);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category =
            await dbContext.Categories
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (category is null)
        {
            return false;
        }

        var name = request.Name.Trim();

        var duplicate =
            await dbContext.Categories
                .AnyAsync(
                    x => x.Id != id &&
                         x.Name.ToLower() ==
                         name.ToLower(),
                    cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                "Category name already exists.");
        }

        category.Name = name;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category =
            await dbContext.Categories
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (category is null)
        {
            return false;
        }

        var hasProducts =
            await dbContext.Products
                .AnyAsync(
                    x => x.CategoryId == id,
                    cancellationToken);

        if (hasProducts)
        {
            throw new BusinessRuleException(
                "A category containing products cannot be deleted.");
        }

        dbContext.Categories.Remove(category);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}