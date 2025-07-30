using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs;
using LocalMartOnline.Models.DTOs.User;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _userRepo;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IRepository<User> userRepo, IUserService userService, IMapper mapper)
        {
            _userRepo = userRepo;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortOrder = "asc")
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { success = false, message = "pageNumber and pageSize must be positive integers greater than zero.", data = (object?)null });
            }
            var (users, total) = await _userService.GetUsersPagingAsync(pageNumber, pageSize, search, role, sortField, sortOrder);
            var userDtos = users.Select(u => _mapper.Map<UserDTO>(u));
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách người dùng thành công",
                data = new
                {
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = userDtos
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { success = false, message = "Invalid id format", data = (object?)null });
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "User not found", data = (object?)null });
            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(new { success = true, message = "Lấy thông tin người dùng thành công", data = userDto });
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDTO userDto)
        {
            await _userService.CreateAsync(userDto);
            return Ok(new { success = true, message = "Tạo người dùng thành công", data = userDto });
        }

        [HttpPut("{id}")]
        [Authorize]
        [Authorize(Roles = "Admin,Buyer,Seller,ProxyShopper")]
        public async Task<IActionResult> Update(string id, [FromBody] UserUpdateDTO updateUserDto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { success = false, message = "Invalid id format", data = (object?)null });

            try
            {
                await _userService.UpdateAsync(id, updateUserDto);
            }
            catch (System.Exception ex)
            {
                // Nếu là lỗi trùng lặp
                if (ex.Message.Contains("Username already exists") || ex.Message.Contains("Email already exists"))
                {
                    return Conflict(new { success = false, message = ex.Message, data = (object?)null });
                }
                return BadRequest(new { success = false, message = ex.Message, data = (object?)null });
            }

            return Ok(new { success = true, message = "Cập nhật người dùng thành công", data = (object?)null });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest(new { success = false, message = "Invalid id format", data = (object?)null });
            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "User not found", data = (object?)null });
            await _userRepo.DeleteAsync(id);
            return Ok(new { success = true, message = "Xóa người dùng thành công", data = (object?)null });
        }

        // UC107: Update language
        [HttpPut("{id}/language")]
        [Authorize(Roles = "Admin,Buyer,Seller,ProxyShopper")]
        public async Task<IActionResult> UpdateLanguage(string id, [FromBody] UserLanguageUpdateDto dto)
        {
            var result = await _userService.UpdateLanguageAsync(id, dto.PreferredLanguage);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/language")]
        [Authorize(Roles = "Admin,Buyer,Seller,ProxyShopper")]
        public async Task<ActionResult<string>> GetLanguage(string id)
        {
            var lang = await _userService.GetLanguageAsync(id);
            if (lang == null) return NotFound();
            return Ok(lang);
        }

        // UC108: Update theme
        [HttpPut("{id}/theme")]
        [Authorize(Roles = "Admin,Buyer,Seller,ProxyShopper")]
        public async Task<IActionResult> UpdateTheme(string id, [FromBody] UserThemeUpdateDto dto)
        {
            var result = await _userService.UpdateThemeAsync(id, dto.PreferredTheme);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/theme")]
        [Authorize(Roles = "Admin,Buyer,Seller,ProxyShopper")]
        public async Task<ActionResult<string>> GetTheme(string id)
        {
            var theme = await _userService.GetThemeAsync(id);
            if (theme == null) return NotFound();
            return Ok(theme);
        }

        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserAccount(string id)
        {
            var result = await _userService.ToggleUserAccountAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "User not found", data = (object?)null });
            return Ok(new { success = true, message = "User status toggled", data = (object?)null });
        }

        [HttpPatch("disable-own")]
        [Authorize]
        public async Task<IActionResult> DisableOwnAccount()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { success = false, message = "Unauthorized", data = (object?)null });
            var result = await _userService.DisableOwnAccountAsync(userId);
            if (!result)
                return NotFound(new { success = false, message = "User not found", data = (object?)null });
            return Ok(new { success = true, message = "Your account has been disabled", data = (object?)null });
        }
    }
}