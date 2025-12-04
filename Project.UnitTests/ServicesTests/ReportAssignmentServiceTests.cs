using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

// testing for Service --> ReportAssignmentService

namespace Gruppe4NLA.UnitTests.ServicesTests
{
    public class ReportAssignmentServiceTests
    {
        private AppDbContext CreateContext(string dbName)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new AppDbContext(opts);
            return ctx;
        }

        private ReportModel SeedReport(AppDbContext ctx, int id = 1, string? assignedTo = null)
        {
            var r = new ReportModel
            {
                Id = id,
                SenderName = "tester",
                Type = ReportModel.DangerTypeEnum.Cable,
                DateSent = DateTime.UtcNow,
                HeightInMeters = 10,
                HeightUnit = "meters",
                AreLighted = false,
                AssignedToUserId = assignedTo,
                StatusCase = assignedTo == null ? ReportStatusCase.Submitted : ReportStatusCase.Assigned
            };
            ctx.Reports.Add(r);
            ctx.SaveChanges();
            return r;
        }

        [Fact]
        public async Task AssignAsync_SetsAssignedAndCreatesLog()
        {
            var ctx = CreateContext(nameof(AssignAsync_SetsAssignedAndCreatesLog));
            var report = SeedReport(ctx, id: 42, assignedTo: null);

            var svc = new ReportAssignmentService(ctx);

            await svc.AssignAsync(report.Id, "user-123");

            var updated = await ctx.Reports.FindAsync(report.Id);
            Assert.Equal("user-123", updated.AssignedToUserId);
            Assert.Equal(ReportStatusCase.Assigned, updated.StatusCase);
            Assert.NotNull(updated.AssignedAtUtc);
            Assert.NotNull(updated.UpdatedAtUtc);

            // assert log created
            var log = ctx.ReportAssignmentLogs.SingleOrDefault(l => l.ReportId == report.Id);
            Assert.NotNull(log);
            Assert.Equal("Assigned", log.Action); // first assignment -> "Assigned"
            Assert.Null(log.FromUserId);
            Assert.Equal("user-123", log.ToUserId);
        }

        [Fact]
        public async Task SelfAssignAsync_Throws_WhenAlreadyAssigned()
        {
            var ctx = CreateContext(nameof(SelfAssignAsync_Throws_WhenAlreadyAssigned));
            var report = SeedReport(ctx, id: 5, assignedTo: "already");

            var svc = new ReportAssignmentService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.SelfAssignAsync(report.Id, "user-1"));
        }

        [Fact]
        public async Task UnassignAsync_UnassignsAndCreatesLog()
        {
            var ctx = CreateContext(nameof(UnassignAsync_UnassignsAndCreatesLog));
            var report = SeedReport(ctx, id: 6, assignedTo: "user-old");

            var svc = new ReportAssignmentService(ctx);

            await svc.UnassignAsync(report.Id);

            var updated = await ctx.Reports.FindAsync(report.Id);
            Assert.Null(updated.AssignedToUserId);
            Assert.Equal(ReportStatusCase.Submitted, updated.StatusCase);

            var log = ctx.ReportAssignmentLogs.SingleOrDefault(l => l.ReportId == report.Id && l.Action == "Unassigned");
            Assert.NotNull(log);
            Assert.Equal("user-old", log.FromUserId);
            Assert.Null(log.ToUserId);
        }

        [Fact]
        public async Task ApproveAsync_RequiresAssigned_And_Approves()
        {
            var ctx = CreateContext(nameof(ApproveAsync_RequiresAssigned_And_Approves));
            var report = SeedReport(ctx, id: 7, assignedTo: "user-7");

            var svc = new ReportAssignmentService(ctx);

            await svc.ApproveAsync(report.Id);

            var updated = await ctx.Reports.FindAsync(report.Id);
            Assert.Equal(ReportStatusCase.Completed, updated.StatusCase);
            Assert.Null(updated.RejectReportReason);

            var log = ctx.ReportAssignmentLogs.SingleOrDefault(l => l.ReportId == report.Id && l.Action == "Approved");
            Assert.NotNull(log);
        }

        [Fact]
        public async Task RejectAsync_SetsReasonAndCreatesLog()
        {
            var ctx = CreateContext(nameof(RejectAsync_SetsReasonAndCreatesLog));
            var report = SeedReport(ctx, id: 8, assignedTo: "user-8");

            var svc = new ReportAssignmentService(ctx);

            await svc.RejectAsync(report.Id, "not valid");

            var updated = await ctx.Reports.FindAsync(report.Id);
            Assert.Equal(ReportStatusCase.Rejected, updated.StatusCase);
            Assert.Equal("not valid", updated.RejectReportReason);

            var log = ctx.ReportAssignmentLogs.SingleOrDefault(l => l.ReportId == report.Id && l.Action == "Rejected");
            Assert.NotNull(log);
        }

        [Fact]
        public async Task Methods_Throw_KeyNotFound_When_ReportMissing()
        {
            var ctx = CreateContext(nameof(Methods_Throw_KeyNotFound_When_ReportMissing));
            var svc = new ReportAssignmentService(ctx);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.AssignAsync(999, "u"));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.UnassignAsync(999));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.SelfAssignAsync(999, "u"));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.ApproveAsync(999));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.RejectAsync(999, "r"));
        }
    }
}
