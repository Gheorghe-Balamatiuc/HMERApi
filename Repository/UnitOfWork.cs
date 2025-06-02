using HMERApi.Data;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

public class UnitOfWork(
    HMERContext context,
    ILogger<UnitOfWork> logger
    ) : IUnitOfWork, IDisposable
{
    private readonly HMERContext _context = context;
    private readonly ILogger<UnitOfWork> _logger = logger;

    private IProductRepository? productRepository;

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

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    private bool disposed = false;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}