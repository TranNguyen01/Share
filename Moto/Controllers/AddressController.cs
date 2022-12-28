using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Models;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly MotoDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AddressController> _logger;
        public AddressController(MotoDBContext context, UserManager<User> userManager, ILogger<AddressController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetAddress()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();
                var addresses = await _context.UserAddresses.Where(add => add.UserId == user.Id)
                    .Select(a => new
                    {
                        Id = a.id,
                        UserId = a.UserId,
                        StateId = a.StateId,
                        DistrictId = a.DistrictId,
                        WardId = a.WardId,
                        Detail = a.Detail,
                        PhoneContact = a.PhoneContact,
                        NameContact = a.NameContact
                    })
                    .ToListAsync();
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> CreateNewAdress(UserAddress address)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != address.UserId) return BadRequest();

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    address = new
                    {
                        Id = address.id,
                        StateId = address.StateId,
                        DistrictId = address.DistrictId,
                        WardId = address.WardId,
                        Detail = address.Detail,
                        NameContact = address.NameContact,
                        PhoneContact = address.PhoneContact
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdateAddress(int id, UserAddress updateAddress)
        {
            try
            {
                if (id != updateAddress.id) return BadRequest();

                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != updateAddress.UserId)
                    return NotFound();

                _context.Addresses.Update(updateAddress);

                var result = await _context.SaveChangesAsync();
                if (result >= 1)
                    return Ok(new
                    {
                        success = true,
                        address = new
                        {
                            id = updateAddress.id,
                            StateId = updateAddress.StateId,
                            DistrictId = updateAddress.DistrictId,
                            WardId = updateAddress.WardId,
                            Detail = updateAddress.Detail,
                            NameContact = updateAddress.NameContact,
                            PhoneContact = updateAddress.PhoneContact
                        }
                    });

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var address = await _context.UserAddresses.FindAsync(id);
                if (address == null || address.UserId != user.Id) return NotFound();

                _context.UserAddresses.Remove(address);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }


    }
}
