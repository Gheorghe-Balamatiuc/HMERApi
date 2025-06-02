namespace HMERApi.Repository.IRepository;

public interface IUnitOfWork
{
    IProductRepository ProductRepository { get; }

    Task CompleteAsync();
}