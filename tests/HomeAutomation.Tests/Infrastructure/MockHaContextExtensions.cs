namespace HomeAutomation.Tests.Infrastructure;

/// <summary>
/// Extension methods for MockHaContext to provide clean, expressive service call verification
/// Based on NetDaemon's actual service call patterns - combines the best of both worlds:
/// our working custom MockHaContext with Moq-style clean assertions
/// </summary>
public static class MockHaContextExtensions
{
    /// <summary>
    /// Verify that a light entity's TurnOn() method was called
    /// </summary>
    public static void ShouldHaveCalledLightTurnOn(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var turnOnCall = lightCalls.FirstOrDefault(call => 
            call.Service == "turn_on" &&
            call.Target?.EntityIds?.Contains(entityId) == true);
        
        turnOnCall.Should().NotBeNull($"Expected light.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a light entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledLightTurnOff(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var turnOffCall = lightCalls.FirstOrDefault(call => 
            call.Service == "turn_off" &&
            call.Target?.EntityIds?.Contains(entityId) == true);
        
        turnOffCall.Should().NotBeNull($"Expected light.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a switch entity's TurnOn() method was called
    /// </summary>
    public static void ShouldHaveCalledSwitchTurnOn(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var turnOnCall = switchCalls.FirstOrDefault(call => 
            call.Service == "turn_on" &&
            call.Target?.EntityIds?.Contains(entityId) == true);
        
        turnOnCall.Should().NotBeNull($"Expected switch.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a switch entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledSwitchTurnOff(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var turnOffCall = switchCalls.FirstOrDefault(call => 
            call.Service == "turn_off" &&
            call.Target?.EntityIds?.Contains(entityId) == true);
        
        turnOffCall.Should().NotBeNull($"Expected switch.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify a generic service call was made
    /// </summary>
    public static void ShouldHaveCalledService(this MockHaContext mock, string domain, string service, string entityId)
    {
        var serviceCalls = mock.GetServiceCalls(domain).ToList();
        var call = serviceCalls.FirstOrDefault(c => 
            c.Service == service &&
            c.Target?.EntityIds?.Contains(entityId) == true);
        
        call.Should().NotBeNull($"Expected {domain}.{service} to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that no service calls were made
    /// </summary>
    public static void ShouldHaveNoServiceCalls(this MockHaContext mock)
    {
        mock.ServiceCalls.Should().BeEmpty("Expected no service calls to be made");
    }

    /// <summary>
    /// Verify that exactly the specified number of service calls were made
    /// </summary>
    public static void ShouldHaveServiceCallCount(this MockHaContext mock, int expectedCount)
    {
        mock.ServiceCalls.Should().HaveCount(expectedCount, $"Expected exactly {expectedCount} service calls");
    }

    /// <summary>
    /// Verify that a specific light entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledLight(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var call = lightCalls.FirstOrDefault(c => 
            c.Target?.EntityIds?.Contains(entityId) == true);
        
        call.Should().BeNull($"Expected no light service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that a specific switch entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledSwitch(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var call = switchCalls.FirstOrDefault(c => 
            c.Target?.EntityIds?.Contains(entityId) == true);
        
        call.Should().BeNull($"Expected no switch service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that exactly the specified number of calls were made to a specific entity
    /// </summary>
    public static void ShouldHaveCalledLightExactly(this MockHaContext mock, string entityId, string service, int times)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var calls = lightCalls.Where(call => 
            call.Service == service &&
            call.Target?.EntityIds?.Contains(entityId) == true).ToList();
        
        calls.Should().HaveCount(times, $"Expected light.{service} to be called exactly {times} times for entity '{entityId}' but was called {calls.Count} times");
    }

    /// <summary>
    /// Verify both lights turn off (useful for motion automation patterns)
    /// </summary>
    public static void ShouldHaveCalledBothLightsTurnOff(this MockHaContext mock, string primaryLight, string secondaryLight)
    {
        mock.ShouldHaveCalledLightTurnOff(primaryLight);
        mock.ShouldHaveCalledLightTurnOff(secondaryLight);
        
        // Verify exactly 2 turn_off calls
        var lightOffCalls = mock.GetServiceCalls("light")
            .Where(call => call.Service == "turn_off")
            .ToList();
        lightOffCalls.Should().HaveCount(2, "Expected exactly 2 light turn_off calls");
    }
}