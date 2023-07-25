# Timecop for NodaTime

Timecop.NodaTime is [Timecop](https://github.com/timecop-net/Timecop) with the NodaTime interface.

Timecop targets .NET Standard 2.0, and supports NodaTime v3.

Timecop.NodaTime and the original [Timecop](https://github.com/timecop-net/Timecop) have been inspired by the [timecop](https://github.com/travisjeffery/timecop) Ruby gem.

## Installation

You can install [Timecop.NodaTime](https://www.nuget.org/packages/Timecop.NodaTime/) from NuGet using the .NET CLI:

```
dotnet add package Timecop.NodaTime
```

##  Basic usage

Timecop.NodaTime allows you to freeze and travel in time. Just use the `NodaClock` class instead of `SystemClock` to get the current instant with the `GetCurrentInstant` method, and manipulate time with the `NodaTimecop` class in your tests.

```csharp
string Greet(DateTimeZone zone)
{
    var timeOfDay = NodaClock.GetCurrentInstant().InZone(zone).Hour switch // Use NodaClock instead of SystemClock.Instance
    {
        >= 0 and < 6 => "night",
        >= 6 and < 12 => "morning",
        >= 12 and < 18 => "afternoon",
        _ => "evening"
    };

    return $"Good {timeOfDay}!";
}

var zone = DateTimeZoneProviders.Tzdb["Europe/Kyiv"];

// freeze at 2pm Kyiv time:
using var tc = NodaTimecop.Frozen(o => o.At(14, 0, 0).InZone(zone));

Greet(zone); // Good afternoon!

// travel to 8pm local time:
tc.TravelBy(Duration.FromHours(6));

Greet(zone); // Good evening!
```

## Available methods

### Freezing and resuming time

You can freeze the time so that it stops running for your tests until you call `Resume` or dispose the `Timecop` instance.

You freeze time with either an instance `Freeze` or a static `Frozen` method, which both have the same set of overloads. Both methods have the same effect, however the static `Frozen` creates an already frozen `Timecop` instance.

```csharp
using var tc = Timecop.Frozen(Instant.FromUtc(1990, 12, 2, 14, 38, 51));

Clock.Now; // 1990-12-02 14:38:51

Thread.Sleep(TimeSpan.FromSeconds(3));

Clock.Now; // 1990-12-02 14:38:51 - still the same value

tc.Resume();

Thread.Sleep(TimeSpan.FromSeconds(3));

Clock.Now; // 1990-12-02 14:38:54 - time has changed
```

`Freeze` and `Frozen` have multiple overloads:

```csharp
// freeze at the current instant:
var frozenAt = tc.Freeze();

// freeze at the specific instant:
var instant = Instant.FromUtc(1990, 12, 2, 14, 53, 27);
var frozenAt = tc.Freeze(instant);

// freeze at the specified ZonedDateTime:
ZonedDateTime zonedDateTime = new LocalDateTime(1990, 12, 2, 14, 38, 51).InUtc();
frozenAt = tc.Freeze(zonedDateTime);

// freeze at the specified date or time using a PointInTimeBuilder:
frozenAt = tc.Freeze(o => o.On(1990, 12, 2)
                .At(14, 13, 51)
                .InUtc());
```

### Traveling in time

Use  the `TravelBy` method to travel forward and backward in time:

```csharp
using var tc = Timecop.Frozen(Instant.FromUtc(1990, 12, 2, 14, 38, 51));

tc.TravelBy(Duration.FromDays(1));

NodaClock.Now; // 1990-12-03 14:38:51 - one day in the future
```

## License

Timecop was created by [Dmytro Khmara](https://dmytrokhmara.com) and is licensed under the [MIT license](LICENSE.txt).