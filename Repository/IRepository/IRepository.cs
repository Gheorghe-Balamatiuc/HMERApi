namespace HMERApi.Repository.IRepository;

/// <summary>
/// Generic repository interface that defines CRUD operations for entities
/// </summary>
/// <typeparam name="T">The entity type for which this repository provides operations</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities of type T
    /// </summary>
    /// <param name="tracked">If true, entities will be change-tracked by EF Core</param>
    /// <returns>List of all entities</returns>
    Task<List<T>> GetAllAsync(bool tracked = true);
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    Task CreateAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to delete</param>
    Task DeleteByIdAsync(int id);
    
    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task SaveAsync();
}