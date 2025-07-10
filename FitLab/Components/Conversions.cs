namespace FitLab.Components
{
    public static class Conversions
    {
        // Height conversions
        public static double CmToInches(double cm) => cm / 2.54;
        public static double InchesToCm(double inches) => inches * 2.54;

        public static double FeetInchesToInches(int feet, double inches) => (feet * 12) + inches;

        // Weight conversions
        public static double KgToLbs(double kg) => kg * 2.20462;
        public static double LbsToKg(double lbs) => lbs / 2.20462;
    }
}
