namespace Gruppe4NLA.Models
{
    // Holds all information the users send in as a report
    public class Report
    {
        public int Id { get; set; }

        // Required means you must set this when creating a Report object
        public required string SenderName { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public required string DangerType { get; set; }

        public DateTime DateSent { get; set; }

        public required string Details { get; set; }

        // Combines Latitude and Longitude into one string for display in views
        public string Coordinates => $"{Latitude:0.000000}, {Longitude:0.000000}";
    }
}