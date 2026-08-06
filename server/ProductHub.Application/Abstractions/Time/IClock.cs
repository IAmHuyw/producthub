namespace ProductHub.Application.Abstractions.Time;

// Clock là một port thời gian. Production dùng SystemClock, test có thể inject thời gian cố định.
// Điều này giúp test CreatedAtUtc/UpdatedAtUtc không phụ thuộc DateTime.UtcNow thật.
public interface IClock
{
    // Luôn trả UTC để audit timestamp không phụ thuộc múi giờ server.
    DateTime UtcNow { get; }
}
