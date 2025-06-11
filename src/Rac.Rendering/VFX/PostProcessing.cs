// ════════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL BLOOM POST-PROCESSING PIPELINE
// ════════════════════════════════════════════════════════════════════════════════
//
// This class demonstrates several fundamental computer graphics concepts:
//
// 1. BLOOM EFFECT THEORY:
//    - Simulates camera lens bloom/glare from bright light sources
//    - Creates ethereal glow around bright objects (HDR → LDR tone mapping)
//    - Multi-pass algorithm: Extract → Blur → Composite
//    - Used extensively in modern games for cinematic visual quality
//
// 2. MULTI-PASS RENDERING PIPELINE:
//    - Scene Pass: Render geometry to offscreen framebuffer
//    - Bright Extract Pass: Isolate pixels above brightness threshold
//    - Blur Passes: Apply Gaussian blur using ping-pong technique
//    - Composite Pass: Blend original scene with blurred bloom
//
// 3. FRAMEBUFFER OBJECTS (FBOs):
//    - Offscreen rendering targets (render-to-texture)
//    - Multiple color attachments for different data streams
//    - Essential for post-processing, shadows, reflections, deferred rendering
//
// 4. GAUSSIAN BLUR IMPLEMENTATION:
//    - Separable filter: 2D blur = horizontal blur + vertical blur
//    - Ping-pong rendering: alternate between two framebuffers
//    - Multiple passes create wider, smoother blur kernels
//
// 5. HIGH DYNAMIC RANGE (HDR) CONCEPTS:
//    - Scene rendered in linear color space (RGB16F format)
//    - Brightness values can exceed 1.0 (real-world luminance)
//    - Tone mapping compresses HDR → display-ready LDR
//
// 6. SHADER PIPELINE MANAGEMENT:
//    - Different shaders for each processing stage
//    - Uniform parameter passing for runtime control
//    - Texture unit management for multi-texture operations
//
// ════════════════════════════════════════════════════════════════════════════════

using Rac.Rendering.Shader;
using Silk.NET.OpenGL;
using System;

namespace Rac.Rendering.VFX;

/// <summary>
/// Manages two-pass bloom post-processing pipeline.
///
/// BLOOM ALGORITHM OVERVIEW:
/// 1. Render scene to HDR framebuffer
/// 2. Extract bright pixels (threshold-based)
/// 3. Blur bright areas using separable Gaussian filter
/// 4. Composite original scene + blurred bloom
///
/// This creates the characteristic "glow" effect seen in photography
/// when bright light sources cause lens aberrations.
/// </summary>
public class PostProcessing : IDisposable
{
    private readonly GL _gl;
    private readonly FramebufferHelper _framebufferHelper;
    private bool _disposed;

    // ═══════════════════════════════════════════════════════════════════════════
    // FRAMEBUFFER OBJECTS AND RENDER TARGETS
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Modern GPUs use Framebuffer Objects (FBOs) for offscreen rendering.
    // Each framebuffer can have multiple attachments (color, depth, stencil).
    // This enables render-to-texture workflows essential for post-processing.

    // Scene rendering: Full-resolution HDR color buffer
    private uint _sceneFramebuffer;    // FBO handle
    private uint _sceneTexture;        // Color attachment (RGB16F HDR format)

    // Brightness extraction: Isolate bright pixels for blooming
    private uint _brightFramebuffer;   // Same resolution as scene
    private uint _brightTexture;       // Filtered bright pixels only

    // Ping-pong blur buffers: Alternate between these for multi-pass blur
    private uint _blurFramebuffer1;    // First blur target (half resolution)
    private uint _blurTexture1;        // Horizontal or vertical blur result
    private uint _blurFramebuffer2;    // Second blur target (half resolution)
    private uint _blurTexture2;        // Alternate blur result

    // ═══════════════════════════════════════════════════════════════════════════
    // SHADER PROGRAM PIPELINE
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Each post-processing stage requires specialized fragment shaders:
    // - Vertex shader: Simple fullscreen quad positioning
    // - Fragment shaders: Pixel-level processing algorithms

    private ShaderProgram? _brightExtractShader;  // Threshold-based bright pixel extraction
    private ShaderProgram? _blurShader;           // Separable Gaussian blur filter
    private ShaderProgram? _compositeShader;      // Final scene + bloom composition

