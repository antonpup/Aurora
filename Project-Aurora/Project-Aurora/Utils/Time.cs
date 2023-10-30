using System;

namespace Aurora.Utils;

/// <summary>
/// A class for various Time Utilities
/// </summary>
public static class Time
{
    private static readonly DateTime Epoch = new(1970, 1, 1);

    /// <summary>
    /// Gets the milliseconds since the epoch
    /// </summary>
    /// <returns>The time, in milliseconds, since the epoch</returns>
    public static long GetMillisecondsSinceEpoch()
    {
        var span = DateTime.Now - Epoch;
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

    public static int GetDays()
    {
        return DateTime.Now.Day;
    }

    public static int GetMonths()
    {
        return DateTime.Now.Month;
    }

    /// <summary>
    /// Determines whether the two specified hour and minute marks are inbetween the current time
    /// </summary>
    /// <param name="startHour">The starting hour</param>
    /// <param name="startMinute">The starting minute</param>
    /// <param name="endHour">The ending hour</param>
    /// <param name="endMinute">The ending minute</param>
    /// <returns>A boolean representing if the current time falls between the two hours and minutes</returns>
    public static bool IsCurrentTimeBetween(int startHour, int startMinute, int endHour, int endMinute)
    {
        return IsCurrentTimeBetween(new TimeSpan(startHour, startMinute, 0), new TimeSpan(endHour, endMinute, 0));
    }

    /// <summary>
    /// Determines whether the two specified hour and minute marks are inbetween the current time
    /// </summary>
    /// <param name="start">The starting TimeSpan</param>
    /// <param name="end">The ending TimeSpan</param>
    /// <returns></returns>
    public static bool IsCurrentTimeBetween(TimeSpan start, TimeSpan end)
    {
        var now = DateTime.Now.TimeOfDay;

        if (start < end)
            return start <= now && now <= end;

        return !(end < now && now < start);
    }
}