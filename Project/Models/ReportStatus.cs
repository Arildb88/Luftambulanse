namespace Gruppe4NLA.Models
{
    public enum ReportStatus
    {
        Draft = 0,      // Report is being created but not yet submitted
        Submitted = 1,  // Report has been submitted by the user

        // Later on we can add more statuses like:
        // UnderReview = 2,   // Report is under review by moderators
        // Approved = 3,   // Report has been resolved
        // Rejected = 4    // Report has been rejected
    }
}
