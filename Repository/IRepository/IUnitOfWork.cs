namespace HMERApi.Repository.IRepository;

/// <summary>
/// Interface for the Unit of Work pattern, which coordinates the work of multiple repositories
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// The Product repository
    /// </summary>
    IProductRepository ProductRepository { get; }

    /// <summary>
    /// Completes the unit of work, saving changes to all repositories
    /// </summary>
    Task CompleteAsync();
}