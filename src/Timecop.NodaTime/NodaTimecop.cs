using System;
using NodaTime;
using TCop.Core.Context;
using TCop.Core.Time;

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

        /// <summary>Freezes an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> at the current instant.</summary>
        /// <returns>The instant the <see cref="T:TCop.NodaTime.NodaTimecop" /> instance was frozen at.</returns>
        public Instant Freeze()
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context, PointInTime now) => context.Freeze(now));
            return PointInTimeToInstant(frozenAt);
        }

        /// <summary>Freezes an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> at the given instant.</summary>
        /// <param name="freezeAt">The instant to freeze at.</param>
        /// <returns>The instant the <see cref="T:TCop.NodaTime.NodaTimecop" /> instance was frozen at.</returns>
        public Instant Freeze(Instant freezeAt)
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context, PointInTime now) => context.Freeze(InstantToPointInTime(freezeAt)));
            return PointInTimeToInstant(frozenAt);
        }

        /// <summary>Freezes an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> at the specified ZonedDateTime.</summary>
        /// <param name="freezeAt">The date and time to freeze at.</param>
        /// <returns>The date and time in the time zone that the <see cref="T:TCop.NodaTime.NodaTimecop" /> instance was frozen at.</returns>
        public ZonedDateTime Freeze(ZonedDateTime freezeAt)
        {
            var frozenAt = _contextStore.Mutate((ref TimecopContext context) => context.Freeze(InstantToPointInTime(freezeAt.ToInstant())));
            var frozenAtInstant = PointInTimeToInstant(frozenAt);
            return new ZonedDateTime(frozenAtInstant, freezeAt.Zone);
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
        /// <param name="freezeAt">The instant to freeze at.</param>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen(Instant freezeAt)
        {
            var timecop = new NodaTimecop();
            timecop.Freeze(freezeAt);
            return timecop;
        }

        /// <summary>Creates an instance of <see cref="T:TCop.NodaTime.NodaTimecop" /> and freezes it at the specified date and time with a time zone.</summary>
        /// <param name="frozenAt">The date and time to freeze at.</param>
        /// <returns>A frozen <see cref="T:TCop.NodaTime.NodaTimecop" /> instance.</returns>
        public static NodaTimecop Frozen(ZonedDateTime frozenAt)
        {
            var timecop = new NodaTimecop();
            timecop.Freeze(frozenAt);
            return timecop;
        }

        /// <summary>Resumes the flow of time of a frozen instance of <see cref="T:TCop.NodaTime.NodaTimecop" />.</summary>
        /// <returns>The instant the <see cref="T:TCop.NodaTime.NodaTimecop" /> instance represented when it was resumed.</returns>
        public Instant Resume()
        {
            var resumedTime = _contextStore.Mutate((ref TimecopContext context, PointInTime now) => context.Resume(now));
            return PointInTimeToInstant(resumedTime);
        }

        /// <summary>Moves in time backward or forward by the specified amount of time.</summary>
        /// <param name="duration">The amount of time to travel by. Can be positive or negative.</param>
        /// <returns>The instant the <see cref="T:TCop.Timecop" /> instance has traveled to.</returns>
        public Instant TravelBy(Duration duration)
        {
            var timeAfterTravel = _contextStore.Mutate((ref TimecopContext context) => context.TravelBy(duration.ToTimeSpan()));
            return PointInTimeToInstant(timeAfterTravel);
        }

    }
}
