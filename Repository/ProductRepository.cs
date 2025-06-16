using HMERApi.Data;
using HMERApi.Models;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

/// <summary>
/// Repository for Product entities that extends the generic repository
/// with Product-specific operations
/// </summary>
class ProductRepository
(
    HMERContext context,
    ILogger logger
) : Repository<Product>
    (
        context,
        logger
    ),
    IProductRepository
{
    // Additional Product-specific repository methods would be added here
}