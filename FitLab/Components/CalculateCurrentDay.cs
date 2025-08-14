using System;

namespace FitLab.Helpers
{
    public static class CalculateCurrentDay
    {
        // gets the current workout plan day number based on the created date and plan length
        public static int GetCurrentDayNumber(DateTime createdOn, int planLength, TimeZoneInfo tz)
        {
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            DateTime startLocal;

            if (createdOn.Kind == DateTimeKind.Utc)
                startLocal = TimeZoneInfo.ConvertTimeFromUtc(createdOn, tz).Date;
            else if (createdOn.Kind == DateTimeKind.Unspecified)
                startLocal = DateTime.SpecifyKind(createdOn, DateTimeKind.Local).Date; // assumes createdOn was saved in local
            else
                startLocal = createdOn.Date;

            var days = (nowLocal - startLocal).Days;
            if (days < 0) days = 0; // clamp
            return (days % planLength) + 1; // still 1-indexed
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

