using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Controllers;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.Tests
{
    public class ReportsControllerTests
    {
        
        // Helper: creates controller with seeded in-memory DB
        private ReportsController GetControllerWithData(string dbName)
        {
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            
            var context = new AppDbContext(options);

            // Clean previous test data (ensures isolation)
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed initial data
            context.Reports.AddRange(
                new ReportModel { Id = 1, SenderName = "Alice", DateSent = DateTime.Now.AddDays(-1) },
                new ReportModel { Id = 2, SenderName = "Bob", DateSent = DateTime.Now }
            );
            context.SaveChanges();

            // --- Mock UserManager<ApplicationUser> ---
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );

            // Default: return a fake user
            userManager
                .Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "user1", Email = "test@example.com" });

            userManager
                .Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("user1");

            // --- Mock IReportAssignmentService ---
            var assigner = new Mock<IReportAssignmentService>();

            // Provide default no-op Tasks
            assigner.Setup(a => a.AssignAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);



            return new ReportsController(context, userManager.Object, assigner.Object);
        }
    }
}