    // ═══════════════════════════════════════════════════════════════════════════
    // FULLSCREEN QUAD GEOMETRY
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Post-processing effects render to fullscreen quads (two triangles).
    // VAO stores vertex array state, VBO contains vertex positions.
    // This eliminates the need for complex 3D geometry during post-processing.

    private uint _quadVao;  // Vertex Array Object (OpenGL state container)
    private uint _quadVbo;  // Vertex Buffer Object (vertex position data)

    // ═══════════════════════════════════════════════════════════════════════════
    // RESOLUTION AND SCALING
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Performance optimization: Use different resolutions for different stages.
    // Scene/bright extraction: Full resolution (visual quality)
    // Blur operations: Half resolution (performance, blur naturally upscales)

    private int _screenWidth;   // Primary display resolution
    private int _screenHeight;
    private int _blurWidth;     // Blur buffer resolution (typically width/2)
    private int _blurHeight;    // Blur buffer resolution (typically height/2)

    // ═══════════════════════════════════════════════════════════════════════════
    // BLOOM ARTISTIC PARAMETERS
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // These parameters control the visual characteristics of the bloom effect.
    // Artists can tune these values to achieve different aesthetic goals.

    /// <summary>Brightness threshold for bloom extraction (0.0-2.0+)</summary>
    /// <remarks>Higher values = only very bright pixels bloom</remarks>
    public float Threshold { get; set; } = 0.8f;

    /// <summary>Multiplier for extracted bright pixels (0.5-3.0+)</summary>
    /// <remarks>Amplifies brightness before blur to enhance glow intensity</remarks>
    public float Intensity { get; set; } = 1.5f;

    /// <summary>Final bloom contribution to composite (0.0-1.0+)</summary>
    /// <remarks>Controls how much bloom blends with original scene</remarks>
    public float BloomStrength { get; set; } = 0.8f;

    /// <summary>HDR exposure compensation (0.1-3.0+)</summary>
    /// <remarks>Tone mapping parameter for HDR → LDR conversion</remarks>
    public float Exposure { get; set; } = 1.0f;

    /// <summary>Blur kernel size multiplier (0.5-2.0+)</summary>
    /// <remarks>Controls spread/radius of the glow effect</remarks>
    public float BlurSize { get; set; } = 1.0f;

    /// <summary>Number of blur iterations (4-20)</summary>
    /// <remarks>More passes = wider, smoother blur but higher cost</remarks>
    public int BlurPasses { get; set; } = 10;

    public PostProcessing(GL gl)
    {
        _gl = gl;
        _framebufferHelper = new FramebufferHelper(gl);
    }

    /// <summary>
    /// Initializes the bloom post-processing pipeline.
    ///
    /// INITIALIZATION SEQUENCE:
    /// 1. Validate OpenGL context and extensions
    /// 2. Calculate optimal buffer resolutions
    /// 3. Create framebuffer objects and texture attachments
    /// 4. Set up fullscreen quad geometry
    /// 5. Compile and link shader programs
    /// </summary>
    public void Initialize(int screenWidth, int screenHeight)
    {
        // ───────────────────────────────────────────────────────────────────────
        // OPENGL CONTEXT VALIDATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // Ensure OpenGL context is valid and supports required features.
        // Post-processing requires: FBO extension, floating-point textures,
        // multiple render targets, GLSL shaders.

        ValidateOpenGLContext();

        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        // ───────────────────────────────────────────────────────────────────────
        // PERFORMANCE OPTIMIZATION: MULTI-RESOLUTION RENDERING
        // ───────────────────────────────────────────────────────────────────────
        //
        // Blur operations are bandwidth-intensive and naturally low-frequency.
        // Rendering blur at half-resolution provides significant performance gains
        // with minimal visual quality loss (blur hides aliasing artifacts).

        _blurWidth = screenWidth / 2;
        _blurHeight = screenHeight / 2;

        // ───────────────────────────────────────────────────────────────────────
        // FRAMEBUFFER CREATION WITH HDR SUPPORT
        // ───────────────────────────────────────────────────────────────────────
        //
        // RGB16F format provides:
        // - 16-bit floating point per channel (vs 8-bit integer in RGB8)
        // - Values beyond [0,1] range (essential for HDR bloom)
        // - Linear color space (correct for lighting calculations)
        // - Hardware filtering support on modern GPUs

        (_sceneFramebuffer, _sceneTexture) = _framebufferHelper.CreateFramebuffer(_screenWidth, _screenHeight, InternalFormat.Rgb16f);
        (_brightFramebuffer, _brightTexture) = _framebufferHelper.CreateFramebuffer(_screenWidth, _screenHeight, InternalFormat.Rgb16f);
        (_blurFramebuffer1, _blurTexture1) = _framebufferHelper.CreateFramebuffer(_blurWidth, _blurHeight, InternalFormat.Rgb16f);
        (_blurFramebuffer2, _blurTexture2) = _framebufferHelper.CreateFramebuffer(_blurWidth, _blurHeight, InternalFormat.Rgb16f);

        // ───────────────────────────────────────────────────────────────────────
        // FULLSCREEN QUAD SETUP
        // ───────────────────────────────────────────────────────────────────────
        //
        // Post-processing renders to screen-aligned quads rather than 3D geometry.
        // Quad covers entire screen in normalized device coordinates [-1,+1].
        // Fragment shader processes each screen pixel independently.

        (_quadVao, _quadVbo) = _framebufferHelper.CreateFullscreenQuad();

        // ───────────────────────────────────────────────────────────────────────
        // SHADER COMPILATION AND LINKING
        // ───────────────────────────────────────────────────────────────────────

        LoadShaders();
    }

