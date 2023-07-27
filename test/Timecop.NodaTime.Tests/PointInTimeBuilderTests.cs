using FluentAssertions;
using NodaTime;
using NodaTime.TimeZones;
using TCop.NodaTime.Time.Builder;

namespace TCop.NodaTime.Tests;

public class PointInTimeBuilderTests
{
    private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);
    private static readonly ulong InstantComparisonPrecision = (ulong)Duration.FromMilliseconds(50).TotalTicks;

    private readonly DateTimeZone _kyivZone = DateTimeZoneProviders.Tzdb["Europe/Kyiv"];
    private readonly IClock _systemClock = SystemClock.Instance;

    private readonly PointInTimeBuilder _builder = new();

    [Fact]
    public void InUtc_ShouldReturnCurrentUtcTime()
    {
        //_builder.InUtc();

        //_builder.Build(out var zone).UnixEpochTicks.Should().BeCloseTo(_systemClock.GetCurrentInstant().ToUnixTimeTicks(), InstantComparisonPrecision);
        //zone.Should().Be(DateTimeZone.Utc);

        string Greet(DateTimeZone zone)
        {
            var timeOfDay = NodaClock.GetCurrentInstant().InZone(zone).Hour switch // Use NodaClock instead of SystemClock.Instance
            {
                >= 0 and < 6 => "night",
                >= 6 and < 12 => "morning",
                >= 12 and < 18 => "afternoon",
                _ => "evening"
            };

            return $"Good {timeOfDay}!";
        }

        var zone = DateTimeZoneProviders.Tzdb["Europe/Kyiv"];

        // freeze at 2pm Kyiv time:
        using var tc = NodaTimecop.Frozen(o => o.At(14, 0, 0).InZone(zone));

        Greet(zone); // Good afternoon!

        // travel to 8pm local time:
        tc.TravelBy(Duration.FromHours(6));

        Greet(zone); // Good evening!
    }

    [Fact]
    public void InZone_ShouldReturnCurrentTimeInZone()
    {
        _builder.InZone(_kyivZone);

        _builder.Build(out var zone).UnixEpochTicks.Should().BeCloseTo(_systemClock.GetCurrentInstant().ToUnixTimeTicks(), InstantComparisonPrecision);
        zone.Should().Be(_kyivZone);
    }

    [Fact]
    public void Build_OnWasCalled_ButTimezoneWasNotSpecified_ShouldThrow()
    {
        _builder.On(1990, 12, 2);

        var build = () => _builder.Build(out _);

        build.Should().Throw<PointInTimeBuilderTimezoneNotSpecifiedException>().WithMessage("Specify time zone by calling either InUtc() or InZone() when configuring the point in time.");
    }

    [Fact]
    public void Build_AtWasCalled_ButTimezoneWasNotSpecified_ShouldThrow()
    {
        _builder.At(14, 0, 0);

        var build = () => _builder.Build(out _);

        build.Should().Throw<PointInTimeBuilderTimezoneNotSpecifiedException>().WithMessage("Specify time zone by calling either InUtc() or InZone() when configuring the point in time.");
    }

    [Fact]
    public void InTheFuture_ShouldReturnPointOfTimeInTheFuture()
    {
        _builder.InTheFuture();

        _builder.Build(out var zone).UnixEpochTicks.Should().BeGreaterOrEqualTo(_systemClock.GetCurrentInstant().ToUnixTimeTicks());
        zone.Should().Be(DateTimeZone.Utc);
    }

    [Fact]
    public void InTheFuture_WithExplicitTimeZone_ShouldReturnPointOfTimeInTheFutureInLocalTime()
    {
        _builder.InTheFuture().InZone(_kyivZone);

        _builder.Build(out var zone).UnixEpochTicks.Should().BeGreaterOrEqualTo(_systemClock.GetCurrentInstant().ToUnixTimeTicks());
        zone.Should().Be(_kyivZone);
    }

    [Fact]
    public void InThePast_ShouldReturnPointOfTimeInThePast()
    {
        _builder.InThePast();

        _builder.Build(out var zone).UnixEpochTicks.Should().BeLessOrEqualTo(_systemClock.GetCurrentInstant().ToUnixTimeTicks());
        zone.Should().Be(DateTimeZone.Utc);
    }

    [Fact]
    public void On_ShouldReturnSetDateAndCurrentTime()
    {
        _builder
            .On(1990, 12, 2)
            .InUtc();

        var now = _systemClock.GetCurrentInstant().InUtc();

        var expectedZonedDateTime = new LocalDateTime(1990, 12, 2, now.Hour, now.Minute, now.Second,
            now.Millisecond).InUtc();

        _builder.Build(out var zone).UnixEpochTicks.Should()
            .BeCloseTo(expectedZonedDateTime.ToInstant().ToUnixTimeTicks(), InstantComparisonPrecision);
        
        zone.Should().Be(DateTimeZone.Utc);
    }

    [Fact]
    public void At_ShouldReturnSetTimeAndCurrentDate()
    {
        _builder
            .At(14, 15, 30, 893)
            .InZone(_kyivZone);

        var now = _systemClock.GetCurrentInstant().InZone(_kyivZone);

        var expectedZonedDateTime = new LocalDateTime(now.Year, now.Month, now.Day,
            14, 15, 30, 893).InZone(_kyivZone, Resolvers.LenientResolver);

        _builder.Build(out var zone).UnixEpochTicks.Should()
            .BeCloseTo(expectedZonedDateTime.ToInstant().ToUnixTimeTicks(), InstantComparisonPrecision);

        zone.Should().Be(_kyivZone);
    }
}