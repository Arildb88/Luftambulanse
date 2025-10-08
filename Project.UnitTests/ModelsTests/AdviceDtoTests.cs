using Gruppe4NLA.Models;
using Xunit;
using Assert = Xunit.Assert;

// Xunit testing for Model --> AdviceDto
// Revision needed later.

namespace Gruppe4NLA.Tests
{
    public class AdviceDtoTests
    {
        [Fact]
        public void AdviceDto_CanBeConstructedWithDefaultValues()
        {
            // Arrange & Act
            var dto = new AdviceDto();

            // Assert
            Assert.Equal(0, dto.AdviceId);
            Assert.Null(dto.Title);
            Assert.Null(dto.Description);
        }

        [Fact]
        public void AdviceDto_CanSetAndGetProperties()
        {
            // Arrange
            var dto = new AdviceDto
            {
                AdviceId = 42,
                Title = "Safety Tip",
                Description = "Always check your equipment before use."
            };

            // Act & Assert
            Assert.Equal(42, dto.AdviceId);
            Assert.Equal("Safety Tip", dto.Title);
            Assert.Equal("Always check your equipment before use.", dto.Description);
        }
    }
}
