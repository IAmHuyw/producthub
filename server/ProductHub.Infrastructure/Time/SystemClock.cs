// IClock là port do Application định nghĩa; Infrastructure cung cấp implementation production.
using ProductHub.Application.Abstractions.Time;

namespace ProductHub.Infrastructure.Time;

// Clock thật của production. Test có thể thay class này bằng fake clock để assert timestamp cố định.
public sealed class SystemClock : IClock
{
    // Không dùng DateTime.Now vì UTC tránh lỗi khác múi giờ/DST trong audit data.
    public DateTime UtcNow => DateTime.UtcNow;
}
