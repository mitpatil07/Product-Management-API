using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Interfaces
{
    /// <summary>
    /// Generic Repository interface defining standard CRUD operations.
    /// </summary>
    /// <typeparam name="T">Entity class type.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves an entity by its primary key asynchronously.
        /// </summary>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities asynchronously.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an entity as updated.
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Marks an entity as deleted.
        /// </summary>
        void Delete(T entity);
    }

    /// <summary>
    /// Product-specific repository operations.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>
        /// Retrieves a product and eagerly loads its child items.
        /// </summary>
        Task<Product?> GetProductWithItemsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns an IQueryable of Products to support deferred execution for filtering, sorting, searching and paging.
        /// </summary>
        IQueryable<Product> GetProductsQueryable();
    }

    /// <summary>
    /// Item-specific repository operations.
    /// </summary>
    public interface IItemRepository : IGenericRepository<Item>
    {
        /// <summary>
        /// Retrieves all items belonging to a specific product.
        /// </summary>
        Task<IEnumerable<Item>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// User-specific repository operations.
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Retrieves a user by their unique username.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a user that owns the specified refresh token, eager-loading the token details.
        /// </summary>
        Task<User?> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Unit of Work pattern representing a complete database transaction context.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Product repository reference.
        /// </summary>
        IProductRepository Products { get; }

        /// <summary>
        /// Item repository reference.
        /// </summary>
        IItemRepository Items { get; }

        /// <summary>
        /// User repository reference.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Commits all changes made in this unit of work context asynchronously.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
