using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.Tests.Common.Config;

public sealed class AreaConfigStoreTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly AreaConfigDescriptor _descriptor;
    private readonly AreaConfigStore _store;
    private readonly string _schemaPath;
    private readonly Mock<IAreaConfigChangeNotifier> _changeNotifier;

    public AreaConfigStoreTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"area-config-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);

        _descriptor = new(
            Key: "bedroom",
            Name: "Bedroom",
            Description: "Bedroom climate",
            DefaultsFilePath: Path.Combine(_tempDirectory, "bedroom.config.json"),
            OverridesFilePath: Path.Combine(_tempDirectory, "bedroom.config.local.json"),
            SchemaFilePath: Path.Combine(_tempDirectory, "bedroom.config.schema.json")
        );
        _schemaPath = _descriptor.SchemaFilePath!;

        File.WriteAllText(_descriptor.DefaultsFilePath, CreateDefaultsJson());
        File.WriteAllText(_schemaPath, CreateSchemaJson());

        var registry = new AreaConfigRegistry([_descriptor]);
        _changeNotifier = new Mock<IAreaConfigChangeNotifier>();
        _store = new AreaConfigStore(
            registry,
            _changeNotifier.Object,
            Mock.Of<ILogger<AreaConfigStore>>()
        );
    }

    [Fact]
    public void GetConfig_WhenOverrideMissing_ShouldReturnDefaults()
    {
        var config = _store.GetConfig("bedroom");

        config["sunrise"]?["comfortTemp"]?.GetValue<int>().Should().Be(24);
        File.Exists(_descriptor.OverridesFilePath).Should().BeFalse();
    }

    [Fact]
    public void SaveConfig_WhenValid_ShouldPersistOverride()
    {
        var config = _store.GetConfig("bedroom");
        config["sunrise"]!["comfortTemp"] = 21;

        var result = _store.SaveConfig("bedroom", config);

        result.IsValid.Should().BeTrue();
        File.Exists(_descriptor.OverridesFilePath).Should().BeTrue();

        var loaded = _store.GetConfig("bedroom");
        loaded["sunrise"]?["comfortTemp"]?.GetValue<int>().Should().Be(21);
        _changeNotifier
            .Verify(
                x =>
                    x.Publish(
                        It.Is<AreaConfigChangedEvent>(e =>
                            e.AreaKey == "bedroom" && e.ChangeType == AreaConfigChangeType.Saved
                        )
                    ),
                Times.Once
            );
    }

    [Fact]
    public void SaveConfig_WhenInvalid_ShouldReturnErrorsAndNotPersistOverride()
    {
        var config = _store.GetConfig("bedroom");
        config["sunrise"]!["comfortTemp"] = 31;

        var result = _store.SaveConfig("bedroom", config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("sunrise.comfortTemp");
        File.Exists(_descriptor.OverridesFilePath).Should().BeFalse();
        _changeNotifier.Verify(x => x.Publish(It.IsAny<AreaConfigChangedEvent>()), Times.Never);
    }

    [Fact]
    public void SaveConfig_WhenCrossFieldRuleFails_ShouldReturnErrors()
    {
        var config = _store.GetConfig("bedroom");
        config["sunrise"]!["comfortTemp"] = 28;
        config["sunrise"]!["doorOpenTemp"] = 24;

        var result = _store.SaveConfig("bedroom", config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("sunrise.comfortTemp");
    }

    [Fact]
    public void SaveConfig_WhenSchemaCannotLoad_ShouldReturnSchemaError()
    {
        File.WriteAllText(_schemaPath, "{ not json }");

        var result = _store.SaveConfig("bedroom", _store.GetConfig("bedroom"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("schema");
    }

    [Fact]
    public void GetConfig_WhenOverrideCorrupt_ShouldFallbackToDefaults()
    {
        File.WriteAllText(_descriptor.OverridesFilePath, "{ not json }");

        var config = _store.GetConfig("bedroom");

        config["sunrise"]?["comfortTemp"]?.GetValue<int>().Should().Be(24);
    }

    [Fact]
    public void ResetConfig_WhenOverrideExists_ShouldDeleteOverrideAndReturnDefaults()
    {
        var config = _store.GetConfig("bedroom");
        config["sunrise"]!["comfortTemp"] = 21;
        _store.SaveConfig("bedroom", config);

        var reset = _store.ResetConfig("bedroom");

        reset["sunrise"]?["comfortTemp"]?.GetValue<int>().Should().Be(24);
        File.Exists(_descriptor.OverridesFilePath).Should().BeFalse();
        _changeNotifier
            .Verify(
                x =>
                    x.Publish(
                        It.Is<AreaConfigChangedEvent>(e =>
                            e.AreaKey == "bedroom" && e.ChangeType == AreaConfigChangeType.Reset
                        )
                    ),
                Times.Once
            );
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private static string CreateDefaultsJson() =>
        """
            {
              "sunrise": {
                "hourStart": 5,
                "hourEnd": 18,
                "doorOpenTemp": 25,
                "ecoAwayTemp": 27,
                "comfortTemp": 24,
                "awayTemp": 27,
                "mode": "cool",
                "activateFan": true
              },
              "sunset": {
                "hourStart": 18,
                "hourEnd": 0,
                "doorOpenTemp": 25,
                "ecoAwayTemp": 27,
                "comfortTemp": 23,
                "awayTemp": 27,
                "mode": "cool",
                "activateFan": false
              },
              "midnight": {
                "hourStart": 0,
                "hourEnd": 5,
                "doorOpenTemp": 24,
                "ecoAwayTemp": 25,
                "comfortTemp": 22,
                "awayTemp": 25,
                "mode": "cool",
                "activateFan": false
              }
            }
            """;

    private static string CreateSchemaJson() =>
        """
            {
              "blocks": ["sunrise", "sunset", "midnight"],
              "blockFields": {
                "hourStart": { "type": "integer", "required": true, "min": 0, "max": 23 },
                "hourEnd": { "type": "integer", "required": true, "min": 0, "max": 23 },
                "doorOpenTemp": { "type": "integer", "required": true, "min": 16, "max": 30 },
                "ecoAwayTemp": { "type": "integer", "required": true, "min": 16, "max": 30 },
                "comfortTemp": { "type": "integer", "required": true, "min": 16, "max": 30 },
                "awayTemp": { "type": "integer", "required": true, "min": 16, "max": 30 },
                "mode": { "type": "string", "required": true, "allowedValues": [ "cool" ] },
                "activateFan": { "type": "boolean", "required": true }
              },
              "blockComparisons": [
                {
                  "leftField": "comfortTemp",
                  "rightField": "doorOpenTemp",
                  "operator": "lte",
                  "message": "ComfortTemp must be less than or equal to DoorOpenTemp."
                }
              ]
            }
            """;
}
