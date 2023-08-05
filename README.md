# Timecop for NodaTime

Timecop.NodaTime is [Timecop](https://github.com/timecop-net/Timecop) with the NodaTime interface.

Timecop targets .NET Standard 2.0, and supports NodaTime v3.

Timecop.NodaTime and [Timecop](https://github.com/timecop-net/Timecop) have been inspired by the [timecop](https://github.com/travisjeffery/timecop) Ruby gem.

## Installation

You can install [Timecop.NodaTime](https://www.nuget.org/packages/Timecop.NodaTime/) from NuGet using the .NET CLI:

```
dotnet add package Timecop.NodaTime
```

## Basics and usage

To test with Timecop.NodaTime, your code must get the current time using either:
- The `NodaTime.IClock` interface
- The static `NodaClock` class

### Usage with the `NodaTime.IClock` interface

Timecop.NodaTime allows to set up NodaTime's `IClock`.

Here's how to use it:
1. Pass the instance of `NodaTime.IClock` to your code as a method or a constructur parameter and use it to get the current instant
1. In your tests, configure the `NodaTimecop` instance and pass its `Clock` property into the code under test
1. When running in production, pass the `IClock` implementaton that always returns the current instant, such as `NodaTime.SystemClock`

Here's an example:

```csharp
string Greet(IClock clock, DateTimeZone zone)
{
    var timeOfDay = clock.GetCurrentInstant().InZone(zone).Hour switch
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

Greet(tc.Clock, zone); // Good afternoon!

// travel to 8pm local time:
tc.TravelBy(Duration.FromHours(6));

Greet(tc.Clock, zone); // Good evening!
```

### Usage with the `NodaClock` class

NodaTimecop provides the static `NodaClock` class that you can use instead of SystemClock.Instance to get the current instant. Despite `NodaClock` being a static class, it is safe to use in tests that run in parallel as it uses [AsyncLocal](https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) under the hood.

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

NodaTimecop allows to manipulate time in any imaginable way. Use it to freeze time, travel in time, and resume the flow of time.

### Freezing and resuming time

You can freeze the time so that it stops running for your tests until you call `Resume`, `Reset`, or dispose the `NOdaTimecop` instance.

To freeze the time, use either an instance `Freeze` or a static `Frozen` method, both having the same set of overloads. Both methods have the same effect, however the static `Frozen` creates an already frozen `NodaTimecop` instance.

```csharp
using var tc = NodaTimecop.Frozen(Instant.FromUtc(1990, 12, 2, 14, 38, 51));

NodaClock.GetCurrentInstant(); // 1990-12-02 14:38:51

Thread.Sleep(TimeSpan.FromSeconds(3));

NodaClock.GetCurrentInstant(); // 1990-12-02 14:38:51 - still the same value

tc.Resume();

Thread.Sleep(TimeSpan.FromSeconds(3));

NodaClock.GetCurrentInstant(); // 1990-12-02 14:38:54 - moved ~3 seconds forward
```

`Freeze` and `Frozen` have multiple overloads:

```csharp
// freeze at the current instant:
var frozenAt = tc.Freeze();

// freeze at the specific instant:
var instant = Instant.FromUtc(1990, 12, 2, 14, 53, 27);
var frozenAt = tc.Freeze(instant);

// freeze at the specific ZonedDateTime:
ZonedDateTime zonedDateTime = new LocalDateTime(1990, 12, 2, 14, 38, 51).InUtc();
frozenAt = tc.Freeze(zonedDateTime);

// freeze at the specific date or time using a PointInTimeBuilder:
frozenAt = tc.Freeze(o => o.On(1990, 12, 2)
                .At(14, 13, 51)
                .InUtc());
```

### Traveling in time

Use the `TravelBy` method to travel forward and backward in time:

```csharp
using var tc = NodaTimecop.Frozen(Instant.FromUtc(1990, 12, 2, 14, 38, 51));

tc.TravelBy(Duration.FromDays(1));

NodaClock.GetCurrentInstant(); // 1990-12-03 14:38:51 - one day in the future
```

Use the `TravelTo` method to travel to the specific point in time:

```csharp
// travel to the specific instant:
var instant = Instant.FromUtc(1990, 12, 2, 14, 53, 27);
traveledTo = tc.TravelTo(instant);

// travel to the specific ZonedDateTime:
ZonedDateTime zonedDateTime = new LocalDateTime(1990, 12, 2, 14, 38, 51).InUtc();
traveledTo = tc.TravelTo(zonedDateTime);

// travel to the specific point in time configured with a PointInTimeBuilder:
traveledTo = tc.TravelTo(o => o.On(1990, 12, 2)
                .At(14, 13, 51)
                .InUtc());
```

### Using PointInTimeBuilder

`Freeze`, `Frozen`, and `TravelTo` methods each accept a lambda that allows to configure the time with the `PointInTimeBuilder` class.

Use `PointInTimeBuilder` to construct an Instant from its components. When using `At` and `On` methods, always specify the time zone using `InZone` or `InUtc` methods.

```csharp
// When only the date matters:
builder.On(1990, 12, 2).InUtc(); // will use the specified date and current time

// When only the time matters:
builder.At(14, 13, 51).InZone(zone); // will use the specified time and current date

// When neither the date nor time matters, but the Instant must be in the future or in the past:
builder.InTheFuture();

builder.InThePast();
```

## License

NodaTimecop was created by [Dmytro Khmara](https://dmytrokhmara.com) and is licensed under the [MIT license](LICENSE.txt).