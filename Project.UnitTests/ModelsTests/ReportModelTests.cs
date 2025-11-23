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
                Type = ReportModel.DangerTypeEnum.Cable,
                DateSent = DateTime.Now,
                Details = "Kran: 30 Meter",
            };

            var results = ValidateModel(model);
            Assert.Empty(results); // no validation errors
        }

        [Fact]
        public void ReportModel_MissingSenderName_FailsValidation()
        {
            var model = new ReportModel
            {
            };

            var results = ValidateModel(model);

            Assert.Contains(results, v => v.MemberNames.Contains("SenderName"));
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
