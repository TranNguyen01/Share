using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moto.Models;

namespace Moto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<User> _UserManager;
        private readonly SignInManager<User> _SignInManager;
        private readonly ILogger<RoleController> _Logger;
        private readonly MotoDBContext _Context;
        private readonly RoleManager<IdentityRole> _RoleManager;

        public RoleController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RoleController> logger, MotoDBContext context, RoleManager<IdentityRole> roleManager)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _Logger = logger;
            _Context = context;
            _RoleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var r = await _RoleManager.Roles.OrderBy(c => c.Name).ToListAsync();
            return Ok(r);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(string? RoleName)
        {
            var newRole = new IdentityRole(RoleName);
            var result = await _RoleManager.CreateAsync(newRole);
            if (result.Succeeded)
            {
                return Ok(newRole);
            }

            return BadRequest();
        }



    }
}


