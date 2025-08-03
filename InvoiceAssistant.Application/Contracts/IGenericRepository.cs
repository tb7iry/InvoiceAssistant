using InvoiceAssistant.Application.Common;


namespace InvoiceAssistant.Application.Contracts;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(object id);
    IQueryable<T> GetQueryable();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<PaginatedList<T>> GetPaginatedAsync(int pageIndex, int pageSize);
}
