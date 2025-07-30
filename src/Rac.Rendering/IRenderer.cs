// File: src/Engine/Rendering/IRenderer.cs

using Rac.Rendering.Shader;
using Rac.Rendering.Camera;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Rac.Rendering;

/// <summary>
/// Defines the contract for rendering services providing both beginner-friendly and advanced rendering functionality.
/// Implements a layered API design enabling progressive learning and usage complexity.
/// </summary>
/// <remarks>
/// The IRenderer interface follows the Facade pattern to simplify complex OpenGL rendering operations.
/// It provides multiple access layers for different user skill levels:
/// 
/// **Layer 1 - Beginner API:**
/// - Simple color and vertex operations (SetColor, UpdateVertices, Draw)
/// - Immediate rendering with minimal configuration
/// - Automatic state management and sensible defaults
/// 
/// **Layer 2 - Intermediate API:**
/// - Camera transformations and matrix operations
/// - Texture and shader mode configuration
/// - Primitive type control for different geometry types
/// 
/// **Layer 3 - Advanced API:**
/// - Custom vertex layouts and generic vertex types
/// - Fine-grained render state control
/// - Performance optimization features
/// 
/// **Educational Design Principles:**
/// - Progressive disclosure: simple methods first, advanced features optional
/// - Consistent naming: methods clearly indicate their purpose and scope
/// - Type safety: generic constraints prevent runtime errors
/// - Documentation: extensive examples show proper usage patterns
/// 
/// **Architecture Integration:**
/// - Service interface pattern enables dependency injection and testing
/// - Null Object pattern implementation (NullRenderer) for headless scenarios
/// - OpenGL abstraction enables future support for other graphics APIs
/// - Event-driven lifecycle matches game engine update patterns
/// 
/// **Performance Considerations:**
/// - Batched vertex uploads minimize GPU state changes
/// - Shader mode caching reduces unnecessary program switches
/// - Matrix operations use efficient SIMD-optimized types
/// - Automatic vertex layout detection eliminates manual configuration
/// </remarks>
/// <example>
/// <code>
/// // Basic rendering workflow
/// renderer.Clear();
/// renderer.SetColor(new Vector4D&lt;float&gt;(1, 0, 0, 1)); // Red
/// renderer.UpdateVertices(triangleVertices);
/// renderer.Draw();
/// renderer.FinalizeFrame();
/// 
/// // Advanced textured rendering
/// renderer.SetShaderMode(ShaderMode.Textured);
/// renderer.SetTexture(playerTexture);
/// renderer.SetCameraMatrix(camera.ViewProjectionMatrix);
/// renderer.UpdateVertices(spriteVertices);
/// renderer.Draw();
/// </code>
/// </example>
public interface IRenderer
{
    // ═══════════════════════════════════════════════════════════════════════════
    // LIFECYCLE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes the renderer with OpenGL state, compiles default shaders, and sets up vertex array objects.
    /// This method must be called before any rendering operations can be performed.
    /// </summary>
    /// <param name="window">
    /// The target window for rendering operations. Provides OpenGL context and surface information.
    /// Must be a valid, initialized window with an active OpenGL context.
    /// </param>
    /// <remarks>
    /// Initialization process includes:
    /// - OpenGL state configuration (depth testing, blending, viewport)
    /// - Default shader program compilation and linking
    /// - Vertex Array Object (VAO) and Vertex Buffer Object (VBO) creation
    /// - Texture management system setup
    /// - Error checking and validation
    /// 
    /// Educational Note: Modern OpenGL requires explicit setup of rendering state,
    /// unlike older fixed-function pipelines. This initialization establishes the
    /// rendering foundation that all subsequent operations depend on.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when window is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when OpenGL context is invalid or shader compilation fails.</exception>
    /// <example>
    /// <code>
    /// // Initialize renderer with window context
    /// var window = WindowBuilder.Configure(windowManager)
    ///     .WithTitle("Game Window")
    ///     .WithSize(1280, 720)
    ///     .Create();
    /// 
    /// renderer.Initialize(window);
    /// </code>
    /// </example>
    void Initialize(IWindow window);

