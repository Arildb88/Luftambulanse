using Gruppe4NLA.Models;
using Gruppe4NLA.Models.Validation;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Assert = Xunit.Assert;

// Xunit testing for Validation Attribute --> HeightRangeAttribute
namespace Gruppe4NLA.UnitTests.Validation
{
    public class HeightRangeAttributeTests
    {
        private readonly HeightRangeAttribute _attribute = new HeightRangeAttribute();

        // Helper to run validation like ASP.NET Core does
        private ValidationResult? Validate(object? value)
        {
            var context = new ValidationContext(new object());
            return _attribute.GetValidationResult(value, context);
        }

        // -------------------------
        // VALID CASES
        // -------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(250)]
        [InlineData(500)]
        public void HeightRange_ValidValues_Pass(double input)
        {
            // ARRANGE done in helper

            // ACT
            var result = Validate(input);

            // ASSERT
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void HeightRange_NullValue_Passes()
        {
            // ARRANGE

            // ACT
            var result = Validate(null);

            // ASSERT
            Assert.Equal(ValidationResult.Success, result);
        }

        // -------------------------
        // INVALID CASES
        // -------------------------

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(500.1)]
        [InlineData(700)]
        public void HeightRange_InvalidValues_Fail(double input)
        {
            // ARRANGE

            // ACT
            var result = Validate(input);

            // ASSERT
            Assert.NotNull(result);
            Assert.Contains("Height must be between 0 and 500", result!.ErrorMessage);
        }
    }
}