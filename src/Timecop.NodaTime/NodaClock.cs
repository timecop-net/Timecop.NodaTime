using NodaTime;

namespace TCop.NodaTime;

public class NodaClock
{
    /// <summary>Returns either current or pre-configured instant. Time can be pre-configured by using <see cref="T:TCop.NodaTime.NodaTimecop" />.</summary>
    public static Instant GetCurrentInstant() => NodaTimecop.Instant;
}