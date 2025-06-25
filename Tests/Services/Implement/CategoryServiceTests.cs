using Xunit;
using Moq;
using AutoMapper;
using LocalMartOnline.Models;
using LocalMartOnline.Models.DTOs.Category;
using LocalMartOnline.Models.DTOs.Common;
using LocalMartOnline.Repositories;
using LocalMartOnline.Services.Implement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace LocalMartOnline.Tests.Services.Implement
{
    public class CategoryServiceTests
    {
        private readonly Mock<IRepository<Category>> _repoMock;
        private readonly IMapper _mapper;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _repoMock = new Mock<IRepository<Category>>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Category, CategoryDto>();
                cfg.CreateMap<CategoryCreateDto, Category>();
                cfg.CreateMap<CategoryUpdateDto, Category>();
            });
            _mapper = config.CreateMapper();
            _service = new CategoryService(_repoMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsPagedResult()
        {
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "A" },
                new Category { Id = "2", Name = "B" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            var result = await _service.GetAllPagedAsync(1, 1);

            Assert.Equal(2, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("A", result.Items.First().Name);
            Assert.Equal(1, result.Page);
            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCategoryDto_WhenFound()
        {
            var category = new Category { Id = "1", Name = "A" };
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(category);

            var result = await _service.GetByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("A", result!.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync((Category?)null);

            var result = await _service.GetByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_CreatesAndReturnsCategoryDto()
        {
            var dto = new CategoryCreateDto { Name = "Test", Description = "Desc" };
            Category? createdCategory = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>()))
                .Callback<Category>(c => createdCategory = c)
                .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.NotNull(createdCategory);
            Assert.Equal(dto.Name, createdCategory!.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenCategoryExists()
        {
            var category = new Category { Id = "1", Name = "Old" };
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(category);
            _repoMock.Setup(r => r.UpdateAsync("1", category)).Returns(Task.CompletedTask);

            var dto = new CategoryUpdateDto { Name = "New", Description = "Desc" };
            var result = await _service.UpdateAsync("1", dto);

            Assert.True(result);
            Assert.Equal("New", category.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenCategoryNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync((Category?)null);

            var dto = new CategoryUpdateDto { Name = "New", Description = "Desc" };
            var result = await _service.UpdateAsync("1", dto);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenCategoryExists()
        {
            var category = new Category { Id = "1" };
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(category);
            _repoMock.Setup(r => r.DeleteAsync("1")).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync("1");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCategoryNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync((Category?)null);

            var result = await _service.DeleteAsync("1");

            Assert.False(result);
        }

        [Fact]
        public async Task ToggleAsync_TogglesIsActive_AndReturnsTrue()
        {
            var category = new Category { Id = "1", IsActive = true };
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(category);
            _repoMock.Setup(r => r.UpdateAsync("1", category)).Returns(Task.CompletedTask);

            var result = await _service.ToggleAsync("1");

            Assert.True(result);
            Assert.False(category.IsActive);
        }

        [Fact]
        public async Task ToggleAsync_ReturnsFalse_WhenCategoryNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync((Category?)null);

            var result = await _service.ToggleAsync("1");

            Assert.False(result);
        }

        [Fact]
        public async Task SearchByNameAsync_ReturnsMatchingCategories()
        {
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Apple" },
                new Category { Id = "2", Name = "Banana" }
            };
            _repoMock.Setup(r => r.FindManyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Expression<Func<Category, bool>> expr) => categories.AsQueryable().Where(expr.Compile()));

            var result = await _service.SearchByNameAsync("app");

            Assert.Single(result);
            Assert.Equal("Apple", result.First().Name);
        }

        [Fact]
        public async Task FilterByAlphabetAsync_ReturnsMatchingCategories()
        {
            var categories = new List<Category>
            {
                new Category { Id = "1", Name = "Apple" },
                new Category { Id = "2", Name = "Banana" }
            };
            _repoMock.Setup(r => r.FindManyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Expression<Func<Category, bool>> expr) => categories.AsQueryable().Where(expr.Compile()));

            var result = await _service.FilterByAlphabetAsync('B');

            Assert.Single(result);
            Assert.Equal("Banana", result.First().Name);
        }
    }
}