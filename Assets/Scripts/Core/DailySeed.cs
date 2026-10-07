using System;

namespace Tilevault.Core
{
    /// <summary>
    /// Turns a calendar date into the day's seed. Pure and stable: every player
    /// on a given UTC date gets the same board, today and in a year's time.
    /// </summary>
    public static class DailySeed
    {
        /// <summary>Mixes yyyymmdd so consecutive days produce unrelated boards.</summary>
        public static ulong ForDate(int year, int month, int day)
        {
            ulong packed = (ulong)(year * 10000 + month * 100 + day);

            // SplitMix64's finaliser, used here purely as an avalanche function.
            ulong z = packed + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public static ulong ForDate(DateTime date) => ForDate(date.Year, date.Month, date.Day);

        /// <summary>Stable key for persisting the day's result.</summary>
        public static int DayNumber(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;
    }
}
