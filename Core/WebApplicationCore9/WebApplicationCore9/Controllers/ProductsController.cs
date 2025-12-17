using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationCore9.Data;
using WebApplicationCore9.Models;
using WebApplicationCore9.Telemetry;

namespace WebApplicationCore9.Controllers
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
            // Create a custom span for business logic
            using (var activity = AppTelemetry.ActivitySource.StartActivity("GetAllProducts"))
            {
                activity?.SetTag("apm.operation", "list_products");
                activity?.SetTag("apm.entity", "Product");

                var products = await _context.Products.ToListAsync();

                // Add business metrics
                activity?.SetTag("apm.products_count", products.Count);
                activity?.SetTag("apm.status", "success");

                return products;
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // Create a custom span for business logic
            using (var activity = AppTelemetry.ActivitySource.StartActivity("GetProductById"))
            {
                activity?.SetTag("apm.operation", "get_product");
                activity?.SetTag("apm.entity", "Product");
                activity?.SetTag("apm.product_id", id);

                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    activity?.SetTag("apm.status", "not_found");
                    activity?.SetTag("apm.error", "Product not found");
                    return NotFound();
                }

                // Add product details to span
                activity?.SetTag("apm.product_name", product.Name);
                activity?.SetTag("apm.product_price", product.Price);
                activity?.SetTag("apm.product_quantity", product.Quantity);
                activity?.SetTag("apm.product_available", product.IsAvailable);
                activity?.SetTag("apm.status", "success");

                return product;
            }
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            // Create a custom span for business logic
            using (var activity = AppTelemetry.ActivitySource.StartActivity("UpdateProduct"))
            {
                activity?.SetTag("apm.operation", "update_product");
                activity?.SetTag("apm.entity", "Product");
                activity?.SetTag("apm.product_id", id);
                activity?.SetTag("apm.product_name", product.Name);
                activity?.SetTag("apm.product_price", product.Price);
                activity?.SetTag("apm.product_quantity", product.Quantity);

                if (id != product.Id)
                {
                    activity?.SetTag("apm.status", "bad_request");
                    activity?.SetTag("apm.error", "ID mismatch");
                    return BadRequest();
                }

                _context.Entry(product).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    activity?.SetTag("apm.status", "success");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        activity?.SetTag("apm.status", "not_found");
                        activity?.SetTag("apm.error", "Product not found during update");
                        return NotFound();
                    }
                    else
                    {
                        activity?.SetTag("apm.status", "error");
                        activity?.SetTag("apm.error", "Concurrency exception");
                        throw;
                    }
                }

                return NoContent();
            }
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            // Create a custom span for business logic
            using (var activity = AppTelemetry.ActivitySource.StartActivity("CreateProduct"))
            {
                // Add custom business attributes before creation
                activity?.SetTag("apm.operation", "create_product");
                activity?.SetTag("apm.entity", "Product");
                activity?.SetTag("apm.product_name", product.Name);
                activity?.SetTag("apm.product_price", product.Price);
                activity?.SetTag("apm.product_quantity", product.Quantity);
                activity?.SetTag("apm.product_weight", product.Weight);
                activity?.SetTag("apm.product_discount", product.Discount);
                activity?.SetTag("apm.product_available", product.IsAvailable);

                // Add array information
                if (product.Tags != null && product.Tags.Length > 0)
                {
                    activity?.SetTag("apm.product_tags", string.Join(", ", product.Tags));
                    activity?.SetTag("apm.product_tags_count", product.Tags.Length);
                }

                if (product.Categories != null && product.Categories.Length > 0)
                {
                    activity?.SetTag("apm.product_categories_count", product.Categories.Length);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Add the generated ID after save
                activity?.SetTag("apm.product_id", product.Id);
                activity?.SetTag("apm.status", "success");

                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Create a custom span for business logic
            using (var activity = AppTelemetry.ActivitySource.StartActivity("DeleteProduct"))
            {
                activity?.SetTag("apm.operation", "delete_product");
                activity?.SetTag("apm.entity", "Product");
                activity?.SetTag("apm.product_id", id);

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    activity?.SetTag("apm.status", "not_found");
                    activity?.SetTag("apm.error", "Product not found");
                    return NotFound();
                }

                // Add product details before deletion
                activity?.SetTag("apm.product_name", product.Name);
                activity?.SetTag("apm.product_price", product.Price);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                activity?.SetTag("apm.status", "success");
                activity?.SetTag("apm.deleted", true);

                return NoContent();
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

