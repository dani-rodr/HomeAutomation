namespace HomeAutomation.apps.Area.Desk.Devices;

public class LgDisplay(Entities entities, Services services) : MediaPlayerBase(entities.MediaPlayer.LgWebosSmartTv)
{
    private const string SourcePC = "PC";
    private const string SourceLaptop = "Laptop";
    private const string SourceScreenSaver = "ScreenSaver";
    private bool IsScreenOn { get; set; }
    private readonly WebostvServices _services = services.Webostv;
    private readonly Dictionary<string, string> _sources = new()
    {
        [SourcePC] = "HDMI 1",
        [SourceLaptop] = "HDMI 3",
        [SourceScreenSaver] = "Always Ready",
    };
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

    public void ShowPC() => ShowSource(SourcePC);

    public void ShowLaptop() => ShowSource(SourceLaptop);

    public void ShowScreenSaver() => ShowSource(SourceScreenSaver);

    public void TurnOnScreen()
    {
        if (IsScreenOn)
            return;
        IsScreenOn = true;
        SendCommand("com.webos.service.tvpower/power/turnOnScreen");
    }

    public void TurnOffScreen()
    {
        if (!IsScreenOn)
            return;
        IsScreenOn = false;
        SendCommand("com.webos.service.tvpower/power/turnOffScreen");
    }

    public override void TurnOn()
    {
        base.TurnOn();
        TurnOnScreen();
    }

    private void ShowSource(string key)
    {
        if (_sources.TryGetValue(key, out var source))
        {
            SelectSource(source);
        }
        else
        {
            throw new ArgumentException($"Source key '{key}' not defined.", nameof(key));
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
}
