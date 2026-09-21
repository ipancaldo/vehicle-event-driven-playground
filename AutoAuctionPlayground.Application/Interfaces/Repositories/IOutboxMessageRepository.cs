using AutoAuctionPlayground.Domain.Entities.Outbox;

namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IOutboxMessageRepository : IBaseRepository<OutboxMessage>
    {
        Task<IReadOnlyList<OutboxMessage>> GetPendingByType(MessageTypeEnum messageType, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OutboxMessage>> GetFailedByType(MessageTypeEnum messageType, CancellationToken cancellationToken = default);

        // Atomically claims up to batchSize Pending (or lease-expired) messages by marking them
        // Processing, so concurrently running workers never claim the same row (Postgres SKIP LOCKED).
        Task<IReadOnlyList<OutboxMessage>> ClaimBatch(int batchSize, string workerId, TimeSpan lease, CancellationToken cancellationToken = default);
    }
}
