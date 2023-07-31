using FluentAssertions;
using FluentAssertions.NodaTime;
using NodaTime;
using TCop.NodaTime;

namespace TCop.Tests
{
    public class IClockTests
    {
        private readonly IClock _systemClock = SystemClock.Instance;
        private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);

        [Fact]
        public void Clock_TimecopWithTravel_ShouldReturnCurrentTime()
        {
            var fiveHours = Duration.FromHours(5);

            using var tc = new NodaTimecop();
            tc.TravelBy(fiveHours);

            tc.Clock.GetCurrentInstant().Should().BeCloseTo(_systemClock.GetCurrentInstant().Plus(fiveHours), InstantComparisonPrecision);
        }

        [Fact]
        public void Clock_TwoClocksShouldBeTheSameInstance()
        {
            using var tc = new NodaTimecop();

            tc.Clock.Should().Be(tc.Clock);
        }
    }
}