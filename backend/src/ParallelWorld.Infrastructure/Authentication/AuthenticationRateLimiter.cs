using System.Collections.Concurrent;
using ParallelWorld.Application.Authentication;

namespace ParallelWorld.Infrastructure.Authentication;

internal sealed class AuthenticationRateLimiter(TimeProvider timeProvider) : IAuthenticationRateLimiter
{
    private static readonly TimeSpan WindowLength = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, Counter> _counters = new(StringComparer.Ordinal);
    private long _acquisitionCount;

    internal int PartitionCount => _counters.Count;

    public RateLimitDecision Acquire(string policy, string partition, int permitLimit)
    {
        var now = timeProvider.GetUtcNow();
        if ((Interlocked.Increment(ref _acquisitionCount) & 63) == 0)
        {
            RemoveExpired(now);
        }

        var key = $"{policy}:{partition}";
        while (true)
        {
            var counter = _counters.GetOrAdd(key, _ => new Counter(now, 0));
            lock (counter)
            {
                if (!_counters.TryGetValue(key, out var current)
                    || !ReferenceEquals(counter, current))
                {
                    continue;
                }

                if (now - counter.WindowStartedAt >= WindowLength)
                {
                    counter.WindowStartedAt = now;
                    counter.Count = 0;
                }

                if (counter.Count >= permitLimit)
                {
                    var retryAfter = counter.WindowStartedAt.Add(WindowLength) - now;
                    return new RateLimitDecision(
                        false,
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)));
                }

                counter.Count++;
                return new RateLimitDecision(true, null);
            }
        }
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var pair in _counters)
        {
            lock (pair.Value)
            {
                if (now - pair.Value.WindowStartedAt >= WindowLength)
                {
                    _counters.TryRemove(new KeyValuePair<string, Counter>(pair.Key, pair.Value));
                }
            }
        }
    }

    private sealed class Counter(DateTimeOffset windowStartedAt, int count)
    {
        public DateTimeOffset WindowStartedAt { get; set; } = windowStartedAt;

        public int Count { get; set; } = count;
    }
}
