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
    public class AccountController : ControllerBase
    {
        private readonly MotoDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(MotoDBContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserValidation user)
        {
            var newUser = new User()
            {
                UserName = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Birthdate = user.BirthDate,
                Gender = user.Gender
            };

            try
            {
                var existUser = await _userManager.FindByEmailAsync(user.Email);
                if (existUser != null) return BadRequest(new
                {
                    success = false,
                    Error = new
                    {
                        field = "email",
                        message = "Email đã tồn tại"
                    }

                });

                if (DateTime.Compare(DateTime.Now, newUser.Birthdate) <= 0) return BadRequest(new
                {
                    success = false,
                    Error = new
                    {
                        field = "birthDate",
                        message = "Ngày sinh không hợp lệ"
                    }
                });

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "user");
                    return Ok(new { success = true });
                }
                else return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginValidation loginUser)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(loginUser.UserName, loginUser.Password, loginUser.IsRemember, false);
            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(loginUser.UserName);
                if (user == null) return NotFound(new
                {
                    success = false,
                    message = "Email hoặc mật khẩu không đúng!"
                });
                var role = await _userManager.GetRolesAsync(user);
                var avatar = await _context.Images.FindAsync(user.AvatarId);
                return Ok(new
                {
                    Success = true,
                    User = new
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Avatar = avatar?.Path,
                        Roles = role
                    }
                });
            }
            else return NotFound(new
            {
                success = false,
                message = "Email hoặc mật khẩu không đúng!"
            });
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }

        }

        [HttpPut("Password")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordValidation updateInfo)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return BadRequest(new { success = false, field = "user", message = "Không tìm thấy người dùng" });
                if (updateInfo.UserId != user.Id) return BadRequest();

                var isValidPass = await _userManager.CheckPasswordAsync(user, updateInfo.Password);
                if (!isValidPass) return BadRequest(new { success = false, field = "password", message = "Mật khẩu không chính xác" });

                var result = await _userManager.ChangePasswordAsync(user, updateInfo.Password, updateInfo.NewPassword);

                if (result.Succeeded) return Ok(new { success = true, message = "Thành công" });
                else return Ok(new { success = false, field = "", message = "Cập nhật không thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }


        [HttpGet("Setting")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetUserInformation()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return NotFound();
                else return Ok(new
                {
                    Email = currentUser.Email,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    BirthDate = currentUser.Birthdate,
                    PhoneNumber = currentUser.PhoneNumber,
                    Gender = currentUser.Gender,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPost("Setting")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> Setting(UserInfomationValidation userInfo)
        {
            try
            {
                //return Ok();
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return BadRequest();
                else
                {
                    if (userInfo.PhoneNumber != currentUser.PhoneNumber)
                    {
                        var changePhoneNumberToken = await _userManager.GenerateChangePhoneNumberTokenAsync(currentUser, userInfo.PhoneNumber);
                        var changePhoneResult = await _userManager.ChangePhoneNumberAsync(currentUser, userInfo.PhoneNumber, changePhoneNumberToken);
                    }

                    var newObjType = userInfo.GetType();
                    var currenObjType = currentUser.GetType();
                    var props = newObjType.GetProperties();
                    foreach (var prop in props)
                    {
                        var propName = prop.Name;
                        var propValue = newObjType?.GetProperty(propName)?.GetValue(userInfo);

                        if (propValue != null)
                        {
                            currenObjType?.GetProperty(prop.Name)?.SetValue(currentUser, propValue);
                        }
                    }
                }

                var result = await _userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                    return Ok(new { success = true, message = "Cập nhật thành công" });
                else
                    return BadRequest(new { success = true, message = "Cập nhật không thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateRole(string id, CustomRole role)
        {

            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            try
            {
                var resultAdd = await _userManager.AddToRoleAsync(user, role.RoleName);

                return Ok(new { success = true, message = "thanh cong", Role = role.RoleName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok(new { success = false, message = "khong thanh cong", Role = role.RoleName });
            }
        }

        [HttpGet("All")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetALlUser()
        {
            try
            {
                //return Ok();
                var today = DateTime.Today;

                var allUsers = await _context.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        BirthDate = u.Birthdate,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Gender = u.Gender,
                        Age = today.Year - u.Birthdate.Year,
                        Order = _context.Orders.Count(c => c.UserId == u.Id)
                    })
                    .ToListAsync();
                return Ok(allUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }



    }
}
