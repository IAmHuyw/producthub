# ProductHub

Backend được cấu trúc theo Clean Architecture, vẫn giữ các endpoint REST hiện có:

```text
ProductHub.Api → ProductHub.Application → ProductHub.Domain
ProductHub.Api → ProductHub.Infrastructure → ProductHub.Application → ProductHub.Domain
```

## Vai trò từng project

- `ProductHub.Domain`: entity và invariant cốt lõi (`Product`, `Category`); không phụ thuộc EF Core, ASP.NET Core hay PostgreSQL.
- `ProductHub.Application`: use case, DTO trả về, command và abstraction/port cho persistence, clock.
- `ProductHub.Infrastructure`: EF Core `AppDbContext`, entity configuration, migration, repository PostgreSQL và implementation của các port.
- `ProductHub.Api`: HTTP contracts, controllers, CORS, exception handler và đăng ký dependency injection.

Controller không truy cập `DbContext`. Application không biết EF Core. Chỉ `Infrastructure` được phép dùng EF Core/Npgsql.

## Các luồng chính

```text
HTTP request
  → API Controller (bind + validate request)
  → Application service (use case + business rule)
  → Repository interface / Unit of Work port
  → Infrastructure EF Core adapter
  → PostgreSQL
```

Các lỗi nghiệp vụ (`NotFoundException`, `ConflictException`, `BusinessRuleException`) được `GlobalExceptionHandler` ở API đổi sang chuẩn `ProblemDetails` HTTP 404/409. Do đó controller chỉ xử lý response thành công.

## Chạy local

1. Khởi động PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Cấu hình connection string bằng User Secrets hoặc environment variable. Ví dụ local:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=producthub;Username=producthub;Password=123456" --project server/ProductHub.Api
   ```

3. Chạy API:

   ```bash
   dotnet run --project server/ProductHub.Api
   ```

## EF Core migration

`DbContext` và migration hiện nằm trong Infrastructure, vì vậy hãy chỉ định Infrastructure là project migration và API là startup project:

```bash
dotnet ef migrations add <MigrationName> \
  --project server/ProductHub.Infrastructure \
  --startup-project server/ProductHub.Api

dotnet ef database update \
  --project server/ProductHub.Infrastructure \
  --startup-project server/ProductHub.Api
```

Không sửa migration đã được dùng trên database chung/production. Hãy đổi configuration rồi tạo migration mới.
