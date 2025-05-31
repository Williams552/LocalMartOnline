using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Repositories;
using LocalMartOnline.Models;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IGenericRepository<User> _userRepo;

        public UserController(IGenericRepository<User> userRepo)
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
    }
}