using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IRepository<User> _userRepo;

        public UserController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("test-auth")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult TestAuth()
        {
            var username = User.Identity?.Name;
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            return Ok(new { Message = $"Authenticated as {username}", Role = role });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid id format");
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            await _userRepo.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, User user)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid id format");
            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            user.Id = id;
            await _userRepo.UpdateAsync(id, user);
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