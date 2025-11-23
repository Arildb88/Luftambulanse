namespace Gruppe4NLA.Services
{
    public static class MetersFeetConverter
    {
        public static double? ToMeters(double? height, string unit)
        {
            if (height == null) return null;

            if (unit == "feet")
                return height * 0.3048;

            return height;
        }
    }
}
