using HomeAutomation.apps.Common.Base;

namespace HomeAutomation.Tests.Common.Base;

public class MediaPlayerBaseTests : HaContextTestBase
{
    [Fact]
    public void StartAutomation_WhenExtendedSourcesOverlapSourceList_ShouldPreferExtendedMapping()
    {
        // Arrange
        var mediaPlayerEntity = new MediaPlayerEntity(HaContext, "media_player.test_player");
        HaContext.SetEntityAttributes(
            mediaPlayerEntity.EntityId,
            new { source_list = new[] { "PC", "HDMI 2" }, source = "HDMI 2" }
        );
        HaContext.SetEntityState(mediaPlayerEntity.EntityId, "on");

        var sut = new TestMediaPlayer(
            mediaPlayerEntity,
            new Mock<ILogger>().Object,
            new Dictionary<string, string> { ["PC"] = "HDMI 1", ["Laptop"] = "HDMI 3" }
        );

        // Act
        sut.StartAutomation();
        sut.Show("PC");

        // Assert
        HaContext.ShouldHaveCalledMediaPlayerSelectSource(mediaPlayerEntity.EntityId, "HDMI 1");
    }

    private sealed class TestMediaPlayer(
        MediaPlayerEntity entity,
        ILogger logger,
        Dictionary<string, string> extendedSources
    ) : MediaPlayerBase(entity, logger)
    {
        protected override Dictionary<string, string> ExtendedSources => extendedSources;

        public void Show(string sourceKey) => ShowSource(sourceKey);
    }
}
