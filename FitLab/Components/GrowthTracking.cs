using System;
using System.Collections.Generic;
using System.Linq;
using FitLab.Data;

namespace FitLab.Components
{
    // Buckets supported for weight
    public enum Bucket
    {
        Weekly,
        Monthly
    }
    // Represents a point in time with a value, used for weight tracking
    public sealed class TimePoint
    {
        public DateTime T { get; set; } // bucket representative date (local date)
        public double V { get; set; }   // value (lbs; convert to kg in UI if needed)
    }
    // Provides methods for tracking user growth metrics such as weight over time
    public static class GrowthTracking
    {
        public static List<TimePoint> Weight(User user, Bucket bucket = Bucket.Weekly) // Default to weekly bucket
            => Weight(user, TimeZoneInfo.Local, bucket); // Use local timezone by default
        public static List<TimePoint> Weight(User user, TimeZoneInfo tz, Bucket bucket = Bucket.Weekly) // Default to weekly bucket
        {
            if (user == null || user.WeightHistory == null || user.WeightHistory.Count == 0) // Check if user or weight history is null or empty
                return new List<TimePoint>(); // Return empty list if no weight history is available
            var createdOnUtc = EnsureUtc(user.CreatedOn); // Ensure the created date is in UTC
            var createdLocalDate = TimeZoneInfo.ConvertTimeFromUtc(createdOnUtc, tz).Date; // Convert created date to local time and get the date part
            var entries = user.WeightHistory // Filter out entries with zero weight
                .OrderBy(w => w.Date) // Order weight entries by date
                .Select(w => // Convert each weight entry to a TimePoint
                {
                    var entryUtc = EnsureUtc(w.Date); // Ensure the entry date is in UTC
                    var entryLocalDate = TimeZoneInfo.ConvertTimeFromUtc(entryUtc, tz).Date; // Convert entry date to local time and get the date part
                    var weekIndex = GetWeekIndex(createdLocalDate, entryLocalDate); // Calculate the week index based on the created date and entry date
                    return (entryLocalDate, weekIndex, w.WeightLbs); // Create a tuple with the entry local date, week index, and weight in lbs
                })
                .ToList(); // Convert to a list of tuples containing entry local date, week index, and weight in lbs
            switch (bucket) // Determine the bucket type for grouping entries
            {
                case Bucket.Weekly: // Group entries by week
                    {
                        var weekly = entries // Group entries by week index
                            .GroupBy(e => e.weekIndex)  // Group by week index
                            .Select(g => // Select the last entry in each week group
                            {
                                var last = g.Last(); // Get the last entry in the group
                                var weekStart = createdLocalDate.AddDays(g.Key * 7); // Calculate the start date of the week based on the created date and week index
                                return new TimePoint { T = weekStart, V = last.WeightLbs }; // Create a TimePoint with the week start date and the last weight in lbs
                            })
                            .OrderBy(p => p.T) // Order the TimePoints by their date
                            .ToList(); // Convert to a list of TimePoints
                        return weekly; // Return the list of weekly TimePoints
                    }
                case Bucket.Monthly: // Group entries by month
                    {
                        var monthly = entries // Group entries by year and month
                            .GroupBy(e => new { e.entryLocalDate.Year, e.entryLocalDate.Month }) // Group by year and month
                            .Select(g => // Select the last entry in each month group
                            {
                                var last = g.Last(); // Get the last entry in the group
                                var monthStart = new DateTime(g.Key.Year, g.Key.Month, 1); // Calculate the start date of the month
                                return new TimePoint { T = monthStart, V = last.WeightLbs }; // Create a TimePoint with the month start date and the last weight in lbs
                            })
                            .OrderBy(p => p.T) // Order the TimePoints by their date
                            .ToList();// Convert to a list of TimePoints
                        return monthly; // Return the list of monthly TimePoints
                    }
                default: // If an unsupported bucket type is provided, return an empty list
                    return new List<TimePoint>(); // Return empty list for unsupported bucket types
            }
        }
        /// Ensures that the given DateTime is in UTC format
        private static DateTime EnsureUtc(DateTime dt)
        {
            return dt.Kind switch // Check the kind of the DateTime and convert to UTC if necessary
            {
                DateTimeKind.Utc => dt, // If already UTC, return as is
                DateTimeKind.Local => dt.ToUniversalTime(), // If local, convert to UTC
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc) // If unspecified, specify as UTC
            };
        }
        /// Calculates the week index based on the created date and entry date
        private static int GetWeekIndex(DateTime createdLocalDate, DateTime entryLocalDate)
        {
            var deltaDays = (entryLocalDate - createdLocalDate).TotalDays; // Calculate the difference in days between the entry date and the created date
            return (int)Math.Floor(deltaDays / 7.0); // Calculate the week index by dividing the difference by 7 and flooring the result
        }
    }
}
