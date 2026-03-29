using System.Globalization;
using System.Text.Json;

namespace HomeAutomation.Tests.Infrastructure;

public static class MockHaContextExtensions
{
    private enum EntityMatchSource
    {
        Target,
        DataEntityId,
    }

    public static void ShouldHaveCalledLightTurnOn(
        this MockHaContext mock,
        string entityId,
        double? expectedBrightness = null
    )
    {
        var turnOnCall = AssertCalled(mock, "light", "turn_on", entityId);

        if (!expectedBrightness.HasValue)
        {
            return;
        }

        TryGetDataValue<double>(
                turnOnCall.Data,
                out var actualBrightness,
                "Brightness",
                "brightness",
                "BrightnessPct",
                "brightness_pct"
            )
            .Should()
            .BeTrue(
                $"Expected Brightness to be set for entity '{entityId}', but no brightness value was found"
            );

        actualBrightness
            .Should()
            .Be(
                expectedBrightness.Value,
                $"Expected Brightness to be set to {expectedBrightness.Value} for entity '{entityId}'"
            );
    }

    public static void ShouldHaveCalledLightTurnOff(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "light", "turn_off", entityId);

    public static void ShouldHaveCalledLightToggle(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "light", "toggle", entityId);

    public static void ShouldHaveCalledSwitchTurnOn(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "switch", "turn_on", entityId);

    public static void ShouldHaveCalledSwitchTurnOff(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "switch", "turn_off", entityId);

    public static void ShouldHaveCalledCounterIncrement(this MockHaContext mock, string entityId) =>
        ShouldHaveCalledService(mock, "counter", "increment", entityId);

    public static void ShouldHaveCalledCounterDecrement(this MockHaContext mock, string entityId) =>
        ShouldHaveCalledService(mock, "counter", "decrement", entityId);

    public static void ShouldHaveCalledService(
        this MockHaContext mock,
        string domain,
        string service,
        string entityId
    ) => AssertCalled(mock, domain, service, entityId);

    public static void ShouldHaveCalledService(
        this MockHaContext mock,
        string domain,
        string service
    ) => AssertCalled(mock, domain, service);

    public static void ShouldHaveCalledWebostvService(
        this MockHaContext mock,
        string service,
        string entityId
    ) => AssertCalled(mock, "webostv", service, entityId, EntityMatchSource.DataEntityId);

    public static void ShouldHaveNoServiceCalls(this MockHaContext mock)
    {
        mock.ServiceCalls.Should().BeEmpty("Expected no service calls to be made");
    }

    public static void ShouldHaveAnyServiceCalls(this MockHaContext mock)
    {
        mock.ServiceCalls.Should().NotBeEmpty("Expected at least one service call to be made");
    }

    public static void ShouldHaveServiceCallCount(this MockHaContext mock, int expectedCount)
    {
        mock.ServiceCalls.Should().HaveCount(expectedCount);
    }

    public static void ShouldHaveNoServiceCallsForDomain(this MockHaContext mock, string domain)
    {
        mock.GetServiceCalls(domain)
            .Should()
            .BeEmpty($"Expected no {domain} service calls to be made");
    }

    public static void ShouldHaveAnyServiceCallsForDomain(this MockHaContext mock, string domain)
    {
        mock.GetServiceCalls(domain)
            .Should()
            .NotBeEmpty($"Expected at least one {domain} service call to be made");
    }

    public static void ShouldHaveCalledButtonPress(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "button", "press", entityId);

    public static void ShouldHaveCalledMediaPlayerTurnOn(
        this MockHaContext mock,
        string entityId
    ) => AssertCalled(mock, "media_player", "turn_on", entityId);

    public static void ShouldHaveCalledMediaPlayerTurnOff(
        this MockHaContext mock,
        string entityId
    ) => AssertCalled(mock, "media_player", "turn_off", entityId);

