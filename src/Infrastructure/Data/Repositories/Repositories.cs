using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetProductWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public IQueryable<Product> GetProductsQueryable()
    {
        return _context.Products.AsQueryable();
    }
}

public class ItemRepository : GenericRepository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Item>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);
    }
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token), cancellationToken);
    }
}

