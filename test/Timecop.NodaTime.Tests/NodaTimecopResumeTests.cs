using FluentAssertions.NodaTime;
using NodaTime;

namespace TCop.NodaTime.Tests;

public class NodaTimecopResumeTests
{
    private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

    [Fact]
    public void Resume_ShouldResumeFrozenTime_AndReturnCurrentInstant()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();

        Thread.Sleep(200);

        var resumedTime = tc.Resume();

        Thread.Sleep(200);

        resumedTime.Should().Be(frozenAt);

        NodaClock.GetCurrentInstant().Should().BeCloseTo(frozenAt.Plus(Duration.FromMilliseconds(200)), InstantComparisonPrecision);
    }
}