    /// <summary>
    /// Handles window resize events by updating viewport dimensions and aspect ratio calculations.
    /// Ensures rendering remains properly scaled and positioned after window size changes.
    /// </summary>
    /// <param name="newSize">
    /// The new window dimensions in pixels. Both width and height must be positive values.
    /// These dimensions define the rendering viewport size.
    /// </param>
    /// <remarks>
    /// Resize operations include:
    /// - OpenGL viewport configuration (glViewport)
    /// - Aspect ratio recalculation for camera systems
    /// - Framebuffer and render target updates if applicable
    /// - UI layout adjustments for responsive interfaces
    /// 
    /// Educational Note: Viewport management is crucial for maintaining proper
    /// graphics scaling across different window sizes and display densities.
    /// Modern applications must handle dynamic resizing gracefully.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when newSize has non-positive dimensions.</exception>
    /// <example>
    /// <code>
    /// // Handle window resize in game loop
    /// windowManager.OnResize += (newSize) => {
    ///     renderer.Resize(newSize);
    ///     camera.UpdateAspectRatio(newSize.X / (float)newSize.Y);
    /// };
    /// </code>
    /// </example>
    void Resize(Vector2D<int> newSize);

    /// <summary>
    /// Releases all OpenGL resources and performs cleanup before renderer destruction.
    /// Should be called during application shutdown to prevent resource leaks.
    /// </summary>
    /// <remarks>
    /// Shutdown process includes:
    /// - Vertex buffer and array object deletion
    /// - Shader program cleanup and resource release
    /// - Texture memory deallocation
    /// - OpenGL context state reset
    /// - Debug validation of proper cleanup
    /// 
    /// Educational Note: Proper resource cleanup is essential in graphics programming
    /// as GPU resources are limited and shared across applications. Failing to
    /// release resources can cause memory leaks and system instability.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Application shutdown sequence
    /// try
    /// {
    ///     renderer.Shutdown();
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError(ex, "Renderer", "Shutdown");
    /// }
    /// </code>
    /// </example>
    void Shutdown();

    // ═══════════════════════════════════════════════════════════════════════════
    // BASIC RENDERING OPERATIONS (LAYER 1: BEGINNER API)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clears the color buffer at the start of a frame with the default background color.
    /// This operation prepares a clean canvas for the current frame's rendering.
    /// </summary>
    /// <remarks>
    /// Frame clearing is essential for proper rendering as it:
    /// - Removes artifacts from the previous frame
    /// - Provides a consistent starting state for new rendering
    /// - Prevents visual corruption from overlapping geometry
    /// - Establishes the background color for transparent objects
    /// 
    /// Educational Note: The clear operation is typically one of the first
    /// operations in a frame, followed by setting up cameras and drawing geometry.
    /// Modern GPUs optimize clear operations for maximum performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical frame rendering sequence
    /// while (running)
    /// {
    ///     renderer.Clear();              // Start with clean frame
    ///     renderer.SetCameraMatrix(cameraMatrix);
    ///     // ... draw game objects ...
    ///     renderer.FinalizeFrame();      // Complete frame rendering
    /// }
    /// </code>
    /// </example>
    void Clear();

    /// <summary>
    /// Sets the RGBA color for subsequent rendering operations.
    /// This color will be applied to all geometry drawn until changed.
    /// </summary>
    /// <param name="rgba">
    /// Color vector in RGBA format where each component is in the range [0, 1].
    /// X=Red, Y=Green, Z=Blue, W=Alpha (transparency).
    /// </param>
    /// <remarks>
    /// Color application depends on the current shader mode:
    /// - Normal mode: Color is applied directly to geometry
    /// - Textured mode: Color acts as a tint/modulation factor
    /// - Alpha channel controls transparency (requires blending enabled)
    /// 
    /// Educational Note: RGBA color format is standard in computer graphics.
    /// Values outside [0,1] range are typically clamped, with 0 being no intensity
    /// and 1 being full intensity. Alpha blending requires proper depth sorting.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Set pure red color
    /// renderer.SetColor(new Vector4D&lt;float&gt;(1f, 0f, 0f, 1f));
    /// 
    /// // Set semi-transparent blue
    /// renderer.SetColor(new Vector4D&lt;float&gt;(0f, 0f, 1f, 0.5f));
    /// 
    /// // Set white (no color modification)
    /// renderer.SetColor(new Vector4D&lt;float&gt;(1f, 1f, 1f, 1f));
    /// </code>
    /// </example>
    void SetColor(Vector4D<float> rgba);

