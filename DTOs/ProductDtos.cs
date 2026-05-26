using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs;

public record ProductRequest(
    [property: Required, MaxLength(200)] string Name,
    string? Description,
    [property: Range(0.01, 1_000_000_000.0)] decimal Price,
    [property: Range(0, int.MaxValue)] int StockQuantity,
    string? Category
);

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Category,
    bool IsActive,
    DateTime CreatedAt
);

public class ProductQueryParams
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
