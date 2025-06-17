namespace HomeAutomation.apps.Area.Desk.Devices;

public enum DisplaySource
{
    PC,
    Laptop,
    ScreenSaver,
}

public class LgDisplay : MediaPlayerBase
{
    private const string MAC_ADDRESS = "D4:8D:26:B8:C4:AA";
    private bool IsScreenOn { get; set; }
    private readonly WebostvServices _webosServices;
    private readonly WakeOnLanServices _wolServices;
    private int _brightness = 90;
    public bool IsShowingPc => CurrentSource == Sources[DisplaySource.PC.ToString()];
    public bool IsShowingLaptop => CurrentSource == Sources[DisplaySource.Laptop.ToString()];

    public LgDisplay(ILgDisplayEntities entities, Services services, ILogger logger)
        : base(entities.MediaPlayer, logger)
    {
        _webosServices = services.Webostv;
        _wolServices = services.WakeOnLan;
        Automations.Add(GetScreenStateAutomation(entities.Screen));
        Automations.Add(GetScreenBrightnessAutomation(entities.Brightness));
        Automations.Add(
            Entity
                .StateChangesWithCurrent()
                .Subscribe(e =>
                {
                    if (e.IsOn())
                    {
                        entities.Screen.TurnOn();
                    }
                    if (e.IsOff())
                    {
                        entities.Screen.TurnOff();
                    }
                })
        );
    }

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

    public void ShowToast(string msg) => SendCommand("system.notifications/createToast", new { message = msg });

    public void ShowPC() => ShowSource(DisplaySource.PC.ToString());

    public void ShowLaptop() => ShowSource(DisplaySource.Laptop.ToString());

    public void ShowScreenSaver() => ShowSource(DisplaySource.ScreenSaver.ToString());

    public void TurnOnScreen() => SetScreenPower(true);

    public void TurnOffScreen() => SetScreenPower(false);

    public override void TurnOn()
    {
        _wolServices.SendMagicPacket(MAC_ADDRESS);
        TurnOnScreen();
    }

    protected override void ExtendSourceDictionary(Dictionary<string, string> sources)
    {
        sources[DisplaySource.PC.ToString()] = "HDMI 1";
        sources[DisplaySource.Laptop.ToString()] = "HDMI 3";
        sources[DisplaySource.ScreenSaver.ToString()] = "Always Ready";
    }

    private void SendCommand(string command, object? payload = null) =>
        _webosServices.Command(EntityId, command, payload);

    private void SendButtonCommand(string command) => _webosServices.Button(EntityId, command);

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

    private IDisposable GetScreenStateAutomation(SwitchEntity screen)
    {
        return screen
            .StateChanges()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    TurnOnScreen();
                    return;
                }
                if (e.IsOff())
                {
                    TurnOffScreen();
                }
            });
    }

    private IDisposable GetScreenBrightnessAutomation(InputNumberEntity brightness)
    {
        return brightness
            .StateChanges()
            .Subscribe(async e =>
            {
                var newValue = e?.New?.State;
                if (newValue is double value)
                {
                    await SetBrightnessAsync((int)value);
                }
            });
    }
}
