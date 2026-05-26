using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.DTOs;
using OrderSystem.Models;

namespace OrderSystem.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db) => _db = db;

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] SalesQueryParams q)
    {
        var groupBy = q.GroupBy?.ToLowerInvariant();
        if (groupBy != "month" && groupBy != "year")
            return BadRequest(new { message = "groupBy must be 'month' or 'year'." });

        var query = _db.Orders.Where(o => o.Status != OrderStatus.Cancelled);

        if (q.StartDate.HasValue)
            query = query.Where(o => o.CreatedAt >= q.StartDate.Value.Date);

        if (q.EndDate.HasValue)
            query = query.Where(o => o.CreatedAt < q.EndDate.Value.Date.AddDays(1));

        var orders = await query
            .Select(o => new { o.CreatedAt, o.TotalPrice })
            .ToListAsync();

        var results = groupBy == "year"
            ? orders
                .GroupBy(o => o.CreatedAt.Year)
                .OrderBy(g => g.Key)
                .Select(g => new SalesPeriodResult(
                    g.Key.ToString(), g.Count(), g.Sum(o => o.TotalPrice)))
            : orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new SalesPeriodResult(
                    $"{g.Key.Year}-{g.Key.Month:D2}", g.Count(), g.Sum(o => o.TotalPrice)));

        return Ok(results);
    }
}
