using HMERApi.Models;

namespace HMERApi.Repository.IRepository;

/// <summary>
/// Interface for the Product repository that extends the generic repository
/// with Product-specific operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    // Product-specific repository methods would be defined here
}