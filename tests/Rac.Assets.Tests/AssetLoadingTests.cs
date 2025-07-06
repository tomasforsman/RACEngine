using Rac.Assets;
using Rac.Assets.FileSystem;
using Rac.Assets.FileSystem.Loaders;
using Rac.Assets.Types;
using Xunit;

namespace Rac.Assets.Tests;

/// <summary>
/// Tests for the basic asset loading functionality.
/// </summary>
public class AssetLoadingTests
{
    [Fact]
    public void AssetServiceBuilder_Create_ReturnsBuilderInstance()
    {
        // Act
        var builder = AssetServiceBuilder.Create();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void AssetServiceBuilder_WithBasePath_SetsBasePath()
    {
        // Arrange
        var basePath = "test_assets";

        // Act & Assert - Should not throw
        var builder = AssetServiceBuilder.Create()
            .WithBasePath(basePath);

        Assert.NotNull(builder);
    }

    [Fact]
    public void AssetServiceBuilder_Build_CreatesValidService()
    {
        // Arrange
        var builder = AssetServiceBuilder.Create()
            .WithBasePath("test_assets");

        // Act
        var service = builder.Build();

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IAssetService>(service);
    }

    [Fact]
    public void FileAssetService_Constructor_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "test_assets_" + Guid.NewGuid().ToString("N")[..8]);

        try
        {
            // Act
            var service = new FileAssetService(tempPath);

            // Assert
            Assert.NotNull(service);
            Assert.True(Directory.Exists(tempPath));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void PngImageLoader_CanLoad_ReturnsTrueForPngExtension()
    {
        // Arrange
        var loader = new PngImageLoader();

        // Act & Assert
        Assert.True(loader.CanLoad(".png"));
        Assert.True(loader.CanLoad(".PNG"));
        Assert.False(loader.CanLoad(".jpg"));
        Assert.False(loader.CanLoad(".txt"));
    }

    [Fact]
    public void WavAudioLoader_CanLoad_ReturnsTrueForWavExtension()
    {
        // Arrange
        var loader = new WavAudioLoader();

        // Act & Assert
        Assert.True(loader.CanLoad(".wav"));
        Assert.True(loader.CanLoad(".WAV"));
        Assert.True(loader.CanLoad(".wave"));
        Assert.False(loader.CanLoad(".mp3"));
        Assert.False(loader.CanLoad(".txt"));
    }

    [Fact]
    public void PlainTextLoader_CanLoad_ReturnsTrueForSupportedExtensions()
    {
        // Arrange
        var loader = new PlainTextLoader();

        // Act & Assert
        Assert.True(loader.CanLoad(".txt"));
        Assert.True(loader.CanLoad(".vert"));
        Assert.True(loader.CanLoad(".frag"));
        Assert.True(loader.CanLoad(".json"));
        Assert.True(loader.CanLoad(".xml"));
        Assert.False(loader.CanLoad(".png"));
        Assert.False(loader.CanLoad(".wav"));
    }

    [Fact]
    public void Assets_StaticFacade_HasValidServiceInstance()
    {
        // Act
        var service = Assets.Service;

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<IAssetService>(service);
    }

    [Fact]
    public void Assets_StaticFacade_LoadTexture_ThrowsForNonexistentFile()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Assets.LoadTexture("nonexistent.png"));
        
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void Assets_StaticFacade_LoadAudio_ThrowsForNonexistentFile()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Assets.LoadAudio("nonexistent.wav"));
        
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void Assets_StaticFacade_LoadShaderSource_ThrowsForNonexistentFile()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Assets.LoadShaderSource("nonexistent.vert"));
        
        Assert.Contains("not found", exception.Message);
    }
}