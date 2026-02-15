using BasicProductApi.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BasicProductApi.API.Controllers;

[Route("api/products/")]
[ApiController]
public class ProductsController(ShopContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllProducts()
    {
        return Ok(await context.Products.ToArrayAsync());
    }
        
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetProductById([FromRoute] int id)
    {
        var result = await context.Products.FindAsync(id);
        if (result is null)
            return NotFound();
        return Ok(result);
    }
        
    [HttpGet("available")]
    public async Task<ActionResult> GetAvailableProducts()
    {
        var result = await context.Products.Where(p => p.IsAvailable).ToArrayAsync();
        if (result.Length == 0)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> PostProduct([FromBody] Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutProductById(
        [FromRoute] int id,
        [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest();
        context.Entry(product).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!(await context.Products.AnyAsync(p => p.Id == id)))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProductById([FromRoute] int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        return Ok(product);
    }

    [HttpPost("delete")]
    public async Task<ActionResult> DeleteMultipleProductsByIds([FromQuery] params int[] ids)
    {
        List<Product> productsToDelete = [];
        foreach (var id in ids)
        {
            var product = await context.Products.FindAsync(id);
            if (product is null)
                return NotFound();
            productsToDelete.Add(product);
        }

        context.Products.RemoveRange(productsToDelete);

        await context.SaveChangesAsync();
        return Ok(productsToDelete);
    }
}