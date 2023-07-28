using FluentAssertions.NodaTime;
using NodaTime;
using NodaTime.TimeZones;

namespace TCop.NodaTime.Tests;

public class NodaTimecopTravelTests
{
    private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

    private readonly DateTimeZone _kyivZone = DateTimeZoneProviders.Tzdb["Europe/Kyiv"];

    [Fact]
    public void TravelBy_InFrozenState_ShouldFreezeAndTravel_AndReturnInstantAfterTravel()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        var traveledTo = tc.TravelBy(Duration.FromDays(3));

        traveledTo.Should().Be(frozenAt.Plus(Duration.FromDays(3)));
    }

    [Fact]
    public void TravelToInstant_ShouldTravelToSpecifiedInstant_AndReturnThatInstant()
    {
        using var tc = new NodaTimecop();

        var travelTo = Instant.FromUtc(1990, 12, 2, 14, 53, 27);
        var traveledTo = tc.TravelTo(travelTo);

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().BeCloseTo(traveledTo.Plus(Duration.FromMilliseconds(100)), InstantComparisonPrecision);

        traveledTo.Should().Be(travelTo);
    }

    [Fact]
    public void TravelToZonedDateTime_ShouldTravelToSpecifiedZonedDateTime_AndReturnIt()
    {
        using var tc = new NodaTimecop();

        var twoPmInKyiv = new LocalDateTime(1990, 12, 2, 14, 53, 27).InZone(_kyivZone, Resolvers.LenientResolver);
        var traveledTo = tc.TravelTo(twoPmInKyiv);

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().BeCloseTo(twoPmInKyiv.Plus(Duration.FromMilliseconds(100)).ToInstant(), InstantComparisonPrecision);

        traveledTo.Should().Be(twoPmInKyiv);
    }

    [Fact]
    public void TravelToUsingBuilder_ShouldTravel_AndReturnTheZonedDateTimeTheTimeHasTraveledTo()
    {
        using var tc = new NodaTimecop();

        var traveledTo = tc.TravelTo(o => o.On(1990, 12, 2).At(14, 53, 27).InZone(_kyivZone));

        Thread.Sleep(100);

        var twoPmInKyiv = new LocalDateTime(1990, 12, 2, 14, 53, 27).InZone(_kyivZone, Resolvers.LenientResolver);

        NodaClock.GetCurrentInstant().Should().BeCloseTo(twoPmInKyiv.Plus(Duration.FromMilliseconds(100)).ToInstant(), InstantComparisonPrecision);
        
        traveledTo.Should().Be(twoPmInKyiv);
    }
}