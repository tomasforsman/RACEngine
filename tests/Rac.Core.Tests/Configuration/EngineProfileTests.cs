using Rac.Core.Configuration;
using Xunit;

namespace Rac.Core.Tests.Configuration;

public class EngineProfileTests
{
    [Fact]
    public void EngineProfile_HasExpectedValues()
    {
        // Verify that all expected enum values exist
        Assert.True(Enum.IsDefined(typeof(EngineProfile), EngineProfile.FullGame));
        Assert.True(Enum.IsDefined(typeof(EngineProfile), EngineProfile.Headless));
        Assert.True(Enum.IsDefined(typeof(EngineProfile), EngineProfile.Custom));
    }

    [Fact]
    public void EngineProfile_EnumValues_HaveCorrectNames()
    {
        Assert.Equal("FullGame", EngineProfile.FullGame.ToString());
        Assert.Equal("Headless", EngineProfile.Headless.ToString());
        Assert.Equal("Custom", EngineProfile.Custom.ToString());
    }
}