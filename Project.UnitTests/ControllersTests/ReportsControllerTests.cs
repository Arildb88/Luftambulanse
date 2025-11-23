using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Controllers;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.UnitTests.ControllersTests
{

    public class ReportsControllerTests

    {


        [Fact]
        public async Task Index_ReturnsViewResult_WithModel()
        {

            /// Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            // Make minimal report data
            context.Reports.Add(new ReportModel // Ensure at least one report exists
            {
                Id = 1,
                Details = "Test Report",
                DateSent = DateTime.UtcNow,
                SenderName = "Test Sender",
                Type = ReportModel.DangerTypeEnum.Other // Assert later checks for this type
            });
            await context.SaveChangesAsync(); // Save changes to the in-memory database

            // Mock UserManager
            var userStore = new Mock<IUserStore<ApplicationUser>>(); 
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null
            );
            userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(new ApplicationUser { UserName = "testuser" });
            
            // Mock service
            var service = new Mock<IReportAssignmentService>();

            /// Act
            var controller = new ReportsController(context, userManager.Object, service.Object);

            // Mock the User and roles
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            /// Assert
            // Optionally, mock IsInRole
            userManager.Setup(um => um.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                       .ReturnsAsync(false);

            var result = await controller.Index("all", "DateSent", "desc");
            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model); // Ensure model is not null
            Assert.Equal(ReportModel.DangerTypeEnum.Other, ((List<ReportModel>)view.Model)[0].Type); // Check the type of the first report
            Assert.IsType<ReportModel>(((List<ReportModel>)view.Model)[0]); // Ensure model contains ReportModel instances


        }
    }
}

