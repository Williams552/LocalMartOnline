using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Product;

namespace LocalMartOnline.Services.Interface
{
    public interface ICategoryService
    {
        Task<GetCategoriesResponseDto> GetActiveCategoriesAsync();
        Task<SearchProductResultDto> GetProductsByCategoryAsync(string categoryId, int page = 1, int pageSize = 20, string sortPrice = "");
    }
}