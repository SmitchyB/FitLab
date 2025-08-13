using System;

namespace FitLab.Helpers
{
    public static class CalculateCurrentDay
    {
        // gets the current workout plan day number based on the created date and plan length
        public static int GetCurrentDayNumber(DateTime createdOn, int planLength)
        {
            int daysSinceStart = (DateTime.UtcNow.Date - createdOn.Date).Days; // Calculate days since the start date
            return (daysSinceStart % planLength) + 1; // calculate the current day number in the plan, ensuring it's 1-indexed
        }
        // gets the absolute day number based on the created date in local time and the current date in the specified timezone
        public static int GetAbsoluteDayNumber(DateTime createdOnLocal, TimeZoneInfo tz, DateTime? nowUtc = null)
        {
            var now = nowUtc.HasValue ? nowUtc.Value : DateTime.UtcNow; // Use provided UTC time or current UTC time
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(now, tz).Date; // Convert current UTC time to local time and get the date part
            var startLocal = createdOnLocal.Date; // Get the date part of the created date in local time
            var diff = (nowLocal - startLocal).Days; // Calculate the difference in days between now and the start date
            if (diff < 0) diff = 0; // Ensure the difference is not negative
            return diff + 1; // Return the absolute day number, ensuring it's 1-indexed
        }
    }
}

