namespace OrderSystem.DTOs;

public class SalesQueryParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string GroupBy { get; set; } = "month";
}

public record SalesPeriodResult(string Period, int OrderCount, decimal TotalRevenue);
