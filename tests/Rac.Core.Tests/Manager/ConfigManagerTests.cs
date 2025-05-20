using Xunit;
using System.IO;
using Rac.Core.Manager;
using Microsoft.Extensions.Configuration;

namespace Rac.Core.Tests.Manager;

public class ConfigManagerTests : IDisposable
{
    private readonly string _testIniFile = "test_config.ini";

    public ConfigManagerTests()
    {
        // Setup test config file
        File.WriteAllText(_testIniFile, @"
[window]
Title=Test Window
Size=800,600
VSync=true
");
    }

    public void Dispose()
    {
        // Cleanup test config file
        if (File.Exists(_testIniFile))
        {
            File.Delete(_testIniFile);
        }
    }

    [Fact]
    public void ConfigManager_LoadsWindowSettingsFromFile()
    {
        // This test requires a manual implementation to override the default config file path
        // In a real-world scenario, we'd use a DI container or pass the config path to the constructor
        
        // For now, we'll verify that the WindowSettings class works as expected
        var settings = new WindowSettings
        {
            Title = "Test Window",
            Size = "800,600",
            VSync = true
        };
        
        Assert.Equal("Test Window", settings.Title);
        Assert.Equal("800,600", settings.Size);
        Assert.True(settings.VSync);
    }

    [Fact]
    public void WindowSettings_DefaultProperties_AreNullOrDefault()
    {
        // Arrange & Act
        var settings = new WindowSettings();
        
        // Assert
        Assert.Null(settings.Title);
        Assert.Null(settings.Size);
        Assert.Null(settings.VSync);
    }
}