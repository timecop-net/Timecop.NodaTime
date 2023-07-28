using FluentAssertions.NodaTime;
using NodaTime;

namespace TCop.NodaTime.Tests;

public class NodaTimecopResetTests
{
    private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

    [Fact]
    public void Reset_ShouldResumeFrozenTimeAndRevertTimeTravel_AndReturnCurrentInstant()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        tc.TravelTo(frozenAt.Plus(Duration.FromDays(3)));

        var resetTime = tc.Reset();

        Thread.Sleep(100);

        resetTime.Should().BeCloseTo(frozenAt, InstantComparisonPrecision);
        NodaClock.GetCurrentInstant().Should().BeCloseTo(resetTime.Plus(Duration.FromMilliseconds(100)), InstantComparisonPrecision);
    }

    [Fact]
    public void Dispose_ShouldReset()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        tc.TravelTo(frozenAt.Plus(Duration.FromDays(3)));

        tc.Dispose();

        Thread.Sleep(100);

        NodaClock.GetCurrentInstant().Should().BeCloseTo(frozenAt.Plus(Duration.FromMilliseconds(100)), InstantComparisonPrecision);
    }
}