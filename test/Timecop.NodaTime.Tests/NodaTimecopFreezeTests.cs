using FluentAssertions;
using FluentAssertions.NodaTime;
using NodaTime;
using NodaTime.TimeZones;

namespace TCop.NodaTime.Tests;

public class NodaTimecopFreezeTests
{
    private readonly IClock _systemClock = SystemClock.Instance;

    private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

    private readonly DateTimeZone _kyivZone = DateTimeZoneProviders.Tzdb["Europe/Kyiv"];

    [Fact]
    public void Freeze_ShouldFreezeAtCurrentInstant_AndReturnThatInstant()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        var realTimeFrozenAt = _systemClock.GetCurrentInstant();

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt);
        frozenAt.Should().BeCloseTo(realTimeFrozenAt, InstantComparisonPrecision);
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

        var twoPmInKyiv = new LocalDateTime(1990, 12, 2, 14, 53, 27).InZone(_kyivZone, Resolvers.LenientResolver);

        var frozenAt = tc.Freeze(twoPmInKyiv);

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt.ToInstant());
        frozenAt.Should().Be(twoPmInKyiv);
    }

    [Fact]
    public void FreezeUsingBuilder_ShouldFreeze_AndReturnFrozenZonedDateTime()
    {
        using var tc = new NodaTimecop();

        var frozenAt = tc.Freeze(o => o.On(1990, 12, 2).At(14, 53, 27).InZone(_kyivZone));

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().Be(frozenAt.ToInstant());

        var twoPmInKyiv = new LocalDateTime(1990, 12, 2, 14, 53, 27).InZone(_kyivZone, Resolvers.LenientResolver);

        frozenAt.Should().Be(twoPmInKyiv);
    }
}