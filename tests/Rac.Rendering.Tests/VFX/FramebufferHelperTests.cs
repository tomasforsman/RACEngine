using Rac.Rendering.VFX;
using Xunit;

namespace Rac.Rendering.Tests.VFX;

/// <summary>
/// Tests for FramebufferHelper functionality.
/// These tests focus on API structure and parameter validation without requiring OpenGL context.
/// </summary>
public class FramebufferHelperTests
{
    [Fact]
    public void FramebufferHelper_HasCreateFramebufferMethod()
    {
        // Verify that CreateFramebuffer method exists with correct signature
        var method = typeof(FramebufferHelper).GetMethod("CreateFramebuffer");
        
        Assert.NotNull(method);
        Assert.Equal(typeof((uint framebuffer, uint texture)), method.ReturnType);
        
        var parameters = method.GetParameters();
        Assert.Equal(3, parameters.Length);
        Assert.Equal(typeof(int), parameters[0].ParameterType); // width
        Assert.Equal(typeof(int), parameters[1].ParameterType); // height
        Assert.Equal("Silk.NET.OpenGL.InternalFormat", parameters[2].ParameterType.FullName); // format
    }

    [Fact]
    public void FramebufferHelper_HasCreateFullscreenQuadMethod()
    {
        // Verify that CreateFullscreenQuad method exists with correct signature
        var method = typeof(FramebufferHelper).GetMethod("CreateFullscreenQuad");
        
        Assert.NotNull(method);
        Assert.Equal(typeof((uint vao, uint vbo)), method.ReturnType);
        Assert.Empty(method.GetParameters()); // Should have no parameters
    }

    [Fact]
    public void FramebufferHelper_HasDeleteFramebufferMethod()
    {
        // Verify that DeleteFramebuffer method exists with correct signature
        var method = typeof(FramebufferHelper).GetMethod("DeleteFramebuffer");
        
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
        
        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(uint), parameters[0].ParameterType); // framebuffer
        Assert.Equal(typeof(uint), parameters[1].ParameterType); // texture
    }

    [Fact]
    public void FramebufferHelper_HasDeleteQuadMethod()
    {
        // Verify that DeleteQuad method exists with correct signature
        var method = typeof(FramebufferHelper).GetMethod("DeleteQuad");
        
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
        
        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(uint), parameters[0].ParameterType); // vao
        Assert.Equal(typeof(uint), parameters[1].ParameterType); // vbo
    }

    [Fact]
    public void FramebufferHelper_HasConstructorWithGLParameter()
    {
        // Verify that constructor accepts GL parameter
        var constructor = typeof(FramebufferHelper).GetConstructor(new[] { typeof(Silk.NET.OpenGL.GL) });
        
        Assert.NotNull(constructor);
        Assert.Single(constructor.GetParameters());
        Assert.Equal("Silk.NET.OpenGL.GL", constructor.GetParameters()[0].ParameterType.FullName);
    }
}