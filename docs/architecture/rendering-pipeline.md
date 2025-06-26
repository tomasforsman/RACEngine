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

#### Shader Management
```csharp
/// <summary>
/// Shader configuration and loading system
/// Educational note: Separating shader loading from usage improves performance
/// </summary>
public class ShaderConfigurationPhase
{
    private readonly Dictionary<ShaderMode, ShaderProgram> _shaderPrograms = new();
    
    /// <summary>
    /// Configures all shader programs for the frame
    /// Academic reference: Real-Time Rendering, 4th Edition (Akenine-Möller et al.)
    /// </summary>
    public void ConfigureShaders()
    {
        // Load vertex shader (shared across all modes)
        var vertexShader = ShaderLoader.LoadShaderFile("vertex.glsl");
        
        // Configure different fragment shaders for visual effects
        ConfigureShaderProgram(ShaderMode.Normal, "normal.frag", vertexShader);
        ConfigureShaderProgram(ShaderMode.SoftGlow, "softglow.frag", vertexShader);
        ConfigureShaderProgram(ShaderMode.Bloom, "bloom.frag", vertexShader);
    }
    
    private void ConfigureShaderProgram(ShaderMode mode, string fragmentFile, string vertexSource)
    {
        var fragmentShader = ShaderLoader.LoadShaderFile(fragmentFile);
        var program = new ShaderProgram(vertexSource, fragmentShader);
        
        // Cache uniform locations for performance
        program.CacheUniformLocations(new[]
        {
            "u_projection", "u_view", "u_model",
            "u_color", "u_texture", "u_time"
        });
        
        _shaderPrograms[mode] = program;
    }
}
```

#### Render State Configuration
```csharp
/// <summary>
/// Global render state configuration
/// Educational note: Batching state changes reduces GPU driver overhead
/// </summary>
public class RenderStateConfiguration
{
    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;
    public DepthTestMode DepthTest { get; set; } = DepthTestMode.LessEqual;
    public CullMode CullMode { get; set; } = CullMode.Back;
    public bool WireframeMode { get; set; } = false;
    
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
}
```

#### Camera Configuration
```csharp
/// <summary>
/// Camera and viewport configuration
/// Educational note: Projection and view matrices transform 3D world to 2D screen
/// </summary>
public class CameraConfiguration
{
    public Matrix4 ProjectionMatrix { get; private set; }
    public Matrix4 ViewMatrix { get; private set; }
    public Viewport Viewport { get; set; }
    
    /// <summary>
    /// Configures camera matrices for rendering
    /// Mathematical reference: Computer Graphics: Principles and Practice (Foley et al.)
    /// </summary>
    public void ConfigureCamera(Camera camera, int windowWidth, int windowHeight)
    {
        // Calculate aspect ratio for proper perspective projection
        float aspectRatio = (float)windowWidth / windowHeight;
        
        // Create perspective projection matrix
        // Parameters: field of view, aspect ratio, near plane, far plane
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(camera.FieldOfView),
            aspectRatio,
            camera.NearPlane,
            camera.FarPlane
        );
        
        // Create view matrix from camera position and orientation
        ViewMatrix = Matrix4.LookAt(
            camera.Position,           // Eye position
            camera.Position + camera.Forward, // Target position
            camera.Up                  // Up vector
        );
        
        // Configure viewport for this camera
        Viewport = new Viewport(0, 0, windowWidth, windowHeight);
    }
}
```

## Phase 2: Preprocessing

### Purpose
Prepares render data, performs culling operations, and optimizes drawing order for maximum GPU efficiency.

### Core Responsibilities

#### Frustum Culling
```csharp
/// <summary>
/// Frustum culling system to eliminate off-screen objects
/// Educational note: Reduces overdraw and improves performance significantly
/// Academic reference: Real-Time Collision Detection (Christer Ericson)
/// </summary>
public class FrustumCuller
{
    private readonly Plane[] _frustumPlanes = new Plane[6];
    
    /// <summary>
    /// Extracts frustum planes from combined projection-view matrix
    /// Mathematical derivation based on Gribb & Hartmann method
    /// </summary>
    public void ExtractFrustumPlanes(Matrix4 projectionViewMatrix)
    {
        var m = projectionViewMatrix;
        
        // Left plane: m[3] + m[0]
        _frustumPlanes[0] = new Plane(
            m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41
        ).Normalized();
        
        // Right plane: m[3] - m[0]
        _frustumPlanes[1] = new Plane(
            m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41
        ).Normalized();
        
        // Bottom, Top, Near, Far planes calculated similarly...
        // [Additional plane calculations omitted for brevity]
    }
    
    /// <summary>
    /// Tests if an axis-aligned bounding box intersects the view frustum
    /// </summary>
    public bool IsVisible(BoundingBox boundingBox)
    {
        foreach (var plane in _frustumPlanes)
        {
            // Test all 8 corners of the bounding box against the plane
            bool allVerticesOutside = true;
            
            for (int i = 0; i < 8; i++)
            {
                var vertex = boundingBox.GetVertex(i);
                if (plane.DistanceToPoint(vertex) >= 0)
                {
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
- [Performance Considerations](performance-considerations.md) - Optimization strategies
- [Rac.Rendering Project Documentation](../projects/Rac.Rendering.md) - Implementation details
- [Shader Programming Guide](../educational-material/shader-programming-guide.md) - Shader development

## Changelog

- 2025-06-26: Comprehensive 4-phase rendering pipeline documentation with examples and performance optimization techniques