---
title: "4-Phase Rendering Pipeline Architecture"
description: "Graphics rendering pipeline architecture with distinct phases for optimal performance and tool development"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["rendering", "graphics", "pipeline", "opengl"]
---

# 4-Phase Rendering Pipeline Architecture

## Overview

RACEngine features a sophisticated 4-phase rendering architecture that separates concerns into distinct phases, enabling better performance, tool development, and maintainability. This design allows for optimal GPU utilization while providing clear extension points for custom rendering features.

## Prerequisites

- Basic understanding of 3D graphics programming
- Familiarity with OpenGL or similar graphics APIs
- Knowledge of shader programming concepts
- [System Overview](system-overview.md) for architectural context

## Pipeline Philosophy

### Why 4 Phases?

Traditional rendering pipelines often mix configuration, data preparation, and rendering operations, leading to:
- **Performance bottlenecks**: State changes interleaved with draw calls
- **Maintenance difficulties**: Unclear separation of concerns
- **Extension complexity**: Hard to add new features without affecting existing code
- **Tool integration challenges**: Difficult to intercept and modify rendering behavior

The 4-phase approach addresses these issues by clearly separating:
1. **Configuration** - What to render and how
2. **Preprocessing** - Data preparation and optimization
3. **Processing** - Actual GPU rendering operations
4. **Post-processing** - Effects and final presentation

## Phase 1: Configuration

### Purpose
Establishes rendering parameters, loads shaders, and configures render state without touching GPU resources.

### Core Responsibilities

#### Shader Management Strategy
The configuration phase handles shader program lifecycle and optimization:

**Shader Architecture:**
- **Program Caching**: Compiled shader programs cached for reuse across frames
- **Uniform Location Caching**: Expensive uniform lookups cached during configuration
- **Modular Shader Design**: Vertex shaders shared across multiple fragment shader variants
- **Hot-Reload Support**: Development-time shader reloading for rapid iteration

**Configuration Benefits:**
- **Performance Optimization**: Expensive operations front-loaded before rendering
- **State Consolidation**: All shader state established in single configuration pass
- **Error Handling**: Shader compilation errors caught early in frame
- **Resource Management**: Proper cleanup and disposal of shader resources

*Implementation: `src/Rac.Rendering/Shaders/` shader management classes*

#### Render State Architecture  
Global rendering parameters established before GPU operations:

**State Management Design:**
- **Batched State Changes**: Multiple state changes batched to reduce driver overhead
- **State Validation**: Configuration validates state combinations for correctness
- **Default Management**: Sensible defaults provided for common rendering scenarios
- **Platform Abstraction**: State configuration abstracted from underlying graphics API

**State Categories:**
- **Blend Modes**: Alpha blending, additive, multiplicative blending configurations
- **Depth Testing**: Z-buffer testing modes for proper depth sorting
- **Culling Modes**: Back-face culling optimization for 3D rendering
- **Wireframe Modes**: Debug visualization options for development

*Implementation: `src/Rac.Rendering/State/` render state management*
    
    /// <summary>
    /// Applies all render state changes in optimal order
    /// </summary>
    public void ApplyRenderState()
    {
        // Apply changes in order that minimizes GPU state switching overhead
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc((DepthFunction)DepthTest);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc((BlendingFactor)BlendMode.Source, (BlendingFactor)BlendMode.Destination);
        
        GL.Enable(EnableCap.CullFace);
        GL.CullFace((CullFaceMode)CullMode);
        
        if (WireframeMode)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        else
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }
#### Camera Architecture
Manages viewing transformations and viewport configuration:

**Camera System Design:**
- **Matrix Management**: Projection and view matrices calculated and cached efficiently
- **Aspect Ratio Handling**: Automatic adjustment for different screen resolutions
- **Perspective Configuration**: Field of view, near/far plane management for 3D rendering
- **Viewport Management**: Screen space coordinate mapping and clipping region setup

