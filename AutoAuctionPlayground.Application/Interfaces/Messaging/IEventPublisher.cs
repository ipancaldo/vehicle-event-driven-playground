using AutoAuctionPlayground.Domain.Entities.Outbox;

namespace AutoAuctionPlayground.Application.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
