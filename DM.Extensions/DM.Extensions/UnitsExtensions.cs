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

        public static double MilesToKilometers(this double miles)
        {
            return miles * 1.6093;
        }

        public static double KilometersToMiles(this double kilometers)
        {
            return kilometers / 1.6093;
        }

        public static double FootsToMeters(this double foots)
        {
            return foots * 0.3048;
        }

        public static double MetersToFoots(this double meters)
        {
            return meters / 0.3048;
        }

        public static double YardsToMeters(this double yards)
        {
            return yards * 0.3048;
        }

        public static double MetersToYards(this double meters)
        {
            return meters / 0.3048;
        }
    }
}
