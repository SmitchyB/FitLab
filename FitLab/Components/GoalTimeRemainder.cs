using System;

namespace FitLab.Components
{
    public static class GoalTimeRemainder
    {
        /// <summary>
        /// Calculates the time remaining for a goal.
        /// Returns a string like "15 days remaining" or "Time expired".
        /// </summary>
        public static string GetTimeRemainingString(FitLab.Data.Goal goal)
        {
            if (goal.IsCompleted)
                return $"Completed on {goal.CompletedOn?.ToShortDateString()}";

            DateTime targetDate = goal.Date;

            switch (goal.TimeframeUnit)
            {
                case "Day(s)":
                    targetDate = targetDate.AddDays(goal.TimeframeAmount);
                    break;
                case "Week(s)":
                    targetDate = targetDate.AddDays(goal.TimeframeAmount * 7);
                    break;
                case "Month(s)":
                    targetDate = targetDate.AddMonths(goal.TimeframeAmount);
                    break;
                case "Year(s)":
                    targetDate = targetDate.AddYears(goal.TimeframeAmount);
                    break;
            }

            var remaining = targetDate - DateTime.UtcNow;

            if (remaining.TotalDays <= 0)
                return "Time expired";

            return $"{Math.Ceiling(remaining.TotalDays)} day(s) remaining";
        }
    }
}
