namespace ProductHub.Api.Features.Products;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount / (double)PageSize);
}
//Đây là một class dùng để trả về kết quả phân trang cho các truy vấn sản phẩm.
//Nó chứa danh sách các mục (Items), số trang hiện tại (Page), kích thước trang (PageSize) và tổng số lượng mục (TotalCount).
//Ngoài ra, nó còn có một thuộc tính TotalPages để tính toán tổng số trang dựa trên tổng số lượng mục và kích thước trang.