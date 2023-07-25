using System;
using NodaTime;
using NodaTime.TimeZones;
using TCop.Core;

namespace TCop.NodaTime.Time.Builder;

public class PointInTimeBuilder
{
    private readonly PointInTimeBuilderContext _context = new();
    private readonly IClock _systemClock = SystemClock.Instance;

    public PointInTimeBuilder At(int hour, int minute, int second, int millisecond = 0)
    {
        _context.Time = new TimePart(hour, minute, second, millisecond);
        return this;
    }

    public PointInTimeBuilder On(int year, int month, int day)
    {
        _context.Date = new DatePart(year, month, day);
        return this;
    }

    public PointInTimeBuilder InUtc()
    {
        _context.Zone = DateTimeZone.Utc;
        return this;
    }

    public PointInTimeBuilder InZone(DateTimeZone zone)
    {
        return InZone(zone, Resolvers.LenientResolver);
    }

    public PointInTimeBuilder InZone(DateTimeZone zone, ZoneLocalMappingResolver resolver)
    {
        _context.Zone = zone;
        _context.ZoneLocalMappingResolver = resolver;
        return this;
    }

    public PointInTime Build(out DateTimeZone zone)
    {
        if (TimezoneShouldHaveBeenSpecifiedExplicitly())
        {
            throw new PointInTimeBuilderTimezoneNotSpecifiedException();
        }

        _context.Zone ??= DateTimeZone.Utc;

        var baseDateTime = GetBaseInstant();

        var zonedBase = baseDateTime.InZone(_context.Zone);
        
        _context.Date ??= new DatePart(zonedBase.Year, zonedBase.Month, zonedBase.Day);
        _context.Time ??= new TimePart(zonedBase.Hour, zonedBase.Minute, zonedBase.Second, zonedBase.Millisecond);

        var localDateTime = new LocalDateTime(_context.Date.Year, _context.Date.Month, _context.Date.Day,
            _context.Time.Hour, _context.Time.Minute, _context.Time.Second, _context.Time.Millisecond);

        var zonedDateTime = localDateTime.InZone(_context.Zone, Resolvers.LenientResolver);

        zone = _context.Zone;

        return new PointInTime(zonedDateTime.ToInstant().ToUnixTimeTicks());
    }

    private Instant GetBaseInstant()
    {
        var now = _systemClock.GetCurrentInstant();

        if (_context.BaseTimePoint == BaseTimePoint.Current)
            return now;

        var randomDouble = new Random().NextDouble();

        var randomOffsetInTicks = ((Duration.FromDays(30).TotalTicks - Duration.FromSeconds(2).TotalTicks)*(1 - randomDouble));

        if (_context.BaseTimePoint == BaseTimePoint.Past)
        {
            now = now.Minus(Duration.FromTicks(randomOffsetInTicks));
        }

        if (_context.BaseTimePoint == BaseTimePoint.Future)
        {
            now = now.Plus(Duration.FromTicks(randomOffsetInTicks));
        }

        return now;
    }

    private bool TimezoneShouldHaveBeenSpecifiedExplicitly()
    {
        return (_context.Date != null || _context.Time != null) && _context.Zone == null;
    }

    public PointInTimeBuilder InTheFuture()
    {
        _context.BaseTimePoint = BaseTimePoint.Future;
        return this;
    }

    public PointInTimeBuilder InThePast()
    {
        _context.BaseTimePoint = BaseTimePoint.Past;
        return this;
    }
}