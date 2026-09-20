namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        TRepository GetRepository<TRepository>() where TRepository : class;
    }
}
