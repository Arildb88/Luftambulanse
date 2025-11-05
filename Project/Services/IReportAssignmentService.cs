using System.Threading;
using System.Threading.Tasks;

namespace Gruppe4NLA.Services
{
    // Defines the public contract for report assignment operations
    public interface IReportAssignmentService
    {
        // Assign a report to a specific user
        Task AssignAsync(int reportId, string toUserId, string performedByUserId, CancellationToken ct = default);

        // Remove assignment (returns report to unassigned state)
        Task UnassignAsync(int reportId, string performedByUserId, CancellationToken ct = default);
    }
}