**Mathematical Foundations:**
- **Projection Matrices**: Transform 3D world coordinates to 2D screen space
- **View Matrices**: Transform world coordinates to camera-relative coordinates
- **Coordinate Systems**: Right-handed 3D coordinate system with proper transformations
- **Clipping Planes**: Near and far plane configuration for depth precision

*Implementation: `src/Rac.Rendering/Camera/` camera and viewport management*

## Phase 2: Preprocessing

### Purpose
Prepares render data, performs culling operations, and optimizes drawing order for maximum GPU efficiency.

### Core Responsibilities

#### Frustum Culling Architecture
Eliminates objects outside the viewing volume before expensive GPU operations:

**Culling Strategy:**
- **Plane Extraction**: View frustum planes computed from projection-view matrix
- **Bounding Volume Testing**: Objects tested against frustum planes using bounding boxes
- **Early Elimination**: Off-screen objects removed before vertex processing
- **Hierarchical Culling**: Spatial data structures enable efficient bulk culling

**Performance Benefits:**
- **Reduced Overdraw**: Eliminates rendering of non-visible geometry
- **Bandwidth Savings**: Less vertex data sent to GPU
- **Fragment Shader Savings**: Fewer fragments processed for off-screen objects
- **Memory Efficiency**: Reduced memory bandwidth usage

**Mathematical Approach:**
- **Gribb-Hartmann Method**: Industry-standard frustum plane extraction
- **Axis-Aligned Bounding Boxes**: Fast intersection testing with simple geometry
- **Conservative Testing**: Ensures no visible objects are incorrectly culled
- **Sphere Culling**: Alternative bounding volume for organic shapes

*Implementation: `src/Rac.Rendering/Culling/` frustum culling systems*
                    allVerticesOutside = false;
                    break;
                }
            }
            
            // If all vertices are outside any plane, the box is not visible
            if (allVerticesOutside)
                return false;
        }
        
        return true;
    }
}
```

#### Render Queue Sorting
```csharp
/// <summary>
/// Render queue that sorts draw calls for optimal GPU performance
/// Educational note: Proper sorting reduces state changes and overdraw
/// </summary>
public class RenderQueue
{
    private readonly List<DrawCall> _opaqueDrawCalls = new();
    private readonly List<DrawCall> _transparentDrawCalls = new();
    
    /// <summary>
    /// Adds a draw call to appropriate queue based on material properties
    /// </summary>
    public void Submit(DrawCall drawCall)
    {
        if (drawCall.Material.IsTransparent)
            _transparentDrawCalls.Add(drawCall);
        else
            _opaqueDrawCalls.Add(drawCall);
    }
    
    /// <summary>
    /// Sorts render queues for optimal rendering performance
    /// Opaque: Front-to-back (early Z rejection)
    /// Transparent: Back-to-front (proper alpha blending)
    /// </summary>
    public void Sort(Vector3 cameraPosition)
    {
        // Sort opaque objects front-to-back for Z-buffer efficiency
        _opaqueDrawCalls.Sort((a, b) =>
        {
            float distA = Vector3.DistanceSquared(cameraPosition, a.WorldPosition);
            float distB = Vector3.DistanceSquared(cameraPosition, b.WorldPosition);
            return distA.CompareTo(distB);
        });
        
        // Sort transparent objects back-to-front for correct alpha blending
        _transparentDrawCalls.Sort((a, b) =>
        {
            float distA = Vector3.DistanceSquared(cameraPosition, a.WorldPosition);
            float distB = Vector3.DistanceSquared(cameraPosition, b.WorldPosition);
            return distB.CompareTo(distA);
        });
    }
    
