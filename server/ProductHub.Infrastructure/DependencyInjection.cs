// UseNpgsql là extension method EF Core để kết nối PostgreSQL.
using Microsoft.EntityFrameworkCore;
// IConfiguration đọc appsettings, User Secrets, environment variables.
using Microsoft.Extensions.Configuration;
// IServiceCollection là DI container được API truyền vào lúc startup.
using Microsoft.Extensions.DependencyInjection;
// Các port của Application cần được gắn với implementation thật ở Infrastructure.
using ProductHub.Application.Abstractions.Persistence;
using ProductHub.Application.Abstractions.Time;
using ProductHub.Infrastructure.Persistence;
using ProductHub.Infrastructure.Persistence.Repositories;
using ProductHub.Infrastructure.Time;

namespace ProductHub.Infrastructure;

// File này là nơi đăng ký dependency cho toàn bộ Infrastructure.
// Program.cs của API chỉ gọi AddInfrastructure; API không cần biết từng repository EF Core được tạo ra thế nào.
public static class DependencyInjection
{
    // Đọc connection string -> cấu hình DbContext/Npgsql -> map Application ports sang implementation EF -> trả IServiceCollection.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fail fast lúc startup nếu cấu hình thiếu, thay vì chờ request đầu tiên mới lỗi kết nối database.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is missing.");

        // AddDbContext đăng ký scoped mặc định: mỗi HTTP request dùng một AppDbContext/change tracker riêng.
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Một request: repository và UnitOfWork dùng cùng AppDbContext scoped để một SaveChanges commit mọi thay đổi.
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();

        // Đăng ký concrete một lần, sau đó hai port command/read cùng resolve đúng instance scoped này.
        // Nhờ vậy nếu một use case cần cả IProductRepository lẫn IProductReadRepository, chúng vẫn cùng DbContext.
        services.AddScoped<EfProductRepository>();
        services.AddScoped<IProductRepository>(provider =>
            provider.GetRequiredService<EfProductRepository>());
        services.AddScoped<IProductReadRepository>(provider =>
            provider.GetRequiredService<EfProductRepository>());
        // SystemClock không giữ state nên singleton an toàn. Test có thể thay registration bằng fake clock.
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
