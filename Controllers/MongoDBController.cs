using Microsoft.AspNetCore.Mvc;
using LocalMartOnline.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace LocalMartOnline.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MongoDBController : ControllerBase
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBService _mongoDBService;

        public MongoDBController(IMongoDatabase database, MongoDBService mongoDBService)
        {
            _database = database;
            _mongoDBService = mongoDBService;
        }

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                var collections = _database.ListCollectionNames().ToList();
                return Ok(new { success = true, message = "Kết nối MongoDB thành công", data = collections });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null });
            }
        }
    }
}