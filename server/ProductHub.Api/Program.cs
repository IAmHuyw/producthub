// Exception handler là API concern, chịu trách nhiệm đổi exception bên trong thành HTTP response.
using ProductHub.Api.ExceptionHandling;
// Các interface/implementation use case được API đăng ký vào DI container.
using ProductHub.Application.Categories;
using ProductHub.Application.Products;
// Extension AddInfrastructure đăng ký EF Core, PostgreSQL và repository implementation.
using ProductHub.Infrastructure;

// File này là composition root - điểm lắp ghép duy nhất của ứng dụng.
// Chỉ API được phép reference đồng thời Application (use case) và Infrastructure (EF Core/PostgreSQL).
// Các layer bên trong không được dependency ngược về API.
var builder = WebApplication.CreateBuilder(args);

// Bật Controller-based API và cơ chế model binding/validation theo [ApiController].
builder.Services.AddControllers();
// Sinh OpenAPI document; chỉ map endpoint document trong Development ở phía dưới.
builder.Services.AddOpenApi();

// Đăng ký lỗi tập trung để NotFound/Conflict/Domain exception có ProblemDetails thống nhất.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Đăng ký Application service scoped: mỗi HTTP request nhận service riêng.
// Các dependency persistence của service là interface; Infrastructure sẽ cung cấp implementation thật.
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Đăng ký AppDbContext/Npgsql, repository, UnitOfWork và SystemClock từ Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

// CORS chỉ là chính sách an toàn của trình duyệt, không phải authentication/authorization.
// FrontendUrl phải cấu hình theo môi trường, không hard-code origin production trong source.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        // Đọc từ appsettings, User Secrets hoặc environment variables theo configuration provider mặc định.
        var frontendUrl = builder.Configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl is missing.");

        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Không public OpenAPI endpoint ở production theo cấu hình hiện tại.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Thứ tự middleware là một luồng từ trên xuống:
// Exception handler bắt lỗi của middleware/controller phía sau -> HTTPS redirect -> CORS -> routing/controller.
// Đặt exception handler sớm để lỗi được chuẩn hóa trước khi response bị ghi.
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("ReactClient");
app.MapControllers();

app.Run();

// Cho phép test project dùng WebApplicationFactory<Program> để khởi động API in-memory khi viết integration test.
public partial class Program;
