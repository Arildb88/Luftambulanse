using System;
using Gruppe4NLA.Models;

///<summary>
/// ViewModel representing an item in a user's queue.
/// </summary>

namespace Gruppe4NLA.ViewModels
{
    public class MyQueueItemVM
    {
        public int Id { get; set; }
        public string? SenderName { get; set; }
        public string? Organization { get; set; }
        public string Type { get; set; } = "";
        public DateTime DateSent { get; set; }
        public DateTime? AssignedAtUtc { get; set; } // When the report was assigned to the user
        public ReportStatusCase StatusCase { get; set; } // Enum representing the status of the report
    }
}
