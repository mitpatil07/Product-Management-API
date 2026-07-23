using System;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IProductRepository? _products;
    private IItemRepository? _items;
    private IUserRepository? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products => _products ??= new Repositories.ProductRepository(_context);
    public IItemRepository Items => _items ??= new Repositories.ItemRepository(_context);
    public IUserRepository Users => _users ??= new Repositories.UserRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
