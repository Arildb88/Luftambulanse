using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Controllers;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Gruppe4NLA.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.UnitTests.ControllersTests
{
    public class ReportsController_CreateTests
    {
        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null
            );
        }

        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private void SetupControllerContext(ReportsController controller, string userId = "test-user-id", string userName = "testuser")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName)
            }, "mock"));

            // Mock IUrlHelper to prevent null reference exceptions
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://localhost/test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            
            controller.Url = mockUrlHelper.Object;
        }

        #region Create GET Tests

        [Fact]
        public async Task Create_Get_ReturnsViewWithModel()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();

            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(testUser);
            userManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                       .Returns("test-user-id");

            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            // Act
            var result = await controller.Create(10.5, 59.5);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportModelWrapper>(viewResult.Model);
            Assert.NotNull(model.NewReport);
            Assert.Equal("test-user-id", model.NewReport.UserId);
            Assert.Equal("test@example.com", model.NewReport.SenderName);
        }

        [Fact]
        public async Task Create_Get_WithCoordinates_ReturnsModelWithEmptyReportsList()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();

            var testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com"
            };

            userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(testUser);
            userManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                       .Returns("test-user-id");

            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            // Act
            var result = await controller.Create(63.4305, 10.3951);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportModelWrapper>(viewResult.Model);
            Assert.NotNull(model.SubmittedReport);
            Assert.Empty(model.SubmittedReport); // No existing reports
        }

        #endregion

        #region CreatePopUp POST Tests

        [Fact]
        public async Task CreatePopUp_Post_WithValidModel_SubmitAction_CreatesSubmittedReport()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com",
                    Type = ReportModel.DangerTypeEnum.Construction,
                    Details = "Test construction obstruction",
                    HeightInMeters = 50,
                    HeightUnit = "meters",
                    AreLighted = true,
                    GeoJson = "{\"type\":\"Point\",\"coordinates\":[10.5,59.5]}"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "submit");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirmation", viewResult.ViewName);
            var confirmationModel = Assert.IsType<ConfirmationViewModel>(viewResult.Model);
            Assert.Equal("Report Submitted", confirmationModel.Title);

            var savedReport = await context.Reports.FirstOrDefaultAsync();
            Assert.NotNull(savedReport);
            Assert.Equal(ReportStatusCase.Submitted, savedReport.StatusCase);
            Assert.NotNull(savedReport.SubmittedAt);
        }

        [Fact]
        public async Task CreatePopUp_Post_WithValidModel_SaveAction_CreatesDraftReport()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com",
                    Type = ReportModel.DangerTypeEnum.Pole,
                    Details = "Draft pole report",
                    HeightInMeters = 100,
                    HeightUnit = "meters",
                    AreLighted = false,
                    GeoJson = "{\"type\":\"Point\",\"coordinates\":[11.0,60.0]}"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "save");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirmation", viewResult.ViewName);
            var confirmationModel = Assert.IsType<ConfirmationViewModel>(viewResult.Model);
            Assert.Equal("Draft Saved", confirmationModel.Title);

            var savedReport = await context.Reports.FirstOrDefaultAsync();
            Assert.NotNull(savedReport);
            Assert.Equal(ReportStatusCase.Draft, savedReport.StatusCase);
            Assert.Null(savedReport.SubmittedAt);
        }

        [Fact]
        public async Task CreatePopUp_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            controller.ModelState.AddModelError("Details", "Required");

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "submit");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ReportModelWrapper>(viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task CreatePopUp_Post_WithCableType_SavesCorrectly()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com",
                    Type = ReportModel.DangerTypeEnum.Cable,
                    Details = "Cable obstruction",
                    HeightInMeters = 25,
                    HeightUnit = "meters",
                    AreLighted = false,
                    GeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "submit");

            // Assert
            var savedReport = await context.Reports.FirstOrDefaultAsync();
            Assert.NotNull(savedReport);
            Assert.Equal(ReportModel.DangerTypeEnum.Cable, savedReport.Type);
            Assert.Equal("Cable obstruction", savedReport.Details);
        }

        [Fact]
        public async Task CreatePopUp_Post_WithLightedObstacle_SavesLightingStatus()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com",
                    Type = ReportModel.DangerTypeEnum.Construction,
                    Details = "Lighted construction",
                    HeightInMeters = 75,
                    HeightUnit = "meters",
                    AreLighted = true,
                    GeoJson = "{\"type\":\"Point\",\"coordinates\":[10.5,59.5]}"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "submit");

            // Assert
            var savedReport = await context.Reports.FirstOrDefaultAsync();
            Assert.NotNull(savedReport);
            Assert.True(savedReport.AreLighted);
        }

        [Fact]
        public async Task CreatePopUp_Post_SetsDateSentToCurrentTime()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userManager = CreateMockUserManager();
            var service = new Mock<IReportAssignmentService>();
            var controller = new ReportsController(context, userManager.Object, service.Object);
            SetupControllerContext(controller);

            var beforeTime = DateTime.Now.AddSeconds(-1);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = "test-user-id",
                    SenderName = "test@example.com",
                    Type = ReportModel.DangerTypeEnum.Other,
                    Details = "Test report",
                    HeightInMeters = 50,
                    HeightUnit = "meters",
                    GeoJson = "{\"type\":\"Point\",\"coordinates\":[10.5,59.5]}"
                }
            };

            // Act
            var result = await controller.CreatePopUp(model, "submit");
            var afterTime = DateTime.Now.AddSeconds(1);

            // Assert
            var savedReport = await context.Reports.FirstOrDefaultAsync();
            Assert.NotNull(savedReport);
            Assert.InRange(savedReport.DateSent, beforeTime, afterTime);
        }

        #endregion
    }
}
