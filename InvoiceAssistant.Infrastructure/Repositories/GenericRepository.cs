using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace InvoiceAssistant.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;
    public GenericRepository(InvoiceAssistantDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }
    public virtual async Task<T> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }
    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
    public virtual async Task DeleteAsync(object id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<PaginatedList<T>> GetPaginatedAsync(int pageIndex, int pageSize)
    {
        return await PaginatedList<T>.CreateAsync(_dbSet.AsNoTracking(), pageIndex, pageSize);
    }
}
