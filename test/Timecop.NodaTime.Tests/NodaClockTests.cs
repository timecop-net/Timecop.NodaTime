using FluentAssertions;

namespace TCop.NodaTime.Tests
{
    public class NodaClockTests
    {
        private static readonly TimeSpan DateTimeComparisonPrecision = TimeSpan.FromMilliseconds(50);

        [Fact]
        public void GetCurrentInstant_NoNodaTimecopCreated_ShouldReturnCurrentRealInstant()
        {
            NodaClock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
        }

        [Fact]
        public void UtcNow_NodaTimecopWithoutTravel_ShouldReturnCurrentTime()
        {
            using var tc = new NodaTimecop();
            NodaClock.GetCurrentInstant().ToDateTimeUtc().Should().BeCloseTo(DateTime.UtcNow, DateTimeComparisonPrecision);
        }
    }
}