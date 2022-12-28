using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Models;
using Moto.Models.ValidationModels;
using Moto.Services;
using Newtonsoft.Json;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly MotoDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly MotoProducer _producer;
        private readonly ILogger<OrderController> _logger;

        public OrderController(MotoDBContext context, UserManager<User> userManager, MotoProducer producer, ILogger<OrderController> logger)
        {
            _context = context;
            _userManager = userManager;
            _producer = producer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrderAdmin(int page = 0, int pageSize = 20)
        {
            try
            {
                var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                .ThenInclude(od => od.Product)
                .Include(o => o.Address)
                .Select(o => new
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    User = new
                    {
                        Id = o.Customer.Id,
                        FirstName = o.Customer.FirstName,
                        LastName = o.Customer.LastName,
                        PhoneNumber = o.Customer.PhoneNumber
                    },
                    CreateAt = o.CreateAt,
                    UpdateAt = o.UpdateAt,
                    Status = o.status,
                    Address = new
                    {
                        StateId = o.Address.StateId,
                        DistrictId = o.Address.DistrictId,
                        WardId = o.Address.WardId,
                        PhoneContact = o.Address.PhoneContact,
                        NameContact = o.Address.NameContact,
                    },
                    Products = o.Details.Select(d => new
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product.Name,
                        Price = d.Price,
                        Quantity = d.Quantity
                    }),
                    Total = o.Total
                }).ToListAsync();

                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderAdmin(int id)
        {
            try
            {
                var order = await _context.Orders
               .Include(o => o.Details)
               .ThenInclude(od => od.Product)
               .Include(o => o.Address)
               .Select(o => new
               {
                   Id = o.Id,
                   UserId = o.UserId,
                   Address = new
                   {
                       StateId = o.Address.StateId,
                       DistrictId = o.Address.DistrictId,
                       WardId = o.Address.WardId,
                       PhoneContact = o.Address.PhoneContact,
                       NameContact = o.Address.NameContact,
                   },
                   Products = o.Details.Select(d => new
                   {
                       ProductId = d.ProductId,
                       ProductName = d.Product.Name,
                       Price = d.Price,
                       Quantity = d.Quantity
                   }),
                   Total = o.Total
               })
               .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("myOrder")]
        public async Task<IActionResult> GetAllOrderUser()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var order = await _context.Orders
                    .Include(o => o.Details)
                    .ThenInclude(od => od.Product)
                    .ThenInclude(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                    .Include(o => o.Address)
                    .Select(o => new
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        Address = new
                        {
                            StateId = o.Address.StateId,
                            DistrictId = o.Address.DistrictId,
                            WardId = o.Address.WardId,
                            PhoneContact = o.Address.PhoneContact,
                            NameContact = o.Address.NameContact,
                        },
                        Products = o.Details.Select(d => new
                        {
                            ProductId = d.ProductId,
                            ProductName = d.Product.Name,
                            Price = d.Price,
                            Quantity = d.Quantity,
                            Images = d.Product.ProductImages.Select(pi => pi.Image.Path).ToList()
                        }),
                        Total = o.Total,
                        Status = o.status
                    })
                    .Where(o => o.UserId == user.Id)
                    .ToListAsync();

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("myOrder/{id}")]
        public async Task<IActionResult> GetOrderUser(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var order = await _context.Orders
               .Include(o => o.Details)
               .ThenInclude(od => od.Product)
               .Include(o => o.Address)
               .Select(o => new
               {
                   Id = o.Id,
                   UserId = o.UserId,
                   Address = new
                   {
                       StateId = o.Address.StateId,
                       DistrictId = o.Address.DistrictId,
                       WardId = o.Address.WardId,
                       PhoneContact = o.Address.PhoneContact,
                       NameContact = o.Address.NameContact,
                   },
                   Products = o.Details.Select(d => new
                   {
                       ProductId = d.ProductId,
                       ProductName = d.Product.Name,
                       Price = d.Price,
                       Quantity = d.Quantity
                   }),
                   Total = o.Total
               })
               .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }


        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> CreateOrder(CreateOrderValidation order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != order.UserId) return BadRequest();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var address = _context.UserAddresses.FirstOrDefault(addr => addr.UserId == user.Id && addr.id == order.AddressId);
                    if (address == null) throw new ArgumentOutOfRangeException();

                    var productIds = order.Details.Select(d => d.ProductId).ToList();

                    var carts = await _context.Carts
                        .Where(c => productIds.Contains(c.ProductId) && c.UserId == user.Id)
                        .ToListAsync();

                    if (carts.Count != order.Details.Count) return BadRequest();

                    _context.Carts.RemoveRange(carts);
                    _context.SaveChanges();

                    var shippingAddress = new ShippingAddress
                    {
                        StateId = address.StateId,
                        DistrictId = address.DistrictId,
                        WardId = address.WardId,
                        PhoneContact = address.PhoneContact,
                        NameContact = address.NameContact,
                        Detail = address.Detail
                    };

                    var newOrder = new Order()
                    {
                        UserId = order.UserId,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow,
                        AddressId = shippingAddress.id,
                        Details = new List<OrderDetail>(),
                        Address = shippingAddress,
                        status = 0,
                        Total = 0
                    };

                    foreach (var item in carts)
                    {
                        var isValidItem = order.Details.Any(dt => dt.ProductId == item.ProductId && dt.Quantity == item.Quantity);
                        if (!isValidItem)
                            throw new ArgumentOutOfRangeException("Có thay đổi chưa được cập nhật trong giỏ hàng");

                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                            throw new ArgumentOutOfRangeException("Không tìm thấy sản phẩm");

                        if (product.Quantity < item.Quantity)
                            throw new ArgumentOutOfRangeException("Số lượng xe không đủ để tạo đơn hàng");

                        var orderDetail = new OrderDetail()
                        {
                            OrderId = newOrder.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            Price = product.Price,
                        };

                        newOrder.Total += (decimal)orderDetail.Price * orderDetail.Quantity;
                        newOrder.Details.Add(orderDetail);
                    }

                    _context.Orders.Add(newOrder);

                    _context.SaveChanges();


                    transaction.Commit();
                    await _producer.SendMessage("update-product-1", JsonConvert.SerializeObject(order.Details));
                    return Ok(new
                    {
                        success = true,
                        orderId = newOrder.Id,
                    });

                }
                catch (ArgumentOutOfRangeException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                    return BadRequest(new { success = true, message = ex.Message });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                    return StatusCode(500);
                }
            }
        }


        [HttpPut("{id}/refuse")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> refuseOrder(int id, UpdateOrderStatusValidation newOrderStatus)
        {
            if (id != newOrderStatus.Id) return BadRequest();

            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();
            else if (order.status != newOrderStatus.OldStatus || order.status != 0 || newOrderStatus.Status != -3)
                return BadRequest();
            else if (order.UserId != user.Id)
                return BadRequest();

            var isValid = isValidNewStatus(order.status, newOrderStatus.Status);
            if (!isValid) return BadRequest();

            try
            {
                order.status = newOrderStatus.Status;
                order.UpdateAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (newOrderStatus.Status == -1 || newOrderStatus.Status == -2 || newOrderStatus.Status == -3)
                {
                    List<OrderDetailValidation> productsNeedUpdate = await _context.OrderDetails
                        .Where(od => od.OrderId == order.Id)
                        .Select(od => new OrderDetailValidation { ProductId = od.ProductId, Quantity = -od.Quantity })
                        .ToListAsync();
                    await _producer.SendMessage("update-product-1", JsonConvert.SerializeObject(productsNeedUpdate));
                }

                return Ok(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusValidation newOrderStatus)
        {
            if (id != newOrderStatus.Id) return BadRequest();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.status != newOrderStatus.OldStatus) return BadRequest();

            var isValid = isValidNewStatus(order.status, newOrderStatus.Status);
            if (!isValid) return BadRequest();

            try
            {
                order.status = newOrderStatus.Status;
                order.UpdateAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (newOrderStatus.Status == -1 || newOrderStatus.Status == -2 || newOrderStatus.Status == -3)
                {
                    List<OrderDetailValidation> productsNeedUpdate = await _context.OrderDetails
                        .Where(od => od.OrderId == order.Id)
                        .Select(od => new OrderDetailValidation { ProductId = od.ProductId, Quantity = -od.Quantity })
                        .ToListAsync();
                    await _producer.SendMessage("update-product-1", JsonConvert.SerializeObject(productsNeedUpdate));
                }

                return Ok(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }


        private bool isValidNewStatus(int currentStatus, int newStatus)
        {
            if (currentStatus == newStatus || currentStatus == -1 || currentStatus == 3 || newStatus < -3 || newStatus > 3) return false;
            else if (currentStatus > 1 && newStatus == -1) return false;
            else return true;
        }

    }
}
