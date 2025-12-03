using System;

// ViewModel representing a report item in a list.

namespace Gruppe4NLA.ViewModels
{
    public class ReportListItemVM
    {
        public int Id { get; set; }
        public string? SenderName { get; set; }
        public string? Organization { get; set; }
        public string? Type { get; set; }
        public DateTime DateSent { get; set; }
        public string Status { get; set; } = "";
        public string? AssignedTo { get; set; }
    }
}