    /// <summary>
    /// Uploads vertex position data (2D coordinates) to the GPU for rendering.
    /// This is the simplest vertex upload method for basic 2D rendering.
    /// </summary>
    /// <param name="vertices">
    /// Array of vertex positions as alternating X,Y coordinates.
    /// Array length must be even (pairs of coordinates).
    /// Coordinates are in world space units.
    /// </param>
    /// <remarks>
    /// This method assumes a simple vertex layout with only position data:
    /// - Each vertex requires exactly 2 float values (X, Y)
    /// - No texture coordinates, colors, or normals included
    /// - Suitable for solid color geometry and basic shapes
    /// - Automatically configures vertex attribute layout
    /// 
    /// Educational Note: Vertex data represents the geometry being rendered.
    /// Position coordinates define where geometry appears in world space,
    /// which is then transformed by camera matrices to screen coordinates.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when vertices array is null.</exception>
    /// <exception cref="ArgumentException">Thrown when vertices array has odd length.</exception>
    /// <example>
    /// <code>
    /// // Define a triangle
    /// float[] triangleVertices = {
    ///     0.0f,  0.5f,  // Top vertex
    ///    -0.5f, -0.5f,  // Bottom left
    ///     0.5f, -0.5f   // Bottom right
    /// };
    /// 
    /// renderer.SetColor(new Vector4D&lt;float&gt;(1, 0, 0, 1)); // Red
    /// renderer.UpdateVertices(triangleVertices);
    /// renderer.Draw();
    /// </code>
    /// </example>
    void UpdateVertices(float[] vertices);

    /// <summary>
    /// Issues a draw call using the currently uploaded vertex data and configured render state.
    /// This operation triggers the actual GPU rendering of the geometry.
    /// </summary>
    /// <remarks>
    /// The draw operation:
    /// - Executes vertex and fragment shaders on uploaded geometry
    /// - Applies current color, texture, and transformation settings
    /// - Renders using the specified primitive type (triangles by default)
    /// - Updates the framebuffer with the rendered results
    /// 
    /// Educational Note: Draw calls are expensive operations that should be
    /// minimized for optimal performance. Batching similar geometry into single
    /// draw calls is a key optimization technique in real-time rendering.
    /// 
    /// The graphics pipeline processes vertices through:
    /// 1. Vertex shader (position transformation)
    /// 2. Primitive assembly (forming triangles/lines)
    /// 3. Rasterization (generating pixels)
    /// 4. Fragment shader (pixel color calculation)
    /// 5. Output merger (final pixel writing)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete rendering sequence
    /// renderer.UpdateVertices(geometryData);
    /// renderer.SetColor(objectColor);
    /// renderer.Draw(); // Execute rendering
    /// 
    /// // Multiple objects with same vertex format
    /// foreach (var gameObject in visibleObjects)
    /// {
    ///     renderer.UpdateVertices(gameObject.Vertices);
    ///     renderer.SetColor(gameObject.Color);
    ///     renderer.Draw();
    /// }
    /// </code>
    /// </example>
    void Draw();