    /// <summary>
    /// Further optimization: Sort by shader and texture to minimize state changes
    /// </summary>
    public void OptimizeStateChanges()
    {
        // Group by shader program first (most expensive state change)
        var groupedByShader = _opaqueDrawCalls.GroupBy(dc => dc.Material.ShaderProgram);
        
        _opaqueDrawCalls.Clear();
        
        foreach (var shaderGroup in groupedByShader)
        {
            // Within each shader group, sort by texture (less expensive state change)
            var sortedByTexture = shaderGroup.OrderBy(dc => dc.Material.Texture?.Id ?? 0);
            _opaqueDrawCalls.AddRange(sortedByTexture);
        }
    }
}
```

#### Vertex Buffer Management
```csharp
/// <summary>
/// Dynamic vertex buffer management for efficient batch rendering
/// Educational note: Batching reduces draw calls and improves performance
/// </summary>
public class VertexBufferManager
{
    private readonly Dictionary<VertexFormat, BatchBuffer> _batchBuffers = new();
    
    /// <summary>
    /// Batches vertices with the same format for efficient rendering
    /// </summary>
    public void BatchVertices<T>(VertexFormat format, IEnumerable<T> vertices) where T : struct
    {
        if (!_batchBuffers.TryGetValue(format, out var buffer))
        {
            buffer = new BatchBuffer(format);
            _batchBuffers[format] = buffer;
        }
        
        buffer.AddVertices(vertices);
    }
    
    /// <summary>
    /// Uploads all batched data to GPU buffers
    /// </summary>
    public void UploadToGPU()
    {
        foreach (var buffer in _batchBuffers.Values)
        {
            buffer.UploadToGPU();
        }
    }
}

/// <summary>
/// Batch buffer for specific vertex format
/// </summary>
public class BatchBuffer
{
    private readonly List<byte> _vertexData = new();
    private readonly VertexFormat _format;
    private int _vertexArrayObject;
    private int _vertexBufferObject;
    
    public void AddVertices<T>(IEnumerable<T> vertices) where T : struct
    {
        foreach (var vertex in vertices)
        {
            var bytes = StructToBytes(vertex);
            _vertexData.AddRange(bytes);
        }
    }
    
    public void UploadToGPU()
    {
        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        
        // Upload vertex data to GPU
        GL.BufferData(BufferTarget.ArrayBuffer, _vertexData.Count, 
                     _vertexData.ToArray(), BufferUsageHint.DynamicDraw);
        
        // Configure vertex attributes based on format
        _format.ConfigureVertexAttributes();
        
        _vertexData.Clear(); // Clear for next frame
    }
}
```

## Phase 3: Processing

### Purpose
Executes actual GPU rendering operations with minimal state changes and optimal draw call ordering.

### Core Responsibilities

#### Render Pass Execution
```csharp
/// <summary>
/// Main rendering phase that executes draw calls
/// Educational note: Minimizing state changes is crucial for GPU performance
/// </summary>
public class RenderPassProcessor
{
    private ShaderProgram _currentShader;
    private int _currentTexture;
    private Matrix4 _currentProjection;
    private Matrix4 _currentView;
    
    /// <summary>
    /// Processes a render queue with state change minimization
    /// </summary>
    public void ProcessRenderQueue(RenderQueue queue, CameraConfiguration camera)
    {
        // Set camera matrices once for the entire pass
        UpdateCameraMatrices(camera.ProjectionMatrix, camera.ViewMatrix);
        
        // Process opaque objects first
        ProcessDrawCalls(queue.OpaqueDrawCalls);
        
        // Enable blending for transparent objects
        GL.Enable(EnableCap.Blend);
        ProcessDrawCalls(queue.TransparentDrawCalls);
        GL.Disable(EnableCap.Blend);
    }
    
