using System.ComponentModel.DataAnnotations;
using Gruppe4NLA.Models;
using Gruppe4NLA.Models.Validation;
using Xunit;
using Assert = Xunit.Assert;

namespace Gruppe4NLA.UnitTests.Validation
{
    public class HeightRangeAttributeTests
    {
        private readonly HeightRangeAttribute _attribute = new HeightRangeAttribute();

        // Helper without unit (default to meters)
        private ValidationResult? Validate(object? value)
            => ValidateWithUnit(value, "meters");

        // Helper with unit
        private ValidationResult? ValidateWithUnit(object? value, string unit)
        {
            var model = new ReportModel
            {
                HeightInMeters = value == null ? (double?)null : Convert.ToDouble(value),
                HeightUnit = unit
            };

            var context = new ValidationContext(model)
            {
                MemberName = nameof(ReportModel.HeightInMeters)
            };

            // Dette simulerer det MVC gjør: sender property-verdien + hele modellen
            return _attribute.GetValidationResult(model.HeightInMeters, context);
        }

        // Valid values
        [Theory]
        [InlineData(0)]
        [InlineData(250)]
        [InlineData(500)]
        public void HeightRange_ValidValues_Pass(double input)
        {
            var result = Validate(input);
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void HeightRange_NullValue_Passes()
        {
            var result = Validate(null);
            Assert.Equal(ValidationResult.Success, result);
        }


        // Invalid values
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(500.1)]
        [InlineData(700)]
        public void HeightRange_InvalidValues_Fail(double input)
        {
            var result = Validate(input);

            Assert.NotNull(result);
            Assert.Contains("Height must be between 0 and 500", result!.ErrorMessage);
        }

        // Feet unit tests

        [Theory]
        [InlineData(10.0)]   
        [InlineData(100.0)]  
        public void HeightRange_FeetValues_WithinRange_Pass(double feetInput)
        {
            var result = ValidateWithUnit(feetInput, "feet");
            Assert.Equal(ValidationResult.Success, result);
        }
    }
}