    /// <summary>
    /// Finalizes the current frame rendering with post-processing effects and buffer swapping.
    /// This completes the frame and presents it to the screen.
    /// </summary>
    /// <remarks>
    /// Frame finalization includes:
    /// - Post-processing effect application (if enabled)
    /// - Final render target resolution
    /// - Buffer swapping to display the completed frame
    /// - Performance metric collection and validation
    /// 
    /// Educational Note: Double buffering is standard in real-time graphics.
    /// While one buffer is displayed, the next frame is rendered to a back buffer.
    /// FinalizeFrame swaps these buffers to present the new frame smoothly.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard frame rendering loop
    /// while (gameRunning)
    /// {
    ///     var deltaTime = timer.GetDeltaTime();
    ///     
    ///     // Clear and setup frame
    ///     renderer.Clear();
    ///     renderer.SetCameraMatrix(camera.ViewProjectionMatrix);
    ///     
    ///     // Render all game objects
    ///     foreach (var obj in gameObjects)
    ///     {
    ///         obj.Render(renderer);
    ///     }
    ///     
    ///     // Complete and present frame
    ///     renderer.FinalizeFrame();
    /// }
    /// </code>
    /// </example>
    void FinalizeFrame();

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERMEDIATE RENDERING OPERATIONS (LAYER 2: CAMERA & SHADERS)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the camera transformation matrix for converting world coordinates to screen coordinates.
    /// This matrix combines view and projection transformations for 3D perspective rendering.
    /// </summary>
    /// <param name="cameraMatrix">
    /// Combined view-projection matrix that transforms vertices from world space to clip space.
    /// Must be a valid 4x4 transformation matrix with proper perspective or orthographic projection.
    /// </param>
    /// <remarks>
    /// Camera matrix transformation pipeline:
    /// 1. World Space: Object positions in game world coordinates
    /// 2. View Space: Positions relative to camera (view matrix)
    /// 3. Clip Space: Projected coordinates with perspective (projection matrix)
    /// 4. Screen Space: Final pixel coordinates (viewport transformation)
    /// 
    /// The matrix is typically calculated as: Projection × View × Model
    /// where Model transforms object space to world space.
    /// 
    /// Educational Note: 3D graphics rendering requires understanding coordinate
    /// space transformations. The camera matrix is fundamental to how 3D scenes
    /// are projected onto 2D screens with proper perspective and depth.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setup perspective camera
    /// var viewMatrix = Matrix4X4.CreateLookAt(cameraPos, targetPos, upVector);
    /// var projMatrix = Matrix4X4.CreatePerspectiveFieldOfView(
    ///     MathF.PI / 4f, aspectRatio, 0.1f, 1000f);
    /// var cameraMatrix = projMatrix * viewMatrix;
    /// 
    /// renderer.SetCameraMatrix(cameraMatrix);
    /// 
    /// // Orthographic 2D camera
    /// var orthoMatrix = Matrix4X4.CreateOrthographic(
    ///     screenWidth, screenHeight, -1f, 1f);
    /// renderer.SetCameraMatrix(orthoMatrix);
    /// </code>
    /// </example>
    void SetCameraMatrix(Matrix4X4<float> cameraMatrix);

    /// <summary>
    /// Sets the active camera for subsequent rendering operations with automatic matrix extraction.
    /// Provides higher-level camera management compared to manual matrix setting.
    /// </summary>
    /// <param name="camera">
    /// Camera instance implementing ICamera interface. Provides view and projection matrices
    /// along with camera state and configuration options.
    /// </param>
    /// <remarks>
    /// Camera object benefits:
    /// - Encapsulates both view and projection matrix calculations
    /// - Provides convenient camera movement and rotation methods
    /// - Supports different camera types (perspective, orthographic, first-person)
    /// - Handles aspect ratio updates and field of view changes
    /// - Enables camera interpolation and smooth transitions
    /// 
    /// Educational Note: Camera abstractions simplify 3D scene management by
    /// providing intuitive methods for positioning, orienting, and configuring
    /// the viewpoint. This is essential for creating engaging 3D experiences.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when camera is null.</exception>
    /// <example>
    /// <code>
    /// // Create and configure camera
    /// var camera = new PerspectiveCamera(
    ///     position: new Vector3D&lt;float&gt;(0, 0, 5),
    ///     target: Vector3D&lt;float&gt;.Zero,
    ///     fieldOfView: 60f);
    /// 
    /// renderer.SetActiveCamera(camera);
    /// 
    /// // Camera automatically handles matrix calculations
    /// camera.MoveForward(deltaTime * speed);
    /// camera.Rotate(mouseInput.X, mouseInput.Y);
    /// </code>
    /// </example>
    void SetActiveCamera(ICamera camera);

    /// <summary>
    /// Configures the shader mode for different rendering techniques and visual effects.
    /// Switches between different shader programs optimized for specific rendering needs.
    /// </summary>
    /// <param name="mode">
    /// Shader mode enumeration specifying the rendering technique:
    /// Normal (solid colors), Textured (texture mapping), or specialized effect modes.
    /// </param>
    /// <remarks>
    /// Shader modes enable different rendering capabilities:
    /// - Normal: Solid color rendering with vertex colors and lighting
    /// - Textured: Texture mapping with UV coordinates and sampling
    /// - Specialized: Custom shaders for effects (particles, post-processing)
    /// 
    /// Each mode uses optimized shader programs for specific use cases:
    /// - Different vertex attribute layouts and processing
    /// - Unique fragment shader calculations and outputs
    /// - Optimized uniform variable configurations
    /// 
    /// Educational Note: Modern graphics programming uses programmable shaders
    /// instead of fixed-function pipelines. Shader programs define how vertices
    /// are transformed and how pixels are colored, enabling unlimited visual effects.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Render solid color geometry
    /// renderer.SetShaderMode(ShaderMode.Normal);
    /// renderer.SetColor(solidColor);
    /// renderer.Draw();
    /// 
    /// // Render textured sprites
    /// renderer.SetShaderMode(ShaderMode.Textured);
    /// renderer.SetTexture(spriteTexture);
    /// renderer.Draw();
    /// </code>
    /// </example>
    void SetShaderMode(ShaderMode mode);