    private void ProcessDrawCalls(IEnumerable<DrawCall> drawCalls)
    {
        foreach (var drawCall in drawCalls)
        {
            // Only change shader if different from current
            if (_currentShader != drawCall.Material.ShaderProgram)
            {
                _currentShader = drawCall.Material.ShaderProgram;
                GL.UseProgram(_currentShader.ProgramId);
                
                // Reapply camera matrices to new shader
                _currentShader.SetUniformMatrix4("u_projection", _currentProjection);
                _currentShader.SetUniformMatrix4("u_view", _currentView);
            }
            
            // Only bind texture if different from current
            if (_currentTexture != drawCall.Material.Texture?.Id)
            {
                _currentTexture = drawCall.Material.Texture?.Id ?? 0;
                GL.BindTexture(TextureTarget.Texture2D, _currentTexture);
            }
            
            // Set per-object uniforms
            _currentShader.SetUniformMatrix4("u_model", drawCall.ModelMatrix);
            _currentShader.SetUniformVector4("u_color", drawCall.Material.Color);
            
            // Execute the draw call
            ExecuteDrawCall(drawCall);
        }
    }
    
    private void ExecuteDrawCall(DrawCall drawCall)
    {
        GL.BindVertexArray(drawCall.VertexArrayObject);
        
        if (drawCall.IndexCount > 0)
        {
            // Indexed drawing for complex meshes
            GL.DrawElements(PrimitiveType.Triangles, drawCall.IndexCount, 
                           DrawElementsType.UnsignedInt, 0);
        }
        else
        {
            // Direct vertex drawing for simple geometry
            GL.DrawArrays(PrimitiveType.Triangles, 0, drawCall.VertexCount);
        }
    }
}
```

#### Instanced Rendering
```csharp
/// <summary>
/// Instanced rendering for drawing many similar objects efficiently
/// Educational note: Instancing reduces CPU-GPU communication overhead
/// Academic reference: GPU Gems 2, Chapter 3 (Hardware-Accelerated Rendering)
/// </summary>
public class InstancedRenderer
{
    private readonly Dictionary<Mesh, InstancedDrawCall> _instancedCalls = new();
    
    /// <summary>
    /// Accumulates instances for batch rendering
    /// </summary>
    public void AddInstance(Mesh mesh, Matrix4 modelMatrix, Vector4 color)
    {
        if (!_instancedCalls.TryGetValue(mesh, out var drawCall))
        {
            drawCall = new InstancedDrawCall(mesh);
            _instancedCalls[mesh] = drawCall;
        }
        
        drawCall.AddInstance(modelMatrix, color);
    }
    
    /// <summary>
    /// Renders all accumulated instances
    /// </summary>
    public void RenderInstances(ShaderProgram shader)
    {
        foreach (var drawCall in _instancedCalls.Values)
        {
            if (drawCall.InstanceCount == 0) continue;
            
            // Upload instance data to GPU
            drawCall.UploadInstanceData();
            
            // Bind mesh geometry
            GL.BindVertexArray(drawCall.Mesh.VAO);
            
            // Execute instanced draw call
            GL.DrawElementsInstanced(
                PrimitiveType.Triangles,
                drawCall.Mesh.IndexCount,
                DrawElementsType.UnsignedInt,
                IntPtr.Zero,
                drawCall.InstanceCount
            );
            
            // Clear instances for next frame
            drawCall.ClearInstances();
        }
    }
}

/// <summary>
/// Instanced draw call data structure
/// </summary>
public class InstancedDrawCall
{
    public Mesh Mesh { get; }
    public List<Matrix4> ModelMatrices { get; } = new();
    public List<Vector4> Colors { get; } = new();
    public int InstanceCount => ModelMatrices.Count;
    
    private int _instanceVBO;
    
    public InstancedDrawCall(Mesh mesh)
    {
        Mesh = mesh;
        
        // Create instance data buffer
        GL.GenBuffers(1, out _instanceVBO);
    }
    
    public void AddInstance(Matrix4 modelMatrix, Vector4 color)
    {
        ModelMatrices.Add(modelMatrix);
        Colors.Add(color);
    }
    
