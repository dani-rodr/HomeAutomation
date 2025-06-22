using System.Text.Json;
using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class WebhookServicesTests
{
    private readonly Mock<ITriggerManager> _mockTriggerManager = new();
    private readonly Mock<ILogger<WebhookServices>> _mockLogger = new();
    private readonly Mock<IObservable<JsonElement>> _mockObservable = new();
    private readonly Mock<IDisposable> _mockSubscription = new();
    private IObserver<JsonElement>? _capturedObserver;

    private WebhookServices CreateService()
    {
        _mockObservable.Reset();
        _mockSubscription.Reset();

        _mockObservable
            .Setup(o => o.Subscribe(It.IsAny<IObserver<JsonElement>>()))
            .Callback<IObserver<JsonElement>>(obs => _capturedObserver = obs)
            .Returns(_mockSubscription.Object);

        _mockTriggerManager
            .Setup(m => m.RegisterTrigger(It.IsAny<object>()))
            .Returns(_mockObservable.Object);

        return new WebhookServices(_mockTriggerManager.Object, _mockLogger.Object);
    }

    [Fact]
    public void Register_NewWebhook_Succeeds()
    {
        var service = CreateService();

        var result = service.Register("test", _ => { });

        Assert.True(result);
        _mockTriggerManager.Verify(m => m.RegisterTrigger(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void Register_TriggerInvokesCallback()
    {
        var service = CreateService();

        bool wasCalled = false;

        service.Register("webhook", _ => wasCalled = true);

        Assert.NotNull(_capturedObserver); // Make sure we got the observer

        // Simulate incoming webhook trigger
        var dummyPayload = JsonDocument.Parse("""{ "key": "value" }""").RootElement;
        _capturedObserver!.OnNext(dummyPayload);

        Assert.True(wasCalled);
    }

    [Fact]
    public void Register_SameWebhookTwice_FailsSecondTime()
    {
        var service = CreateService();
        service.Register("duplicate", _ => { });

        var result = service.Register("duplicate", _ => { });

        Assert.False(result);
        _mockTriggerManager.Verify(m => m.RegisterTrigger(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void Unregister_ExistingWebhook_SucceedsAndDisposes()
    {
        var service = CreateService();
        service.Register("to_remove", _ => { });

        var result = service.Unregister("to_remove");

        Assert.True(result);
        _mockSubscription.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public void Unregister_NonexistentWebhook_ReturnsFalse()
    {
        var service = CreateService();

        var result = service.Unregister("not_registered");

        Assert.False(result);
    }

    [Fact]
    public void Dispose_CleansUpAllSubscriptions()
    {
        var service = CreateService();
        service.Register("one", _ => { });
        service.Register("two", _ => { });

        service.Dispose();

        _mockSubscription.Verify(s => s.Dispose(), Times.Exactly(2));
    }
}
