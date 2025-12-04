using Gruppe4NLA.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assert = Xunit.Assert;

/// Xunit testing for Service --> MetersFeetConverter

namespace Gruppe4NLA.UnitTests.ServicesTests
{
    public class MetersFeetConverterTests
    {
        [Theory]
        [InlineData(10.0, "feet", 3.048)]
        [InlineData(100.0, "feet", 30.48)]
        [InlineData(10.0, "meters", 10.0)]
        [InlineData(0.0, "feet", 0.0)]
        [InlineData(null, "feet", null)]
        public void ToMeters_ConvertsCorrectly(double? input, string unit, double? expected)
        {
            // ARRANGE

            // ACT
            var result = MetersFeetConverter.ToMeters(input, unit);

            // ASSERT
            if (expected == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.InRange(result.Value, expected.Value - 0.001, expected.Value + 0.001);
            }
        }
    }
}