    public void UploadInstanceData()
    {
        // Create interleaved instance data (matrix + color)
        var instanceData = new float[InstanceCount * 20]; // 16 floats for matrix + 4 for color
        
        for (int i = 0; i < InstanceCount; i++)
        {
            var matrix = ModelMatrices[i];
            var color = Colors[i];
            int offset = i * 20;
            
            // Copy matrix (16 floats)
            for (int j = 0; j < 16; j++)
                instanceData[offset + j] = matrix[j / 4, j % 4];
            
            // Copy color (4 floats)
            instanceData[offset + 16] = color.X;
            instanceData[offset + 17] = color.Y;
            instanceData[offset + 18] = color.Z;
            instanceData[offset + 19] = color.W;
        }
        
        // Upload to GPU
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, instanceData.Length * sizeof(float),
                     instanceData, BufferUsageHint.DynamicDraw);
        
        // Configure instance attributes
        ConfigureInstanceAttributes();
    }
    
    private void ConfigureInstanceAttributes()
    {
        GL.BindVertexArray(Mesh.VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
        
        // Matrix attribute (4 vec4s)
        for (int i = 0; i < 4; i++)
        {
            GL.EnableVertexAttribArray(3 + i);
            GL.VertexAttribPointer(3 + i, 4, VertexAttribPointerType.Float, 
                                  false, 20 * sizeof(float), i * 4 * sizeof(float));
            GL.VertexAttribDivisor(3 + i, 1); // Advance per instance
        }
        
        // Color attribute
        GL.EnableVertexAttribArray(7);
        GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, 
                              false, 20 * sizeof(float), 16 * sizeof(float));
        GL.VertexAttribDivisor(7, 1); // Advance per instance
    }
}
```

## Phase 4: Post-Processing

### Purpose
Applies visual effects, tone mapping, and final presentation operations to the rendered frame.

### Core Responsibilities

#### HDR and Tone Mapping
```csharp
/// <summary>
/// High Dynamic Range rendering and tone mapping
/// Educational note: HDR allows more realistic lighting calculations
/// Academic reference: Real-Time HDR Rendering (Kalogirou)
/// </summary>
public class HDRPostProcessor
{
    private readonly Framebuffer _hdrFramebuffer;
    private readonly Framebuffer _bloomFramebuffer;
    private readonly ShaderProgram _toneMapShader;
    private readonly ShaderProgram _bloomShader;
    
    /// <summary>
    /// Processes HDR frame with bloom and tone mapping
    /// </summary>
    public void ProcessFrame(Texture hdrTexture)
    {
        // Step 1: Extract bright areas for bloom
        ExtractBrightAreas(hdrTexture);
        
        // Step 2: Apply Gaussian blur to bright areas
        ApplyBloomBlur();
        
        // Step 3: Tone map HDR to LDR with bloom
        ToneMapWithBloom(hdrTexture);
    }
    
    private void ExtractBrightAreas(Texture hdrTexture)
    {
        _bloomFramebuffer.Bind();
        _bloomShader.Use();
        
        // Set brightness threshold for bloom extraction
        _bloomShader.SetUniform("u_threshold", 1.0f);
        _bloomShader.SetUniform("u_hdrTexture", 0);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, hdrTexture.Id);
        
        // Render full-screen quad
        RenderFullScreenQuad();
    }
    
    private void ToneMapWithBloom(Texture hdrTexture)
    {
        // Bind default framebuffer for final output
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        _toneMapShader.Use();
        
        // Bind HDR scene texture
        _toneMapShader.SetUniform("u_hdrTexture", 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, hdrTexture.Id);
        
        // Bind bloom texture
        _toneMapShader.SetUniform("u_bloomTexture", 1);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _bloomFramebuffer.ColorTexture.Id);
        
        // Tone mapping parameters
        _toneMapShader.SetUniform("u_exposure", 1.0f);
        _toneMapShader.SetUniform("u_bloomStrength", 0.1f);
        
        // Render final frame
        RenderFullScreenQuad();
    }
}
```

#### Screen-Space Ambient Occlusion (SSAO)
```csharp
/// <summary>
/// Screen-Space Ambient Occlusion implementation
/// Educational note: SSAO approximates global illumination in real-time
/// Academic reference: "Ambient Occlusion Mapping" (Bunnell, 2005)
/// </summary>
public class SSAOPostProcessor
{
    private readonly ShaderProgram _ssaoShader;
    private readonly ShaderProgram _blurShader;
    private readonly Texture _noiseTexture;
    private readonly Vector3[] _sampleKernel;
    
