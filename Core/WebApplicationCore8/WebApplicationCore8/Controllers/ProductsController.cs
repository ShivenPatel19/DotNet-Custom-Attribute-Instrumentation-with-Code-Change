using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplicationCore8.Data;
using WebApplicationCore8.Models;

namespace WebApplicationCore8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Get the current active span created by auto-instrumentation
            var currentActivity = Activity.Current;

            // Add custom business attributes
            currentActivity?.SetTag("apm.operation", "list_products");
            currentActivity?.SetTag("apm.user_id", User?.Identity?.Name ?? "anonymous");
            currentActivity?.SetTag("apm.endpoint", "GetProducts");

            var products = await _context.Products.ToListAsync();

            // Add result attributes
            currentActivity?.SetTag("apm.result_count", products.Count);
            currentActivity?.SetTag("apm.result", "success");

            return products;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // Get the current active span created by auto-instrumentation
            var currentActivity = Activity.Current;

            // Add custom business attributes
            currentActivity?.SetTag("apm.operation", "get_product");
            currentActivity?.SetTag("apm.product_id", id);
            currentActivity?.SetTag("apm.user_id", User?.Identity?.Name ?? "anonymous");
            currentActivity?.SetTag("apm.endpoint", "GetProduct");

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                currentActivity?.SetTag("apm.result", "not_found");
                currentActivity?.SetTag("apm.error_reason", "product_not_found");
                return NotFound();
            }

            // Add product-specific attributes
            currentActivity?.SetTag("apm.result", "success");
            currentActivity?.SetTag("apm.product_name", product.Name);
            currentActivity?.SetTag("apm.product_price", product.Price);
            currentActivity?.SetTag("apm.product_available", product.IsAvailable);
            currentActivity?.SetTag("apm.product_quantity", product.Quantity);

            return product;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            // Get the current active span created by auto-instrumentation
            var currentActivity = Activity.Current;

            // Add custom business attributes
            currentActivity?.SetTag("apm.operation", "update_product");
            currentActivity?.SetTag("apm.product_id", id);
            currentActivity?.SetTag("apm.user_id", User?.Identity?.Name ?? "anonymous");
            currentActivity?.SetTag("apm.endpoint", "PutProduct");

            if (id != product.Id)
            {
                currentActivity?.SetTag("apm.result", "bad_request");
                currentActivity?.SetTag("apm.error_reason", "id_mismatch");
                return BadRequest();
            }

            // Add product update details
            currentActivity?.SetTag("apm.product_name", product.Name);
            currentActivity?.SetTag("apm.product_price", product.Price);
            currentActivity?.SetTag("apm.product_quantity", product.Quantity);

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                currentActivity?.SetTag("apm.result", "success");
                currentActivity?.SetTag("apm.status", "updated");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                {
                    currentActivity?.SetTag("apm.result", "not_found");
                    currentActivity?.SetTag("apm.error_reason", "product_not_found");
                    return NotFound();
                }
                else
                {
                    currentActivity?.SetTag("apm.result", "error");
                    currentActivity?.SetTag("apm.error_type", "concurrency_exception");
                    currentActivity?.SetTag("apm.error_message", ex.Message);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // Get the current active span created by auto-instrumentation
            var currentActivity = Activity.Current;

            // Add custom business attributes
            currentActivity?.SetTag("apm.operation", "create_product");
            currentActivity?.SetTag("apm.user_id", User?.Identity?.Name ?? "anonymous");
            currentActivity?.SetTag("apm.endpoint", "PostProduct");
            currentActivity?.SetTag("apm.product_name", product.Name);
            currentActivity?.SetTag("apm.product_price", product.Price);
            currentActivity?.SetTag("apm.product_quantity", product.Quantity);
            currentActivity?.SetTag("apm.product_available", product.IsAvailable);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Add result attributes
            currentActivity?.SetTag("apm.product_id", product.Id);
            currentActivity?.SetTag("apm.result", "success");
            currentActivity?.SetTag("apm.status", "created");

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Get the current active span created by auto-instrumentation
            var currentActivity = Activity.Current;

            // Add custom business attributes
            currentActivity?.SetTag("apm.operation", "delete_product");
            currentActivity?.SetTag("apm.product_id", id);
            currentActivity?.SetTag("apm.user_id", User?.Identity?.Name ?? "anonymous");
            currentActivity?.SetTag("apm.endpoint", "DeleteProduct");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                currentActivity?.SetTag("apm.result", "not_found");
                currentActivity?.SetTag("apm.error_reason", "product_not_found");
                return NotFound();
            }

            // Add product details before deletion
            currentActivity?.SetTag("apm.product_name", product.Name);
            currentActivity?.SetTag("apm.product_price", product.Price);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Add result attributes
            currentActivity?.SetTag("apm.result", "success");
            currentActivity?.SetTag("apm.status", "deleted");

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
