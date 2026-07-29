using Microsoft.EntityFrameworkCore;
using ProductHub.Api.Data;

// Vai trò: Tạo một đối tượng builder để thiết lập ứng dụng.
// Hoạt động: Tự động nạp toàn bộ cấu hình từ các file (appsettings.json), biến môi trường, và tham số dòng lệnh (args).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

/* 
Đăng ký AppDbContext với DI container, sử dụng PostgreSQL
Sau này controller hoặc service yêu cầu ví dụ: public productservice(AppDbContext dbContext) thì DI container sẽ inject vào, 
DI sẽ: 
1. Phát hiện constructor cần AppDbContext
2. Tìm registration
3. Tạo AppDbContext
4. Truyền vào constructor
5. Dùng instance đó trong request
6. Dispose khi request kết thúc
*/
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection");

    options.UseNpgsql(connectionString);
});


// Vai trò: Xử lý lỗi CORS (Cross-Origin Resource Sharing) cho Frontend.
// Hoạt động: Trình duyệt mặc định chặn website ở tên miền này (vd: localhost:3000 của React) gọi API ở tên miền khác (vd: localhost:5000 của .NET) để bảo mật.
// Đoạn này tạo 1 "Giấy phép" tên là "ReactClient" để vượt qua rào cản đó.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        var frontendUrl =
            builder.Configuration["FrontendUrl"]
            ?? throw new InvalidOperationException(
                "FrontendUrl is missing.");

        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Vai trò: Áp dụng giấy phép CORS đã viết ở trên vào thực tế.
// Hoạt động: Request từ React bay vào sẽ bị chặn lại kiểm tra giấy phép "ReactClient", đúng mới cho đi tiếp.
app.UseCors("ReactClient");

// Vai trò: Điều hướng URL (Routing).
// Hoạt động: Đọc URL của Request (ví dụ: GET /api/products) và dò tìm xem nó khớp với Controller và Action nào trong code để chạy.
app.MapControllers();

app.Run();

public partial class Program;