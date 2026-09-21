using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class OutboxMessageRepository(AutoAuctionDbContext context)
        : BaseRepository<OutboxMessage>(context), IOutboxMessageRepository
    {
        public Task<IReadOnlyList<OutboxMessage>> GetPendingByType(MessageTypeEnum messageType, CancellationToken cancellationToken = default)
            => GetOrderedByCreatedAt(messageType, StatusEnum.Pending, cancellationToken);

        public Task<IReadOnlyList<OutboxMessage>> GetFailedByType(MessageTypeEnum messageType, CancellationToken cancellationToken = default)
            => GetOrderedByCreatedAt(messageType, StatusEnum.Failed, cancellationToken);

        // Single round trip: claims the oldest unlocked Pending rows by marking them Processing and
        // leasing them to this worker. FOR UPDATE SKIP LOCKED lets concurrently running workers each
        // grab a disjoint batch without blocking on or re-claiming each other's rows. A lease that has
        // expired (locked_until_utc < now) is treated as claimable again, recovering from a worker
        // that crashed mid-batch.
        public async Task<IReadOnlyList<OutboxMessage>> ClaimBatch(int batchSize, string workerId, TimeSpan lease, CancellationToken cancellationToken = default)
        {
            var lockedUntil = DateTime.UtcNow.Add(lease);

            return await _dbSet.FromSqlInterpolated($@"
                UPDATE outbox_messages
                SET status = {StatusEnum.Processing.ToString()},
                    locked_by = {workerId},
                    locked_until_utc = {lockedUntil}
                WHERE id IN (
                    SELECT id FROM outbox_messages
                    WHERE status = {StatusEnum.Pending.ToString()}
                      AND (locked_until_utc IS NULL OR locked_until_utc < now())
                    ORDER BY created_at_utc
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *;
            ").ToListAsync(cancellationToken);
        }

        private async Task<IReadOnlyList<OutboxMessage>> GetOrderedByCreatedAt(
            MessageTypeEnum messageType,
            StatusEnum status,
            CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(GetNonBlockedByTypePredicate(messageType, status))
                .OrderBy(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        private static Expression<Func<OutboxMessage, bool>> GetNonBlockedByTypePredicate(
            MessageTypeEnum messageType,
            StatusEnum status)
        {
            var now = DateTime.UtcNow;

            return x =>
                x.MessageType == messageType &&
                x.Status == status &&
                (x.LockedUntilUtc == null || x.LockedUntilUtc < now);
        }
    }
}
