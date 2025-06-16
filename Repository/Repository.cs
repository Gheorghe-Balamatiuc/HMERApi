using Microsoft.EntityFrameworkCore;
using HMERApi.Data;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

/// <summary>
/// Generic repository implementation that provides CRUD operations for any entity type
/// </summary>
/// <typeparam name="T">The entity type for which this repository provides operations</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected HMERContext _context;
    protected DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    /// <summary>
    /// Constructor for the generic repository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger for error handling</param>
    public Repository(
        HMERContext context,
        ILogger logger
    )
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _logger = logger;
    }

    /// <summary>
    /// Creates a new entity in the database
    /// </summary>
    /// <param name="entity">The entity to create</param>
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

    /// <summary>
    /// Retrieves an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, null otherwise</returns>
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
    
    /// <summary>
    /// Retrieves all entities of type T from the database
    /// </summary>
    /// <param name="tracked">Whether entities should be change-tracked by EF Core</param>
    /// <returns>List of all entities</returns>
    public async Task<List<T>> GetAllAsync(bool tracked = true)
    {
        IQueryable<T> query = _dbSet;

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Updates an existing entity in the database
    /// </summary>
    /// <param name="entity">The updated entity</param>
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

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to delete</param>
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

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}