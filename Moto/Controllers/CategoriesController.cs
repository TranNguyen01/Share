using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Atributes;
using Moto.Attributes;
using Moto.Models;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly MotoDBContext _context;
        private readonly ILogger<CategoriesController> _logger;
        public CategoriesController(MotoDBContext context, ILogger<CategoriesController> logger) : base()
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        [Cache]
        public async Task<ActionResult<List<Category>>> GetCategory()
        {
            return Ok(await _context.Categories.Where(c => c.IsDeleted == false).ToListAsync());
        }

        [HttpGet("{id}")]
        [Cache]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPut("{id}/Restore")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/categories")]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                category.IsDeleted = false;

                var productOfCategory = await _context.Products
                    .Where(p => p.CategoryId == category.Id)
                    .ToListAsync();

                foreach (var product in productOfCategory)
                {
                    product.IsDeleted = false;
                }

                _context.Products.UpdateRange(productOfCategory);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"RESTORE CATEGORY AND PRODUCT OF CATEGORY WHERE ID={category.Id}");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }

        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/categories")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id) return BadRequest();

            if (!ModelState.IsValid) return BadRequest();

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"UPDATE CATEGORY ID={category.Id}");
                return Ok(new { success = true, category = category });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IsCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();

        }


        [HttpPost]
        [Authorize(Roles = "admin")]
        [ClearCache]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (!ModelState.IsValid) return BadRequest();
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"CREATE CATEGORY ID={category.Id}");
                return CreatedAtAction("GetCategory", new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}/Delete")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/categories")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/categories")]
        public async Task<IActionResult> DeleteCategorySoft(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {

                category.IsDeleted = true;

                var productOfCategory = await _context.Products
                    .Where(p => p.CategoryId == category.Id)
                    .ToListAsync();

                foreach (var product in productOfCategory)
                {
                    product.IsDeleted = true;
                }

                _context.Products.UpdateRange(productOfCategory);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"DELETE CATEGORY AND PRODUCT OF CATEGORY WHERE ID={category.Id}");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }

        }



        private bool IsCategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
