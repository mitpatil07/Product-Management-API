using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.API.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Identity;
using ProductManagement.Application.Features.Products;
using ProductManagement.Infrastructure.Persistence;
using Xunit;

namespace ProductManagement.API.Tests
{
    public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProductsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase_Api");
                    });
                });
            });
        }

        private async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            var registerCommand = new RegisterCommand("admin_test", "AdminPassword123!", "Admin");
            var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                if (result?.Data?.AccessToken != null)
                {
                    return result.Data.AccessToken;
                }
            }

            var loginCommand = new LoginCommand("admin_test", "AdminPassword123!");
            var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                return loginResult?.Data?.AccessToken ?? string.Empty;
            }

            return string.Empty;
        }

        [Fact]
        public async Task GetProducts_WithoutToken_Should_ReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/products");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthenticatedUser_Can_Create_And_Get_Product()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            Assert.False(string.IsNullOrEmpty(token));

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 1. Create Product
            var createCommand = new CreateProductCommand("Integration Test Laptop");
            var createResponse = await client.PostAsJsonAsync("/api/v1/products", createCommand);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            Assert.NotNull(createResult?.Data);
            Assert.Equal("Integration Test Laptop", createResult.Data.ProductName);

            // 2. Get Product By Id
            var getResponse = await client.GetAsync($"/api/v1/products/{createResult.Data.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
            Assert.Equal("Integration Test Laptop", getResult?.Data?.ProductName);
        }
    }
}
