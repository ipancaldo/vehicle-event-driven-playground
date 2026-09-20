using System.Linq.Expressions;

namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> FindMany(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> FindOne(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> Exists(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task Add(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
