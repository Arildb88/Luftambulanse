using Gruppe4NLA.Controllers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Gruppe4NLA.Models
{
    /// <summary>
    /// Represents the lifecycle status of a report as it moves through the workflow
    /// </summary>
    public enum ReportStatusCase
    {
        Draft = 0,        // Initial state, not yet submitted
        Submitted = 1,    // Sent by user, awaiting assignment
        Assigned = 2,     // Assigned to a specific user for review
        InReview = 3,     // Currently being reviewed
        Completed = 4,    // Successfully processed and closed
        Rejected = 5      // Declined with a reason
    }

    /// <summary>
    /// Main model representing an aviation obstacle report submitted by users
    /// Contains location data, obstacle details, and workflow tracking information
    /// </summary>
    public class ReportModel
    {
        // Primary key for database
        public int Id { get; set; }

        // Links the report to the user who created it
        public string? UserId { get; set; }

        // Name of the person submitting the report (required field)
        [Required(ErrorMessage = "Sendername is required")]
        public string? SenderName { get; set; }

        // Category of obstacle (Cable, Pole, Construction, Other)
        [Required(ErrorMessage = "You need to select an ObstacleType")]
        public DangerTypeEnum? Type { get; set; }

        // Timestamp when the report was originally sent
        public DateTime DateSent { get; set; }

        // Additional information about the obstacle
        public string? Details { get; set; }

        // Height of the obstacle in meters (stored in database)
        // [Range validation commented out but kept for potential future use]
        public double? HeightInMeters { get; set; }

        // Unit preference for display purposes only (not saved to database)
        [NotMapped]
        public string HeightUnit { get; set; } = "meters"; // Can be "meters" or "feet"

        // Indicates whether the obstacle has lighting (important for aviation safety)
        public bool AreLighted { get; set; } = false;

        // Stores geographic location data in GeoJSON format for mapping
        public string? GeoJson { get; set; }

        // User ID of the person assigned to review this report
        public string? AssignedToUserId { get; set; }

        // Timestamp of when the report was assigned
        public DateTime? AssignedAtUtc { get; set; }

        // Current stage in the workflow (Draft, Submitted, Assigned, etc.)
        public ReportStatusCase StatusCase { get; set; } = ReportStatusCase.Draft;

        // Explanation provided when a report is rejected
        public string? RejectReportReason { get; set; }

        // Tracks when the report was last modified
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Defines the types of obstacles that can be reported
        /// </summary>
        public enum DangerTypeEnum
        {
            Cable = 0,         
            Pole = 1,          
            Construction = 2,  
            Other = 99         
        }

        // Timestamp when the report was submitted (moved from Draft to Submitted)
        public DateTime? SubmittedAt { get; set; }
    }

    /// <summary>
    /// Container model for the report form view
    /// Combines a new report being created with a list of previously submitted reports
    /// </summary>
    public class ReportModelWrapper
    {
        // The report currently being filled out
        public ReportModel NewReport { get; set; } = new ReportModel();
        
        // Historical list of reports for display (e.g., user's past submissions)
        public List<ReportModel> SubmittedReport { get; set; } = new List<ReportModel>();
    }
}