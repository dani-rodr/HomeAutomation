using System.Text.Json.Nodes;
using HomeAutomation.apps.Area.Bathroom.Config;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.Tests.Common.Settings;

public sealed class AreaSettingsStoreTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly AreaSettingsDescriptor _descriptor;
    private readonly AreaSettingsStore _store;
    private readonly Mock<IAreaSettingsChangeNotifier> _changeNotifier;

    public AreaSettingsStoreTests()
    {
        _tempDirectory = Path.Combine(
            Path.GetTempPath(),
            $"area-settings-tests-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(_tempDirectory);

        _descriptor = new AreaSettingsDescriptor(
            Key: "bathroom",
            Name: "Bathroom",
            Description: "Bathroom settings",
            SettingsType: typeof(BathroomSettings),
            SettingsFilePath: Path.Combine(_tempDirectory, "bathroom.settings.yaml")
        );

        File.WriteAllText(_descriptor.SettingsFilePath, CreateDefaultsYaml());

        var registry = new AreaSettingsRegistry([_descriptor]);
        _changeNotifier = new Mock<IAreaSettingsChangeNotifier>();

        _store = new AreaSettingsStore(
            registry,
            new AreaSettingsValidator(),
            _changeNotifier.Object,
            Mock.Of<ILogger<AreaSettingsStore>>()
        );
    }

    [Fact]
    public void GetSettings_WhenFileExists_ShouldReturnCurrentSettings()
    {
        var settings = _store.GetSettings("bathroom");

        settings["light"]?["motionOnDelaySeconds"]?.GetValue<int>().Should().Be(2);
    }

    [Fact]
    public void SaveSettings_WhenValid_ShouldPersistAndPublish()
    {
        var settings = _store.GetSettings("bathroom");
        settings["light"]!["motionOnDelaySeconds"] = 5;

        var result = _store.SaveSettings("bathroom", settings);

        result.IsValid.Should().BeTrue();
        _store
            .GetSettings("bathroom")["light"]
            ?["motionOnDelaySeconds"]?.GetValue<int>()
            .Should()
            .Be(5);

        _changeNotifier.Verify(
            x =>
                x.Publish(
                    It.Is<AreaSettingsChangedEvent>(e =>
                        e.AreaKey == "bathroom" && e.ChangeType == AreaSettingsChangeType.Saved
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public void SaveSettings_WhenInvalid_ShouldReturnErrorsAndNotPublish()
    {
        var settings = _store.GetSettings("bathroom");
        settings["light"]!["motionOnDelaySeconds"] = 99;

        var result = _store.SaveSettings("bathroom", settings);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("light.motionOnDelaySeconds");

        _changeNotifier.Verify(x => x.Publish(It.IsAny<AreaSettingsChangedEvent>()), Times.Never);
    }

    [Fact]
    public void SaveSettings_WhenPayloadMalformed_ShouldReturnSettingsError()
    {
        var malformed = new JsonObject { ["light"] = "not-an-object" };

        var result = _store.SaveSettings("bathroom", malformed);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("settings");
    }

    [Fact]
    public void ResetSettings_ShouldRestoreBootDefaultsAndPublish()
    {
        var settings = _store.GetSettings("bathroom");
        settings["light"]!["motionOnDelaySeconds"] = 6;
        _store.SaveSettings("bathroom", settings);

        var reset = _store.ResetSettings("bathroom");

        reset["light"]?["motionOnDelaySeconds"]?.GetValue<int>().Should().Be(2);
        _store
            .GetSettings("bathroom")["light"]
            ?["motionOnDelaySeconds"]?.GetValue<int>()
            .Should()
            .Be(2);

        _changeNotifier.Verify(
            x =>
                x.Publish(
                    It.Is<AreaSettingsChangedEvent>(e =>
                        e.AreaKey == "bathroom" && e.ChangeType == AreaSettingsChangeType.Reset
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

    private static string CreateDefaultsYaml() =>
        $"""
            {typeof(BathroomSettings).FullName}:
              light:
                motionOnDelaySeconds: 2
                masterSwitchDisableDelayMinutes: 5
            """;
}
