using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence;

public static class DbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        if (!await context.Users.AnyAsync())
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = passwordHasher.HashPassword("AdminPassword123!"),
                Role = "Admin",
                CreatedBy = "SystemSeed",
                CreatedOn = DateTime.UtcNow
            };

            var user = new User
            {
                Username = "user",
                PasswordHash = passwordHasher.HashPassword("UserPassword123!"),
                Role = "User",
                CreatedBy = "SystemSeed",
                CreatedOn = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(admin, user);
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "High Performance Laptop",
                    CreatedBy = "SystemSeed",
                    CreatedOn = DateTime.UtcNow,
                    Items = new List<Item>
                    {
                        new Item { Quantity = 15 },
                        new Item { Quantity = 5 }
                    }
                },
                new Product
                {
                    ProductName = "Wireless Mouse",
                    CreatedBy = "SystemSeed",
                    CreatedOn = DateTime.UtcNow,
                    Items = new List<Item>
                    {
                        new Item { Quantity = 50 }
                    }
                },
                new Product
                {
                    ProductName = "Mechanical Keyboard",
                    CreatedBy = "SystemSeed",
                    CreatedOn = DateTime.UtcNow,
                    Items = new List<Item>
                    {
                        new Item { Quantity = 30 }
                    }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}