    /// <summary>
    /// Configures the primitive type for interpreting vertex data during rendering.
    /// Determines how vertices are connected to form geometric shapes.
    /// </summary>
    /// <param name="primitiveType">
    /// OpenGL primitive type specifying vertex interpretation:
    /// Triangles (default), Lines, Points, TriangleStrip, LineStrip, etc.
    /// </param>
    /// <remarks>
    /// Primitive types define vertex connectivity:
    /// - Triangles: Every 3 vertices form an independent triangle
    /// - Lines: Every 2 vertices form an independent line segment  
    /// - Points: Each vertex renders as an individual point
    /// - Strips: Vertices shared between adjacent primitives for efficiency
    /// 
    /// Triangle primitives are most common for solid geometry:
    /// - Efficient GPU processing and rasterization
    /// - Natural representation for complex 3D shapes
    /// - Compatible with all lighting and shading techniques
    /// 
    /// Educational Note: Understanding primitive types is fundamental to 3D graphics.
    /// Different primitives serve different purposes: triangles for solid objects,
    /// lines for wireframes and debugging, points for particle effects.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Render solid triangular faces
    /// renderer.SetPrimitiveType(PrimitiveType.Triangles);
    /// renderer.UpdateVertices(meshVertices);
    /// renderer.Draw();
    /// 
    /// // Render wireframe outline
    /// renderer.SetPrimitiveType(PrimitiveType.Lines);
    /// renderer.UpdateVertices(wireframeVertices);
    /// renderer.Draw();
    /// 
    /// // Render particle system
    /// renderer.SetPrimitiveType(PrimitiveType.Points);
    /// renderer.UpdateVertices(particlePositions);
    /// renderer.Draw();
    /// </code>
    /// </example>
    void SetPrimitiveType(PrimitiveType primitiveType);

    /// <summary>
    /// Binds a texture for subsequent textured rendering operations.
    /// The texture will be sampled by fragment shaders during textured rendering.
    /// </summary>
    /// <param name="texture">
    /// Texture asset containing image data loaded from files or generated procedurally.
    /// Must be a valid, loaded texture with proper format and dimensions.
    /// </param>
    /// <remarks>
    /// Texture binding process:
    /// - Activates the specified texture unit (typically unit 0)
    /// - Binds texture to current OpenGL context
    /// - Configures texture sampling parameters
    /// - Makes texture available to fragment shaders
    /// 
    /// Texture coordinates (UV mapping) determine how textures are applied:
    /// - U: Horizontal texture coordinate (0.0 to 1.0)
    /// - V: Vertical texture coordinate (0.0 to 1.0)
    /// - Coordinates outside [0,1] can repeat or clamp based on settings
    /// 
    /// Educational Note: Texture mapping is fundamental to realistic 3D rendering.
    /// Textures provide surface detail, color variation, and material properties
    /// that would be impossible to achieve with simple vertex colors alone.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when texture is null.</exception>
    /// <example>
    /// <code>
    /// // Load and apply texture
    /// var brickTexture = assetService.LoadTexture("brick_wall.png");
    /// renderer.SetShaderMode(ShaderMode.Textured);
    /// renderer.SetTexture(brickTexture);
    /// renderer.UpdateVertices(wallVerticesWithUV);
    /// renderer.Draw();
    /// 
    /// // Multiple textures for different objects
    /// foreach (var renderable in texturedObjects)
    /// {
    ///     renderer.SetTexture(renderable.Texture);
    ///     renderer.UpdateVertices(renderable.Vertices);
    ///     renderer.Draw();
    /// }
    /// </code>
    /// </example>
    void SetTexture(Rac.Assets.Types.Texture texture);

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED RENDERING OPERATIONS (LAYER 3: PERFORMANCE & FLEXIBILITY)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Uploads strongly-typed vertex data with automatic layout detection and optimal performance.
    /// Provides type safety and eliminates manual layout specification for custom vertex structures.
    /// </summary>
    /// <typeparam name="T">
    /// Vertex type implementing a vertex structure with position, UV, color, or other attributes.
    /// Must be unmanaged (value type with no managed references) for safe GPU upload.
    /// </typeparam>
    /// <param name="vertices">
    /// Array of vertex structures containing geometry data.
    /// Type T determines the vertex layout and attribute configuration automatically.
    /// </param>
    /// <remarks>
    /// Generic vertex upload advantages:
    /// - Compile-time type safety prevents attribute mismatches
    /// - Automatic layout detection from vertex structure reflection
    /// - Optimal memory layout and GPU transfer performance
    /// - Support for custom vertex formats and specialized attributes
    /// - Eliminates error-prone manual layout specification
    /// 
    /// Supported vertex types (examples):
    /// - BasicVertex: Position only (X, Y)
    /// - TexturedVertex: Position + UV coordinates
    /// - FullVertex: Position + UV + Color + Normal
    /// - Custom vertex types with appropriate attributes
    /// 
    /// Educational Note: Strongly-typed vertex systems enable complex geometry
    /// with multiple attributes while maintaining code safety and performance.
    /// This approach scales from simple 2D sprites to complex 3D meshes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when vertices array is null.</exception>
    /// <exception cref="ArgumentException">Thrown when vertex type T has invalid layout.</exception>
    /// <example>
    /// <code>
    /// // Define custom vertex structure
    /// public struct ColoredVertex
    /// {
    ///     public Vector2D&lt;float&gt; Position;
    ///     public Vector4D&lt;float&gt; Color;
    /// }
    /// 
    /// // Create and upload vertices
    /// var vertices = new ColoredVertex[]
    /// {
    ///     new() { Position = new(0, 1), Color = new(1, 0, 0, 1) },
    ///     new() { Position = new(-1, -1), Color = new(0, 1, 0, 1) },
    ///     new() { Position = new(1, -1), Color = new(0, 0, 1, 1) }
    /// };
    /// 
    /// renderer.UpdateVertices(vertices);
    /// renderer.Draw();
    /// </code>
    /// </example>
    void UpdateVertices<T>(T[] vertices) where T : unmanaged;

