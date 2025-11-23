using Microsoft.EntityFrameworkCore;
using Gruppe4NLA.DataContext;  
using Gruppe4NLA.Models;

/// <summary>
/// Workflow service for assigning reports to users
/// Allows caseworkers and admins to manage report assignments
/// CaseworkerAdm can Assign, Unassign, Self-Assign, Approve, and Reject reports
/// Caseworker can Self-Assign reports or get reports assigned to them
/// </summary>


namespace Gruppe4NLA.Services
{
    public class ReportAssignmentService : IReportAssignmentService
    {
        private readonly AppDbContext _db;

        public ReportAssignmentService(AppDbContext db)
        {
            _db = db;
        }

        // Assigns a report to a specific user
        public async Task AssignAsync(int reportId, string toUserId, CancellationToken ct = default)
        {
            // Fetch the report from the database
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");
            
            // Store the previous assignee for logging
            var fromUserId = report.AssignedToUserId;

            // Update the report assignment details
            report.AssignedToUserId = toUserId;
            report.AssignedAtUtc = DateTime.UtcNow;
            report.StatusCase = ReportStatusCase.Assigned;
            report.UpdatedAtUtc = DateTime.UtcNow;

            if (_db.ReportAssignmentLogs != null)
            {
                // Log the assignment action to be stored in the database
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = fromUserId == null ? "Assigned" : "Reassigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Removes assignment from a report
        public async Task UnassignAsync(int reportId, CancellationToken ct = default)
        {
            // Fetch the report from the database
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId is null) return;

            var fromUserId = report.AssignedToUserId;

            report.AssignedToUserId = null;
            report.AssignedAtUtc = DateTime.UtcNow;
            report.StatusCase = ReportStatusCase.Submitted;
            report.UpdatedAtUtc = DateTime.UtcNow;

            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = fromUserId,
                    ToUserId = null,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Unassigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }
       
        // Allows a caseworker to assign an unassigned report to themselves
        public async Task SelfAssignAsync(int reportId, string userId, CancellationToken ct = default)
        {
            // Fetch the report from the database
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            // Check if report is already assigned
            if (report.AssignedToUserId != null)
            {
                throw new InvalidOperationException("Report is already assigned to someone");
            }

            report.AssignedToUserId = userId;
            report.AssignedAtUtc = DateTime.UtcNow;
            report.StatusCase = ReportStatusCase.Assigned;
            report.UpdatedAtUtc = DateTime.UtcNow;

            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = null,
                    ToUserId = userId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Self-Assigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Approves a report and marks it as completed
        public async Task ApproveAsync(int reportId, CancellationToken ct = default)
        {
            // Fetch the report from the database
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId == null)
            {
                throw new InvalidOperationException("Report must be assigned before approval");
            }

            report.StatusCase = ReportStatusCase.Completed;
            report.UpdatedAtUtc = DateTime.UtcNow;

            // If this report was previously rejected, clear any old reason
            report.RejectReportReason = null;

            // Log the approval action
            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = report.AssignedToUserId,
                    ToUserId = report.AssignedToUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Approved"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Rejects a report and marks it as rejected
        public async Task RejectAsync(int reportId, string? reason, CancellationToken ct = default)
        {
            // Fetch the report from the database
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId == null)
            {
                throw new InvalidOperationException("Report must be assigned before rejection");
            }

            report.StatusCase = ReportStatusCase.Rejected;
            report.UpdatedAtUtc = DateTime.UtcNow;
            // Store the recetion reason
            report.RejectReportReason = reason;

            // Log the rejection action
            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = report.AssignedToUserId,
                    ToUserId = report.AssignedToUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Rejected"
                });
            }

            await _db.SaveChangesAsync(ct);
        }
        
    } 
}