    private void ValidateOpenGLContext()
    {
        // ───────────────────────────────────────────────────────────────────────
        // OPENGL ERROR CHECKING
        // ───────────────────────────────────────────────────────────────────────
        //
        // OpenGL uses error flags rather than exceptions. Check for existing
        // errors before proceeding to avoid confusing error sources.
        // Production code should also validate required extensions.

        var error = _gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new InvalidOperationException($"OpenGL context has existing error: {error}");
        }
    }

    private void LoadShaders()
    {
        // ───────────────────────────────────────────────────────────────────────
        // SHADER PROGRAM COMPILATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // Each post-processing stage requires specialized fragment shaders:
        //
        // 1. Brightness Extract: Threshold function + intensity scaling
        //    if (luminance(color) > threshold) output = color * intensity;
        //
        // 2. Gaussian Blur: Separable convolution filter
        //    Horizontal pass: blur in X direction
        //    Vertical pass: blur in Y direction
        //
        // 3. Composite: Additive blending + tone mapping
        //    final_color = tonemap(scene_color + bloom_color * strength);

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
    ///
    /// RENDER TARGET SWITCHING:
    /// - Binds scene framebuffer as active render target
    /// - All subsequent draw calls render to scene texture (not screen)
    /// - Clears color and depth buffers for new frame
    /// - Sets viewport to match framebuffer dimensions
    /// </summary>
    public void BeginScenePass()
    {
        // ───────────────────────────────────────────────────────────────────────
        // FRAMEBUFFER BINDING AND SETUP
        // ───────────────────────────────────────────────────────────────────────
        //
        // Bind framebuffer: redirects all rendering to offscreen texture
        // Viewport: maps normalized device coordinates to pixel coordinates
        // Clear: initializes frame with background color and far depth value

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _sceneFramebuffer);
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    /// <summary>
    /// Ends scene rendering and applies bloom effect.
    ///
    /// POST-PROCESSING PIPELINE EXECUTION:
    /// 1. Extract bright pixels from scene using threshold
    /// 2. Apply multi-pass Gaussian blur to bright areas
    /// 3. Composite original scene with blurred bloom
    /// 4. Output final result to screen framebuffer
    /// </summary>
    public void EndScenePassAndApplyBloom()
    {
        // ───────────────────────────────────────────────────────────────────────
        // THREE-STAGE BLOOM PIPELINE
        // ───────────────────────────────────────────────────────────────────────

        // Stage 1: Brightness Thresholding
        // Isolate pixels above luminance threshold for blooming
        ExtractBrightAreas();

        // Stage 2: Blur Generation
        // Apply separable Gaussian blur using ping-pong technique
        BlurBrightAreas();

        // Stage 3: Final Composition
        // Blend original scene with blurred bloom using additive blending
        CompositeScene();
    }

    private void ExtractBrightAreas()
    {
        // ───────────────────────────────────────────────────────────────────────
        // BRIGHTNESS EXTRACTION PASS
        // ───────────────────────────────────────────────────────────────────────
        //
        // ALGORITHM:
        // for each pixel in scene:
        //     luminance = 0.299*R + 0.587*G + 0.114*B  (perceptual brightness)
        //     if luminance > threshold:
        //         output = input_color * intensity_multiplier
        //     else:
        //         output = black (0,0,0)
        //
        // This creates a mask of only the brightest scene areas.

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _brightFramebuffer);
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.UseProgram(_brightExtractShader!.Handle);

        // ───────────────────────────────────────────────────────────────────────
        // TEXTURE BINDING AND UNIFORM SETUP
        // ───────────────────────────────────────────────────────────────────────
        //
        // Texture Unit 0: Scene color data (input)
        // Uniforms: Runtime parameters passed to fragment shader

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uSceneTexture"), 0);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uThreshold"), Threshold);
        _gl.Uniform1(_gl.GetUniformLocation(_brightExtractShader.Handle, "uIntensity"), Intensity);

        RenderFullscreenQuad();
    }

    private void BlurBrightAreas()
    {
        // ───────────────────────────────────────────────────────────────────────
        // SEPARABLE GAUSSIAN BLUR IMPLEMENTATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // SEPARABLE FILTER THEORY:
        // 2D Gaussian kernel can be separated into two 1D passes:
        // - Horizontal pass: blur each row independently
        // - Vertical pass: blur each column independently
        // Complexity: O(n²) → O(2n) for n×n kernel
        //
        // PING-PONG RENDERING:
        // Alternate between two framebuffers to avoid read/write conflicts:
        // Pass 1: Read from bright texture → Write to blur1
        // Pass 2: Read from blur1 → Write to blur2
        // Pass 3: Read from blur2 → Write to blur1
        // ... continue alternating

        _gl.Viewport(0, 0, (uint)_blurWidth, (uint)_blurHeight);
        _gl.UseProgram(_blurShader!.Handle);
        _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uBlurSize"), BlurSize);

        bool horizontal = true;           // Start with horizontal blur
        uint sourceTexture = _brightTexture;  // Initial input from bright extraction

        for (int i = 0; i < BlurPasses; i++)
        {
            // ───────────────────────────────────────────────────────────────────
            // PING-PONG BUFFER SELECTION
            // ───────────────────────────────────────────────────────────────────
            //
            // Alternate target framebuffer each pass to prevent read/write hazards.
            // GPU cannot simultaneously read from and write to same texture.

            uint targetFramebuffer = horizontal ? _blurFramebuffer1 : _blurFramebuffer2;

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, targetFramebuffer);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            // ───────────────────────────────────────────────────────────────────
            // BLUR DIRECTION CONTROL
            // ───────────────────────────────────────────────────────────────────
            //
            // Fragment shader samples pixels along single axis per pass:
            // Horizontal: Sample (x-n, y), (x-n+1, y), ..., (x+n, y)
            // Vertical:   Sample (x, y-n), (x, y-n+1), ..., (x, y+n)

            _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uHorizontal"), horizontal ? 1 : 0);

            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, sourceTexture);
            _gl.Uniform1(_gl.GetUniformLocation(_blurShader.Handle, "uBrightTexture"), 0);

            RenderFullscreenQuad();

            // ───────────────────────────────────────────────────────────────────
            // PING-PONG STATE UPDATE
            // ───────────────────────────────────────────────────────────────────
            //
            // Update source texture and blur direction for next iteration.
            // Final result will be in texture corresponding to last pass.

            sourceTexture = horizontal ? _blurTexture1 : _blurTexture2;
            horizontal = !horizontal;
        }
    }

    private void CompositeScene()
    {
        // ───────────────────────────────────────────────────────────────────────
        // FINAL COMPOSITION AND TONE MAPPING
        // ───────────────────────────────────────────────────────────────────────
        //
        // ADDITIVE BLENDING:
        // final_color = scene_color + (bloom_color * bloom_strength)
        //
        // TONE MAPPING (HDR → LDR):
        // Exposure adjustment compresses high dynamic range to displayable range.
        // Common operators: Reinhard, ACES, linear scaling
        //
        // OUTPUT TARGET:
        // Framebuffer 0 = default framebuffer (screen/backbuffer)

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);  // Render to screen
        _gl.Viewport(0, 0, (uint)_screenWidth, (uint)_screenHeight);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.UseProgram(_compositeShader!.Handle);

        // ───────────────────────────────────────────────────────────────────────
        // MULTI-TEXTURE SAMPLING
        // ───────────────────────────────────────────────────────────────────────
        //
        // Fragment shader samples from two textures simultaneously:
        // Texture Unit 0: Original scene (full detail)
        // Texture Unit 1: Blurred bloom (glow contribution)

        // Bind original scene texture
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _sceneTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uSceneTexture"), 0);

        // ───────────────────────────────────────────────────────────────────────
        // FINAL BLOOM TEXTURE SELECTION
        // ───────────────────────────────────────────────────────────────────────
        //
        // Determine which texture contains the final blur result.
        // Depends on whether total blur passes is even or odd.
        // This is because ping-pong alternates between textures.

        _gl.ActiveTexture(TextureUnit.Texture1);
        uint finalBloomTexture = (BlurPasses % 2 == 0) ? _blurTexture2 : _blurTexture1;
        _gl.BindTexture(TextureTarget.Texture2D, finalBloomTexture);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uBloomTexture"), 1);

        // ───────────────────────────────────────────────────────────────────────
        // ARTISTIC PARAMETER SETUP
        // ───────────────────────────────────────────────────────────────────────

        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uBloomStrength"), BloomStrength);
        _gl.Uniform1(_gl.GetUniformLocation(_compositeShader.Handle, "uExposure"), Exposure);

        RenderFullscreenQuad();
    }

    private void RenderFullscreenQuad()
    {
        // ───────────────────────────────────────────────────────────────────────
        // FULLSCREEN QUAD RENDERING
        // ───────────────────────────────────────────────────────────────────────
        //
        // VAO contains:
        // - 4 vertices at screen corners: (-1,-1), (1,-1), (1,1), (-1,1)
        // - 2 triangles (6 indices): [0,1,2] and [2,3,0]
        // - UV coordinates for texture sampling: (0,0) to (1,1)
        //
        // Fragment shader executes once per screen pixel, sampling input textures
        // and computing post-processing effects (blur, composition, tone mapping).

        _gl.BindVertexArray(_quadVao);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, in System.IntPtr.Zero);
    }

    /// <summary>
    /// Handles screen resize by recreating framebuffers.
    ///
    /// DYNAMIC RESOLUTION HANDLING:
    /// - Framebuffer textures have fixed dimensions
    /// - Window resize requires texture reallocation
    /// - Clean up old resources to prevent memory leaks
    /// - Maintain blur resolution scaling (half of screen resolution)
    /// </summary>
    public void Resize(int newWidth, int newHeight)
    {
        // Early exit if dimensions unchanged (common case)
        if (newWidth == _screenWidth && newHeight == _screenHeight)
            return;

        // ───────────────────────────────────────────────────────────────────────
        // RESOURCE LIFECYCLE MANAGEMENT
        // ───────────────────────────────────────────────────────────────────────
        //
        // OpenGL resources must be explicitly freed to prevent memory leaks.
        // Modern GPUs have limited VRAM; leaked framebuffers can exhaust memory.

        CleanupFramebuffers();

        // Recreate framebuffers with new dimensions
        Initialize(newWidth, newHeight);
    }

    private void CleanupFramebuffers()
    {
        // ───────────────────────────────────────────────────────────────────────
        // OPENGL RESOURCE DEALLOCATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // Each framebuffer consists of:
        // - Framebuffer object (FBO handle)
        // - Color texture attachment(s)
        // - Optional depth/stencil attachments
        // All must be explicitly deleted to free GPU memory.

        _framebufferHelper.DeleteFramebuffer(_sceneFramebuffer, _sceneTexture);
        _framebufferHelper.DeleteFramebuffer(_brightFramebuffer, _brightTexture);
        _framebufferHelper.DeleteFramebuffer(_blurFramebuffer1, _blurTexture1);
        _framebufferHelper.DeleteFramebuffer(_blurFramebuffer2, _blurTexture2);
    }

    /// <summary>
    /// Releases all resources used by the PostProcessing system.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up if Dispose() is not called.
    /// </summary>
    ~PostProcessing()
    {
        Dispose(false);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            _brightExtractShader?.Dispose();
            _blurShader?.Dispose();
            _compositeShader?.Dispose();
        }

        // Free unmanaged resources
        CleanupFramebuffers();
        _framebufferHelper.DeleteQuad(_quadVao, _quadVbo);

        _disposed = true;
    }

    /// <summary>
    /// Cleans up all resources.
    /// 
    /// LEGACY METHOD - Use Dispose() instead.
    /// Maintained for backwards compatibility.
    /// </summary>
    [Obsolete("Use Dispose() instead. This method will be removed in a future version.")]
    public void Shutdown()
    {
        Dispose();
    }
}
