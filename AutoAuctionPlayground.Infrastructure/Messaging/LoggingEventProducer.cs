using AutoAuctionPlayground.Application.Interfaces.Messaging;
using AutoAuctionPlayground.Domain.Entities.Outbox;
using Microsoft.Extensions.Logging;

namespace AutoAuctionPlayground.Infrastructure.Messaging
{
    // Stands in for a real broker client (RabbitMQ, Service Bus, ...) until one is wired up.
    public class LoggingEventProducer(ILogger<LoggingEventProducer> logger) : IEventPublisher
    {
        public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation(
                "Publishing outbox message {MessageId} ({MessageType})",
                message.Id,
                message.MessageType);

            return Task.CompletedTask;
        }
    }
}
