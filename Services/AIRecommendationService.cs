// Services/IAIRecommendationService.cs
using System.Text.Json;

namespace LocalMartOnline.Services
{
    public class ProductRecommendationDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Score { get; set; }
        public string? Brand { get; set; }
    }

    public class RecommendationResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<ProductRecommendationDto> Recommendations { get; set; } = new();
        public string Timestamp { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
    }

    public class AIStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? LastModelUpdate { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalInteractions { get; set; }
        public double? ModelAccuracy { get; set; }
    }

    public class RetrainResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EstimatedDuration { get; set; } = string.Empty;
    }

    public interface IAIRecommendationService
    {
        Task<List<ProductRecommendationDto>> GetRecommendationsAsync(string userId, int count = 5);
        Task<bool> TriggerRetrainingAsync();
        Task<AIStatusDto?> GetAIStatusAsync();
        Task<bool> IsHealthyAsync();
    }

    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIRecommendationService> _logger;
        private readonly string _aiServiceUrl;

        public AIRecommendationService(HttpClient httpClient, ILogger<AIRecommendationService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:5001";
            
            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_aiServiceUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<ProductRecommendationDto>> GetRecommendationsAsync(string userId, int count = 5)
        {
            try
            {
                _logger.LogInformation("Getting recommendations for user {UserId}, count: {Count}", userId, count);

                var response = await _httpClient.GetAsync($"/api/recommendations/{userId}?count={count}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var recommendationResponse = JsonSerializer.Deserialize<RecommendationResponseDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return recommendationResponse?.Recommendations ?? new List<ProductRecommendationDto>();
                }
                else
                {
                    _logger.LogWarning("AI service returned {StatusCode} for user {UserId}", response.StatusCode, userId);
                    return GetFallbackRecommendations(count);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when getting recommendations for user {UserId}", userId);
                return GetFallbackRecommendations(count);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout when getting recommendations for user {UserId}", userId);
                return GetFallbackRecommendations(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when getting recommendations for user {UserId}", userId);
                return new List<ProductRecommendationDto>();
            }
        }

        public async Task<bool> TriggerRetrainingAsync()
        {
            try
            {
                _logger.LogInformation("Triggering AI model retraining");

                var requestBody = JsonSerializer.Serialize(new { force = false });
                var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/ai/retrain", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var retrainResponse = JsonSerializer.Deserialize<RetrainResponseDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Retraining triggered: {Message}", retrainResponse?.Message);
                    return retrainResponse?.Success ?? false;
                }
                else
                {
                    _logger.LogWarning("AI service returned {StatusCode} for retraining request", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering AI model retraining");
                return false;
            }
        }

        public async Task<AIStatusDto?> GetAIStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/ai/status");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<AIStatusDto>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return status;
                }
                else
                {
                    _logger.LogWarning("AI service returned {StatusCode} for status request", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI service status");
                return null;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private List<ProductRecommendationDto> GetFallbackRecommendations(int count)
        {
            // Fallback to popular products from your database
            // This should be implemented based on your Product model
            _logger.LogInformation("Using fallback recommendations");
            
            // TODO: Implement fallback logic here
            // For example, get most popular products from MongoDB
            return new List<ProductRecommendationDto>();
        }
    }
}
