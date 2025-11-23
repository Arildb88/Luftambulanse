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

        [Fact] // test for valid ReportModel
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

        [Fact] // test for default values of ReportModel
        public void ReportModel_DefaultValues_AreCorrect()
        {
            // ARRANGE
            var model = new ReportModel();

            // ACT — nothing

            // ASSERT
            Assert.Equal(ReportStatusCase.Draft, model.StatusCase);
            Assert.Equal("meters", model.HeightUnit);
            Assert.False(model.AreLighted);
        }

        [Fact] // test for missing SenderName property
        public void ReportModel_MissingSenderName_FailsValidation()
        {
            // ARRANGE
            var model = new ReportModel();

            // ACT
            var results = ValidateModel(model);

            // ASSERT
            Assert.Contains(results, r => r.MemberNames.Contains("SenderName"));
        }

        [Fact] // test for ReportModelWrapper default values
        public void ReportModelWrapper_HasDefaultValues()
        {
            var wrapper = new ReportModelWrapper();

            Assert.NotNull(wrapper.NewReport);
            Assert.NotNull(wrapper.SubmittedReport);
            Assert.Empty(wrapper.SubmittedReport);
        }

        [Fact] // test for missing Type property
        public void ReportModel_MissingType_FailsValidation()
        {
            // ARRANGE
            var model = new ReportModel
            {
                SenderName = "Test",
                Type = null
            };

            // ACT
            var results = ValidateModel(model);

            // ASSERT
            Assert.Contains(results, v => v.MemberNames.Contains("Type"));
        }

        [Fact] // test for HeightInMeters allowing null
        public void ReportModel_HeightInMeters_AllowsNull()
        {
            // ARRANGE
            var model = new ReportModel
            {
                SenderName = "Test",
                Type = ReportModel.DangerTypeEnum.Cable,
                HeightInMeters = null
            };

            // ACT
            var results = ValidateModel(model);

            // ASSERT
            Assert.Empty(results);
        }
    }
}
