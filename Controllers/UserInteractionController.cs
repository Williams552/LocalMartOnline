using LocalMartOnline.Models;
using LocalMartOnline.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/user-interactions")]
    public class UserInteractionController : ControllerBase
    {
        private readonly IUserInteractionService _service;
        public UserInteractionController(IUserInteractionService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserInteraction interaction)
        {
            await _service.AddInteractionAsync(interaction);
            return Ok(new { success = true });
        }
    }
}
