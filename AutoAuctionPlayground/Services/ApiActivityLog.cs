namespace AutoAuctionPlayground.Web.Services
{
    public sealed record ApiActivityEntry(
        DateTime TimestampUtc,
        string Method,
        string Path,
        int StatusCode,
        long DurationMs,
        string? Error,
        string? Note)
    {
        public bool Success => StatusCode is >= 200 and < 300;
    }

    /// <summary>
    /// Every API call the console makes, newest first. This is the Web app's own record of its
    /// outgoing requests — it is not the API's server-side log, which lives in that process.
    /// Bounded on purpose (doc 8.3: UI telemetry must not grow without limit).
    /// </summary>
    public sealed class ApiActivityLog
    {
        private const int MaxEntries = 200;

        private readonly LinkedList<ApiActivityEntry> _entries = new();
        private readonly Lock _gate = new();

        public event Action? Changed;

        public IReadOnlyList<ApiActivityEntry> Snapshot()
        {
            lock (_gate)
                return [.. _entries];
        }

        public void Record(ApiActivityEntry entry)
        {
            lock (_gate)
            {
                _entries.AddFirst(entry);
                while (_entries.Count > MaxEntries)
                    _entries.RemoveLast();
            }

            Changed?.Invoke();
        }

        public void Clear()
        {
            lock (_gate)
                _entries.Clear();

            Changed?.Invoke();
        }
    }
}
