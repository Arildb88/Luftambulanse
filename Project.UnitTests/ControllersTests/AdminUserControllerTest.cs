using System.Security.Claims;
using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.UnitTests.ControllersTests
{
    public class AdminUsersControllerTests
    {
        // Creates a mocked UserManager. ASP.NET Identity requires a UserStore,
        // so we mock it even though we don't use its internal behavior.
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        // Creates a mocked RoleManager with a mocked RoleStore.
        private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null
            );
        }

        //// TEST 1 — Adminpage(): It must return the correct view + users + roles
        //[Fact]
        //public async Task Adminpage_ReturnsViewWithUsersAndRoles()
        //{
        //    // Arrange: fake users and roles for the controller to load
        //    var users = new List<ApplicationUser>
        //    {
        //        new ApplicationUser { Id = "1", UserName = "user1" },
        //        new ApplicationUser { Id = "2", UserName = "user2" }
        //    };

        //    var roles = new List<IdentityRole>
        //    {
        //        new IdentityRole("Admin"),
        //        new IdentityRole("Caseworker")
        //    };

        //    var userManagerMock = CreateUserManagerMock();
        //    var roleManagerMock = CreateRoleManagerMock();

        //    // Make .Users and .Roles return our fake lists
        //    userManagerMock.SetupGet(um => um.Users)
        //                   .Returns(users.AsQueryable());

        //    roleManagerMock.SetupGet(rm => rm.Roles)
        //                   .Returns(roles.AsQueryable());

        //    // Controller instance
        //    var controller = new AdminUsersController(
        //        userManagerMock.Object,
        //        roleManagerMock.Object
        //    );

        //    // Needed so TempData works without a real Http request
        //    var httpContext = new DefaultHttpContext();
        //    controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        //    // Act: call the Adminpage action
        //    var result = await controller.Adminpage();

        //    // Assert: correct view is returned
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    Assert.Equal("~/Views/Home/AdminUsers/Adminpage.cshtml", viewResult.ViewName);

        //    // Assert: model contains all users
        //    var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(viewResult.Model);
        //    Assert.Equal(2, model.Count());

        //    // Assert: ViewBag contains alphabetically sorted roles
        //    var allRoles = Assert.IsAssignableFrom<List<string>>(controller.ViewBag.AllRoles);
        //    Assert.Equal(new[] { "Admin", "Caseworker" }, allRoles);
        //}


        // TEST 2 — SetRole(): old roles removed, new role added, redirect OK
        [Fact]
        public async Task SetRole_ReplacesExistingRoles_AndRedirects()
        {
            // Arrange: fake user and role
            var userId = "u1";
            var newRole = "Admin";

            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            var userManagerMock = CreateUserManagerMock();
            var roleManagerMock = CreateRoleManagerMock();

            // Mock Identity operations
            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Caseworker" });
            userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                           .ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(m => m.AddToRoleAsync(user, newRole))
                           .ReturnsAsync(IdentityResult.Success);

            var controller = new AdminUsersController(userManagerMock.Object, roleManagerMock.Object);

            // Act: assign new role
            var result = await controller.SetRole(userId, newRole);

            // Assert: redirect back to Adminpage
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminUsersController.Adminpage), redirect.ActionName);

            // Assert: old role removed once
            userManagerMock.Verify(m => m.RemoveFromRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r => r.Contains("Caseworker"))
            ), Times.Once);

            // Assert: new role added once
            userManagerMock.Verify(m => m.AddToRoleAsync(user, newRole), Times.Once);
        }

        // TEST 3 — DeleteUser(): user cannot delete themselves
        [Fact]
        public async Task DeleteUser_WhenCurrentAdminEqualsTarget_SetsErrorAndDoesNotDelete()
        {
            // Arrange: target user and current admin are the SAME
            var userId = "admin-1";
            var target = new ApplicationUser { Id = userId, Email = "admin@test.no" };
            var currentAdmin = target;

            var userManagerMock = CreateUserManagerMock();
            var roleManagerMock = CreateRoleManagerMock();

            // Mock fetching target user + current admin
            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(target);
            userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(currentAdmin);

            var controller = new AdminUsersController(userManagerMock.Object, roleManagerMock.Object);

            // Provide TempData + HttpContext
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act: attempt to delete yourself
            var result = await controller.DeleteUser(userId);

            // Assert: redirect happens
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminUsersController.Adminpage), redirect.ActionName);

            // Assert: error stored in TempData
            Assert.True(controller.TempData.ContainsKey("AdminUsersError"));
            Assert.Equal("You cannot delete yourself.", controller.TempData["AdminUsersError"]);

            // Assert: DeleteAsync was NOT invoked
            userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        // TEST 4 — DeleteUser(): cannot delete the last admin
        [Fact]
        public async Task DeleteUser_LastAdmin_CannotBeDeleted()
        {
            // Arrange: one admin exists (target)
            var target = new ApplicationUser { Id = "admin-1", UserName = "admin1" };
            var currentAdmin = new ApplicationUser { Id = "admin-2", UserName = "admin2" };

            var userManagerMock = CreateUserManagerMock();
            var roleManagerMock = CreateRoleManagerMock();

            // Mock data: target exists and is an admin
            userManagerMock.Setup(m => m.FindByIdAsync(target.Id)).ReturnsAsync(target);
            userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(currentAdmin);
            userManagerMock.Setup(m => m.IsInRoleAsync(target, "Admin")).ReturnsAsync(true);

            // Only one admin in DB
            userManagerMock.Setup(m => m.GetUsersInRoleAsync("Admin"))
                           .ReturnsAsync(new List<ApplicationUser> { target });

            var controller = new AdminUsersController(userManagerMock.Object, roleManagerMock.Object);

            // Provide TempData
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act: try deleting the last admin
            var result = await controller.DeleteUser(target.Id);

            // Assert: redirect to Adminpage
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AdminUsersController.Adminpage), redirect.ActionName);

            // Assert: correct Norwegian error
            Assert.True(controller.TempData.ContainsKey("AdminUsersError"));
            Assert.Equal("Kan ikke slette siste gjenværende admin.", controller.TempData["AdminUsersError"]);

            // Assert: delete never called
            userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}
