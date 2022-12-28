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
    public class BrandController : ControllerBase
    {
        private readonly MotoDBContext _context;
        private readonly ILogger<BrandController> _logger;
        public BrandController(MotoDBContext context, ILogger<BrandController> logger) : base()
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Cache]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrand()
        {
            var brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            _logger.LogInformation($"Get All Brands");
            return Ok(brands);
        }

        [HttpGet("{id}")]
        [Cache]
        public async Task<ActionResult<Brand>> GetBrand(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);

            if (brand == null) return NotFound();
            else return Ok(brand);
        }

        [HttpPost]
        [ClearCache]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Brand>> PostBrand(Brand brand)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"CREATE BRAND ID={brand.Id}");
                return CreatedAtAction("PostBrand", new { Id = brand.Id }, brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }

        }

        [HttpPut("{id}/Restore")]
        [ClearCache(true, "/api/brand")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> RestoreBrand(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return NotFound();

                brand.IsDeleted = false;

                var productOfCategory = await _context.Products
                    .Where(p => p.BrandId == brand.Id)
                    .ToListAsync();

                foreach (var product in productOfCategory)
                {
                    product.IsDeleted = false;
                }

                _context.Products.UpdateRange(productOfCategory);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"DELETE BRAND ID={brand.Id}");
                return Ok(new { success = true });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }



        [HttpPut("{id}")]
        [ClearCache(true, "/api/brand")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Brand>> PutBrand(int id, Brand brand)
        {
            if (id != brand.Id) return BadRequest();

            if (!ModelState.IsValid) return BadRequest();

            if (!(await IsBrandExists(id))) return NotFound();

            try
            {
                _context.Entry(brand).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"UPDATE BRAND ID={brand.Id}");
                return Ok(new { success = true, brand = brand });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}")]
        [ClearCache(true, "/api/brand")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteBrandSoft(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return NotFound();

                brand.IsDeleted = true;

                var productOfCategory = await _context.Products
                    .Where(p => p.BrandId == brand.Id)
                    .ToListAsync();

                foreach (var product in productOfCategory)
                {
                    product.IsDeleted = true;
                }

                _context.Products.UpdateRange(productOfCategory);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"DELETE BRAND ID={brand.Id}");
                return Ok(new { success = true });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}/Delete")]
        [ClearCache(true, "/api/brand")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteBrand(int id)
        {
            try
            {
                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return NotFound();

                _context.Remove(brand);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }



        private async Task<bool> IsBrandExists(int id)
        {
            return await _context.Brands.AnyAsync(b => b.Id == id);
        }
    }
}
