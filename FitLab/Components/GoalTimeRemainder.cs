using System;

namespace FitLab.Components
{
    public static class GoalTimeRemainder
    {
        // Calculates the time remaining for a goal. Returns a string like "15 days remaining" or "Time expired".
        public static string GetTimeRemainingString(FitLab.Data.Goal goal)
        {
            if (goal.IsCompleted)  // If the goal is completed, return the completion date.
                return $"Completed on {goal.CompletedOn?.ToShortDateString()}"; // Use the short date format for better readability.
            DateTime targetDate = goal.Date; // Start with the goal's target date.
            switch (goal.TimeframeUnit) // Determine the timeframe unit and adjust the target date accordingly.
            {
                case "Day(s)":
                    targetDate = targetDate.AddDays(goal.TimeframeAmount); // Add the number of days specified in the goal.
                    break;
                case "Week(s)":
                    targetDate = targetDate.AddDays(goal.TimeframeAmount * 7); // Add the number of weeks specified in the goal, converted to days.
                    break;
                case "Month(s)":
                    targetDate = targetDate.AddMonths(goal.TimeframeAmount); // Add the number of months specified in the goal.
                    break;
                case "Year(s)":
                    targetDate = targetDate.AddYears(goal.TimeframeAmount); // Add the number of years specified in the goal.
                    break;
            }
            var remaining = targetDate - DateTime.UtcNow; // Calculate the time remaining by subtracting the current UTC time from the target date.
            if (remaining.TotalDays <= 0) // If the remaining time is less than or equal to zero, the goal time has expired.
                return "Time expired"; // Return a message indicating that the time has expired.
            return $"{Math.Ceiling(remaining.TotalDays)} day(s) remaining"; // Return the remaining time in days, rounded up to the nearest whole number.
        }
    }
}
