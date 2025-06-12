namespace HomeAutomation.apps.Area.Desk.Devices;

public enum DisplaySource
{
    PC,
    Laptop,
    ScreenSaver,
}

public class LgDisplay(Entities entities, Services services, ILogger logger)
    : MediaPlayerBase(entities.MediaPlayer.LgWebosSmartTv, logger)
{
    private bool IsScreenOn { get; set; }
    private readonly WebostvServices _services = services.Webostv;
    private int _brightness = 90;

    public async Task SetBrightnessAsync(int value)
    {
        if (value == _brightness)
        {
            return;
        }

        SendCommand("system.notifications/createAlert", CreateBrightnessPayload(value));

        await Task.Delay(20);

        SendButtonCommand("ENTER");

        _brightness = value;
    }

    public IObservable<StateChange<MediaPlayerEntity, EntityState<MediaPlayerAttributes>>> StateChanges() =>
        Entity.StateChanges();

    public void SendToast(string msg) => SendCommand("system.notifications/createToast", new { message = msg });

    public void ShowPC() => ShowSource(DisplaySource.PC.ToString());

    public void ShowLaptop() => ShowSource(DisplaySource.Laptop.ToString());

    public void ShowScreenSaver() => ShowSource(DisplaySource.ScreenSaver.ToString());

    public void TurnOnScreen() => SetScreenPower(true);

    public void TurnOffScreen() => SetScreenPower(false);

    public override void TurnOn()
    {
        base.TurnOn();
        TurnOnScreen();
    }

    protected override void ExtendSourceDictionary(Dictionary<string, string> sources)
    {
        foreach (var kv in sources)
        {
            sources[kv.Key.ToString()] = kv.Value;
        }
    }

    private void SendCommand(string command, object? payload = null) => _services.Command(EntityId, command, payload);

    private void SendButtonCommand(string command) => _services.Button(EntityId, command);

    private static object CreateBrightnessPayload(int value)
    {
        return new
        {
            message = "Change Brightness",
            modal = false,
            buttons = new[]
            {
                new
                {
                    label = "ok",
                    focus = true,
                    buttonType = "ok",
                    onClick = "luna://com.webos.settingsservice/setSystemSettings",
                    @params = new { category = "picture", settings = new { backlight = value.ToString() } },
                },
            },
            type = "confirm",
            isSysReq = true,
        };
    }

    private void SetScreenPower(bool on)
    {
        if (IsScreenOn == on)
        {
            return;
        }

        IsScreenOn = on;
        var command = on
            ? "com.webos.service.tvpower/power/turnOnScreen"
            : "com.webos.service.tvpower/power/turnOffScreen";

        SendCommand(command);
    }
}
