using Rac.Audio;
using Xunit;

namespace Rac.Audio.Tests;

public class SoundTests
{
    [Fact]
    public void Constructor_ValidParameters_InitializesCorrectly()
    {
        // Arrange
        const int id = 1;
        const string filePath = "test.ogg";
        const uint bufferId = 100;
        const uint sourceId = 200;
        const bool is3D = true;

        // Act
        var sound = new Sound(id, filePath, bufferId, sourceId, is3D);

        // Assert
        Assert.Equal(id, sound.Id);
        Assert.Equal(filePath, sound.FilePath);
        Assert.Equal(bufferId, sound.BufferId);
        Assert.Equal(sourceId, sound.SourceId);
        Assert.Equal(is3D, sound.Is3D);
        Assert.False(sound.IsDisposed);
        Assert.Equal(1.0f, sound.Volume);
        Assert.Equal(1.0f, sound.Pitch);
        Assert.False(sound.IsLooping);
    }

    [Fact]
    public void Constructor_NegativeId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sound(-1, "test.ogg", 100, 200));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidFilePath_ThrowsArgumentException(string filePath)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Sound(1, filePath, 100, 200));
    }

    [Fact]
    public void Volume_ValidValues_SetsCorrectly()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200);

        // Act
        sound.Volume = 0.5f;

        // Assert
        Assert.Equal(0.5f, sound.Volume);
    }

    [Fact]
    public void Pitch_ValidValues_SetsCorrectly()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200);

        // Act
        sound.Pitch = 2.0f;

        // Assert
        Assert.Equal(2.0f, sound.Pitch);
    }

    [Fact]
    public void IsLooping_ValidValues_SetsCorrectly()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200);

        // Act
        sound.IsLooping = true;

        // Assert
        Assert.True(sound.IsLooping);
    }

    [Fact]
    public void SetPosition_ValidSound3D_SetsPositionCorrectly()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200, true);

        // Act
        sound.SetPosition(1.0f, 2.0f, 3.0f);

        // Assert
        Assert.Equal(1.0f, sound.PositionX);
        Assert.Equal(2.0f, sound.PositionY);
        Assert.Equal(3.0f, sound.PositionZ);
    }

    [Fact]
    public void SetPosition_Non3DSound_ThrowsInvalidOperationException()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200, false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            sound.SetPosition(1.0f, 2.0f, 3.0f));
    }

    [Fact]
    public void SetPosition_DisposedSound_ThrowsObjectDisposedException()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200, true);
        sound.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            sound.SetPosition(1.0f, 2.0f, 3.0f));
    }

    [Fact]
    public void Dispose_MarksAsDisposed()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200);

        // Act
        sound.Dispose();

        // Assert
        Assert.True(sound.IsDisposed);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200);

        // Act & Assert
        sound.Dispose();
        sound.Dispose(); // Should not throw
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var sound = new Sound(1, "test.ogg", 100, 200, true);

        // Act
        var result = sound.ToString();

        // Assert
        Assert.Contains("Sound[", result);
        Assert.Contains("Id=1", result);
        Assert.Contains("Path=test.ogg", result);
        Assert.Contains("3D=True", result);
        Assert.Contains("Disposed=False", result);
    }

    [Fact]
    public void Constructor_2DSound_HasCorrect3DStatus()
    {
        // Arrange & Act
        var sound = new Sound(1, "test.ogg", 100, 200, false);

        // Assert
        Assert.False(sound.Is3D);
        Assert.Equal(0.0f, sound.PositionX);
        Assert.Equal(0.0f, sound.PositionY);
        Assert.Equal(0.0f, sound.PositionZ);
    }

    [Fact]
    public void Constructor_3DSound_HasCorrect3DStatus()
    {
        // Arrange & Act
        var sound = new Sound(1, "test.ogg", 100, 200, true);

        // Assert
        Assert.True(sound.Is3D);
    }
}