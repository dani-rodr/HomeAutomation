using System.Text.Json;

namespace HomeAutomation.apps.Area.Desk.Devices;

public enum DisplaySource
{
    PC,
    Laptop,
    ScreenSaver,
}

public class LgDisplay(ILgDisplayEntities entities, IServices services, ILogger<LgDisplay> logger)
    : MediaPlayerBase(entities.MediaPlayer, logger),
        ILgDisplay
{
    private const string MAC_ADDRESS = "D4:8D:26:B8:C4:AA";
    private const int HIGH_BRIGHTNESS = 230;
    private const int LOW_BRIGTNESS = 125;
    private readonly WebostvServices _webosServices = services.Webostv;
    private readonly WakeOnLanServices _wolServices = services.WakeOnLan;
    private readonly LightEntity _lightDisplay = entities.Display;
    private int _brightness = HIGH_BRIGHTNESS;
    public bool IsShowingPc => CurrentSource == Sources[DisplaySource.PC.ToString()];
    public bool IsShowingLaptop => CurrentSource == Sources[DisplaySource.Laptop.ToString()];

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [
            .. base.GetAutomations(),
            AdjustBrightnessFromInput(),
            ToggleScreenAutomation(),
            .. SyncLightEntityWithMediaState(),
        ];

    public async Task SetBrightnessHighAsync() => await SetBrightnessAsync(HIGH_BRIGHTNESS);

    public async Task SetBrightnessLowAsync() => await SetBrightnessAsync(LOW_BRIGTNESS);

    public async Task SetBrightnessAsync(int value)
    {
        if (value == _brightness)
        {
            Logger.LogDebug("Brightness is already set to {Value}. No action taken.", value);
            return;
        }

        var screenOn = await IsScreenOnAsync();
        if (!screenOn)
        {
            Logger.LogWarning("Cannot set brightness, screen is off.");
            return;
        }
        try
        {
            await SendCommandAsync(
                "system.notifications/createAlert",
                CreateBrightnessPayload(value)
            );
            SendButtonCommand("ENTER");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set brightness on LG display.");
        }
        UpdateLightValues(value);
    }

    public IObservable<
        StateChange<MediaPlayerEntity, EntityState<MediaPlayerAttributes>>
    > StateChanges() => MediaPlayer.StateChanges();

    public void ShowToast(string msg, params object[] args) =>
        SendCommand("system.notifications/createToast", new { message = string.Format(msg, args) });

    public void ShowPC() => ShowSource(DisplaySource.PC.ToString());

    public void ShowLaptop() => ShowSource(DisplaySource.Laptop.ToString());

    public void ShowScreenSaver() => ShowSource(DisplaySource.ScreenSaver.ToString());

    public override void TurnOn() => _wolServices.SendMagicPacket(MAC_ADDRESS);

    public override void TurnOff()
    {
        Logger.LogDebug("Turning off LG display.");
        base.TurnOff();
    }

    protected override Dictionary<string, string> ExtendedSources =>
        new()
        {
            { DisplaySource.PC.ToString(), "HDMI 1" },
            { DisplaySource.Laptop.ToString(), "HDMI 3" },
            { DisplaySource.ScreenSaver.ToString(), "Always Ready" },
        };

    private void SendCommand(string command, object? payload = null) =>
        _webosServices.Command(EntityId, command, payload);

    private void UpdateLightValues(int value)
    {
        _lightDisplay.TurnOn(brightness: value);
        _brightness = value;
    }

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
        int percent = Math.Clamp(value * 100 / 255, 0, 100);
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
                    @params = new
                    {
                        category = "picture",
                        settings = new { backlight = percent.ToString() },
                    },
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
                _lightDisplay.TurnOn();
                return;
            }

            Logger.LogError(ex, "Failed to set screen power state on LG display.");
        }
    }

    private IDisposable ToggleScreenAutomation() =>
        _lightDisplay.StateChanges().SubscribeAsync(async e => await SetScreenPowerAsync(e.IsOn()));

    private IEnumerable<IDisposable> SyncLightEntityWithMediaState()
    {
        yield return MediaPlayer.OnTurnedOn().Subscribe(_ => _lightDisplay.TurnOn());
        yield return MediaPlayer.OnTurnedOff().Subscribe(_ => _lightDisplay.TurnOff());
        yield return _lightDisplay
            .OnTurnedOn()
            .Where(_ => MediaPlayer.IsOff())
            .Subscribe(_ => TurnOn());
    }

    private IDisposable AdjustBrightnessFromInput()
    {
        return _lightDisplay
            .StateAllChanges()
            .SubscribeAsync(async e =>
            {
                if (e.IsOn() && e?.New?.Attributes?.Brightness is double brightness)
                {
                    if (brightness != _brightness)
                    {
                        await SetBrightnessAsync((int)brightness);
                    }
                }
            });
    }

    private async Task<bool> IsScreenOnAsync()
    {
        if (IsOff())
        {
            Logger.LogWarning("Cannot check screen state, LG display is off.");
            return false;
        }
        var response = await SendCommandAsync("com.webos.service.tvpower/power/getPowerState");

        if (response is not JsonElement json)
        {
            return false;
        }

        if (!json.TryGetProperty(EntityId, out var deviceData))
        {
            return false;
        }

        if (!deviceData.TryGetProperty("state", out var stateProp))
        {
            return false;
        }

        return stateProp.GetString() == "Active";
    }
}
