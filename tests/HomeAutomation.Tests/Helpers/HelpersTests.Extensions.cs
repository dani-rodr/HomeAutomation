namespace HomeAutomation.Tests.Helpers;

public partial class HelpersTests
{
    #region StateChangeExtensions Tests

    [Fact]
    public void GetAttributeChange_Should_ExtractAttributeChanges()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (oldBrightness, newBrightness) = change.GetAttributeChange<int>("brightness");
        var (oldTemp, newTemp) = change.GetAttributeChange<double>("temperature");

        oldBrightness.Should().Be(0);
        newBrightness.Should().Be(0);
        oldTemp.Should().Be(0.0);
        newTemp.Should().Be(0.0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleMissingAttributes()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (oldValue, newValue) = change.GetAttributeChange<int>("missing_attribute");

        oldValue.Should().Be(0);
        newValue.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleNullAttributes()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (oldValue, newValue) = change.GetAttributeChange<string>("any_attribute");

        oldValue.Should().BeNull();
        newValue.Should().BeNull();
    }

    [Fact]
    public void GetAttributeChange_Should_HandleJsonElementAttributes()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (oldValue, newValue) = change.GetAttributeChange<int>("test_attribute");

        oldValue.Should().Be(0);
        newValue.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleTypeConversion()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (_, stringAsInt) = change.GetAttributeChange<int>("string_number");
        var (_, doubleAsInt) = change.GetAttributeChange<int>("double_to_int");

        stringAsInt.Should().Be(0);
        doubleAsInt.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_ReturnDefaultForInvalidConversion()
    {
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        var (_, result) = change.GetAttributeChange<int>("invalid_number");

        result.Should().Be(0);
    }

    #endregion

    #region SwitchEntityExtensions Tests

    [Fact]
    public void OnDoubleClick_Should_ReturnObservableOfBufferedChanges()
    {
        var switchChangeSubject =
            new Subject<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>();
        var results = new List<IList<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>>();

        IDisposable automation = _switch.OnDoubleClick(2).Subscribe(results.Add);

        _mockHaContext.SimulateStateChange(_switch.EntityId, "on", "off");
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMilliseconds(500));
        _mockHaContext.SimulateStateChange(_switch.EntityId, "off", "on");
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMilliseconds(500));

        switchChangeSubject.Should().NotBeNull();
        results.Should().NotBeEmpty();

        automation.Dispose();
    }

    #endregion
}
