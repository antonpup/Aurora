using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class Time
    {
        private static DateTime epoch = new DateTime(1970, 1, 1);

        public static long GetSecondsSinceEpoch()
        {
            TimeSpan span = DateTime.Now - epoch;
            return (long)span.TotalSeconds;
        }

        public static long GetMillisecondsSinceEpoch()
        {
            TimeSpan span = DateTime.Now - epoch;
            return (long)span.TotalMilliseconds;
        }

        public static int GetMilliSeconds()
        {
            return DateTime.Now.Millisecond;
        }

        public static int GetSeconds()
        {
            return DateTime.Now.Second;
        }

        public static int GetHours()
        {
            return DateTime.Now.Hour;
        }

        public static int GetMinutes()
        {
            return DateTime.Now.Minute;
        }

        public static bool IsCurrentTimeBetween(int start_hour, int end_hour)
        {
            return IsCurrentTimeBetween(new TimeSpan(start_hour, 0, 0), new TimeSpan(end_hour, 0, 0));
        }

        public static bool IsCurrentTimeBetween(int start_hour, int start_minute, int end_hour, int end_minute)
        {
            return IsCurrentTimeBetween(new TimeSpan(start_hour, start_minute, 0), new TimeSpan(end_hour, end_minute, 0));
        }

        public static bool IsCurrentTimeBetween(TimeSpan start, TimeSpan end)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            if (start < end)
                return start <= now && now <= end;

            return !(end < now && now < start);
        }
    }
}
