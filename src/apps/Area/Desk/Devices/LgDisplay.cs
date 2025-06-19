using System.Text.Json;

namespace HomeAutomation.apps.Area.Desk.Devices;

public enum DisplaySource
{
    PC,
    Laptop,
    ScreenSaver,
}

public class LgDisplay : MediaPlayerBase, ILgDisplay
{
    private const string MAC_ADDRESS = "D4:8D:26:B8:C4:AA";
    private readonly WebostvServices _webosServices;
    private readonly WakeOnLanServices _wolServices;
    private readonly LightEntity _screen;
    private int _brightness = 90;
    public bool IsShowingPc => CurrentSource == Sources[DisplaySource.PC.ToString()];
    public bool IsShowingLaptop => CurrentSource == Sources[DisplaySource.Laptop.ToString()];

    public LgDisplay(ILgDisplayEntities entities, IServices services, ILogger logger)
        : base(entities.MediaPlayer, logger)
    {
        _webosServices = services.Webostv;
        _wolServices = services.WakeOnLan;
        _screen = entities.Display;
        Automations.Add(AdjustBrightnessFromInput());
        Automations.Add(ToggleScreenAutomation());
        Automations.Add(SyncScreenSwitchWithMediaState());
    }

    private async Task SetBrightnessAsync(int value)
    {
        if (value == _brightness)
        {
            return;
        }
        try
        {
            await SendCommandAsync("system.notifications/createAlert", CreateBrightnessPayload(value));

            SendButtonCommand("ENTER");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set brightness on LG display.");
        }

        _brightness = value;
    }

    public IObservable<StateChange<MediaPlayerEntity, EntityState<MediaPlayerAttributes>>> StateChanges() =>
        Entity.StateChanges();

    public void ShowToast(string msg) => SendCommand("system.notifications/createToast", new { message = msg });

    public void ShowPC() => ShowSource(DisplaySource.PC.ToString());

    public void ShowLaptop() => ShowSource(DisplaySource.Laptop.ToString());

    public void ShowScreenSaver() => ShowSource(DisplaySource.ScreenSaver.ToString());

    public override void TurnOn() => _wolServices.SendMagicPacket(MAC_ADDRESS);

    protected override void ExtendSourceDictionary(Dictionary<string, string> sources)
    {
        sources[DisplaySource.PC.ToString()] = "HDMI 1";
        sources[DisplaySource.Laptop.ToString()] = "HDMI 3";
        sources[DisplaySource.ScreenSaver.ToString()] = "Always Ready";
    }

    private void SendCommand(string command, object? payload = null) =>
        _webosServices.Command(EntityId, command, payload);

    private Task<JsonElement?> SendCommandAsync(string command, object? payload = null) =>
        _webosServices.CommandAsync(
            new WebostvCommandParameters
            {
                EntityId = EntityId,
                Command = command,
                Payload = payload,
            }
        );

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

    private async Task SetScreenPowerAsync(bool on)
    {
        if (IsOff())
        {
            Logger.LogWarning("TV is off, cannot control screen power.");
            return;
        }

        var command = on
            ? "com.webos.service.tvpower/power/turnOnScreen"
            : "com.webos.service.tvpower/power/turnOffScreen";

        try
        {
            await SendCommandAsync(command);
        }
        catch (Exception ex)
        {
            var msg = ex.ToString();

            // Suppress only the known "screen already on" error
            if (on && msg.Contains("errorCode': '-102'") && msg.Contains("must be 'screen off'"))
            {
                Logger.LogDebug("Screen is already on. Ignoring redundant turnOnScreen command.");
                _screen.TurnOn();
                return;
            }

            Logger.LogError(ex, "Failed to set screen power state on LG display.");
        }
    }

    private IDisposable ToggleScreenAutomation() =>
        _screen.StateChanges().SubscribeAsync(async e => await SetScreenPowerAsync(e.IsOn()));

    private IDisposable SyncScreenSwitchWithMediaState() =>
        Entity
            .StateChangesWithCurrent()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    _screen.TurnOn();
                }
                else if (e.IsOff())
                {
                    _screen.TurnOff();
                }
            });

    private IDisposable AdjustBrightnessFromInput()
    {
        return _screen
            .StateAllChanges()
            .SubscribeAsync(async e =>
            {
                if (e.IsOn() && e?.New?.Attributes?.Brightness is double brightness)
                {
                    var brightnessPercent = (int)(brightness / 255.0 * 100);
                    await SetBrightnessAsync(brightnessPercent);
                }
            });
    }
}
