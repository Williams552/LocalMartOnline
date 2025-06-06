using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;
using LocalMartOnline.Services;
using LocalMartOnline.Models.DTOs;
using AutoMapper;

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
            var (users, total) = await _userService.GetUsersPagingAsync(pageNumber, pageSize, search, role, sortField, sortOrder);
            var userDtos = users.Select(u => _mapper.Map<RegisterDTO>(u));
            return Ok(new
            {
                Total = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = userDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid id format");
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();
            var userDto = _mapper.Map<RegisterDTO>(user);
            return Ok(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterDTO userDto)
        {
            var user = _mapper.Map<User>(userDto);
            await _userRepo.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromBody] RegisterDTO updateUserDto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid id format");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            if (currentUserId != id && !isAdmin)
                return Forbid();

            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Map các trường cho phép cập nhật
            _mapper.Map(updateUserDto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(id, existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid id format");
            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _userRepo.DeleteAsync(id);
            return NoContent();
        }
    }
}