using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Models;
using Moto.Models.ValidationModels;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly UserManager<User> _usermanager;
        private readonly SignInManager<User> _signInManager;
        private readonly MotoDBContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(MotoDBContext context, SignInManager<User> signInManager, UserManager<User> usermanager, ILogger<CartController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _usermanager = usermanager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> AddToCart(AddToCartValidation cart)
        {
            var user = await _usermanager.GetUserAsync(User);
            if (user == null) return NotFound(new { success = false, message = "Không tim thấy User" });
            if (user.Id != cart.UserId) return BadRequest(new { success = false, message = "UserID không không trùng khớp" });

            var product = await _context.Products.FindAsync(cart.ProductId);

            if (product == null) return NotFound();
            else if (product.Quantity == 0) return BadRequest(new { success = false, message = "Số lượng mặt hàng không đủ" });
            else if (product.Quantity < cart.Quantity) return BadRequest(new { success = false, message = "Số lượng không đủ" });

            var existCart = _context.Carts.FirstOrDefault(c => c.ProductId == cart.ProductId && c.UserId == user.Id);

            if (existCart == null)
            {
                existCart = new Cart { ProductId = product.Id, UserId = cart.UserId, Quantity = cart.Quantity };
                _context.Carts.Add(existCart);
            }
            else
            {
                if (existCart.Quantity + cart.Quantity > product.Quantity)
                    return BadRequest(new { success = false, message = "Số lượng không đủ" });

                existCart.Quantity += cart.Quantity;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Cart = new
                    {
                        Id = existCart.Id,
                        ProductId = existCart.ProductId,
                        Product = new
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price
                        },
                        Quantity = existCart.Quantity
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpPut]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdateCart(UpdateCartValidation updateCart)
        {
            var oldCart = await _context.Carts.FirstOrDefaultAsync(c => c.ProductId == updateCart.ProductId && c.UserId == updateCart.UserId);
            if (oldCart == null) return NotFound();

            var product = await _context.Products.FindAsync(oldCart.ProductId);

            if (product == null) return NotFound();
            else if (product.Quantity < updateCart.Quantity) return BadRequest(new { message = "Số lượng hàng không đủ" });

            if (oldCart.Quantity != updateCart.OldQuantity) return BadRequest();
            if (updateCart.Quantity <= 0) _context.Carts.Remove(oldCart);
            else oldCart.Quantity = updateCart.Quantity;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Success = true,
                    Cart = new
                    {
                        Id = oldCart.Id,
                        ProductId = oldCart.ProductId,
                        UserId = oldCart.UserId,
                        Quantity = oldCart.Quantity
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> RemoveCart(string userId, int productId)
        {
            var user = await _usermanager.GetUserAsync(User);
            if (user == null) return NotFound();
            if (user.Id == userId) return BadRequest();

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId);
            if (cart == null) return NotFound();
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetAllCart()
        {
            var user = await _usermanager.GetUserAsync(User);
            if (user == null) return NotFound();

            var carts = await _context.Carts
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Where(c => c.UserId == user.Id)
                .Select(c => new
                {
                    Id = c.Id,
                    Product = new
                    {
                        Id = c.ProductId,
                        Name = c.Product.Name,
                        Price = c.Product.Price,
                        Images = c.Product.ProductImages.Select(pi => pi.Image.Path).ToList()
                    },
                    Quantity = c.Quantity
                })
                .ToListAsync();
            return Ok(carts);
        }
    }
}
