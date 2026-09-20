using AutoAuctionPlayground.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class BaseRepository<T>(AutoAuctionDbContext context) : IBaseRepository<T> where T : class
    {
        protected readonly AutoAuctionDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T?> GetById(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync([id], cancellationToken);

        public async Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default)
            => await _dbSet.ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> FindMany(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

        public Task<T?> FindOne(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<bool> Exists(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => _dbSet.AnyAsync(predicate, cancellationToken);

        public async Task Add(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public void Update(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
                _dbSet.Attach(entity);

            entry.State = EntityState.Modified;
        }

        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}
