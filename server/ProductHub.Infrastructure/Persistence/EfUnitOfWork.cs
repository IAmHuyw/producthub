// DbUpdateException là lỗi EF Core trả khi database từ chối SaveChanges.
using Microsoft.EntityFrameworkCore;
// PostgresException và error code giúp nhận diện đúng loại constraint của PostgreSQL.
using Npgsql;
// IUnitOfWork/ConflictException là contract của Application mà adapter này phải thực thi.
using ProductHub.Application.Abstractions.Persistence;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Infrastructure.Persistence;

// Adapter Unit of Work của EF Core.
// Nhiệm vụ: gọi SaveChangesAsync và che giấu lỗi PostgreSQL khỏi Application bằng cách đổi sang ConflictException.
public sealed class EfUnitOfWork(AppDbContext dbContext)
    : IUnitOfWork
{
    // Commit toàn bộ Entity đang được AppDbContext track trong request hiện tại.
    // Input cancellationToken được truyền xuống EF/Npgsql để dừng query khi request bị hủy.
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Đây là điểm INSERT/UPDATE/DELETE thực sự được gửi xuống database.
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgresException &&
                  postgresException.SqlState is PostgresErrorCodes.UniqueViolation or
                      PostgresErrorCodes.ForeignKeyViolation)
        {
            // Check trước ở Application chỉ tốt cho UX, không an toàn tuyệt đối khi hai request chạy đồng thời.
            // Đây là lớp cuối: unique/FK violation của PostgreSQL được đổi thành lỗi Application HTTP 409.
            throw new ConflictException(
                "The requested change conflicts with existing data.");
        }
    }
}
