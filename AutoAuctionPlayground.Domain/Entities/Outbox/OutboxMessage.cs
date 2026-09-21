namespace AutoAuctionPlayground.Domain.Entities.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; private set; }

        public MessageTypeEnum MessageType { get; private set; }

        /// <summary>
        /// Serialized event/message.
        ///
        /// Example:
        /// {
        ///     "vehicleId": "123",
        ///     "vin": "ABC123",
        ///     "price": 250000
        /// }
        /// </summary>
        public string Payload { get; private set; } = null!;
        public StatusEnum Status { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? ProcessedAtUtc { get; private set; }

        public int RetryCount { get; private set; }

        public string? Error { get; private set; }

        // Used when multiple workers compete for messages.
        public string? LockedBy { get; private set; }
        // Lease/timeout so another worker can recover the message
        // if the current worker crashes.
        public DateTime? LockedUntilUtc { get; private set; }

        private OutboxMessage() { }

        private OutboxMessage(MessageTypeEnum messageType, string payload, DateTime nowUtc)
        {
            Id = Guid.NewGuid();
            MessageType = messageType;
            Payload = payload;
            Status = StatusEnum.Pending;
            CreatedAtUtc = nowUtc;
        }

        public static OutboxMessage Create(MessageTypeEnum messageType, string payload, DateTime nowUtc)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);
            return new OutboxMessage(messageType, payload, nowUtc);
        }

        public void MarkAsProcessing(DateTime? lockedUntil = null, string? lockedBy = "")
        {
            Status = StatusEnum.Processing;
            LockedUntilUtc = lockedUntil == null ? DateTime.UtcNow.AddMinutes(1) : lockedUntil;
            LockedBy = lockedBy;
        }

        public void MarkAsPublished(DateTime processedAtUtc)
        {
            Status = StatusEnum.Published;
            ProcessedAtUtc = processedAtUtc;
            Error = null;
        }

        // Below MaxRetries: goes back to Pending with LockedUntilUtc pushed out by an exponential
        // backoff, so ClaimBatch's own "Pending and lease expired" predicate naturally retries it
        // later without needing a separate query. At MaxRetries: parked as Failed (dead-letter) —
        // no longer claimable, needs manual/operator attention.
        public const int MaxRetries = 5;

        public void RecordFailure(string error, DateTime nowUtc)
        {
            RetryCount++;
            Error = error;
            ProcessedAtUtc = nowUtc;
            LockedBy = null;

            if (RetryCount >= MaxRetries)
            {
                Status = StatusEnum.Failed;
                LockedUntilUtc = null;
                return;
            }

            Status = StatusEnum.Pending;
            LockedUntilUtc = nowUtc + ComputeBackoff(RetryCount);
        }

        private static TimeSpan ComputeBackoff(int retryCount)
        {
            var baseDelay = TimeSpan.FromSeconds(5);
            var maxDelay = TimeSpan.FromMinutes(5);

            // 2^retryCount growth, capped, plus jitter so many messages that failed together don't
            // all wake up and get re-claimed in the same instant.
            var exponential = baseDelay * Math.Pow(2, retryCount - 1);
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

            var delay = exponential + jitter;
            return delay < maxDelay ? delay : maxDelay;
        }
    }
}
