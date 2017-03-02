namespace DM.Extensions
{
    public static class UnitsExtensions
    {
        public static double InchesToMillimeters(this double inches)
        {
            return inches * 25.4;
        }

        public static double MillimetersToInches(this double millimeters)
        {
            return millimeters / 25.4;
        }
    }
}
