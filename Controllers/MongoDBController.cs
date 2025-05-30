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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Ví dụ: Lấy tất cả documents từ collection "your_collection"
            var collection = _mongoDBService.GetCollection<object>("users");
            var documents = await collection.Find(_ => true).ToListAsync();
            return Ok(documents);
        }

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                // Thử lấy danh sách collection
                var collections = _database.ListCollectionNames().ToList();
                return Ok(new { success = true, collections });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
} 