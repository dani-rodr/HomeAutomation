using HomeAutomation.Helpers;

namespace HomeAutomation.Tests.Helpers;

public class TimeRangeTests
{
    [Theory]
    [InlineData(10, 8, 12, true)]  // 10 is between 8 and 12
    [InlineData(6, 8, 12, false)]  // 6 is not between 8 and 12
    [InlineData(14, 8, 12, false)] // 14 is not between 8 and 12
    [InlineData(8, 8, 12, true)]   // 8 is included (start boundary)
    [InlineData(12, 8, 12, false)] // 12 is excluded (end boundary)
    public void IsTimeInBetween_DayTimeRange_ReturnsExpectedResult(int currentHour, int startHour, int endHour, bool expected)
    {
        // Act
        var result = TimeRange.IsTimeInBetween(currentHour, startHour, endHour);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(23, 22, 6, true)]  // 23 is between 22 and 6 (overnight)
    [InlineData(2, 22, 6, true)]   // 2 is between 22 and 6 (overnight)
    [InlineData(5, 22, 6, true)]   // 5 is between 22 and 6 (overnight)
    [InlineData(22, 22, 6, true)]  // 22 is included (start boundary)
    [InlineData(6, 22, 6, false)]  // 6 is excluded (end boundary)
    [InlineData(12, 22, 6, false)] // 12 is not between 22 and 6
    public void IsTimeInBetween_OvernightRange_ReturnsExpectedResult(int currentHour, int startHour, int endHour, bool expected)
    {
        // Act
        var result = TimeRange.IsTimeInBetween(currentHour, startHour, endHour);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 8, 12)]
    [InlineData(24, 8, 12)]
    [InlineData(10, -1, 12)]
    [InlineData(10, 24, 12)]
    [InlineData(10, 8, -1)]
    [InlineData(10, 8, 24)]
    public void IsTimeInBetween_InvalidHours_ThrowsArgumentOutOfRangeException(int currentHour, int startHour, int endHour)
    {
        // Act & Assert
        var action = () => TimeRange.IsTimeInBetween(currentHour, startHour, endHour);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsCurrentTimeInBetween_CallsIsTimeInBetweenWithCurrentHour()
    {
        // This test would need to mock DateTime.Now or use a time provider
        // For now, we just verify it doesn't throw
        
        // Act & Assert
        var action = () => TimeRange.IsCurrentTimeInBetween(10, 14);
        action.Should().NotThrow();
    }
}