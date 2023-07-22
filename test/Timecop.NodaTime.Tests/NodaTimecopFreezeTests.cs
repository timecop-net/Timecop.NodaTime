using FluentAssertions;
using NodaTime;

namespace TCop.NodaTime.Tests;

public class NodaTimecopFreezeTests
{
    private readonly IClock _systemClock = SystemClock.Instance;

    private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

    [Fact]
    public void Freeze_ShouldFreezeAtCurrentInstant_AndReturnThatInstant()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        var realTimeFrozenAt = _systemClock.GetCurrentInstant();

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt);
        (realTimeFrozenAt - frozenAt).Should().BeLessOrEqualTo(InstantComparisonPrecision);
    }

    [Fact]
    public void FreezeAtInstant_ShouldFreezeAtSpecifiedInstant_AndReturnFrozenInstant()
    {
        using var tc = new NodaTimecop();

        var freezeAt = Instant.FromUtc(1990, 12, 2, 14, 53, 27);
        var frozenAt = tc.Freeze(freezeAt);

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt);
        frozenAt.Should().Be(freezeAt);
    }

    [Fact]
    public void FreezeAtZonedDateTime_ShouldFreezeAtSpecifiedDateTime_AndReturnFrozenZonedDateTime()
    {
        using var tc = new NodaTimecop();

        var twoPmInKyiv = new ZonedDateTime(Instant.FromUtc(1990, 12, 2, 14, 53, 27),
            DateTimeZoneProviders.Tzdb["Europe/Kyiv"]);

        var frozenAt = tc.Freeze(twoPmInKyiv);

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt.ToInstant());
        frozenAt.Should().Be(twoPmInKyiv);
    }
}