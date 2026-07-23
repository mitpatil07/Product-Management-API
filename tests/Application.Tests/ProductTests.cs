using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Validators;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using Xunit;

namespace ProductManagement.Application.Tests
{
    public class ProductTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly ProductCommandHandler _handler;

        public ProductTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            
            _handler = new ProductCommandHandler(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockCurrentUserService.Object
            );
        }

        [Fact]
        public async Task Handle_CreateProductCommand_Should_SaveAndReturnDto()
        {
            // Arrange
            var command = new CreateProductCommand("Test Product");
            var product = new Product { Id = 1, ProductName = "Test Product" };
            var productDto = new ProductDto { Id = 1, ProductName = "Test Product" };

            _mockUnitOfWork.Setup(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(productDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Product", result.Value.ProductName);
            _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateProductValidator_Should_Fail_When_ProductName_IsEmpty(string? name)
        {
            // Arrange
            var validator = new CreateProductCommandValidator();
            var command = new CreateProductCommand(name!);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ProductName");
        }

        [Fact]
        public void CreateProductValidator_Should_Fail_When_ProductName_ExceedsMaxLength()
        {
            // Arrange
            var validator = new CreateProductCommandValidator();
            var longName = new string('A', 101);
            var command = new CreateProductCommand(longName);

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ProductName");
        }
    }
}
