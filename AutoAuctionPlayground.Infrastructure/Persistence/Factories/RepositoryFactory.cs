using AutoAuctionPlayground.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Factories
{
    // Resolves repositories lazily from the current scope so UnitOfWork does not need every
    // repository injected up front; they all share the scope's single DbContext.
    public class RepositoryFactory(IServiceProvider serviceProvider) : IRepositoryFactory
    {
        public TRepository GetRepository<TRepository>() where TRepository : class
            => serviceProvider.GetRequiredService<TRepository>();
    }
}
