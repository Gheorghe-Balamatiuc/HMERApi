using Microsoft.EntityFrameworkCore;
using HMERApi.Data;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    protected HMERContext _context;
    protected DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    public Repository(
        HMERContext context,
        ILogger logger
    )
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _logger = logger;
    }

    public async Task CreateAsync(T entity)
    {
        try {
            await _dbSet.AddAsync(entity);
        }
        catch (Exception e) 
        {
            _logger.LogError(e, "Error creating entity");
        }
        await SaveAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        try {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception e) 
        {
            _logger.LogError(e, "Error getting entity with id {id}", id);
            return null;
        }
    }
    
    public async Task<List<T>> GetAllAsync(bool tracked = true)
    {
        IQueryable<T> query = _dbSet;

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        try {
            _dbSet.Update(entity);
        }
        catch (Exception e) 
        {
            _logger.LogError(e, "Error updating entity");
        }
        await SaveAsync();
    } 

    public async Task DeleteByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity != null) 
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }
        else 
        {
            _logger.LogWarning("Entity with id {id} not found for deletion", id);
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}