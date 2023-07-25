using System;

namespace TCop.NodaTime.Time.Builder;

public class PointInTimeBuilderTimezoneNotSpecifiedException : Exception
{
    public PointInTimeBuilderTimezoneNotSpecifiedException() : base($"Specify time zone by calling either {nameof(PointInTimeBuilder.InUtc)}() or {nameof(PointInTimeBuilder.InZone)}() when configuring the point in time.")
    {
    }
}