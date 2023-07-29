using System;
using NodaTime;
using TCop.Core;
using TCop.Core.Context;
using TCop.NodaTime.Time.Builder;

namespace TCop.NodaTime
{
    public class NodaTimecop : IDisposable
    {
        private readonly TimecopContextStore _contextStore = new();

        public static Instant Instant => Instant.FromUnixTimeTicks(TimecopContextStore.AsyncContextUtcNow.UnixEpochTicks);


        private static PointInTime InstantToPointInTime(Instant instant)
        {
            return new PointInTime(instant.ToUnixTimeTicks());
        }

        private static Instant PointInTimeToInstant(PointInTime pointInTime)
        {
            return Instant.FromUnixTimeTicks(pointInTime.UnixEpochTicks);
        }

        public void Dispose()
        {
            _contextStore.ResetContext();
        }

        /// <summary>Freezes the time at the current instant.</summary>
        /// <returns>The instant the time was frozen at.</returns>
        public Instant Freeze()
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => context.Freeze(realNow));
            return PointInTimeToInstant(frozenAt);
        }

        /// <summary>Freezes the time at the given instant.</summary>
        /// <param name="destination">The instant to freeze at.</param>
        /// <returns>The instant the time was frozen at.</returns>
        public Instant Freeze(Instant destination)
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => context.FreezeAt(InstantToPointInTime(destination), realNow));
            return PointInTimeToInstant(frozenAt);
        }

        /// <summary>Freezes the time at the specified ZonedDateTime.</summary>
        /// <param name="destination">The date and time to freeze at.</param>
        /// <returns>The date and time with a time zone that the time was frozen at.</returns>
        public ZonedDateTime Freeze(ZonedDateTime destination)
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => context.FreezeAt(InstantToPointInTime(destination.ToInstant()), realNow));
            var frozenAtInstant = PointInTimeToInstant(frozenAt);
            return new ZonedDateTime(frozenAtInstant, destination.Zone);
        }

        /// <summary>Freezes the time at the specified date and time.</summary>
        /// <param name="configureDestination">The function to configure the date and time to freeze at.</param>
        /// <returns>The date and time with a time zone that the time was frozen at.</returns>
        public ZonedDateTime Freeze(Action<PointInTimeBuilder> configureDestination)
        {
            var builder = new PointInTimeBuilder();
            configureDestination(builder);
            var frozenAt = Freeze(PointInTimeToInstant(builder.Build(out var zone)));
            return frozenAt.InZone(zone);
        }

        /// <summary>Creates an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> and freezes it at the current instant.</summary>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen()
        {
            var timecop = new NodaTimecop();
            timecop.Freeze();
            return timecop;
        }

        /// <summary>Creates an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> and freezes it at the given instant.</summary>
        /// <param name="destination">The instant to freeze at.</param>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen(Instant destination)
        {
            var timecop = new NodaTimecop();
            timecop.Freeze(destination);
            return timecop;
        }

        /// <summary>Creates an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> and freezes it at the specified date and time with a time zone.</summary>
        /// <param name="destination">The date and time to freeze at.</param>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen(ZonedDateTime destination)
        {
            var timecop = new NodaTimecop();
            timecop.Freeze(destination);
            return timecop;
        }

        /// <summary>Creates an instance of <see cref="T:TCop.Timecop" /> and freezes it at the specified date and time.</summary>
        /// <param name="config">The function to configure the date and time to freeze at.</param>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen(Action<PointInTimeBuilder> config)
        {
            var timecop = new NodaTimecop();
            timecop.Freeze(config);
            return timecop;
        }

        /// <summary>Resumes the flow of time of a frozen instance of <see cref="T:TCop.NodaTime.NodaTimecop" />.</summary>
        /// <returns>The instant the <see cref="T:TCop.NodaTime.NodaTimecop" /> instance represented when it was resumed.</returns>
        public Instant Resume()
        {
            var resumedTime = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => context.Resume(realNow));
            return PointInTimeToInstant(resumedTime);
        }

        /// <summary>Resets the <see cref="T:TCop.NodaTime.NodaTimecop" /> to its initial state so that it represents the current time.</summary>
        /// <returns>The instant that represents the current time.</returns>
        public Instant Reset()
        {
            var resetTime = _contextStore.Mutate((ref TimecopContext context) => context.Reset());
            return PointInTimeToInstant(resetTime);
        }

        /// <summary>Moves in time backward or forward by the given amount of time.</summary>
        /// <param name="duration">The amount of time to travel by. Can be positive or negative.</param>
        /// <returns>The instant that the time has traveled to.</returns>
        public Instant TravelBy(Duration duration)
        {
            var timeAfterTravel = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => context.TravelBy(duration.ToTimeSpan(), realNow));
            return PointInTimeToInstant(timeAfterTravel);
        }

        /// <summary>Moves the time to the given instant.</summary>
        /// <param name="destination">The instant to travel to.</param>
        /// <returns>The instant the time has traveled to.</returns>
        public Instant TravelTo(Instant destination)
        {
            var timeAfterTravel = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) => 
                context.TravelTo(InstantToPointInTime(destination), realNow));
            return PointInTimeToInstant(timeAfterTravel);
        }

        /// <summary>Moves the time to the specified ZonedDateTime.</summary>
        /// <param name="destination">The date and time to travel to.</param>
        /// <returns>The date and time with a time zone that the time has traveled to.</returns>
        public ZonedDateTime TravelTo(ZonedDateTime destination)
        {
            var timeAfterTravel = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) =>
                context.TravelTo(InstantToPointInTime(destination.ToInstant()), realNow));
            var instantAfterTravel = PointInTimeToInstant(timeAfterTravel);
            return new ZonedDateTime(instantAfterTravel, destination.Zone);
        }

        /// <summary>Moves the time to the specified date and time.</summary>
        /// <param name="configureDestination">The function to configure the date and time to travel to.</param>
        /// <returns>The date and time with a time zone that the time has traveled to.</returns>
        public ZonedDateTime TravelTo(Action<PointInTimeBuilder> configureDestination)
        {
            var builder = new PointInTimeBuilder();
            configureDestination(builder);
            var travelTo =builder.Build(out var zone);

            var timeAfterTravel = _contextStore.Mutate((ref TimecopContext context, PointInTime realNow) =>
                context.TravelTo(travelTo, realNow));

            return PointInTimeToInstant(timeAfterTravel).InZone(zone);
        }
    }
}
