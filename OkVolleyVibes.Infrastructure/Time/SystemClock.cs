using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Infrastructure.Time;

internal sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
