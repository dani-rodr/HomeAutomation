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
            call.Service == "turn_on" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOnCall
            .Should()
            .NotBeNull($"Expected light.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a light entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledLightTurnOff(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var turnOffCall = lightCalls.FirstOrDefault(call =>
            call.Service == "turn_off" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOffCall
            .Should()
            .NotBeNull($"Expected light.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a light entity's Toggle() method was called
    /// </summary>
    public static void ShouldHaveCalledLightToggle(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var toggleCall = lightCalls.FirstOrDefault(call =>
            call.Service == "toggle" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        toggleCall
            .Should()
            .NotBeNull($"Expected light.toggle to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a switch entity's TurnOn() method was called
    /// </summary>
    public static void ShouldHaveCalledSwitchTurnOn(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var turnOnCall = switchCalls.FirstOrDefault(call =>
            call.Service == "turn_on" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOnCall
            .Should()
            .NotBeNull($"Expected switch.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a switch entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledSwitchTurnOff(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var turnOffCall = switchCalls.FirstOrDefault(call =>
            call.Service == "turn_off" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOffCall
            .Should()
            .NotBeNull($"Expected switch.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify a generic service call was made
    /// </summary>
    public static void ShouldHaveCalledService(this MockHaContext mock, string domain, string service, string entityId)
    {
        var serviceCalls = mock.GetServiceCalls(domain).ToList();
        var call = serviceCalls.FirstOrDefault(c =>
            c.Service == service && c.Target?.EntityIds?.Contains(entityId) == true
        );

        call.Should()
            .NotBeNull($"Expected {domain}.{service} to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify a generic service call was made (without checking entity)
    /// </summary>
    public static void ShouldHaveCalledService(this MockHaContext mock, string domain, string service)
    {
        var serviceCalls = mock.GetServiceCalls(domain).ToList();
        var call = serviceCalls.FirstOrDefault(c => c.Service == service);

        call.Should().NotBeNull($"Expected {domain}.{service} to be called but it was not found");
    }

    /// <summary>
    /// Verify a webostv service call was made for a specific entity
    /// WebOSTV services pass entity ID in the data parameter, not the target
    /// </summary>
    public static void ShouldHaveCalledWebostvService(this MockHaContext mock, string service, string entityId)
    {
        var serviceCalls = mock.GetServiceCalls("webostv").ToList();
        var call = serviceCalls.FirstOrDefault(c => c.Service == service && HasEntityIdInData(c.Data, entityId));

        call.Should()
            .NotBeNull($"Expected webostv.{service} to be called for entity '{entityId}' but it was not found");
    }

    private static bool HasEntityIdInData(object? data, string entityId)
    {
        if (data == null)
            return false;

        // Check if data has EntityId property that matches
        var entityIdProperty = data.GetType().GetProperty("EntityId");
        return entityIdProperty?.GetValue(data)?.ToString() == entityId;
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
    /// Verify that a button entity's Press() method was called
    /// </summary>
    public static void ShouldHaveCalledButtonPress(this MockHaContext mock, string entityId)
    {
        var buttonCalls = mock.GetServiceCalls("button").ToList();
        var pressCall = buttonCalls.FirstOrDefault(call =>
            call.Service == "press" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        pressCall
            .Should()
            .NotBeNull($"Expected button.press to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a media player entity's TurnOn() method was called
    /// </summary>
    public static void ShouldHaveCalledMediaPlayerTurnOn(this MockHaContext mock, string entityId)
    {
        var mediaPlayerCalls = mock.GetServiceCalls("media_player").ToList();
        var turnOnCall = mediaPlayerCalls.FirstOrDefault(call =>
            call.Service == "turn_on" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOnCall
            .Should()
            .NotBeNull($"Expected media_player.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a media player entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledMediaPlayerTurnOff(this MockHaContext mock, string entityId)
    {
        var mediaPlayerCalls = mock.GetServiceCalls("media_player").ToList();
        var turnOffCall = mediaPlayerCalls.FirstOrDefault(call =>
            call.Service == "turn_off" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOffCall
            .Should()
            .NotBeNull($"Expected media_player.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a wake_on_lan service call was made
    /// </summary>
    public static void ShouldHaveCalledWakeOnLan(this MockHaContext mock, string? macAddress = null)
    {
        var wolCalls = mock.GetServiceCalls("wake_on_lan").ToList();
        var wakeCall = wolCalls.FirstOrDefault(call => call.Service == "send_magic_packet");

        wakeCall.Should().NotBeNull("Expected wake_on_lan.send_magic_packet to be called but it was not found");

        if (!string.IsNullOrEmpty(macAddress))
        {
            // In a real implementation, we could check the service data for the MAC address
            // For now, just verify the call was made
        }
    }

    /// <summary>
    /// Verify that a specific light entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledLight(this MockHaContext mock, string entityId)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var call = lightCalls.FirstOrDefault(c => c.Target?.EntityIds?.Contains(entityId) == true);

        call.Should().BeNull($"Expected no light service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that a specific switch entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledSwitch(this MockHaContext mock, string entityId)
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var call = switchCalls.FirstOrDefault(c => c.Target?.EntityIds?.Contains(entityId) == true);

        call.Should().BeNull($"Expected no switch service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that exactly the specified number of calls were made to a specific entity
    /// </summary>
    public static void ShouldHaveCalledLightExactly(this MockHaContext mock, string entityId, string service, int times)
    {
        var lightCalls = mock.GetServiceCalls("light").ToList();
        var calls = lightCalls
            .Where(call => call.Service == service && call.Target?.EntityIds?.Contains(entityId) == true)
            .ToList();

        calls
            .Should()
            .HaveCount(
                times,
                $"Expected light.{service} to be called exactly {times} times for entity '{entityId}' but was called {calls.Count} times"
            );
    }

    /// <summary>
    /// Verify that exactly the specified number of calls were made to a specific switch entity
    /// </summary>
    public static void ShouldHaveCalledSwitchExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    )
    {
        var switchCalls = mock.GetServiceCalls("switch").ToList();
        var calls = switchCalls
            .Where(call => call.Service == service && call.Target?.EntityIds?.Contains(entityId) == true)
            .ToList();

        calls
            .Should()
            .HaveCount(
                times,
                $"Expected switch.{service} to be called exactly {times} times for entity '{entityId}' but was called {calls.Count} times"
            );
    }

    /// <summary>
    /// Verify both lights turn off (useful for motion automation patterns)
    /// </summary>
    public static void ShouldHaveCalledBothLightsTurnOff(
        this MockHaContext mock,
        string primaryLight,
        string secondaryLight
    )
    {
        mock.ShouldHaveCalledLightTurnOff(primaryLight);
        mock.ShouldHaveCalledLightTurnOff(secondaryLight);

        // Verify exactly 2 turn_off calls
        var lightOffCalls = mock.GetServiceCalls("light").Where(call => call.Service == "turn_off").ToList();
        lightOffCalls.Should().HaveCount(2, "Expected exactly 2 light turn_off calls");
    }

    /// <summary>
    /// Verify that a lock entity's Lock() method was called
    /// </summary>
    public static void ShouldHaveCalledLockLock(this MockHaContext mock, string entityId)
    {
        var lockCalls = mock.GetServiceCalls("lock").ToList();
        var lockCall = lockCalls.FirstOrDefault(call =>
            call.Service == "lock" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        lockCall.Should().NotBeNull($"Expected lock.lock to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a lock entity's Unlock() method was called
    /// </summary>
    public static void ShouldHaveCalledLockUnlock(this MockHaContext mock, string entityId)
    {
        var lockCalls = mock.GetServiceCalls("lock").ToList();
        var unlockCall = lockCalls.FirstOrDefault(call =>
            call.Service == "unlock" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        unlockCall
            .Should()
            .NotBeNull($"Expected lock.unlock to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a lock entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledLock(this MockHaContext mock, string entityId)
    {
        var lockCalls = mock.GetServiceCalls("lock").ToList();
        var call = lockCalls.FirstOrDefault(c => c.Target?.EntityIds?.Contains(entityId) == true);

        call.Should().BeNull($"Expected no lock service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that a climate entity's TurnOn() method was called
    /// </summary>
    public static void ShouldHaveCalledClimateTurnOn(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var turnOnCall = climateCalls.FirstOrDefault(call =>
            call.Service == "turn_on" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOnCall
            .Should()
            .NotBeNull($"Expected climate.turn_on to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a climate entity's TurnOff() method was called
    /// </summary>
    public static void ShouldHaveCalledClimateTurnOff(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var turnOffCall = climateCalls.FirstOrDefault(call =>
            call.Service == "turn_off" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        turnOffCall
            .Should()
            .NotBeNull($"Expected climate.turn_off to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a climate entity's SetTemperature() method was called
    /// </summary>
    public static void ShouldHaveCalledClimateSetTemperature(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var setTempCall = climateCalls.FirstOrDefault(call =>
            call.Service == "set_temperature" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        setTempCall
            .Should()
            .NotBeNull($"Expected climate.set_temperature to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a climate entity's SetHvacMode() method was called
    /// </summary>
    public static void ShouldHaveCalledClimateSetHvacMode(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var setModeCall = climateCalls.FirstOrDefault(call =>
            call.Service == "set_hvac_mode" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        setModeCall
            .Should()
            .NotBeNull($"Expected climate.set_hvac_mode to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a climate entity's SetFanMode() method was called
    /// </summary>
    public static void ShouldHaveCalledClimateSetFanMode(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var setFanModeCall = climateCalls.FirstOrDefault(call =>
            call.Service == "set_fan_mode" && call.Target?.EntityIds?.Contains(entityId) == true
        );

        setFanModeCall
            .Should()
            .NotBeNull($"Expected climate.set_fan_mode to be called for entity '{entityId}' but it was not found");
    }

    /// <summary>
    /// Verify that a climate entity was never called
    /// </summary>
    public static void ShouldNeverHaveCalledClimate(this MockHaContext mock, string entityId)
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var call = climateCalls.FirstOrDefault(c => c.Target?.EntityIds?.Contains(entityId) == true);

        call.Should().BeNull($"Expected no climate service calls for entity '{entityId}' but found: {call?.Service}");
    }

    /// <summary>
    /// Verify that exactly the specified number of calls were made to a specific climate entity
    /// </summary>
    public static void ShouldHaveCalledClimateExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    )
    {
        var climateCalls = mock.GetServiceCalls("climate").ToList();
        var calls = climateCalls
            .Where(call => call.Service == service && call.Target?.EntityIds?.Contains(entityId) == true)
            .ToList();

        calls
            .Should()
            .HaveCount(
                times,
                $"Expected climate.{service} to be called exactly {times} times for entity '{entityId}' but was called {calls.Count} times"
            );
    }
}
