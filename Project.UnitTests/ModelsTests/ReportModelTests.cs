using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gruppe4NLA.Models;
using Xunit;
using Assert = Xunit.Assert;

// Xunit testing for Model --> ReportModel
// Revision needed later.

namespace Gruppe4NLA.Tests
{
    public class ReportModelTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void ReportModel_ValidModel_PassesValidation()
        {
            var model = new ReportModel
            {
                Id = 1,
                SenderName = "Test",
                DangerType = "Kran",
                DateSent = DateTime.Now,
                Details = "Kran: 30 Meter",
                Latitude = 59.91,
                Longitude = 10.75
            };

            var results = ValidateModel(model);
            Assert.Empty(results); // no validation errors
        }

        [Fact]
        public void ReportModel_MissingSenderName_FailsValidation()
        {
            var model = new ReportModel
            {
                Latitude = 59.91,
                Longitude = 10.75
            };

            var results = ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains("SenderName"));
        }

        [Fact]
        public void ReportModel_MissingLatitudeLongitude_FailsValidation()
        {
            var model = new ReportModel
            {
                SenderName = "Test User"
            };

            var results = ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains("Latitude"));
            Assert.Contains(results, v => v.MemberNames.Contains("Longitude"));
        }

        [Theory]
        [InlineData(-91)]
        [InlineData(91)]
        public void ReportModel_LatitudeOutOfRange_FailsValidation(double invalidLatitude)
        {
            var model = new ReportModel
            {
                SenderName = "Test",
                Latitude = invalidLatitude,
                Longitude = 10
            };

            var results = ValidateModel(model);
            Assert.Contains(results, v => v.MemberNames.Contains("Latitude"));
        }

        [Theory]
        [InlineData(-181)]
        [InlineData(181)]
        public void ReportModel_LongitudeOutOfRange_FailsValidation(double invalidLongitude)
        {
            var model = new ReportModel
            {
                SenderName = "Test",
                Latitude = 59,
                Longitude = invalidLongitude
            };

            var results = ValidateModel(model);
            Assert.Contains(results, v => v.MemberNames.Contains("Longitude"));
        }

        [Fact]
        public void ReportModelWrapper_HasDefaultValues()
        {
            var wrapper = new ReportModelWrapper();

            Assert.NotNull(wrapper.NewReport);
            Assert.NotNull(wrapper.SubmittedReport);
            Assert.Empty(wrapper.SubmittedReport);
        }
    }
}
