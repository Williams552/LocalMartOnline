using LocalMartOnline.Models.DTOs.Favorite;

namespace LocalMartOnline.Services
{
    public interface IFavoriteService
    {
        Task<FavoriteActionResponseDto> AddToFavoriteAsync(string userId, string productId);
        Task<GetFavoriteProductsResponseDto> GetUserFavoriteProductsAsync(string userId, int page = 1, int pageSize = 20);
        Task<FavoriteActionResponseDto> RemoveFromFavoriteAsync(string userId, string productId);
        Task<bool> IsProductInFavoriteAsync(string userId, string productId);
    }
}
