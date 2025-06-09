using HomeAutomation.Helpers;

namespace HomeAutomation.Tests.Helpers;

public class HaIdentityTests
{
    [Theory]
    [InlineData("e3eb7e60f8a54f2c93f5254778f84b93", true)]  // Valid user ID
    [InlineData("a1b2c3d4e5f678901234567890abcdef", true)]  // Another valid user ID
    [InlineData(null, true)]                                  // Physical/manual action
    [InlineData("", true)]                                    // Empty string (manual)
    public void IsManuallyOperated_ValidUserIdOrNull_ReturnsTrue(string? userId, bool expected)
    {
        // Act
        var result = HaIdentity.IsManuallyOperated(userId);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("homeassistant.turn_on", false)]
    [InlineData("homeassistant.turn_off", false)]
    [InlineData("system", false)]
    [InlineData("automation.bedroom_lights", false)]
    public void IsManuallyOperated_AutomationUserId_ReturnsFalse(string userId, bool expected)
    {
        // Act
        var result = HaIdentity.IsManuallyOperated(userId);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("homeassistant.turn_on", true)]
    [InlineData("homeassistant.turn_off", true)]
    [InlineData("system", true)]
    [InlineData("automation.bedroom_lights", true)]
    [InlineData("homeassistant.update", true)]
    public void IsAutomated_AutomationUserId_ReturnsTrue(string userId, bool expected)
    {
        // Act
        var result = HaIdentity.IsAutomated(userId);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("e3eb7e60f8a54f2c93f5254778f84b93", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("regularuser", false)]
    public void IsAutomated_NonAutomationUserId_ReturnsFalse(string? userId, bool expected)
    {
        // Act
        var result = HaIdentity.IsAutomated(userId);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsManuallyOperated_And_IsAutomated_AreMutuallyExclusive()
    {
        // Arrange
        var testUserIds = new[]
        {
            "homeassistant.turn_on",
            "e3eb7e60f8a54f2c93f5254778f84b93",
            null,
            "",
            "system",
            "automation.test"
        };

        // Act & Assert
        foreach (var userId in testUserIds)
        {
            var isManual = HaIdentity.IsManuallyOperated(userId);
            var isAutomated = HaIdentity.IsAutomated(userId);

            // They should be mutually exclusive (not both true)
            (isManual && isAutomated).Should().BeFalse(
                $"UserId '{userId}' cannot be both manual and automated");

            // At least one should be true (covers all cases)
            (isManual || isAutomated).Should().BeTrue(
                $"UserId '{userId}' should be either manual or automated");
        }
    }
}