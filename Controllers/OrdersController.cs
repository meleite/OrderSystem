using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.DTOs;
using OrderSystem.Models;

namespace OrderSystem.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    private int CurrentUserId =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest req)
    {
        if (req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "Order must contain at least one item." });

        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missingIds.Count > 0)
            return BadRequest(new { message = $"Products not found or inactive: {string.Join(", ", missingIds)}." });

        var stockErrors = req.Items
            .Where(i => products.First(p => p.Id == i.ProductId).StockQuantity < i.Quantity)
            .Select(i => $"Product {i.ProductId} has insufficient stock.")
            .ToList();

        if (stockErrors.Count > 0)
            return UnprocessableEntity(new { message = "Stock validation failed.", errors = stockErrors });

        var order = new Order
        {
            UserId = CurrentUserId,
            Status = OrderStatus.Pending
        };

        var productMap = products.ToDictionary(p => p.Id);
        foreach (var item in req.Items)
        {
            var product = productMap[item.ProductId];
            product.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = product.Price * item.Quantity
            });
        }

        order.RecalculateTotal();
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            new OrderResponse(
                order.Id, order.UserId, order.Status.ToString(),
                order.TotalPrice, order.CreatedAt,
                order.Items.Select(i => new OrderItemResponse(
                    i.Id, i.ProductId, productMap[i.ProductId].Name,
                    i.UnitPrice, i.Quantity, i.LineTotal))));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = CurrentUserId;
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToOrderResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        if (!IsAdmin && order.UserId != CurrentUserId) return Forbid();

        return Ok(ToOrderResponse(order));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest req)
    {
        if (!Enum.TryParse<OrderStatus>(req.Status, ignoreCase: true, out var newStatus))
            return BadRequest(new { message = $"Invalid status '{req.Status}'. Valid values: {string.Join(", ", Enum.GetNames<OrderStatus>())}." });

        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();

        var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Pending]    = [OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped]    = [OrderStatus.Delivered, OrderStatus.Cancelled],
            [OrderStatus.Delivered]  = [],
            [OrderStatus.Cancelled]  = []
        };

        if (!validTransitions[order.Status].Contains(newStatus))
            return UnprocessableEntity(new { message = $"Cannot transition from '{order.Status}' to '{newStatus}'." });

        order.Status = newStatus;
        await _db.SaveChangesAsync();

        return Ok(new { id = order.Id, status = order.Status.ToString() });
    }

    private static OrderResponse ToOrderResponse(Order order) => new(
        order.Id, order.UserId, order.Status.ToString(),
        order.TotalPrice, order.CreatedAt,
        order.Items.Select(i => new OrderItemResponse(
            i.Id, i.ProductId, i.Product?.Name ?? string.Empty,
            i.UnitPrice, i.Quantity, i.LineTotal)));
}
