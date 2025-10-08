using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gruppe4NLA.Controllers;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.Tests
{
    public class ReportsControllerTests
    {
        // Helper: creates controller with seeded in-memory DB
        private ReportsController GetControllerWithData(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationContext(options);

            // Clean previous test data (ensures isolation)
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed initial data
            context.Reports.AddRange(
                new ReportModel { Id = 1, SenderName = "Alice", DateSent = DateTime.Now.AddDays(-1) },
                new ReportModel { Id = 2, SenderName = "Bob", DateSent = DateTime.Now }
            );
            context.SaveChanges();

            return new ReportsController(context);
        }

        [Fact]
        public async Task Index_ReturnsViewWithReportsOrderedByDate()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(Index_ReturnsViewWithReportsOrderedByDate));

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<ReportModel>>(result.Model);

            // Compare order by IDs rather than direct sequence equality
            var expectedOrder = model.OrderByDescending(r => r.DateSent).Select(r => r.Id).ToList();
            var actualOrder = model.Select(r => r.Id).ToList();
            Assert.Equal(expectedOrder, actualOrder);
        }

        [Fact]
        public async Task Details_ReturnsView_WhenIdExists()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(Details_ReturnsView_WhenIdExists));

            // Act
            var result = await controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ReportModel>(result.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(Details_ReturnsNotFound_WhenIdDoesNotExist));

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateFromMap_ReturnsViewWithWrapper()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(CreateFromMap_ReturnsViewWithWrapper));

            // Act
            var result = await controller.CreateFromMap(null, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ReportModelWrapper>(result.Model);
            Assert.NotEmpty(model.SubmittedCoordinates);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(Create_Post_InvalidModel_ReturnsViewWithErrors));
            controller.ModelState.AddModelError("Latitude", "Required");

            var model = new ReportModelWrapper();

            // Act
            var result = await controller.Create(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ReportModelWrapper>(result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AddsReportAndReturnsView()
        {
            // Arrange
            var controller = GetControllerWithData(nameof(Create_Post_ValidModel_AddsReportAndReturnsView));
            var newReport = new ReportModelWrapper
            {
                NewCoordinate = new ReportModel
                {
                    SenderName = "Test",
                    Latitude = 59.0,
                    Longitude = 10.0,
                    DangerType = "Crane",
                    Details = "Crane: 40 meters"
                }
            };

            // Act
            var result = await controller.Create(newReport) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ReportModelWrapper>(result.Model);

            // Verify the new record exists
            Assert.Contains(model.SubmittedCoordinates, r => r.SenderName == "Test");
            Assert.Equal("Submitted successfully!", controller.ViewBag.Message);
        }
    }
}
