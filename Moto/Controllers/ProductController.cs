using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Atributes;
using Moto.Attributes;
using Moto.Models;
using Moto.Models.ValidationModels;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly MotoDBContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<ProductController> _logger;

        public ProductController(MotoDBContext context, Cloudinary cloudinary, ILogger<ProductController> logger) : base()
        {
            _context = context;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        [HttpGet]
        [Cache]
        public async Task<ActionResult<ICollection<Product>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                 .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(p => p.IsDeleted == false)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    Category = new
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    },
                    BrandId = p.BrandId,
                    Brand = new
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name
                    },
                    Description = p.Description,
                    ModelYear = p.ModelYear,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Images = p.ProductImages.Select(pi => pi.Image.Path)
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsDeleted == false)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = new
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name
                    },
                    Brand = new
                    {
                        Id = p.Brand.Id,
                        Name = p.Brand.Name
                    },
                    Description = p.Description,
                    ModelYear = p.ModelYear,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Images = p.ProductImages.Select(pi => pi.Image.Path)
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            else return Ok(product);
        }

        [HttpPut("{id}/image")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/product")]
        public async Task<ActionResult<Product>> PutProductImage(int id, [FromForm] List<IFormFile> imageFiles)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var oldProductImages = product.ProductImages.ToList();
                    var oldImages = oldProductImages.Select(pi => pi.Image).ToList();

                    List<Image> newImages = (await UploadImage(imageFiles)).ToList();

                    _context.Images.AddRange(newImages);
                    _context.SaveChanges();

                    foreach (var image in newImages)
                    {
                        var productImage = new ProductImage { ImageId = image.Id, ProductId = product.Id };
                        _context.ProductImages.Add(productImage);
                    }
                    _context.SaveChanges();

                    _context.ProductImages.RemoveRange(oldProductImages);
                    _context.SaveChanges();

                    _context.Images.RemoveRange(oldImages);
                    _context.SaveChanges();

                    transaction.Commit();

                    await DetroyImage(oldImages);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }
        }

        [HttpPut("{id}/all")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/product")]
        public async Task<ActionResult<Product>> PutProductAll(int id, [FromForm] UpdateProductValidation product, [FromForm] List<IFormFile> imageFiles)
        {
            var currentProduct = await _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (currentProduct == null) return NotFound();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var oldProductImages = currentProduct.ProductImages.ToList();
                    var oldImages = oldProductImages.Select(pi => pi.Image).ToList();


                    _context.ProductImages.RemoveRange(oldProductImages);
                    _context.SaveChanges();

                    _context.Images.RemoveRange(oldImages);
                    _context.SaveChanges();

                    List<Image> newImages = (await UploadImage(imageFiles)).ToList();

                    _context.Images.AddRange(newImages);
                    _context.SaveChanges();

                    foreach (var image in newImages)
                    {
                        var productImage = new ProductImage { ImageId = image.Id, ProductId = currentProduct.Id };
                        _context.ProductImages.Add(productImage);
                    }
                    _context.SaveChanges();





                    currentProduct.BrandId = product.BrandId;
                    currentProduct.Name = product.Name;
                    currentProduct.Price = product.Price;
                    currentProduct.CategoryId = product.CategoryId;
                    currentProduct.Description = product.Description;
                    currentProduct.Quantity = product.Quantity;
                    currentProduct.ModelYear = product.ModelYear;

                    _context.Entry(currentProduct).State = EntityState.Modified;

                    _context.SaveChanges();

                    transaction.Commit();

                    var returnProduct = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.Image)
                        .Where(p => p.IsDeleted == false)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Name = p.Name,
                            CategoryId = p.CategoryId,
                            Category = new
                            {
                                Id = p.Category.Id,
                                Name = p.Category.Name
                            },
                            BrandId = p.BrandId,
                            Brand = new
                            {
                                Id = p.Brand.Id,
                                Name = p.Brand.Name
                            },
                            Description = p.Description,
                            ModelYear = p.ModelYear,
                            Price = p.Price,
                            Quantity = p.Quantity,
                            Images = p.ProductImages.Select(pi => pi.Image.Path)
                        })
                        .FirstOrDefaultAsync(p => p.Id == id);
                    _logger.LogInformation($"UPDATE PRODUCT ID={currentProduct.Id}");


                    await DetroyImage(oldImages);

                    return Ok(returnProduct);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/product")]
        public async Task<ActionResult<Product>> PutProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();
            if (!(await _context.Products.AnyAsync(p => p.Id == id))) return NotFound();
            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();


                var returnProduct = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                        .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                    .Where(p => p.IsDeleted == false)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryId = p.CategoryId,
                        Category = new
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name
                        },
                        BrandId = p.BrandId,
                        Brand = new
                        {
                            Id = p.Brand.Id,
                            Name = p.Brand.Name
                        },
                        Description = p.Description,
                        ModelYear = p.ModelYear,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Images = p.ProductImages.Select(pi => pi.Image.Path)
                    })
                     .FirstOrDefaultAsync(p => p.Id == id);
                _logger.LogInformation($"UPDATE PRODUCT ID={product.Id}");
                return Ok(returnProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}/delete")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/product")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var oldProductImage = product.ProductImages.ToList();
            var oldImages = oldProductImage.Select(pi => pi.Image).ToList();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.ProductImages.RemoveRange(oldProductImage);
                    _context.SaveChanges();

                    _context.Images.RemoveRange(oldImages);
                    _context.SaveChanges();

                    _context.Products.Remove(product);
                    _context.SaveChanges();

                    transaction.Commit();

                    await DetroyImage(oldImages);

                    return NoContent();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [ClearCache(true, "/api/product")]
        public async Task<ActionResult> DeleteProductSoft(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"DELETE PRODUCT ID={product.Id}");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }


        [HttpPost]
        [Authorize(Roles = "admin")]
        [ClearCache]
        public async Task<ActionResult<Product>> PostProduct([FromForm] Product product, [FromForm] List<IFormFile> imageFiles)
        {

            //if (!ModelState.IsValid) return BadRequest();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Products.Add(product);

                    List<Image> images = (await UploadImage(imageFiles)).ToList();

                    _context.Images.AddRange(images);
                    _context.SaveChanges();

                    foreach (var image in images)
                    {
                        var productImage = new ProductImage { Image = image, Product = product };
                        product.ProductImages.Add(productImage);
                        _context.ProductImages.Add(productImage);
                    }

                    _context.SaveChanges();

                    transaction.Commit();

                    var products = await _context.Products
                       .Include(p => p.Category)
                       .Include(p => p.Brand)
                        .Include(p => p.ProductImages)
                       .ThenInclude(pi => pi.Image)
                       .Where(p => p.IsDeleted == false)
                       .Select(p => new
                       {
                           Id = p.Id,
                           Name = p.Name,
                           CategoryId = p.CategoryId,
                           Category = new
                           {
                               Id = p.Category.Id,
                               Name = p.Category.Name
                           },
                           BrandId = p.BrandId,
                           Brand = new
                           {
                               Id = p.Brand.Id,
                               Name = p.Brand.Name
                           },
                           Description = p.Description,
                           ModelYear = p.ModelYear,
                           Price = p.Price,
                           Quantity = p.Quantity,
                           Images = p.ProductImages.Select(pi => pi.Image.Path)
                       })
                       .FirstOrDefaultAsync(p => p.Id == product.Id);
                    _logger.LogInformation($"CREATE PRODUCT ID={product.Id}");
                    return Ok(products);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    transaction.Rollback();
                    return StatusCode(500);
                }
            }


        }


        private async Task<ICollection<Image>> UploadImage(ICollection<IFormFile> imageFiles)
        {
            List<Image> images = new();
            foreach (IFormFile file in imageFiles)
            {
                if (file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        try
                        {
                            var result = await _cloudinary.UploadAsync(new ImageUploadParams
                            {
                                File = new FileDescription(file.FileName, stream),
                            });

                            var newImage = new Image
                            {
                                CreatedAt = DateTime.UtcNow,
                                PublicId = result.PublicId,
                                Version = result.Version,
                                Signature = result.Signature,
                                Format = result.Format,
                                ResourceType = result.ResourceType,
                                Bytes = result.Bytes,
                                Type = result.Type,
                                Url = result.Url.ToString(),
                                SecureUrl = result.SecureUrl.ToString(),
                                Path = result.Url.ToString()
                            };
                            images.Add(newImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

            }

            return images;
        }

        private async Task<bool> DetroyImage(ICollection<Image> images)
        {
            try
            {
                foreach (var image in images)
                {
                    var result = await _cloudinary.DestroyAsync(new DeletionParams(image.PublicId));
                    Console.WriteLine(result.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }
}
