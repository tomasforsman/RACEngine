using Rac.Audio;
using Xunit;

namespace Rac.Audio.Tests;

public class AudioMixerTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var mixer = new AudioMixer();

        // Assert
        Assert.Equal(AudioMixer.DefaultVolume, mixer.MasterVolume);
        Assert.Equal(AudioMixer.DefaultVolume, mixer.MusicVolume);
        Assert.Equal(AudioMixer.DefaultVolume, mixer.SfxVolume);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void MasterVolume_ValidValues_SetsCorrectly(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act
        mixer.MasterVolume = volume;

        // Assert
        Assert.Equal(volume, mixer.MasterVolume);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void MasterVolume_InvalidValues_ThrowsArgumentOutOfRangeException(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mixer.MasterVolume = volume);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void MusicVolume_ValidValues_SetsCorrectly(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act
        mixer.MusicVolume = volume;

        // Assert
        Assert.Equal(volume, mixer.MusicVolume);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void MusicVolume_InvalidValues_ThrowsArgumentOutOfRangeException(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mixer.MusicVolume = volume);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void SfxVolume_ValidValues_SetsCorrectly(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act
        mixer.SfxVolume = volume;

        // Assert
        Assert.Equal(volume, mixer.SfxVolume);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void SfxVolume_InvalidValues_ThrowsArgumentOutOfRangeException(float volume)
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mixer.SfxVolume = volume);
    }

    [Fact]
    public void GetFinalMusicVolume_CalculatesCorrectly()
    {
        // Arrange
        var mixer = new AudioMixer
        {
            MasterVolume = 0.8f,
            MusicVolume = 0.6f
        };
        const float individualVolume = 0.5f;

        // Act
        var result = mixer.GetFinalMusicVolume(individualVolume);

        // Assert
        Assert.Equal(0.8f * 0.6f * 0.5f, result, precision: 6);
    }

    [Fact]
    public void GetFinalSfxVolume_CalculatesCorrectly()
    {
        // Arrange
        var mixer = new AudioMixer
        {
            MasterVolume = 0.9f,
            SfxVolume = 0.7f
        };
        const float individualVolume = 0.4f;

        // Act
        var result = mixer.GetFinalSfxVolume(individualVolume);

        // Assert
        Assert.Equal(0.9f * 0.7f * 0.4f, result, precision: 6);
    }

    [Fact]
    public void GetFinalVolume_CalculatesCorrectly()
    {
        // Arrange
        var mixer = new AudioMixer
        {
            MasterVolume = 0.8f
        };
        const float individualVolume = 0.6f;

        // Act
        var result = mixer.GetFinalVolume(individualVolume);

        // Assert
        Assert.Equal(0.8f * 0.6f, result, precision: 6);
    }

    [Fact]
    public void ResetToDefaults_RestoresAllVolumes()
    {
        // Arrange
        var mixer = new AudioMixer
        {
            MasterVolume = 0.5f,
            MusicVolume = 0.3f,
            SfxVolume = 0.7f
        };

        // Act
        mixer.ResetToDefaults();

        // Assert
        Assert.Equal(AudioMixer.DefaultVolume, mixer.MasterVolume);
        Assert.Equal(AudioMixer.DefaultVolume, mixer.MusicVolume);
        Assert.Equal(AudioMixer.DefaultVolume, mixer.SfxVolume);
    }

    [Fact]
    public void MuteAll_SetsMasterVolumeToZero()
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act
        mixer.MuteAll();

        // Assert
        Assert.Equal(AudioMixer.MinVolume, mixer.MasterVolume);
    }

    [Fact]
    public void UnmuteAll_SetsMasterVolumeToMax()
    {
        // Arrange
        var mixer = new AudioMixer();

        // Act
        mixer.UnmuteAll();

        // Assert
        Assert.Equal(AudioMixer.MaxVolume, mixer.MasterVolume);
    }

    [Fact]
    public void MasterVolumeChanged_FiresEvent()
    {
        // Arrange
        var mixer = new AudioMixer();
        var eventFired = false;
        var receivedVolume = 0.0f;

        mixer.MasterVolumeChanged += volume =>
        {
            eventFired = true;
            receivedVolume = volume;
        };

        // Act
        mixer.MasterVolume = 0.5f;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(0.5f, receivedVolume);
    }

    [Fact]
    public void MusicVolumeChanged_FiresEvent()
    {
        // Arrange
        var mixer = new AudioMixer();
        var eventFired = false;
        var receivedVolume = 0.0f;

        mixer.MusicVolumeChanged += volume =>
        {
            eventFired = true;
            receivedVolume = volume;
        };

        // Act
        mixer.MusicVolume = 0.7f;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(0.7f, receivedVolume);
    }

    [Fact]
    public void SfxVolumeChanged_FiresEvent()
    {
        // Arrange
        var mixer = new AudioMixer();
        var eventFired = false;
        var receivedVolume = 0.0f;

        mixer.SfxVolumeChanged += volume =>
        {
            eventFired = true;
            receivedVolume = volume;
        };

        // Act
        mixer.SfxVolume = 0.3f;

        // Assert
        Assert.True(eventFired);
        Assert.Equal(0.3f, receivedVolume);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var mixer = new AudioMixer
        {
            MasterVolume = 0.8f,
            MusicVolume = 0.6f,
            SfxVolume = 0.4f
        };

        // Act
        var result = mixer.ToString();

        // Assert
        Assert.Contains("AudioMixer", result);
        Assert.Contains("Master=0.80", result);
        Assert.Contains("Music=0.60", result);
        Assert.Contains("SFX=0.40", result);
    }
}