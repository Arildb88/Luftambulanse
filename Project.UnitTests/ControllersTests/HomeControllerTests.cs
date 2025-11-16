using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Gruppe4NLA.Controllers;
using Gruppe4NLA.Models;
using System.Diagnostics;
using Assert = Xunit.Assert;



// Xunit testing for Controller --> HomeController
// Revision needed later.

/*
namespace Gruppe4NLA.Tests
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            // Mock the logger dependency
            var mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(mockLogger.Object);
        }

        [Theory]
        [InlineData("Index")]
        [InlineData("Privacy")]
        [InlineData("FAQ")]
        [InlineData("LogIn")]
        [InlineData("SignIn")] 
        [InlineData("Administrator")]
        [InlineData("Leaflet")]
        public void HomeControllerTests_ViewActions_ReturnViewResult(string actionName)
        {
            // Arrange
            // This block tells the test to only look at the controllers declared methods
            // and not inherited methods.

            var method = typeof(HomeController).GetMethod(
                actionName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.DeclaredOnly
                );

            Assert.NotNull(method);

            // Act
            var result = method.Invoke(_controller, null) as IActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HomeControllerTests_Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            Activity.Current = new Activity("TestActivity").Start();

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}*/
