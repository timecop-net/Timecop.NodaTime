using FluentAssertions;
using NodaTime;

namespace TCop.NodaTime.Tests;

public class NodaTimecopTravelTests
{
    [Fact]
    public void TravelBy_InFrozenState_ShouldFreezeAndTravel_AndReturnInstantAfterTravel()
    {
        using var tc = new NodaTimecop();
        var frozenAt = tc.Freeze();
        var traveledTo = tc.TravelBy(Duration.FromDays(3));

        traveledTo.Should().Be(frozenAt.Plus(Duration.FromDays(3)));
    }
}