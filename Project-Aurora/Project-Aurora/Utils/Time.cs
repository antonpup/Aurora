using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    /// <summary>
    /// A class for various Time Utilities
    /// </summary>
    public static class Time
    {
        private static DateTime epoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// Gets the seconds since the epoch
        /// </summary>
        /// <returns>The time, in seconds, since the epoch</returns>
        public static long GetSecondsSinceEpoch()
        {
            TimeSpan span = DateTime.Now - epoch;
            return (long)span.TotalSeconds;
        }

        /// <summary>
        /// Gets the milliseconds since the epoch
        /// </summary>
        /// <returns>The time, in milliseconds, since the epoch</returns>
        public static long GetMillisecondsSinceEpoch()
        {
            TimeSpan span = DateTime.Now - epoch;
            return (long)span.TotalMilliseconds;
        }

        /// <summary>
        /// Get the current milliseconds
        /// </summary>
        /// <returns>The current millisecond</returns>
        public static int GetMilliSeconds()
        {
            return DateTime.Now.Millisecond;
        }

        /// <summary>
        /// Gets the current seconds
        /// </summary>
        /// <returns>The current second</returns>
        public static int GetSeconds()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Gets the current hour
        /// </summary>
        /// <returns>The current hour</returns>
        public static int GetHours()
        {
            return DateTime.Now.Hour;
        }

        /// <summary>
        /// Gets the current minute
        /// </summary>
        /// <returns>The current minue</returns>
        public static int GetMinutes()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Determines whether the two specified hour marks are inbetween the current time
        /// </summary>
        /// <param name="start_hour">The starting hour</param>
        /// <param name="end_hour">The ending hour</param>
        /// <returns>A boolean representing if the current time falls between the two hours</returns>
        public static bool IsCurrentTimeBetween(int start_hour, int end_hour)
        {
            return IsCurrentTimeBetween(new TimeSpan(start_hour, 0, 0), new TimeSpan(end_hour, 0, 0));
        }

        /// <summary>
        /// Determines whether the two specified hour and minute marks are inbetween the current time
        /// </summary>
        /// <param name="start_hour">The starting hour</param>
        /// <param name="start_minute">The starting minute</param>
        /// <param name="end_hour">The ending hour</param>
        /// <param name="end_minute">The ending minute</param>
        /// <returns>A boolean representing if the current time falls between the two hours and minutes</returns>
        public static bool IsCurrentTimeBetween(int start_hour, int start_minute, int end_hour, int end_minute)
        {
            return IsCurrentTimeBetween(new TimeSpan(start_hour, start_minute, 0), new TimeSpan(end_hour, end_minute, 0));
        }

        /// <summary>
        /// Determines whether the two specified hour and minute marks are inbetween the current time
        /// </summary>
        /// <param name="start">The starting TimeSpan</param>
        /// <param name="end">The ending TimeSpan</param>
        /// <returns></returns>
        public static bool IsCurrentTimeBetween(TimeSpan start, TimeSpan end)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (start < end)
                return start <= now && now <= end;

            return !(end < now && now < start);
        }
    }
}
