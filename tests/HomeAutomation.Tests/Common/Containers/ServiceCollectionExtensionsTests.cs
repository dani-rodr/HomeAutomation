using HomeAutomation.apps.Area.Bathroom;
using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Kitchen;
using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace HomeAutomation.Tests.Common.Containers;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBathroomLightEntities()
    {
        using var provider = CreateServiceProvider();

        var entities = provider.GetRequiredService<IBathroomLightEntities>();

        entities.MasterSwitch.EntityId.Should().Be("switch.bathroom_motion_sensor");
        entities.MotionSensor.EntityId.Should().Be("binary_sensor.bathroom_presence_sensors");
        entities.Light.EntityId.Should().Be("light.bathroom_lights");
        entities.SensorDelay.EntityId.Should().Be("number.bathroom_motion_sensor_still_target_delay");
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveBathroomApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<BathroomApp>();

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
        entities.AirFryerStatus.EntityId.Should().Be(
            "sensor.xiaomi_smart_air_fryer_pro_4l_air_fryer_operating_status"
        );
    }

    [Fact]
    public void AddHomeEntitiesAndServices_ShouldResolveKitchenApp()
    {
        using var provider = CreateServiceProvider();

        var app = provider.GetRequiredService<KitchenApp>();

        app.Should().NotBeNull();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        var haContext = new MockHaContext();

        services.AddLogging();
        services.AddSingleton<IHaContext>(haContext);
        services.AddSingleton<IScheduler>(haContext.Scheduler);
        services.AddHomeAssistantGenerated();
        services.AddHomeEntitiesAndServices();
        services.AddTransient<BathroomApp>();
        services.AddTransient<KitchenApp>();

        return services.BuildServiceProvider();
    }
}
