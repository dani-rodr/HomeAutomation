using HomeAutomation.apps.Area.Bathroom;
using HomeAutomation.apps.Area.Bathroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Desk;
using HomeAutomation.apps.Area.Desk.Automations.Entities;
using HomeAutomation.apps.Area.Desk.Devices.Entities;
using HomeAutomation.apps.Area.Kitchen;
using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Devices.Entities;
using HomeAutomation.apps.Area.Pantry;
using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Security;
using HomeAutomation.apps.Security.Automations.Entities;
using HomeAutomation.apps.Security.People;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.Tests.Common.Containers;

public class ServiceCollectionExtensionsTests : HaContextTestBase
{
    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBathroomLightEntities()
    {
        using var provider = CreateServiceProvider();

        var entities = provider.GetRequiredService<IBathroomLightEntities>();

        entities.MasterSwitch.EntityId.Should().Be("switch.bathroom_motion_sensor");
        entities.MotionSensor.EntityId.Should().Be("binary_sensor.bathroom_presence_sensors");
        entities.Light.EntityId.Should().Be("light.bathroom_lights");
        entities
            .SensorDelay.EntityId.Should()
            .Be("number.bathroom_motion_sensor_still_target_delay");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBathroomApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<BathroomApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBedroomEntities()
    {
        using var provider = CreateServiceProvider();

        var lightEntities = provider.GetRequiredService<IBedroomLightEntities>();
        var fanEntities = provider.GetRequiredService<IBedroomFanEntities>();
        var climateEntities = provider.GetRequiredService<IClimateEntities>();

        lightEntities.MasterSwitch.EntityId.Should().Be("switch.bedroom_motion_sensor");
        lightEntities.MotionSensor.EntityId.Should().Be("binary_sensor.bedroom_presence_sensors");
        lightEntities.Light.EntityId.Should().Be("light.bed_lights");
        lightEntities
            .SensorDelay.EntityId.Should()
            .Be("number.bedroom_motion_sensor_still_target_delay");
        lightEntities.RightSideEmptySwitch.EntityId.Should().Be("switch.sonoff_1002352c40_1");
        lightEntities.LeftSideFanSwitch.EntityId.Should().Be("switch.sonoff_100238104e_1");

        fanEntities.MasterSwitch.EntityId.Should().Be("switch.bedroom_fan_automation");
        fanEntities
            .Fans.Select(fan => fan.EntityId)
            .Should()
            .ContainSingle("switch.sonoff_100238104e_1");

        climateEntities.MasterSwitch.EntityId.Should().Be("switch.ac_automation");
        climateEntities.AirConditioner.EntityId.Should().Be("climate.ac");
        climateEntities.Door.EntityId.Should().Be("binary_sensor.contact_sensor_door");
        climateEntities.HouseMotionSensor.EntityId.Should().Be("binary_sensor.house");
        climateEntities.AcFanModeToggle.EntityId.Should().Be("button.ac_fan_mode_toggle");
        climateEntities.Fan.EntityId.Should().Be("switch.sonoff_100238104e_1");
        climateEntities.PowerSavingMode.EntityId.Should().Be("input_boolean.ac_power_saving_mode");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBedroomApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<BedroomApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveKitchenCookingEntities()
    {
        using var provider = CreateServiceProvider();

        var entities = provider.GetRequiredService<ICookingEntities>();

        entities.MasterSwitch.EntityId.Should().Be("switch.cooking_automation");
        entities.InductionPower.EntityId.Should().Be("sensor.smart_plug_3_sonoff_s31_power");
        entities.InductionTurnOff.EntityId.Should().Be("button.induction_cooker_power");
        entities
            .AirFryerStatus.EntityId.Should()
            .Be("sensor.xiaomi_smart_air_fryer_pro_4l_air_fryer_operating_status");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveKitchenApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<KitchenApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveDeskLightEntities()
    {
        using var provider = CreateServiceProvider();

        var entities = provider.GetRequiredService<IDeskLightEntities>();

        entities.MasterSwitch.EntityId.Should().Be("switch.lg_tv_motion_sensor");
        entities
            .MotionSensor.EntityId.Should()
            .Be("binary_sensor.desk_motion_sensor_smart_presence");
        entities.Light.EntityId.Should().Be("light.lg_display");
        entities.SensorDelay.EntityId.Should().Be("number.desk_motion_sensor_still_target_delay");
        entities.SalaLights.EntityId.Should().Be("light.sala_lights");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveDeskDeviceEntities()
    {
        using var provider = CreateServiceProvider();

        var desktop = provider.GetRequiredService<IDesktopEntities>();
        var laptop = provider.GetRequiredService<ILaptopEntities>();
        var display = provider.GetRequiredService<ILgDisplayEntities>();

        desktop.Power.EntityId.Should().Be("switch.daniel_pc");
        desktop.RemotePcButton.EntityId.Should().Be("input_button.remote_pc");
        laptop.VirtualSwitch.EntityId.Should().Be("switch.laptop");
        laptop.MotionSensor.EntityId.Should().Be("binary_sensor.desk_motion_sensor_smart_presence");
        display.MediaPlayer.EntityId.Should().Be("media_player.lg_webos_smart_tv");
        display.Display.EntityId.Should().Be("light.lg_display");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveDeskApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<DeskApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveLivingRoomEntities()
    {
        using var provider = CreateServiceProvider();

        var lightEntities = provider.GetRequiredService<ILivingRoomLightEntities>();
        var fanEntities = provider.GetRequiredService<ILivingRoomFanEntities>();
        var airQualityEntities = provider.GetRequiredService<IAirQualityEntities>();
        var tabletEntities = provider.GetRequiredService<ITabletEntities>();
        var tclDisplayEntities = provider.GetRequiredService<ITclDisplayEntities>();

        lightEntities.MasterSwitch.EntityId.Should().Be("switch.sala_motion_sensor");
        lightEntities.TclTv.EntityId.Should().Be("media_player.tcl65c755");
        fanEntities.MasterSwitch.EntityId.Should().Be("switch.sala_fan_automation");
        fanEntities.ExhaustFan.EntityId.Should().Be("switch.cozylife_955f");
        airQualityEntities.MasterSwitch.EntityId.Should().Be("switch.clean_air");
        airQualityEntities
            .Pm25Sensor.EntityId.Should()
            .Be("sensor.xiaomi_sg_753990712_cpa4_pm2_5_density_p_3_4");
        tabletEntities.Light.EntityId.Should().Be("light.mipad_screen");
        tabletEntities.TabletActive.EntityId.Should().Be("binary_sensor.mipad");
        tclDisplayEntities.MasterSwitch.EntityId.Should().Be("switch.tv_automation");
        tclDisplayEntities.Light.EntityId.Should().Be("light.tv_backlight_3_lite");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveLivingRoomApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<LivingRoomApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolvePantryLightEntities()
    {
        using var provider = CreateServiceProvider();

        var entities = provider.GetRequiredService<IPantryLightEntities>();

        entities.MasterSwitch.EntityId.Should().Be("switch.pantry_motion_sensor");
        entities
            .MiScalePresenceSensor.EntityId.Should()
            .Be("binary_sensor.bedroom_motion_sensor_mi_scale_presence");
        entities.MirrorLight.EntityId.Should().Be("light.controller_rgb_df1c0d");
        entities.BathroomMotionAutomation.EntityId.Should().Be("switch.bathroom_motion_sensor");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolvePantryApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<PantryApp>();

        app.Should().NotBeNull();
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveSecurityEntities()
    {
        using var provider = CreateServiceProvider();

        var lockEntities = provider.GetRequiredService<ILockingEntities>();
        var accessControlEntities = provider.GetRequiredService<IAccessControlAutomationEntities>();
        var danielEntities = provider.GetRequiredService<DanielEntities>();
        var athenaEntities = provider.GetRequiredService<AthenaEntities>();

        lockEntities.MasterSwitch.EntityId.Should().Be("switch.lock_automation");
        lockEntities.Lock.EntityId.Should().Be("lock.lock_wrapper");
        lockEntities.Door.EntityId.Should().Be("binary_sensor.door_wrapper");
        lockEntities.HouseStatus.EntityId.Should().Be("binary_sensor.house");
        lockEntities.MotionSensor.EntityId.Should().Be("binary_sensor.house");

        accessControlEntities.Door.EntityId.Should().Be("binary_sensor.door_wrapper");
        accessControlEntities.Lock.EntityId.Should().Be("lock.lock_wrapper");
        accessControlEntities.House.EntityId.Should().Be("binary_sensor.house");

        danielEntities.Person.EntityId.Should().Be("person.daniel_rodriguez");
        danielEntities.ToggleLocation.EntityId.Should().Be("button.manual_tracker_button_daniel");
        danielEntities
            .HomeTriggers.Select(trigger => trigger.EntityId)
            .Should()
            .Contain(["binary_sensor.redmi_watch_5_ble", "binary_sensor.oneplus_13_ble"]);

        athenaEntities.Person.EntityId.Should().Be("person.athena_bezos");
        athenaEntities.ToggleLocation.EntityId.Should().Be("button.manual_tracker_button_athena");
        athenaEntities
            .DirectUnlockTriggers.Select(trigger => trigger.EntityId)
            .Should()
            .ContainSingle("binary_sensor.baseus_tag_ble");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveSecurityApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<SecurityApp>();

        app.Should().NotBeNull();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IHaContext>(HaContext);
        services.AddSingleton<IScheduler>(HaContext.Scheduler);
        services.AddHomeAssistantGenerated();
        services.AddHomeEntitiesAndServices();
        services.AddTransient<BathroomApp>();
        services.AddTransient<BedroomApp>();
        services.AddTransient<DeskApp>();
        services.AddTransient<KitchenApp>();
        services.AddTransient<LivingRoomApp>();
        services.AddTransient<PantryApp>();
        services.AddTransient<SecurityApp>();

        return services.BuildServiceProvider();
    }
}
