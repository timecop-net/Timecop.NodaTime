using NodaTime;
using NodaTime.TimeZones;

namespace TCop.NodaTime.Time.Builder;

internal class PointInTimeBuilderContext
{
    public BaseTimePoint BaseTimePoint { get; set; } = BaseTimePoint.Current;

    public DatePart? Date { get; set; }

    public TimePart? Time { get; set; }

    public DateTimeZone? Zone { get; set; } = null;
    public ZoneLocalMappingResolver ZoneLocalMappingResolver { get; set; } = Resolvers.LenientResolver;
}