    public SSAOPostProcessor()
    {
        _sampleKernel = GenerateSampleKernel();
        _noiseTexture = GenerateNoiseTexture();
    }
    
    /// <summary>
    /// Applies SSAO to the G-buffer
    /// </summary>
    public void ApplySSAO(Texture positionTexture, Texture normalTexture, Matrix4 projection)
    {
        _ssaoShader.Use();
        
        // Bind G-buffer textures
        _ssaoShader.SetUniform("u_positionTexture", 0);
        _ssaoShader.SetUniform("u_normalTexture", 1);
        _ssaoShader.SetUniform("u_noiseTexture", 2);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, positionTexture.Id);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, normalTexture.Id);
        
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, _noiseTexture.Id);
        
        // Upload sample kernel and matrices
        _ssaoShader.SetUniformArray("u_samples", _sampleKernel);
        _ssaoShader.SetUniformMatrix4("u_projection", projection);
        
        // SSAO parameters
        _ssaoShader.SetUniform("u_radius", 0.5f);
        _ssaoShader.SetUniform("u_bias", 0.025f);
        
        RenderFullScreenQuad();
    }
    
    private Vector3[] GenerateSampleKernel()
    {
        var samples = new Vector3[64];
        var random = new Random();
        
        for (int i = 0; i < samples.Length; i++)
        {
            // Generate random sample in hemisphere
            var sample = new Vector3(
                (float)(random.NextDouble() * 2.0 - 1.0),
                (float)(random.NextDouble() * 2.0 - 1.0),
                (float)random.NextDouble()
            );
            
            sample.Normalize();
            
            // Scale samples to distribute more near origin
            float scale = (float)i / samples.Length;
            scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
            sample *= scale;
            
            samples[i] = sample;
        }
        
        return samples;
    }
}
```

## Performance Optimization

### GPU Profiling Integration
```csharp
/// <summary>
/// GPU timing queries for performance profiling
/// Educational note: Measuring GPU performance requires special timing queries
/// </summary>
public class GPUProfiler
{
    private readonly Dictionary<string, GPUTimer> _timers = new();
    
    public void BeginSample(string name)
    {
        if (!_timers.TryGetValue(name, out var timer))
        {
            timer = new GPUTimer(name);
            _timers[name] = timer;
        }
        
        timer.Begin();
    }
    
    public void EndSample(string name)
    {
        if (_timers.TryGetValue(name, out var timer))
        {
            timer.End();
        }
    }
    
    public void PrintResults()
    {
        Console.WriteLine("GPU Performance Profile:");
        foreach (var (name, timer) in _timers)
        {
            Console.WriteLine($"  {name}: {timer.LastFrameTime:F3}ms");
        }
    }
}

public class GPUTimer
{
    private readonly string _name;
    private readonly int[] _queryObjects = new int[2];
    private int _currentQuery = 0;
    
    public float LastFrameTime { get; private set; }
    
    public GPUTimer(string name)
    {
        _name = name;
        GL.GenQueries(2, _queryObjects);
    }
    
    public void Begin()
    {
        GL.BeginQuery(QueryTarget.TimeElapsed, _queryObjects[_currentQuery]);
    }
    
