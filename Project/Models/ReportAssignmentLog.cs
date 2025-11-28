using System;
using Microsoft.EntityFrameworkCore;

// Report assignment log model that tracks changes to report assignments

namespace Gruppe4NLA.Models
{
    // Tracks assignment history for a report (who changed it, when, and how)
    public class ReportAssignmentLog
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string? FromUserId { get; set; }
        public string? ToUserId { get; set; }
        public DateTime PerformedAtUtc { get; set; }
        public string Action { get; set; } = "Assigned";
    }
}