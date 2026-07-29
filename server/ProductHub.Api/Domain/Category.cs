namespace ProductHub.Api.Domain;

public sealed class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    
    // mối quan hệ 1-n với Product
    public ICollection<Product> Products { get; set; }
        = new List<Product>();
}