    public void End()
    {
        GL.EndQuery(QueryTarget.TimeElapsed);
        
        // Read previous frame's result
        int prevQuery = 1 - _currentQuery;
        if (GL.GetQueryObject(_queryObjects[prevQuery], GetQueryObjectParam.ResultAvailable) != 0)
        {
            long timeNs = GL.GetQueryObject(_queryObjects[prevQuery], GetQueryObjectParam.Result);
            LastFrameTime = timeNs / 1_000_000.0f; // Convert to milliseconds
        }
        
        _currentQuery = 1 - _currentQuery;
    }
}
```

### Render Statistics
```csharp
/// <summary>
/// Comprehensive rendering statistics for optimization
/// </summary>
public class RenderStatistics
{
    public int DrawCalls { get; set; }
    public int TrianglesRendered { get; set; }
    public int StateChanges { get; set; }
    public int TextureBinds { get; set; }
    public int ShaderSwitches { get; set; }
    public float FrameTime { get; set; }
    public float CPUTime { get; set; }
    public float GPUTime { get; set; }
    
    public void Reset()
    {
        DrawCalls = 0;
        TrianglesRendered = 0;
        StateChanges = 0;
        TextureBinds = 0;
        ShaderSwitches = 0;
    }
    
    public void DisplayStats()
    {
        Console.WriteLine($"Render Stats:");
        Console.WriteLine($"  Frame Time: {FrameTime:F2}ms ({1000.0f / FrameTime:F1} FPS)");
        Console.WriteLine($"  CPU Time: {CPUTime:F2}ms");
        Console.WriteLine($"  GPU Time: {GPUTime:F2}ms");
        Console.WriteLine($"  Draw Calls: {DrawCalls}");
        Console.WriteLine($"  Triangles: {TrianglesRendered:N0}");
        Console.WriteLine($"  State Changes: {StateChanges}");
        Console.WriteLine($"  Texture Binds: {TextureBinds}");
        Console.WriteLine($"  Shader Switches: {ShaderSwitches}");
    }
}
```

## Integration with ECS

### Rendering Components
```csharp
/// <summary>
/// Rendering-related components for ECS integration
/// </summary>
public readonly record struct TransformComponent(
    Vector3 Position,
    Quaternion Rotation,
    Vector3 Scale
) : IComponent;

public readonly record struct MeshRendererComponent(
    Mesh Mesh,
    Material Material,
    bool CastShadows = true,
    bool ReceiveShadows = true
) : IComponent;

public readonly record struct CameraComponent(
    float FieldOfView,
    float NearPlane,
    float FarPlane,
    int RenderOrder = 0
) : IComponent;
```

### Rendering System Integration
```csharp
/// <summary>
/// ECS system that integrates with the 4-phase rendering pipeline
/// </summary>
public class RenderingSystem : ISystem
{
    private readonly RenderPipeline _pipeline;
    
    public void Update(World world, float deltaTime)
    {
        // Phase 1: Configuration
        _pipeline.Configure();
        
        // Phase 2: Preprocessing - collect renderable entities
        foreach (var (entity, transform, renderer) in world.Query<TransformComponent, MeshRendererComponent>())
        {
            var modelMatrix = CreateModelMatrix(transform);
            var drawCall = new DrawCall(renderer.Mesh, renderer.Material, modelMatrix);
            _pipeline.Submit(drawCall);
        }
        
        // Phase 3: Processing
        _pipeline.Render();
        
        // Phase 4: Post-processing
        _pipeline.PostProcess();
    }
    
    private Matrix4 CreateModelMatrix(TransformComponent transform)
    {
        return Matrix4.CreateScale(transform.Scale) *
               Matrix4.CreateFromQuaternion(transform.Rotation) *
               Matrix4.CreateTranslation(transform.Position);
    }
}
```

## See Also

- [System Overview](system-overview.md) - High-level architecture context

- [Rac.Rendering Project Documentation](../projects/Rac.Rendering.md) - Implementation details
- [Shader Programming Guide](../educational-material/shader-programming-guide.md) - Shader development

## Changelog

- 2025-06-26: Comprehensive 4-phase rendering pipeline documentation with examples and performance optimization techniques