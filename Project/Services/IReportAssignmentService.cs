using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// IReportAssignmentService defines the  for assigning reports to users
/// CaseworkerAdm can Assign, Unassign, Self-Assign, Approve, and Reject reports
/// Caseworker can Self-Assign reports or get reports assigned to them
/// </summary>


namespace Gruppe4NLA.Services
{
    // Defines the public contract for report assignment operations
    public interface IReportAssignmentService
    {
        Task AssignAsync(int reportId, string toUserId, CancellationToken ct = default);

        Task UnassignAsync(int reportId, CancellationToken ct = default); 
        
        Task SelfAssignAsync(int reportId, string userId, CancellationToken ct = default);
        
        Task ApproveAsync(int reportId, CancellationToken ct = default);
        
        // Deny cases, with a string reason for rejecting
        Task RejectAsync(int reportId, string? reason, CancellationToken ct = default);
    }
}