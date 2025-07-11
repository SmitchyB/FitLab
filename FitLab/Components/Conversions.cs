namespace FitLab.Components
{
    /// <summary>
    /// This class contains methods for converting between different units of measurement.
    /// </summary>
    public static class Conversions
    {
        // Height conversions
        public static double CmToInches(double cm) => cm / 2.54; // 1 inch = 2.54 cm
        public static double InchesToCm(double inches) => inches * 2.54; // 1 cm = 0.393701 inches
        public static double FeetInchesToInches(int feet, double inches) => (feet * 12) + inches; // 1 foot = 12 inches
        public static (int Feet, double Inches) InchesToFeetInches(double inches) // Converts inches to feet and remaining inches
        {
            int feet = (int)(inches / 12); // Calculate feet from inches
            double remainingInches = inches - (feet * 12); // Calculate remaining inches
            return (feet, remainingInches); // Return as a tuple
        }

        // Weight conversions
        public static double KgToLbs(double kg) => kg * 2.20462; // 1 kg = 2.20462 lbs
        public static double LbsToKg(double lbs) => lbs / 2.20462; // 1 lb = 0.453592 kg
    }
}
