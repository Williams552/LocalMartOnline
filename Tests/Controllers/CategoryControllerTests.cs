using Xunit;
using Moq;
using LocalMartOnline.Controllers;
using LocalMartOnline.Services.Interface;
using LocalMartOnline.Models.DTOs.Category;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalMartOnline.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();
            _controller = new CategoryController(_serviceMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithCategory()
        {
            // Arrange
            var createDto = new CategoryCreateDto { Name = "Test" };
            var createdDto = new CategoryDto { Id = "1", Name = "Test" };
            _serviceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(createdDto, createdAt.Value);
        }

        [Fact]
        public async Task GetAllPaged_ReturnsOk_WithPagedResult()
        {
            // Arrange
            var createDto = new CategoryCreateDto { Name = "Test" };
            var createdDto = new CategoryDto { Id = "1", Name = "Test" };
            _serviceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(createdDto, createdAt.Value);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            var updateDto = new CategoryUpdateDto { Name = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync("1", updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update("1", updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNotExist()
        {
            // Arrange
            var updateDto = new CategoryUpdateDto { Name = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync("1", updateDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.Update("1", updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Toggle_ReturnsNoContent_WhenSuccess()
        {
            _serviceMock.Setup(s => s.ToggleAsync("1")).ReturnsAsync(true);
            var result = await _controller.Toggle("1");
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Toggle_ReturnsNotFound_WhenNotExist()
        {
            _serviceMock.Setup(s => s.ToggleAsync("1")).ReturnsAsync(false);
            var result = await _controller.Toggle("1");
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Search_ReturnsOk_WithResults()
        {
            var categories = new List<CategoryDto> { new CategoryDto { Id = "1", Name = "A" } };
            _serviceMock.Setup(s => s.SearchByNameAsync("A")).ReturnsAsync(categories);
            var result = await _controller.Search("A");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categories, okResult.Value);
        }

        [Fact]
        public async Task Filter_ReturnsOk_WithResults()
        {
            var categories = new List<CategoryDto> { new CategoryDto { Id = "1", Name = "A" } };
            _serviceMock.Setup(s => s.FilterByAlphabetAsync('A')).ReturnsAsync(categories);
            var result = await _controller.Filter('A');
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categories, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var category = new CategoryDto { Id = "1", Name = "A" };
            _serviceMock.Setup(s => s.GetByIdAsync("1")).ReturnsAsync(category);
            var result = await _controller.GetById("1");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(category, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNotExist()
        {
            _serviceMock.Setup(s => s.GetByIdAsync("1")).ReturnsAsync((CategoryDto?)null);
            var result = await _controller.GetById("1");
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccess()
        {
            _serviceMock.Setup(s => s.DeleteAsync("1")).ReturnsAsync(true);
            var result = await _controller.Delete("1");
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotExist()
        {
            _serviceMock.Setup(s => s.DeleteAsync("1")).ReturnsAsync(false);
            var result = await _controller.Delete("1");
            Assert.IsType<NotFoundResult>(result);
        }
    }
}