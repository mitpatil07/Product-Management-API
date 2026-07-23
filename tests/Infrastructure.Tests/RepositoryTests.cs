using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.Persistence;
using ProductManagement.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ProductManagement.Infrastructure.Tests
{
    public class RepositoryTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockUserService = new Mock<ICurrentUserService>();
            mockUserService.Setup(u => u.Username).Returns("TestUser");

            return new ApplicationDbContext(options, mockUserService.Object);
        }

        [Fact]
        public async Task ProductRepository_AddAsync_Should_InsertProduct()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);
            var product = new Product { ProductName = "Test Product 1" };

            // Act
            await repository.AddAsync(product);
            await context.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(product.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test Product 1", retrieved.ProductName);
            Assert.Equal("TestUser", retrieved.CreatedBy);
        }

        [Fact]
        public async Task ProductRepository_GetProductWithItemsAsync_Should_IncludeItems()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);

            var product = new Product
            {
                ProductName = "Product With Items",
                Items = new[] { new Item { Quantity = 10 } }
            };

            await repository.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetProductWithItemsAsync(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(10, result.Items.First().Quantity);
        }

        [Fact]
        public async Task UserRepository_GetByUsernameAsync_Should_ReturnCorrectUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Username = "johndoe",
                PasswordHash = "hash123",
                Role = "Admin"
            };

            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var foundUser = await repository.GetByUsernameAsync("johndoe");

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal("johndoe", foundUser.Username);
            Assert.Equal("Admin", foundUser.Role);
        }
    }
}
