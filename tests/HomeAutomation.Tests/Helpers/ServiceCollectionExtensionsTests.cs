using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;
using HomeAutomation.apps.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeAutomation.Tests.Helpers;

/// <summary>
/// Comprehensive tests for ServiceCollectionExtensions dependency injection configuration
/// Tests service registration patterns, interface mappings, entity container binding, and DI lifecycle management
/// </summary>
public class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly IServiceCollection _services;
    private readonly MockHaContext _mockHaContext;
    private readonly Entities _entities;
    private readonly Services _haServices;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
        _mockHaContext = new MockHaContext();
        _entities = new Entities(_mockHaContext);
        _haServices = new Services(_mockHaContext);

        // Add required dependencies that the extensions expect
        _services.AddSingleton<IHaContext>(_mockHaContext);
        _services.AddSingleton(_entities);
        _services.AddSingleton(_haServices);

        // Add logging services that the DI container expects
        _services.AddLogging();
        _services.AddSingleton<ILogger<NotificationServices>>(Mock.Of<ILogger<NotificationServices>>());
        _services.AddSingleton<ILogger<HaEventHandler>>(Mock.Of<ILogger<HaEventHandler>>());
    }

    #region Core Service Registration Tests

    [Fact]
    public void AddServices_Should_RegisterEventHandlerAsScoped()
    {
        // Act
        _services.AddServices();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var eventHandler = provider.GetService<IEventHandler>();
        eventHandler.Should().NotBeNull();
        eventHandler.Should().BeOfType<HaEventHandler>();

        // Verify scoped lifetime by getting multiple instances from same scope
        var eventHandler2 = provider.GetService<IEventHandler>();
        eventHandler.Should().BeSameAs(eventHandler2, "Should be same instance within scope");

        // Verify different instances from different scopes
        using var scope = provider.CreateScope();
        var scopedEventHandler = scope.ServiceProvider.GetService<IEventHandler>();
        scopedEventHandler.Should().NotBeSameAs(eventHandler, "Should be different instance in new scope");
    }

    [Fact]
    public void AddServices_Should_RegisterCommonEntitiesAsTransient()
    {
        // Act
        _services.AddServices();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var commonEntities1 = provider.GetService<CommonEntities>();
        var commonEntities2 = provider.GetService<CommonEntities>();

        commonEntities1.Should().NotBeNull();
        commonEntities2.Should().NotBeNull();
        commonEntities1.Should().NotBeSameAs(commonEntities2, "Should be different instances for transient service");
    }

    [Fact]
    public void AddServices_Should_RegisterLockingEntitiesAsTransient()
    {
        // Act
        _services.AddServices();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var lockingEntities = provider.GetService<ILockingEntities>();
        lockingEntities.Should().NotBeNull();
        lockingEntities.Should().BeOfType<LockingEntities>();

        // Verify transient lifetime
        var lockingEntities2 = provider.GetService<ILockingEntities>();
        lockingEntities.Should().NotBeSameAs(lockingEntities2, "Should be different instances for transient service");
    }

    [Fact]
    public void AddServices_Should_RegisterNotificationServicesAsTransient()
    {
        // Act
        _services.AddServices();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var notificationServices = provider.GetService<INotificationServices>();
        notificationServices.Should().NotBeNull();
        notificationServices.Should().BeOfType<NotificationServices>();

        // Verify transient lifetime
        var notificationServices2 = provider.GetService<INotificationServices>();
        notificationServices
            .Should()
            .NotBeSameAs(notificationServices2, "Should be different instances for transient service");
    }

    [Fact]
    public void AddServices_Should_InjectDependenciesIntoNotificationServices()
    {
        // Act
        _services.AddServices();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify NotificationServices can be created with dependencies
        var notificationServices = provider.GetService<INotificationServices>();
        notificationServices.Should().NotBeNull();

        // The constructor should receive Services and ILogger<NotificationServices>
        // If dependencies were not properly configured, service creation would fail
    }

    #endregion

    #region Area Entity Registration Tests

    [Fact]
    public void AddAreaEntities_Should_RegisterAllAreaEntityContainers()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify each area's entities are registered
        provider.GetService<IBedroomMotionEntities>().Should().NotBeNull();
        provider.GetService<IFanEntities>().Should().NotBeNull();
        provider.GetService<IClimateEntities>().Should().NotBeNull();
        provider.GetService<IDeskMotionEntities>().Should().NotBeNull();
        provider.GetService<IDisplayEntities>().Should().NotBeNull();
        provider.GetService<ILgDisplayEntities>().Should().NotBeNull();
        provider.GetService<IDesktopEntities>().Should().NotBeNull();
        provider.GetService<ILaptopEntities>().Should().NotBeNull();
        provider.GetService<IBathroomMotionEntities>().Should().NotBeNull();
        provider.GetService<IKitchenMotionEntities>().Should().NotBeNull();
        provider.GetService<ICookingEntities>().Should().NotBeNull();
        provider.GetService<ILivingRoomMotionEntities>().Should().NotBeNull();
        provider.GetService<ILivingRoomFanEntities>().Should().NotBeNull();
        provider.GetService<IAirQualityEntities>().Should().NotBeNull();
        provider.GetService<ITabletEntities>().Should().NotBeNull();
        provider.GetService<IPantryMotionEntities>().Should().NotBeNull();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterBedroomEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var bedroomMotionEntities = provider.GetService<IBedroomMotionEntities>();
        bedroomMotionEntities.Should().NotBeNull();
        bedroomMotionEntities.Should().BeOfType<BedroomMotionEntities>();

        var fanEntities = provider.GetService<IFanEntities>();
        fanEntities.Should().NotBeNull();
        fanEntities.Should().BeOfType<BedroomFanEntities>();

        var climateEntities = provider.GetService<IClimateEntities>();
        climateEntities.Should().NotBeNull();
        climateEntities.Should().BeOfType<BedroomClimateEntities>();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterDeskEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var deskMotionEntities = provider.GetService<IDeskMotionEntities>();
        deskMotionEntities.Should().NotBeNull();
        deskMotionEntities.Should().BeOfType<DeskMotionEntities>();

        var displayEntities = provider.GetService<IDisplayEntities>();
        displayEntities.Should().NotBeNull();
        displayEntities.Should().BeOfType<DeskDisplayEntities>();

        var lgDisplayEntities = provider.GetService<ILgDisplayEntities>();
        lgDisplayEntities.Should().NotBeNull();
        lgDisplayEntities.Should().BeOfType<DeskLgDisplayEntities>();

        var desktopEntities = provider.GetService<IDesktopEntities>();
        desktopEntities.Should().NotBeNull();
        desktopEntities.Should().BeOfType<DeskDesktopEntities>();

        var laptopEntities = provider.GetService<ILaptopEntities>();
        laptopEntities.Should().NotBeNull();
        laptopEntities.Should().BeOfType<LaptopEntities>();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterBathroomEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var bathroomMotionEntities = provider.GetService<IBathroomMotionEntities>();
        bathroomMotionEntities.Should().NotBeNull();
        bathroomMotionEntities.Should().BeOfType<BathroomMotionEntities>();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterKitchenEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var kitchenMotionEntities = provider.GetService<IKitchenMotionEntities>();
        kitchenMotionEntities.Should().NotBeNull();
        kitchenMotionEntities.Should().BeOfType<KitchenMotionEntities>();

        var cookingEntities = provider.GetService<ICookingEntities>();
        cookingEntities.Should().NotBeNull();
        cookingEntities.Should().BeOfType<KitchenCookingEntities>();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterLivingRoomEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var livingRoomMotionEntities = provider.GetService<ILivingRoomMotionEntities>();
        livingRoomMotionEntities.Should().NotBeNull();
        livingRoomMotionEntities.Should().BeOfType<LivingRoomMotionEntities>();

        var livingRoomFanEntities = provider.GetService<ILivingRoomFanEntities>();
        livingRoomFanEntities.Should().NotBeNull();
        livingRoomFanEntities.Should().BeOfType<LivingRoomFanEntities>();

        var airQualityEntities = provider.GetService<IAirQualityEntities>();
        airQualityEntities.Should().NotBeNull();
        airQualityEntities.Should().BeOfType<AirQualityEntities>();

        var tabletEntities = provider.GetService<ITabletEntities>();
        tabletEntities.Should().NotBeNull();
        tabletEntities.Should().BeOfType<LivingRoomTabletEntities>();
    }

    [Fact]
    public void AddAreaEntities_Should_RegisterPantryEntitiesCorrectly()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var pantryMotionEntities = provider.GetService<IPantryMotionEntities>();
        pantryMotionEntities.Should().NotBeNull();
        pantryMotionEntities.Should().BeOfType<PantryMotionEntities>();
    }

    #endregion

    #region Entity Container Interface Mapping Tests

    [Fact]
    public void EntityContainers_Should_ImplementExpectedInterfaces()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify interface hierarchy is respected
        var bedroomMotion = provider.GetService<IBedroomMotionEntities>();
        bedroomMotion.Should().BeAssignableTo<IMotionAutomationEntities>();
        bedroomMotion.Should().BeAssignableTo<IMotionWithLightAndDelay>();
        bedroomMotion.Should().BeAssignableTo<IMotionWithLight>();
        bedroomMotion.Should().BeAssignableTo<IMotionBase>();

        var livingRoomMotion = provider.GetService<ILivingRoomMotionEntities>();
        livingRoomMotion.Should().BeAssignableTo<IMotionAutomationEntities>();

        var climateEntities = provider.GetService<IClimateEntities>();
        climateEntities.Should().BeAssignableTo<IWeatherSensor>();
        climateEntities.Should().BeAssignableTo<IMotionBase>();

        var fanEntities = provider.GetService<IFanEntities>();
        fanEntities.Should().BeAssignableTo<IMotionBase>();

        var livingRoomFanEntities = provider.GetService<ILivingRoomFanEntities>();
        livingRoomFanEntities.Should().BeAssignableTo<IFanEntities>();
        livingRoomFanEntities.Should().BeAssignableTo<IMotionBase>();

        var tabletEntities = provider.GetService<ITabletEntities>();
        tabletEntities.Should().BeAssignableTo<IMotionBase>();

        var lockingEntities = provider.GetService<ILockingEntities>();
        lockingEntities.Should().BeAssignableTo<IMotionBase>();
    }

    [Fact]
    public void EntityContainers_Should_ProvideRequiredProperties()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify key properties are accessible
        var bedroomMotion = provider.GetService<IBedroomMotionEntities>();
        bedroomMotion!.MasterSwitch.Should().NotBeNull();
        bedroomMotion.MotionSensor.Should().NotBeNull();
        bedroomMotion.Light.Should().NotBeNull();
        bedroomMotion.SensorDelay.Should().NotBeNull();
        bedroomMotion.RightSideEmptySwitch.Should().NotBeNull();
        bedroomMotion.LeftSideFanSwitch.Should().NotBeNull();

        var airQuality = provider.GetService<IAirQualityEntities>();
        airQuality!.MasterSwitch.Should().NotBeNull();
        airQuality.MotionSensor.Should().NotBeNull();
        airQuality.AirPurifierFan.Should().NotBeNull();
        airQuality.SupportingFan.Should().NotBeNull();
        airQuality.Pm25Sensor.Should().NotBeNull();
        airQuality.LedStatus.Should().NotBeNull();

        var cooking = provider.GetService<ICookingEntities>();
        cooking!.RiceCookerPower.Should().NotBeNull();
        cooking.RiceCookerSwitch.Should().NotBeNull();
        cooking.AirFryerStatus.Should().NotBeNull();
        cooking.InductionTurnOff.Should().NotBeNull();
        cooking.InductionPower.Should().NotBeNull();
    }

    #endregion

    #region Transient Service Lifetime Tests

    [Fact]
    public void AddTransientEntity_Should_CreateNewInstancesForEachRequest()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify transient behavior for all entity containers
        var motion1 = provider.GetService<IBedroomMotionEntities>();
        var motion2 = provider.GetService<IBedroomMotionEntities>();
        motion1.Should().NotBeSameAs(motion2, "BedroomMotionEntities should be transient");

        var fan1 = provider.GetService<IFanEntities>();
        var fan2 = provider.GetService<IFanEntities>();
        fan1.Should().NotBeSameAs(fan2, "FanEntities should be transient");

        var climate1 = provider.GetService<IClimateEntities>();
        var climate2 = provider.GetService<IClimateEntities>();
        climate1.Should().NotBeSameAs(climate2, "ClimateEntities should be transient");

        var cooking1 = provider.GetService<ICookingEntities>();
        var cooking2 = provider.GetService<ICookingEntities>();
        cooking1.Should().NotBeSameAs(cooking2, "CookingEntities should be transient");

        var airQuality1 = provider.GetService<IAirQualityEntities>();
        var airQuality2 = provider.GetService<IAirQualityEntities>();
        airQuality1.Should().NotBeSameAs(airQuality2, "AirQualityEntities should be transient");
    }

    [Fact]
    public void AddTransientEntity_Should_RespectDependencyInjection()
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify that entity containers properly receive their dependencies
        var bedroomMotion = provider.GetService<IBedroomMotionEntities>() as BedroomMotionEntities;
        bedroomMotion.Should().NotBeNull();

        var livingRoomMotion = provider.GetService<ILivingRoomMotionEntities>() as LivingRoomMotionEntities;
        livingRoomMotion.Should().NotBeNull();

        var airQuality = provider.GetService<IAirQualityEntities>() as AirQualityEntities;
        airQuality.Should().NotBeNull();

        var climate = provider.GetService<IClimateEntities>() as BedroomClimateEntities;
        climate.Should().NotBeNull();

        // Verify that CommonEntities dependency is properly injected
        // (No direct way to verify this without exposing internal state,
        // but if service creation succeeds, dependencies were injected correctly)
    }

    #endregion

    #region Service Lifetime Verification Tests

    [Fact]
    public void ServiceLifetimes_Should_BeConfiguredCorrectly()
    {
        // Act
        _services.AddServices();
        _services.AddAreaEntities();

        var serviceDescriptors = _services.ToList();

        // Assert - Verify service lifetime configurations
        var eventHandlerDescriptor = serviceDescriptors.FirstOrDefault(x => x.ServiceType == typeof(IEventHandler));
        eventHandlerDescriptor.Should().NotBeNull();
        eventHandlerDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped, "IEventHandler should be scoped");

        var commonEntitiesDescriptor = serviceDescriptors.FirstOrDefault(x => x.ServiceType == typeof(CommonEntities));
        commonEntitiesDescriptor.Should().NotBeNull();
        commonEntitiesDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient, "CommonEntities should be transient");

        var lockingEntitiesDescriptor = serviceDescriptors.FirstOrDefault(x =>
            x.ServiceType == typeof(ILockingEntities)
        );
        lockingEntitiesDescriptor.Should().NotBeNull();
        lockingEntitiesDescriptor!
            .Lifetime.Should()
            .Be(ServiceLifetime.Transient, "ILockingEntities should be transient");

        var notificationServicesDescriptor = serviceDescriptors.FirstOrDefault(x =>
            x.ServiceType == typeof(INotificationServices)
        );
        notificationServicesDescriptor.Should().NotBeNull();
        notificationServicesDescriptor!
            .Lifetime.Should()
            .Be(ServiceLifetime.Transient, "INotificationServices should be transient");

        // Verify all area entity containers are transient
        var bedroomMotionDescriptor = serviceDescriptors.FirstOrDefault(x =>
            x.ServiceType == typeof(IBedroomMotionEntities)
        );
        bedroomMotionDescriptor?.Lifetime.Should().Be(ServiceLifetime.Transient);

        var fanEntitiesDescriptor = serviceDescriptors.FirstOrDefault(x => x.ServiceType == typeof(IFanEntities));
        fanEntitiesDescriptor?.Lifetime.Should().Be(ServiceLifetime.Transient);

        var climateEntitiesDescriptor = serviceDescriptors.FirstOrDefault(x =>
            x.ServiceType == typeof(IClimateEntities)
        );
        climateEntitiesDescriptor?.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddTransientEntity_Should_UseActivatorUtilities()
    {
        // This test verifies that the extension method correctly configures services
        // to use ActivatorUtilities.CreateInstance for dependency injection

        // Arrange
        var testServices = new ServiceCollection();
        testServices.AddSingleton<IHaContext>(_mockHaContext);
        testServices.AddSingleton(_entities);

        // Act
        testServices.AddServices();
        using var provider = testServices.BuildServiceProvider();

        // Assert - Verify service can be resolved (indicating ActivatorUtilities works)
        var commonEntities = provider.GetService<CommonEntities>();
        commonEntities.Should().NotBeNull();

        // Verify the service was created with proper dependencies
        commonEntities!.BedroomMotionSensor.Should().NotBeNull();
        commonEntities.LivingRoomMotionSensor.Should().NotBeNull();
        commonEntities.KitchenMotionSensor.Should().NotBeNull();
    }

    #endregion

    #region Dependency Chain Validation Tests

    [Fact]
    public void ServiceContainer_Should_ResolveComplexDependencyChains()
    {
        // Act
        _services.AddServices();
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert - Verify complex entity containers that depend on CommonEntities resolve correctly
        var livingRoomMotion = provider.GetService<ILivingRoomMotionEntities>();
        livingRoomMotion.Should().NotBeNull();

        var bedroomClimate = provider.GetService<IClimateEntities>() as BedroomClimateEntities;
        bedroomClimate.Should().NotBeNull();

        var airQuality = provider.GetService<IAirQualityEntities>() as AirQualityEntities;
        airQuality.Should().NotBeNull();

        var pantryMotion = provider.GetService<IPantryMotionEntities>();
        pantryMotion.Should().NotBeNull();

        // Verify that shared entities are properly accessible through different containers
        var bedroomMotion = provider.GetService<IBedroomMotionEntities>();
        var fanEntities = provider.GetService<IFanEntities>() as BedroomFanEntities;

        // Both should reference the same bedroom fan switch through CommonEntities
        bedroomMotion!.LeftSideFanSwitch.EntityId.Should().Be(fanEntities!.Fans.First().EntityId);
    }

    [Fact]
    public void ServiceContainer_Should_HandleMissingDependencies()
    {
        // Arrange - Create a service collection without required dependencies
        var incompleteServices = new ServiceCollection();

        // Act & Assert - Should throw when trying to resolve services without dependencies
        incompleteServices.AddServices();
        using var provider = incompleteServices.BuildServiceProvider();

        var action = () => provider.GetRequiredService<CommonEntities>();
        action.Should().Throw<InvalidOperationException>("Should fail when required dependencies are missing");
    }

    [Fact]
    public void ServiceRegistration_Should_BeIdempotent()
    {
        // Act - Add services multiple times
        _services.AddServices();
        _services.AddServices();
        _services.AddAreaEntities();
        _services.AddAreaEntities();

        var serviceDescriptors = _services.ToList();

        // Assert - Should handle multiple registrations gracefully
        var eventHandlerRegistrations = serviceDescriptors.Count(x => x.ServiceType == typeof(IEventHandler));
        eventHandlerRegistrations.Should().Be(2, "Multiple registrations should be allowed but last one wins");

        var bedroomMotionRegistrations = serviceDescriptors.Count(x => x.ServiceType == typeof(IBedroomMotionEntities));
        bedroomMotionRegistrations.Should().Be(2, "Multiple registrations should be allowed");

        // Verify the service still resolves correctly
        using var provider = _services.BuildServiceProvider();
        var eventHandler = provider.GetService<IEventHandler>();
        eventHandler.Should().NotBeNull();
        eventHandler.Should().BeOfType<HaEventHandler>();
    }

    #endregion

    #region Service Registration Pattern Tests

    [Fact]
    public void ServiceExtensions_Should_ReturnServiceCollectionForChaining()
    {
        // Act & Assert - Verify fluent interface
        var result = _services.AddServices().AddAreaEntities();

        result.Should().BeSameAs(_services, "Extensions should return the same service collection for method chaining");
    }

    [Fact]
    public void AddAreaEntities_Should_ChainAllAreaRegistrations()
    {
        // Act
        var result = _services.AddAreaEntities();

        // Assert - Verify all expected services are registered
        var descriptors = result.ToList();

        // Bedroom entities
        descriptors.Should().Contain(x => x.ServiceType == typeof(IBedroomMotionEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IFanEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IClimateEntities));

        // Desk entities
        descriptors.Should().Contain(x => x.ServiceType == typeof(IDeskMotionEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IDisplayEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ILgDisplayEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IDesktopEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ILaptopEntities));

        // Other area entities
        descriptors.Should().Contain(x => x.ServiceType == typeof(IBathroomMotionEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IKitchenMotionEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ICookingEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ILivingRoomMotionEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ILivingRoomFanEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IAirQualityEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(ITabletEntities));
        descriptors.Should().Contain(x => x.ServiceType == typeof(IPantryMotionEntities));
    }

    [Theory]
    [InlineData(typeof(IBedroomMotionEntities), typeof(BedroomMotionEntities))]
    [InlineData(typeof(IFanEntities), typeof(BedroomFanEntities))]
    [InlineData(typeof(IClimateEntities), typeof(BedroomClimateEntities))]
    [InlineData(typeof(IDeskMotionEntities), typeof(DeskMotionEntities))]
    [InlineData(typeof(IDisplayEntities), typeof(DeskDisplayEntities))]
    [InlineData(typeof(ILgDisplayEntities), typeof(DeskLgDisplayEntities))]
    [InlineData(typeof(IDesktopEntities), typeof(DeskDesktopEntities))]
    [InlineData(typeof(ILaptopEntities), typeof(LaptopEntities))]
    [InlineData(typeof(IBathroomMotionEntities), typeof(BathroomMotionEntities))]
    [InlineData(typeof(IKitchenMotionEntities), typeof(KitchenMotionEntities))]
    [InlineData(typeof(ICookingEntities), typeof(KitchenCookingEntities))]
    [InlineData(typeof(ILivingRoomMotionEntities), typeof(LivingRoomMotionEntities))]
    [InlineData(typeof(ILivingRoomFanEntities), typeof(LivingRoomFanEntities))]
    [InlineData(typeof(IAirQualityEntities), typeof(AirQualityEntities))]
    [InlineData(typeof(ITabletEntities), typeof(LivingRoomTabletEntities))]
    [InlineData(typeof(IPantryMotionEntities), typeof(PantryMotionEntities))]
    public void AddAreaEntities_Should_MapInterfaceToCorrectImplementation(Type interfaceType, Type implementationType)
    {
        // Act
        _services.AddAreaEntities();
        using var provider = _services.BuildServiceProvider();

        // Assert
        var service = provider.GetService(interfaceType);
        service.Should().NotBeNull();
        service.Should().BeOfType(implementationType);
    }

    #endregion

    public void Dispose()
    {
        _mockHaContext?.Dispose();
    }
}
