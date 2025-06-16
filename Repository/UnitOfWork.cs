using HMERApi.Data;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

/// <summary>
/// Implementation of the Unit of Work pattern that coordinates the work of multiple repositories
/// and provides a single transaction scope
/// </summary>
public class UnitOfWork(
    HMERContext context,
    ILogger<UnitOfWork> logger
    ) : IUnitOfWork, IDisposable
{
    private readonly HMERContext _context = context;
    private readonly ILogger<UnitOfWork> _logger = logger;

    // Lazy-loaded repository instances
    private IProductRepository? productRepository;

    /// <summary>
    /// Gets the Product repository, creating it if it doesn't exist yet (lazy loading)
    /// </summary>
    public IProductRepository ProductRepository
    {
        get
        {
            productRepository ??= new ProductRepository(
                _context,
                _logger
            );
            return productRepository;
        }
    }

    /// <summary>
    /// Saves all changes made through the repositories to the database
    /// </summary>
    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    #region Disposal Pattern Implementation
    private bool disposed = false;

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        disposed = true;
    }

    /// <summary>
    /// Public implementation of Dispose pattern callable by consumers
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}