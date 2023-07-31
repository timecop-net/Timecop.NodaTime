using System;
using NodaTime;

namespace TCop.NodaTime;

public class NodaTimecopClock: IClock
{
    private readonly Func<Instant> _getInstant;

    public NodaTimecopClock(Func<Instant> getInstant)
    {
        _getInstant = getInstant;
    }

    public Instant GetCurrentInstant() => _getInstant();
}