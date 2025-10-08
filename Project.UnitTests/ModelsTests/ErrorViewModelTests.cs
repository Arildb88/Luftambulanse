using Gruppe4NLA.Models;
using Assert = Xunit.Assert;

// Xunit testing for Model --> ErrorViewModel
// Revision needed later.

namespace Gruppe4NLA.Tests
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void ShowRequestId_ReturnsTrue_WhenRequestIdIsSet()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "abc123" };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty(string? requestId)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }
    }
}
