using NodaTime;

namespace Infrastructure.Extensions
{
    public static class IClockExtensions
    {
        public static DateTimeOffset GetUtcDateTimeOffsetToUnixTimeSeconds(this IClock clock)
        {
            var secondsInstant = clock.GetCurrentInstant().ToUnixTimeSeconds();
            return Instant
                .FromUnixTimeMilliseconds(secondsInstant)
                .InUtc()
                .ToDateTimeOffset();
        }
    }
}
