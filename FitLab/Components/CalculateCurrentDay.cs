using System;

namespace FitLab.Helpers
{
    public static class CalculateCurrentDay
    {
        public static int GetCurrentDayNumber(DateTime createdOn, int planLength)
        {
            int daysSinceStart = (DateTime.UtcNow.Date - createdOn.Date).Days;
            return (daysSinceStart % planLength) + 1; // +1 to make it 1-indexed
        }
    }
}

