using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Gruppe4NLA.DataContext;  // <-- this has AppDbContext
using Gruppe4NLA.Models;       // ReportModel, ReportStatus, ReportAssignmentLog

namespace Gruppe4NLA.Services
{
    public class ReportAssignmentService : IReportAssignmentService
    {
        private readonly AppDbContext _db; // <-- use AppDbContext

        public ReportAssignmentService(AppDbContext db)
        {
            _db = db;
        }

        // Assigns a report to a specific user
        public async Task AssignAsync(int reportId, string toUserId, string performedByUserId, CancellationToken ct = default)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            var fromUserId = report.AssignedToUserId;

            report.AssignedToUserId = toUserId;
            report.AssignedByUserId = performedByUserId;
            report.AssignedAtUtc = DateTime.UtcNow;
            report.StatusCase = ReportStatusCase.Assigned;
            report.UpdatedAtUtc = DateTime.UtcNow;

            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    PerformedByUserId = performedByUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = fromUserId == null ? "Assigned" : "Reassigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Removes assignment from a report
        public async Task UnassignAsync(int reportId, string performedByUserId, CancellationToken ct = default)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId is null) return;

            var fromUserId = report.AssignedToUserId;

            report.AssignedToUserId = null;
            report.AssignedByUserId = performedByUserId;
            report.AssignedAtUtc = DateTime.UtcNow;
            report.Status = ReportStatus.Submitted;
            report.UpdatedAtUtc = DateTime.UtcNow;

            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = fromUserId,
                    ToUserId = null,
                    PerformedByUserId = performedByUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Unassigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }
       
        // Allows a caseworker to assign an unassigned report to themselves
        public async Task SelfAssignAsync(int reportId, string userId, CancellationToken ct = default)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            // Check if report is already assigned
            if (report.AssignedToUserId != null)
            {
                throw new InvalidOperationException("Report is already assigned to someone");
            }

            report.AssignedToUserId = userId;
            report.AssignedByUserId = userId; // Self-assigned
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
                    PerformedByUserId = userId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Self-Assigned"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Approves a report and marks it as completed
        public async Task ApproveAsync(int reportId, string performedByUserId, CancellationToken ct = default)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId == null)
            {
                throw new InvalidOperationException("Report must be assigned before approval");
            }

            report.StatusCase = ReportStatusCase.Completed;
            report.UpdatedAtUtc = DateTime.UtcNow;

            // Log the approval action
            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = report.AssignedToUserId,
                    ToUserId = report.AssignedToUserId,
                    PerformedByUserId = performedByUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Approved"
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        // Rejects a report and marks it as rejected
        public async Task RejectAsync(int reportId, string performedByUserId, CancellationToken ct = default)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct)
                         ?? throw new KeyNotFoundException("Report not found");

            if (report.AssignedToUserId == null)
            {
                throw new InvalidOperationException("Report must be assigned before rejection");
            }

            report.StatusCase = ReportStatusCase.Rejected;
            report.UpdatedAtUtc = DateTime.UtcNow;

            // Log the rejection action
            if (_db.ReportAssignmentLogs != null)
            {
                _db.ReportAssignmentLogs.Add(new ReportAssignmentLog
                {
                    ReportId = reportId,
                    FromUserId = report.AssignedToUserId,
                    ToUserId = report.AssignedToUserId,
                    PerformedByUserId = performedByUserId,
                    PerformedAtUtc = DateTime.UtcNow,
                    Action = "Rejected"
                });
            }

            await _db.SaveChangesAsync(ct);
        }
        
    } 
}