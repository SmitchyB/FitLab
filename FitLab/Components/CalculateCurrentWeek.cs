using System;

namespace FitLab.Components
{
    /// <summary>
    /// This class calculates the current week number based on a given date and time zone.
    /// </summary>
    public static class CalculateCurrentWeek
    {
        public static int GetWeekNumber(DateTime createdOnUtc, TimeZoneInfo timeZone, DateTime? currentUtc = null) 
        {
            var utcNow = currentUtc ?? DateTime.UtcNow; // Use current UTC time if not provided

            createdOnUtc = DateTime.SpecifyKind(createdOnUtc, DateTimeKind.Utc); // Ensure createdOnUtc is in UTC
            utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc); // Ensure utcNow is in UTC

            var createdLocal = TimeZoneInfo.ConvertTimeFromUtc(createdOnUtc, timeZone).Date; // Convert createdOnUtc to local time and get the date part
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone).Date; // Convert utcNow to local time and get the date part

            if (nowLocal < createdLocal) // If the current date is before the created date, return 0
                return 0;

            var elapsed = nowLocal - createdLocal; // Calculate the elapsed time in days
            return (int)(elapsed.TotalDays / 7); // Calculate the week number by dividing the total days by 7
        }
    }
}
