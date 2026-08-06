namespace ProductHub.Application.Abstractions.Persistence;

// Unit of Work là port chịu trách nhiệm commit các thay đổi đang được repository theo dõi.
// Nhờ abstraction này, Application không cần gọi DbContext.SaveChangesAsync trực tiếp.
public interface IUnitOfWork
{
    // Commit INSERT/UPDATE/DELETE; cancellationToken được truyền xuyên từ HTTP request để hủy sớm khi client ngắt kết nối.
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
