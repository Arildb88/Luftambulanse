using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Gruppe4NLA.Controllers;
using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Models;

namespace Gruppe4NLA.Tests
{
    public class HomeControllerTests
    {
        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = Mock.Of<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var store = Mock.Of<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store, null, null, null, null);
        }

        private HomeController CreateController(ClaimsPrincipal? user = null,
                                                Mock<UserManager<ApplicationUser>>? userManagerMock = null,
                                                Mock<RoleManager<IdentityRole>>? roleManagerMock = null)
        {
            var logger = Mock.Of<ILogger<HomeController>>();
            var um = userManagerMock ?? CreateMockUserManager();
            var rm = roleManagerMock ?? CreateMockRoleManager();

            var controller = new HomeController(logger, um.Object, rm.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) // unauthenticated by default
                }
            };

            return controller;
        }

        [Fact]
        public void FAQ_Returns_ViewResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.FAQ();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void LogIn_RedirectsToIdentityLoginPage()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.LogIn();

            // Assert
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Account/Login", redirect.PageName);
        }

            
        [Fact]
        public void Index_AdminUser_RedirectsToAdminpage()
        {
            // Arrange: authenticated admin user
            var claims = new[] { new Claim(ClaimTypes.Name, "admin@test.com"), new Claim(ClaimTypes.Role, "Admin") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            // userManager is not used by Index branch for role checks, so we can pass default mocks
            var controller = CreateController(principal);

            // Act
            var result = controller.Index();

            // Assert: RedirectToAction to Adminpage
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Adminpage", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public void Leaflet_PilotRole_ReturnsView()
        {
            // Arrange: user with Pilot role
            var claims = new[] { new Claim(ClaimTypes.Name, "pilot@test.com"), new Claim(ClaimTypes.Role, "Pilot") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            var controller = CreateController(principal);

            // Act
            var result = controller.Leaflet();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsView_WithErrorViewModel_HasRequestId()
        {
            // Arrange
            var controller = CreateController();

            // Simulate Activity to allow non-empty RequestId
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("UnitTest").Start();

            // Act
            var result = controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ErrorViewModel>(result!.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}
