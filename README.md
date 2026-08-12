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

## Checkpoint 01 - Gia cố Catalog hiện tại

Checkpoint đầu tiên được triển khai trước Auth, Variant và Order để giữ phạm vi thay đổi nhỏ, dễ review:

- `Category` lưu thêm `NormalizedName` (`trim + uppercase invariant`) và unique index. Vì vậy `Laptop`, ` laptop ` và `LAPTOP` là một danh mục.
- `Product`/`Category` tự trim, kiểm tra độ dài và invariant ngay trong Domain Entity; API và EF Core dùng chung các hằng số độ dài đó.
- PostgreSQL có thêm `CHECK` cho tên/SKU không rỗng, giá lớn hơn 0 và tồn kho không âm. Đây là hàng rào khi import script ghi thẳng vào database.
- Có endpoint `GET /health/live`; endpoint này chỉ kiểm tra process API còn sống, chưa kiểm tra kết nối PostgreSQL.

Luồng tạo/sửa Category sau checkpoint:

```text
Request Name
  → Category.NormalizeName (trim + validate)
  → Application kiểm tra ExistsByNormalizedNameAsync để trả lỗi rõ
  → Category.Create/Rename cập nhật Name + NormalizedName
  → EF Core SaveChanges
  → PostgreSQL unique index NormalizedName chống race condition
```

Trước khi chạy migration mới trên database có dữ liệu, kiểm tra duplicate không phân biệt hoa/thường. Nếu query trả về dòng nào, hãy gộp/đổi tên dữ liệu đó trước; migration sẽ chủ động dừng để không tạo index nửa chừng.

```sql
SELECT upper(btrim("Name")) AS normalized_name, COUNT(*)
FROM categories
GROUP BY upper(btrim("Name"))
HAVING COUNT(*) > 1;
```

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

Migration `20260812000000_AddCatalogIntegrity` là migration của Checkpoint 01. Sau khi chạy `database update`, khởi động API và gọi:

```http
GET /health/live
```

Kết quả `200 Healthy` cho biết process API hoạt động. Nó không thay thế bài kiểm tra endpoint Catalog hoặc kết nối PostgreSQL; readiness check database sẽ được thêm ở phần Docker/deployment.
