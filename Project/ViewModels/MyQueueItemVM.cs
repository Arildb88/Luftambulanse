using System;
using Gruppe4NLA.Models;

// ViewModel representing an item in a user's queue.

namespace Gruppe4NLA.ViewModels
{
    public class MyQueueItemVM
    {
        public int Id { get; set; }
        public string? SenderName { get; set; }
        public string? Organization { get; set; }
        public string Type { get; set; } = "";
        public DateTime DateSent { get; set; }
        public DateTime? AssignedAtUtc { get; set; } 
        public ReportStatusCase StatusCase { get; set; } 
    }
}