    public static void ShouldHaveCalledWakeOnLan(this MockHaContext mock, string? macAddress = null)
    {
        var wakeCall = AssertCalled(mock, "wake_on_lan", "send_magic_packet");
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            return;
        }

        TryGetDataValue<string>(wakeCall.Data, out var actualMac, "mac", "mac_address")
            .Should()
            .BeTrue("Expected wake_on_lan.send_magic_packet to include a MAC address");

        actualMac
            .Should()
            .Be(
                macAddress,
                $"Expected wake_on_lan.send_magic_packet to use MAC address '{macAddress}'"
            );
    }

    public static void ShouldNotHaveCalledService(
        this MockHaContext mock,
        string domain,
        string service,
        string? entityId = null
    ) => AssertNotCalled(mock, domain, service, entityId);

    public static void ShouldHaveCalledDomainServiceExactly(
        this MockHaContext mock,
        string domain,
        string service,
        int times,
        string? entityId = null
    ) => AssertCalledExactly(mock, domain, service, times, entityId);

    public static void ShouldHaveCalledMediaPlayerSelectSource(
        this MockHaContext mock,
        string entityId,
        string? expectedSource = null
    )
    {
        var sourceCall = AssertCalled(mock, "media_player", "select_source", entityId);
        if (string.IsNullOrWhiteSpace(expectedSource))
        {
            return;
        }

        TryGetDataValue<string>(sourceCall.Data, out var actualSource, "Source", "source")
            .Should()
            .BeTrue("Expected media_player.select_source data to include a source value");

        actualSource
            .Should()
            .Be(
                expectedSource,
                $"Expected media_player.select_source to use source '{expectedSource}'"
            );
    }

    public static void ShouldHaveCalledWebostvCommand(
        this MockHaContext mock,
        string entityId,
        string expectedCommand
    )
    {
        var commandCall = AssertCalled(
            mock,
            "webostv",
            "command",
            entityId,
            EntityMatchSource.DataEntityId
        );

        TryGetDataValue<string>(commandCall.Data, out var actualCommand, "command", "Command")
            .Should()
            .BeTrue("Expected webostv.command data to include a command value");

        actualCommand
            .Should()
            .Be(expectedCommand, $"Expected webostv.command to use command '{expectedCommand}'");
    }

    public static void ShouldHaveCalledWebostvButton(this MockHaContext mock, string entityId)
    {
        AssertCalled(mock, "webostv", "button", entityId, EntityMatchSource.DataEntityId);
    }

    public static void ShouldHaveCalledWebostvButtonContaining(
        this MockHaContext mock,
        string entityId,
        string expectedContent
    )
    {
        var buttonCall = AssertCalled(
            mock,
            "webostv",
            "button",
            entityId,
            EntityMatchSource.DataEntityId
        );

        JsonSerializer
            .Serialize(buttonCall.Data)
            .Should()
            .Contain(
                expectedContent,
                "Expected webostv.button payload to contain '{0}'",
                expectedContent
            );
    }

    public static void ShouldHaveCalledWebostvCommandContaining(
        this MockHaContext mock,
        string entityId,
        string expectedContent
    )
    {
        var matchingPayload = FindCalls(
                mock,
                "webostv",
                "command",
                entityId,
                EntityMatchSource.DataEntityId
            )
            .Select(c => JsonSerializer.Serialize(c.Data))
            .FirstOrDefault(payload => payload.Contains(expectedContent, StringComparison.Ordinal));

        matchingPayload
            .Should()
            .NotBeNull(
                "Expected at least one webostv.command payload to contain '{0}' for entity '{1}'",
                expectedContent,
                entityId
            );
    }

    public static void ShouldHaveCalledWebostvCommandExactly(
        this MockHaContext mock,
        string entityId,
        string expectedCommand,
        int times
    )
    {
        var matchingCommandCalls = FindCalls(
                mock,
                "webostv",
                "command",
                entityId,
                EntityMatchSource.DataEntityId
            )
            .Where(c =>
                TryGetDataValue<string>(c.Data, out var actualCommand, "command", "Command")
                && string.Equals(actualCommand, expectedCommand, StringComparison.Ordinal)
            )
            .ToList();

        matchingCommandCalls
            .Should()
            .HaveCount(
                times,
                "Expected webostv.command to be called {0} time(s) with command '{1}' for entity '{2}'",
                times,
                expectedCommand,
                entityId
            );
    }

    public static void ShouldHaveServiceCallSequence(
        this MockHaContext mock,
        params (string Domain, string Service)[] expectedSequence
    )
    {
        mock.ServiceCalls.Should()
            .HaveCount(
                expectedSequence.Length,
                "Expected exactly {0} service calls in sequence",
                expectedSequence.Length
            );

        for (var i = 0; i < expectedSequence.Length; i++)
        {
            var call = mock.ServiceCalls[i];
            call.Domain.Should()
                .Be(expectedSequence[i].Domain, "Expected call #{0} domain to match", i + 1);
            call.Service.Should()
                .Be(expectedSequence[i].Service, "Expected call #{0} service to match", i + 1);
        }
    }

    public static void ShouldNeverHaveCalledLight(this MockHaContext mock, string entityId) =>
        AssertNotCalled(mock, "light", entityId: entityId);

    public static void ShouldNeverHaveCalledSwitch(this MockHaContext mock, string entityId) =>
        AssertNotCalled(mock, "switch", entityId: entityId);

    public static void ShouldHaveCalledServiceExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    )
    {
        var parts = entityId.Split('.', 2);
        parts
            .Length.Should()
            .Be(2, $"Expected '{entityId}' to be a valid entity ID like 'domain.object_id'");

        AssertCalledExactly(mock, parts[0], service, times, entityId);
    }

    public static void ShouldHaveCalledLightExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    ) => AssertCalledExactly(mock, "light", service, times, entityId);

    public static void ShouldHaveCalledSwitchExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    ) => AssertCalledExactly(mock, "switch", service, times, entityId);

    public static void ShouldHaveCalledBothLightsTurnOff(
        this MockHaContext mock,
        string primaryLight,
        string secondaryLight
    )
    {
        mock.ShouldHaveCalledLightTurnOff(primaryLight);
        mock.ShouldHaveCalledLightTurnOff(secondaryLight);
        AssertCalledExactly(mock, "light", "turn_off", 2);
    }

    public static void ShouldHaveCalledLockLock(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "lock", "lock", entityId);

    public static void ShouldHaveCalledLockUnlock(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "lock", "unlock", entityId);

    public static void ShouldNeverHaveCalledLock(this MockHaContext mock, string entityId) =>
        AssertNotCalled(mock, "lock", entityId: entityId);

    public static void ShouldHaveCalledClimateTurnOn(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "climate", "turn_on", entityId);

    public static void ShouldHaveCalledClimateTurnOff(this MockHaContext mock, string entityId) =>
        AssertCalled(mock, "climate", "turn_off", entityId);

    public static void ShouldHaveCalledClimateSetTemperature(
        this MockHaContext mock,
        string entityId,
        string expectedMode = "",
        double? expectedTemperature = null
    )
    {
        var setTempCall = AssertCalled(mock, "climate", "set_temperature", entityId);

        if (expectedTemperature.HasValue)
        {
            TryGetDataValue<double>(
                    setTempCall.Data,
                    out var actualTemp,
                    "temperature",
                    "Temperature"
                )
                .Should()
                .BeTrue("Expected climate.set_temperature data to include a temperature value");

            actualTemp
                .Should()
                .Be(
                    expectedTemperature.Value,
                    $"Expected temperature to be set to {expectedTemperature.Value}°C for entity '{entityId}'"
                );
        }

        if (!string.IsNullOrWhiteSpace(expectedMode))
        {
            TryGetDataValue<string>(setTempCall.Data, out var actualMode, "hvac_mode", "HvacMode")
                .Should()
                .BeTrue("Expected climate.set_temperature data to include an HVAC mode value");

            actualMode
                .Should()
                .Be(
                    expectedMode,
                    $"Expected hvac_mode to be set to {expectedMode} for entity '{entityId}'"
                );
        }
    }

    public static void ShouldHaveCalledClimateSetHvacMode(
        this MockHaContext mock,
        string entityId,
        string? expectedMode = null
    )
    {
        var setModeCall = AssertCalled(mock, "climate", "set_hvac_mode", entityId);
        if (expectedMode is null)
        {
            return;
        }

        TryGetDataValue<string>(setModeCall.Data, out var actualMode, "hvac_mode", "HvacMode")
            .Should()
            .BeTrue("Expected climate.set_hvac_mode data to include an HVAC mode value");

        actualMode
            .Should()
            .Be(
                expectedMode,
                $"Expected HVAC mode to be set to '{expectedMode}' for entity '{entityId}'"
            );
    }

    public static void ShouldHaveCalledClimateSetFanMode(
        this MockHaContext mock,
        string entityId
    ) => AssertCalled(mock, "climate", "set_fan_mode", entityId);

    public static void ShouldNeverHaveCalledClimate(this MockHaContext mock, string entityId) =>
        AssertNotCalled(mock, "climate", entityId: entityId);

    public static void ShouldHaveCalledClimateExactly(
        this MockHaContext mock,
        string entityId,
        string service,
        int times
    ) => AssertCalledExactly(mock, "climate", service, times, entityId);

    private static ServiceCall AssertCalled(
        MockHaContext mock,
        string domain,
        string service,
        string? entityId = null,
        EntityMatchSource source = EntityMatchSource.Target
    )
    {
        var call = FindCall(mock, domain, service, entityId, source);
        call.Should()
            .NotBeNull(
                entityId is null
                    ? $"Expected {domain}.{service} to be called but it was not found"
                    : $"Expected {domain}.{service} to be called for entity '{entityId}' but it was not found"
            );

        return call!;
    }

    private static void AssertNotCalled(
        MockHaContext mock,
        string domain,
        string? service = null,
        string? entityId = null,
        EntityMatchSource source = EntityMatchSource.Target
    )
    {
        var calls = FindCalls(mock, domain, service, entityId, source).ToList();
        calls
            .Should()
            .BeEmpty(
                entityId is null
                    ? $"Expected no {domain} service calls but found: {string.Join(", ", calls.Select(c => c.Service))}"
                    : $"Expected no {domain} service calls for entity '{entityId}'"
            );
    }

    private static void AssertCalledExactly(
        MockHaContext mock,
        string domain,
        string service,
        int times,
        string? entityId = null,
        EntityMatchSource source = EntityMatchSource.Target
    )
    {
        var calls = FindCalls(mock, domain, service, entityId, source).ToList();
        calls
            .Should()
            .HaveCount(
                times,
                entityId is null
                    ? $"Expected {domain}.{service} to be called exactly {times} times"
                    : $"Expected {domain}.{service} to be called exactly {times} times for entity '{entityId}'"
            );
    }

    private static ServiceCall? FindCall(
        MockHaContext mock,
        string domain,
        string service,
        string? entityId = null,
        EntityMatchSource source = EntityMatchSource.Target
    ) => FindCalls(mock, domain, service, entityId, source).FirstOrDefault();

    private static IEnumerable<ServiceCall> FindCalls(
        MockHaContext mock,
        string domain,
        string? service = null,
        string? entityId = null,
        EntityMatchSource source = EntityMatchSource.Target
    )
    {
        var calls = mock.GetServiceCalls(domain);

        if (!string.IsNullOrWhiteSpace(service))
        {
            calls = calls.Where(c => c.Service == service);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            calls = calls.Where(c => MatchesEntity(c, entityId, source));
        }

        return calls;
    }

    private static bool MatchesEntity(
        ServiceCall call,
        string entityId,
        EntityMatchSource source
    ) =>
        source == EntityMatchSource.Target
            ? TargetContainsEntityId(call.Target, entityId)
                || DataContainsEntityId(call.Data, entityId)
            : DataContainsEntityId(call.Data, entityId);

    private static bool TargetContainsEntityId(ServiceTarget? target, string entityId) =>
        target?.EntityIds?.Contains(entityId) == true;

    private static bool DataContainsEntityId(object? data, string entityId)
    {
        if (!TryGetDataValue<string>(data, out var actualEntityId, "EntityId", "entity_id"))
        {
            return false;
        }

        return string.Equals(actualEntityId, entityId, StringComparison.Ordinal);
    }

    private static bool TryGetDataValue<T>(object? data, out T? value, params string[] keysOrProps)
    {
        value = default;
        if (data is null)
        {
            return false;
        }

        if (
            TryGetFromDictionary(data, keysOrProps, out var dictValue)
            && TryConvert(dictValue, out value)
        )
        {
            return true;
        }

        if (
            TryGetFromObjectProperties(data, keysOrProps, out var propValue)
            && TryConvert(propValue, out value)
        )
        {
            return true;
        }

        if (
            TryGetFromJsonElement(data, keysOrProps, out var jsonValue)
            && TryConvert(jsonValue, out value)
        )
        {
            return true;
        }

        return false;
    }

    private static bool TryGetFromDictionary(object data, string[] keysOrProps, out object? value)
    {
        value = null;
        if (data is not IDictionary<string, object> dict)
        {
            return false;
        }

        foreach (var key in keysOrProps)
        {
            var match = dict.FirstOrDefault(kvp =>
                string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase)
            );

            if (!string.IsNullOrEmpty(match.Key))
            {
                if (match.Value is null)
                {
                    continue;
                }

                value = match.Value;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetFromObjectProperties(
        object data,
        string[] keysOrProps,
        out object? value
    )
    {
        value = null;
        var properties = data.GetType().GetProperties();
        foreach (var key in keysOrProps)
        {
            var property = properties.FirstOrDefault(p =>
                string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase)
            );

            if (property is null)
            {
                continue;
            }

            var propertyValue = property.GetValue(data);
            if (propertyValue is null)
            {
                continue;
            }

            value = propertyValue;
            return true;
        }

        return false;
    }

    private static bool TryGetFromJsonElement(object data, string[] keysOrProps, out object? value)
    {
        value = null;

        JsonElement json;
        if (data is JsonElement jsonElement)
        {
            json = jsonElement;
        }
        else
        {
            json = JsonSerializer.SerializeToElement(data);
        }

        if (json.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var key in keysOrProps)
        {
            foreach (var property in json.EnumerateObject())
            {
                if (!string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                value = property.Value;
                return true;
            }
        }

        return false;
    }

    private static bool TryConvert<T>(object? rawValue, out T? converted)
    {
        converted = default;
        if (rawValue is null)
        {
            return false;
        }

        if (rawValue is T typed)
        {
            converted = typed;
            return true;
        }

        if (rawValue is JsonElement jsonElement)
        {
            try
            {
                converted = jsonElement.Deserialize<T>();
                return converted is not null;
            }
            catch
            {
                return false;
            }
        }

        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (targetType.IsEnum)
            {
                if (rawValue is string rawString)
                {
                    converted = (T)Enum.Parse(targetType, rawString, true);
                    return true;
                }

                converted = (T)
                    Enum.ToObject(
                        targetType,
                        Convert.ChangeType(
                            rawValue,
                            Enum.GetUnderlyingType(targetType),
                            CultureInfo.InvariantCulture
                        )
                    );
                return true;
            }

            var changed = Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
            converted = (T?)changed;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
