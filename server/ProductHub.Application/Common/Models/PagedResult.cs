namespace ProductHub.Application.Common.Models;

// Model phân trang độc lập transport: hôm nay API HTTP serialize nó, sau này gRPC/worker vẫn tái dùng được.
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    // Tính số trang từ tổng bản ghi. Nếu không có bản ghi thì trả 0, tránh chia cho 0.
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
