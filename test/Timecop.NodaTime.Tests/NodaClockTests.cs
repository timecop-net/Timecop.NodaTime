using FluentAssertions.NodaTime;
using NodaTime;

namespace TCop.NodaTime.Tests
{
    public class NodaClockTests
    {
        private static readonly Duration InstantComparisonPrecision = Duration.FromMilliseconds(50);
        private readonly IClock _clock = SystemClock.Instance;

        [Fact]
        public void GetCurrentInstant_NoNodaTimecopCreated_ShouldReturnCurrentRealInstant()
        {
            NodaClock.GetCurrentInstant().Should().BeCloseTo(_clock.GetCurrentInstant(), InstantComparisonPrecision);
        }

        [Fact]
        public void UtcNow_NodaTimecopWithoutTravel_ShouldReturnCurrentTime()
        {
            using var tc = new NodaTimecop();
            NodaClock.GetCurrentInstant().Should().BeCloseTo(_clock.GetCurrentInstant(), InstantComparisonPrecision);
        }
    }
}