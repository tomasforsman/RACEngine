using Silk.NET.OpenGL;

namespace Rac.Rendering.VFX;

/// <summary>
/// Utility class for managing OpenGL framebuffers and textures for post-processing.
/// </summary>
public class FramebufferHelper
{
    private readonly GL _gl;

    public FramebufferHelper(GL gl)
    {
        _gl = gl;
    }

    /// <summary>
    /// Creates a framebuffer with an attached color texture.
    /// </summary>
    /// <param name="width">Texture width</param>
    /// <param name="height">Texture height</param>
    /// <param name="format">Internal texture format (e.g., RGB16F for HDR)</param>
    /// <returns>Tuple of (framebuffer handle, texture handle)</returns>
    public (uint framebuffer, uint texture) CreateFramebuffer(int width, int height, InternalFormat format)
    {
        // Generate framebuffer
        uint framebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

        // Generate texture
        uint texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, texture);
        
        // Setup texture parameters
        // NOTE: The original code used "in System.IntPtr.Zero" which caused Silk.NET
        // to try taking the address of IntPtr.Zero, leading to crashes. See issue #69.
        // 
        // ROBUST SOLUTION: Instead of using null data, allocate and initialize
        // the texture with zero data to ensure it's properly initialized.
        // This prevents potential undefined behavior with uninitialized texture memory.
        
        // Determine proper component count and pixel type based on internal format
        int componentCount;
        PixelFormat pixelFormat;
        PixelType pixelType;
        
        switch (format)
        {
            case InternalFormat.Rgb16f:
            case InternalFormat.Rgb8:
            case InternalFormat.Rgb:
                componentCount = 3;
                pixelFormat = PixelFormat.Rgb;
                pixelType = PixelType.Float;
                break;
                
            case InternalFormat.Rgba16f:
            case InternalFormat.Rgba8:
            case InternalFormat.Rgba:
                componentCount = 4;
                pixelFormat = PixelFormat.Rgba;
                pixelType = PixelType.Float;
                break;
                
            default:
                // Default to RGB for unknown formats
                componentCount = 3;
                pixelFormat = PixelFormat.Rgb;
                pixelType = PixelType.Float;
                break;
        }
        
        int pixelCount = width * height * componentCount;
        float[] zeroData = new float[pixelCount]; // Initialize to all zeros
        
        unsafe
        {
            fixed (float* dataPtr = zeroData)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, format, (uint)width, (uint)height, 0, 
                              pixelFormat, pixelType, dataPtr);
            }
        }
        
        // Check for OpenGL errors immediately after texture creation
        var error = _gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new InvalidOperationException($"OpenGL error after TexImage2D with format {format}: {error}");
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        // Attach texture to framebuffer
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, 
                                TextureTarget.Texture2D, texture, 0);

        // Check framebuffer completeness
        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new InvalidOperationException("Framebuffer is not complete!");
        }

        // Unbind framebuffer
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return (framebuffer, texture);
    }

    /// <summary>
    /// Creates a vertex array object and vertex buffer for a fullscreen quad.
    /// </summary>
    /// <returns>Tuple of (VAO handle, VBO handle)</returns>
    public (uint vao, uint vbo) CreateFullscreenQuad()
    {
        float[] quadVertices = new float[]
        {
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 1.0f
        };

        uint[] indices = new uint[] { 0, 1, 2, 2, 3, 0 };

        uint vao = _gl.GenVertexArray();
        uint vbo = _gl.GenBuffer();
        uint ebo = _gl.GenBuffer();

        _gl.BindVertexArray(vao);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* ptr = quadVertices)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), 
                              ptr, BufferUsageARB.StaticDraw);
            }
        }

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe
        {
            fixed (uint* ptr = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), 
                              ptr, BufferUsageARB.StaticDraw);
            }
        }

        // Position attribute
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), System.IntPtr.Zero);
        _gl.EnableVertexAttribArray(0);

        // Texture coordinate attribute
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), new System.IntPtr(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Clean up EBO - it's now part of the VAO state
        _gl.DeleteBuffer(ebo);

        _gl.BindVertexArray(0);

        return (vao, vbo);
    }

    /// <summary>
    /// Cleans up OpenGL resources.
    /// </summary>
    public void DeleteFramebuffer(uint framebuffer, uint texture)
    {
        _gl.DeleteTexture(texture);
        _gl.DeleteFramebuffer(framebuffer);
    }

    /// <summary>
    /// Cleans up quad resources.
    /// </summary>
    public void DeleteQuad(uint vao, uint vbo)
    {
        _gl.DeleteBuffer(vbo);
        _gl.DeleteVertexArray(vao);
    }
}