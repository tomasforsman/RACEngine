using Rac.Rendering.Shader;
using Silk.NET.OpenGL;

namespace Rac.Rendering.VFX;

/// <summary>
/// Manages two-pass bloom post-processing pipeline.
/// </summary>
public class PostProcessing
{
    private readonly GL _gl;
    private readonly FramebufferHelper _framebufferHelper;

    // Framebuffers and textures
    private uint _sceneFramebuffer;
    private uint _sceneTexture;
    private uint _brightFramebuffer;
    private uint _brightTexture;
    private uint _blurFramebuffer1;
    private uint _blurTexture1;
    private uint _blurFramebuffer2;
    private uint _blurTexture2;

    // Shader programs
    private ShaderProgram? _brightExtractShader;
    private ShaderProgram? _blurShader;
    private ShaderProgram? _compositeShader;

    // Fullscreen quad
    private uint _quadVao;
    private uint _quadVbo;

    // Screen dimensions
    private int _screenWidth;
    private int _screenHeight;
    private int _blurWidth;
    private int _blurHeight;

    // Bloom parameters
    public float Threshold { get; set; } = 0.8f;
    public float Intensity { get; set; } = 1.5f;
    public float BloomStrength { get; set; } = 0.8f;
    public float Exposure { get; set; } = 1.0f;
    public float BlurSize { get; set; } = 1.0f;
    public int BlurPasses { get; set; } = 10;

    public PostProcessing(GL gl)
    {
        _gl = gl;
        _framebufferHelper = new FramebufferHelper(gl);
    }

    /// <summary>
    /// Initializes the bloom post-processing pipeline.
    /// </summary>
    public void Initialize(int screenWidth, int screenHeight)
    {
        // Validate OpenGL context and required features
        ValidateOpenGLContext();
        
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        
        // Use half resolution for blur to improve performance
        _blurWidth = screenWidth / 2;
        _blurHeight = screenHeight / 2;

        // Create framebuffers
        (_sceneFramebuffer, _sceneTexture) = _framebufferHelper.CreateFramebuffer(_screenWidth, _screenHeight, InternalFormat.Rgb16f);
        (_brightFramebuffer, _brightTexture) = _framebufferHelper.CreateFramebuffer(_screenWidth, _screenHeight, InternalFormat.Rgb16f);
        (_blurFramebuffer1, _blurTexture1) = _framebufferHelper.CreateFramebuffer(_blurWidth, _blurHeight, InternalFormat.Rgb16f);
        (_blurFramebuffer2, _blurTexture2) = _framebufferHelper.CreateFramebuffer(_blurWidth, _blurHeight, InternalFormat.Rgb16f);

        // Create fullscreen quad
        (_quadVao, _quadVbo) = _framebufferHelper.CreateFullscreenQuad();

        // Load shaders
        LoadShaders();
    }

    private void ValidateOpenGLContext()
    {
        // Check that basic OpenGL functions are available
        var error = _gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new InvalidOperationException($"OpenGL context has existing error: {error}");
        }
    }

    private void LoadShaders()
    {
        try
        {
            var quadVertexSource = ShaderLoader.LoadShaderFromFile("fullscreen_quad.vert");
            var brightExtractFragSource = ShaderLoader.LoadShaderFromFile("brightness_extract.frag");
            var blurFragSource = ShaderLoader.LoadShaderFromFile("gaussian_blur.frag");
            var compositeFragSource = ShaderLoader.LoadShaderFromFile("bloom_composite.frag");

            _brightExtractShader = new ShaderProgram(_gl, quadVertexSource, brightExtractFragSource);
            _blurShader = new ShaderProgram(_gl, quadVertexSource, blurFragSource);
            _compositeShader = new ShaderProgram(_gl, quadVertexSource, compositeFragSource);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load bloom shaders: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Begins scene rendering to the scene framebuffer.
    /// </summary>
    public void BeginScenePass()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _sceneFramebuffer);
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    /// <summary>
    /// Ends scene rendering and applies bloom effect.
    /// </summary>
    public void EndScenePassAndApplyBloom()
    {
        // Step 1: Extract bright areas
        ExtractBrightAreas();

        // Step 2: Blur bright areas with ping-pong rendering
        BlurBrightAreas();

        // Step 3: Composite scene with bloom
        CompositeScene();
    }

    private void ExtractBrightAreas()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _brightFramebuffer);
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.UseProgram(_brightExtractShader!.Handle);

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uSceneTexture"), 0);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uThreshold"), Threshold);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uIntensity"), Intensity);

        RenderFullscreenQuad();
    }

    private void BlurBrightAreas()
    {
        _gl.Viewport(0, 0, (uint)_blurWidth, (uint)_blurHeight);
        _gl.UseProgram(_blurShader!.Handle);
        _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uBlurSize"), BlurSize);

        bool horizontal = true;
        uint sourceTexture = _brightTexture;

        for (int i = 0; i < BlurPasses; i++)
        {
            uint targetFramebuffer = horizontal ? _blurFramebuffer1 : _blurFramebuffer2;
            
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, targetFramebuffer);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uHorizontal"), horizontal ? 1 : 0);

            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, sourceTexture);
            _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uBrightTexture"), 0);

            RenderFullscreenQuad();

            // Ping-pong between textures
            sourceTexture = horizontal ? _blurTexture1 : _blurTexture2;
            horizontal = !horizontal;
        }
    }

    private void CompositeScene()
    {
        // Render to screen
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.UseProgram(_compositeShader!.Handle);

        // Bind original scene texture
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uSceneTexture"), 0);

        // Bind final bloom texture (the last one written to)
        _gl.ActiveTexture(TextureUnit.Texture1);
        uint finalBloomTexture = (BlurPasses % 2 == 0) ? _blurTexture2 : _blurTexture1;
        _gl.BindTexture(TextureTarget.Texture2D, finalBloomTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uBloomTexture"), 1);

        // Set bloom parameters
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uBloomStrength"), BloomStrength);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uExposure"), Exposure);

        RenderFullscreenQuad();
    }

    private void RenderFullscreenQuad()
    {
        _gl.BindVertexArray(_quadVao);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, in System.IntPtr.Zero);
    }

    /// <summary>
    /// Handles screen resize by recreating framebuffers.
    /// </summary>
    public void Resize(int newWidth, int newHeight)
    {
        if (newWidth == _screenWidth && newHeight == _screenHeight)
            return;

        // Clean up old framebuffers
        CleanupFramebuffers();

        // Recreate with new dimensions
        Initialize(newWidth, newHeight);
    }

    private void CleanupFramebuffers()
    {
        _framebufferHelper.DeleteFramebuffer(_sceneFramebuffer, _sceneTexture);
        _framebufferHelper.DeleteFramebuffer(_brightFramebuffer, _brightTexture);
        _framebufferHelper.DeleteFramebuffer(_blurFramebuffer1, _blurTexture1);
        _framebufferHelper.DeleteFramebuffer(_blurFramebuffer2, _blurTexture2);
    }

    /// <summary>
    /// Cleans up all resources.
    /// </summary>
    public void Shutdown()
    {
        CleanupFramebuffers();
        _framebufferHelper.DeleteQuad(_quadVao, _quadVbo);
        
        _brightExtractShader?.Dispose();
        _blurShader?.Dispose();
        _compositeShader?.Dispose();
    }
}
