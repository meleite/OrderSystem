using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.DTOs;
using OrderSystem.Models;

namespace OrderSystem.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams q)
    {
        var query = _db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p =>
                p.Name.Contains(q.Search) ||
                (p.Description != null && p.Description.Contains(q.Search)));

        if (!string.IsNullOrWhiteSpace(q.Category))
            query = query.Where(p => p.Category == q.Category);

        if (q.MinPrice.HasValue)
            query = query.Where(p => p.Price >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= q.MaxPrice.Value);

        var total = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Description, p.Price,
                p.StockQuantity, p.Category, p.IsActive, p.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<ProductResponse>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null || !p.IsActive) return NotFound();

        return Ok(new ProductResponse(
            p.Id, p.Name, p.Description, p.Price,
            p.StockQuantity, p.Category, p.IsActive, p.CreatedAt));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            StockQuantity = req.StockQuantity,
            Category = req.Category,
            IsActive = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            new ProductResponse(
                product.Id, product.Name, product.Description, product.Price,
                product.StockQuantity, product.Category, product.IsActive, product.CreatedAt));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductRequest req)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.StockQuantity = req.StockQuantity;
        product.Category = req.Category;

        await _db.SaveChangesAsync();

        return Ok(new ProductResponse(
            product.Id, product.Name, product.Description, product.Price,
            product.StockQuantity, product.Category, product.IsActive, product.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
