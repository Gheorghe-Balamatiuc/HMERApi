using HMERApi.Data;
using HMERApi.Models;
using HMERApi.Repository.IRepository;

namespace HMERApi.Repository;

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
}