    /// <summary>
    /// Uploads raw vertex data with explicit layout specification for maximum control and flexibility.
    /// Enables custom vertex formats and advanced rendering techniques requiring specific layouts.
    /// </summary>
    /// <param name="vertices">
    /// Raw vertex data as float array. Layout and interpretation determined by layout parameter.
    /// Array must contain valid floating-point vertex attribute data.
    /// </param>
    /// <param name="layout">
    /// Vertex layout specification defining attribute sizes, types, and stride information.
    /// Determines how the raw float data is interpreted as vertex attributes.
    /// </param>
    /// <remarks>
    /// Explicit layout specification enables:
    /// - Custom vertex attribute configurations not supported by predefined types
    /// - Interleaved vertex data optimization for cache performance
    /// - Compatibility with external mesh loading libraries
    /// - Advanced rendering techniques requiring specific data layouts
    /// - Fine-grained control over GPU memory usage and access patterns
    /// 
    /// Layout considerations:
    /// - Attribute stride (bytes between vertices)
    /// - Attribute offset (position within vertex)
    /// - Data type and component count per attribute
    /// - Normalization requirements for integer attributes
    /// 
    /// Educational Note: Manual layout specification provides ultimate flexibility
    /// but requires deep understanding of GPU vertex processing. Use this method
    /// when predefined vertex types don't meet specific requirements.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when vertices or layout is null.</exception>
    /// <exception cref="ArgumentException">Thrown when vertex data doesn't match layout specification.</exception>
    /// <example>
    /// <code>
    /// // Define custom interleaved layout: Position(2) + UV(2) + Normal(3)
    /// var layout = new VertexLayout()
    ///     .AddAttribute(AttributeType.Position, 2, 0)   // X,Y at offset 0
    ///     .AddAttribute(AttributeType.TexCoord, 2, 8)   // U,V at offset 8
    ///     .AddAttribute(AttributeType.Normal, 3, 16)    // Nx,Ny,Nz at offset 16
    ///     .SetStride(28); // 7 floats * 4 bytes = 28 bytes per vertex
    /// 
    /// // Raw vertex data: [x,y,u,v,nx,ny,nz, x,y,u,v,nx,ny,nz, ...]
    /// float[] vertexData = { /* interleaved data */ };
    /// 
    /// renderer.UpdateVertices(vertexData, layout);
    /// renderer.Draw();
    /// </code>
    /// </example>
    void UpdateVertices(float[] vertices, VertexLayout layout);
}
