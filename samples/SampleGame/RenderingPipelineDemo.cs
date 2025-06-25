// ═══════════════════════════════════════════════════════════════════════════════
// RENDERING PIPELINE DEMONSTRATION - EDUCATIONAL 4-PHASE ARCHITECTURE SHOWCASE
// ═══════════════════════════════════════════════════════════════════════════════
//
// This sample provides a comprehensive, educational demonstration of RACEngine's
// sophisticated 4-phase rendering pipeline architecture. It showcases how the
// separation of concerns enables better performance, tool development, and
// maintainability in modern graphics applications.
//
// EDUCATIONAL OBJECTIVES:
//
// 1. PHASE SEPARATION UNDERSTANDING:
//    - Configuration: Pure data structures (immutable, thread-safe)
//    - Preprocessing: Asset loading, validation, compilation (one-time)
//    - Processing: Fast GPU operations (every frame)
//    - Post-Processing: Screen-space effects (frame finalization)
//
// 2. GRAPHICS PIPELINE CONCEPTS:
//    - Vertex submission and transformation
//    - Shader compilation and uniform management
//    - Framebuffer operations and state management
//    - Post-processing effects and HDR color handling
//
// 3. PERFORMANCE OPTIMIZATION PRINCIPLES:
//    - Asset loading outside render loop (preprocessing phase)
//    - Minimal per-frame overhead (processing phase)
//    - Batched operations and state minimization
//    - Memory allocation patterns for real-time graphics
//
// 4. TOOL DEVELOPMENT ENABLEMENT:
//    - Phase isolation for independent tool access
//    - Configuration inspection without GPU dependency
//    - Asset management separate from rendering logic
//    - Debug instrumentation at phase boundaries
//
// 5. MODERN GRAPHICS ARCHITECTURE:
//    - Immutable configuration patterns
//    - Command-based rendering design
//    - Resource management and lifecycle
//    - Error handling and graceful degradation
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Core.Extension;
using Rac.Core.Manager;
using Rac.Engine;
using Rac.Input.Service;
using Rac.Input.State;
using Rac.Rendering;
using Rac.Rendering.Geometry;
using Rac.Rendering.Pipeline;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;

namespace SampleGame;

public static class RenderingPipelineDemo
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEMONSTRATION STATE AND CONTROLS
    // ═══════════════════════════════════════════════════════════════════════════
    
    private static int _currentPhaseIndex = 0;
    private static readonly List<string> _phaseNames = new()
    {
        "Configuration",
        "Preprocessing", 
        "Processing",
        "Post-Processing"
    };
    
    private static readonly List<string> _phaseDescriptions = new()
    {
        "Pure data structures defining render behavior",
        "Asset loading, shader compilation, GPU resource setup",
        "Fast per-frame GPU operations and vertex rendering",
        "Screen-space effects: bloom, tone mapping, HDR processing"
    };
    
    private static bool _showPhaseDetails = true;
    private static bool _showPerformanceMetrics = true;
    private static float _rotationAngle = 0f;
    private static ShaderMode _currentShaderMode = ShaderMode.Normal;
    
    // ═══════════════════════════════════════════════════════════════════════════
    // SAMPLE GEOMETRY FOR PIPELINE DEMONSTRATION - USING GEOMETRY GENERATORS
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Demonstrates the standardized geometry creation utilities that provide:
    // - Consistent vertex layout and UV mapping across all shapes
    // - Proper [0,1] texture coordinate ranges for predictable texture application
    // - Educational examples of common geometric primitives
    // - Simplified geometry creation compared to manual vertex array construction
    
    // Create base geometries using standardized utilities
    // These are generated once and then transformed during rendering
    private static readonly FullVertex[] _triangleVertices = GeometryGenerators.CreateTriangle(
        1.0f,  // Size (base width)
        new Vector4D<float>(0.8f, 0.4f, 0.4f, 1.0f)  // Soft red color
    );
    
    private static readonly FullVertex[] _quadVertices = GeometryGenerators.CreateSquare(
        0.6f,  // Side length
        new Vector4D<float>(0.6f, 0.8f, 0.6f, 1.0f)  // Soft green color
    );

    /// <summary>
    /// Main entry point for the rendering pipeline demonstration.
    /// 
    /// DEMONSTRATION FLOW:
    /// 1. Creates engine with explicit phase management
    /// 2. Shows configuration phase in isolation
    /// 3. Demonstrates preprocessing with asset loading
    /// 4. Runs processing phase with real-time rendering
    /// 5. Applies post-processing effects for final output
    /// 
    /// INTERACTIVE FEATURES:
    /// - Tab: Cycle through phase explanations
    /// - Space: Toggle shader modes (Normal/SoftGlow/Bloom)
    /// - P: Show/hide performance metrics
    /// - D: Show/hide detailed phase information
    /// </summary>
    public static void Run(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              RENDERING PIPELINE DEMONSTRATION");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Educational showcase of RACEngine's 4-phase rendering pipeline:");
        Console.WriteLine();
        Console.WriteLine("PHASES:");
        Console.WriteLine("  1. Configuration  - Pure data setup (no GPU interaction)");
        Console.WriteLine("  2. Preprocessing  - Asset loading & validation");
        Console.WriteLine("  3. Processing     - Fast GPU rendering operations");
        Console.WriteLine("  4. Post-Processing - Screen-space effects & finalization");
        Console.WriteLine();
        Console.WriteLine("CONTROLS:");
        Console.WriteLine("  Tab   - Cycle through phase explanations");
        Console.WriteLine("  Space - Toggle shader modes (Normal/SoftGlow/Bloom)");
        Console.WriteLine("  P     - Toggle performance metrics display");
        Console.WriteLine("  D     - Toggle detailed phase information");
        Console.WriteLine("  ESC   - Exit demonstration");
        Console.WriteLine();
        Console.WriteLine("Starting demonstration...");
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // ENGINE INITIALIZATION WITH EXPLICIT PHASE DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════
        //
        // This demonstrates how the engine coordinates all four phases while
        // allowing us to highlight each phase's unique responsibilities.

        var windowManager = new WindowManager();
        var inputService = new SilkInputService();
        var configurationManager = new ConfigManager();
        
        // Configure window for the demonstration
        configurationManager.Window.Size = "1200,800";
        configurationManager.Window.Title = "RACEngine - 4-Phase Rendering Pipeline Demo";
        
        var engine = new EngineFacade(windowManager, inputService, configurationManager);

        Console.WriteLine("✓ Phase 1: Configuration completed");
        Console.WriteLine("  - Window configuration: 1200x800");
        Console.WriteLine("  - Render settings established");
        Console.WriteLine("  - No GPU resources allocated yet");
        Console.WriteLine();

        // ═══════════════════════════════════════════════════════════════════════
        // INPUT EVENT HANDLERS FOR INTERACTIVE DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════

        engine.KeyEvent += (key, keyEvent) =>
        {
            if (keyEvent == KeyboardKeyState.KeyEvent.Pressed)
            {
                HandleKeyPress(key, engine);
            }
        };

        // ═══════════════════════════════════════════════════════════════════════
        // LOAD EVENT: PREPROCESSING AND PROCESSING PHASE DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════

        engine.LoadEvent += () =>
        {
            Console.WriteLine("✓ Phase 2: Preprocessing completed");
            Console.WriteLine("  - OpenGL context initialized");
            Console.WriteLine("  - Shaders compiled and validated");
            Console.WriteLine("  - GPU resources allocated");
            Console.WriteLine();
            
            Console.WriteLine("✓ Phase 3: Processing setup completed");
            Console.WriteLine("  - Vertex buffers created");
            Console.WriteLine("  - Render state initialized");
            Console.WriteLine("  - Ready for per-frame operations");
            Console.WriteLine();
            
            Console.WriteLine("✓ Phase 4: Post-Processing initialized");
            Console.WriteLine("  - Framebuffers configured");
            Console.WriteLine("  - Effect pipeline ready");
            Console.WriteLine("  - HDR and tone mapping available");
            Console.WriteLine();
            
            Console.WriteLine("🎮 Demo ready! Use Tab to explore phases, Space for effects");
            Console.WriteLine();
        };

        // ═══════════════════════════════════════════════════════════════════════
        // UPDATE EVENT: ANIMATION AND LOGIC
        // ═══════════════════════════════════════════════════════════════════════

        engine.UpdateEvent += deltaSeconds =>
        {
            // Smooth rotation animation for visual appeal
            _rotationAngle += deltaSeconds * 45f; // 45 degrees per second
            if (_rotationAngle > 360f) _rotationAngle -= 360f;
        };

        // ═══════════════════════════════════════════════════════════════════════
        // RENDER EVENT: PROCESSING AND POST-PROCESSING PHASE DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════

        engine.RenderEvent += deltaSeconds =>
        {
            // ───────────────────────────────────────────────────────────────────
            // PHASE 3: PROCESSING - Fast GPU Operations
            // ───────────────────────────────────────────────────────────────────
            
            engine.Renderer.Clear();
            engine.Renderer.SetShaderMode(_currentShaderMode);
            
            // Demonstrate different geometry rendering
            DrawDemonstrationGeometry(engine);
            
            // ───────────────────────────────────────────────────────────────────
            // PHASE 4: POST-PROCESSING - Screen-space Effects
            // ───────────────────────────────────────────────────────────────────
            
            // The engine automatically applies post-processing based on shader mode
            engine.Renderer.FinalizeFrame();
            
            // Draw educational UI overlay
            DrawPhaseInformationUI(engine);
        };

        // ═══════════════════════════════════════════════════════════════════════
        // START DEMONSTRATION
        // ═══════════════════════════════════════════════════════════════════════

        engine.Run();
        
        Console.WriteLine();
        Console.WriteLine("Demonstration completed. Thank you for exploring the pipeline!");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INPUT HANDLING FOR INTERACTIVE PHASE EXPLORATION
    // ═══════════════════════════════════════════════════════════════════════════

    private static void HandleKeyPress(Key key, IEngineFacade engine)
    {
        switch (key)
        {
            case Key.Tab:
                CyclePhaseExplanation();
                break;
                
            case Key.Space:
                CycleShaderMode(engine);
                break;
                
            case Key.P:
                _showPerformanceMetrics = !_showPerformanceMetrics;
                Console.WriteLine($"Performance metrics: {(_showPerformanceMetrics ? "ON" : "OFF")}");
                break;
                
            case Key.D:
                _showPhaseDetails = !_showPhaseDetails;
                Console.WriteLine($"Phase details: {(_showPhaseDetails ? "ON" : "OFF")}");
                break;
                
            case Key.Escape:
                Console.WriteLine("Exiting pipeline demonstration...");
                Environment.Exit(0);
                break;
        }
    }

    private static void CyclePhaseExplanation()
    {
        _currentPhaseIndex = (_currentPhaseIndex + 1) % _phaseNames.Count;
        
        Console.WriteLine($"═══ PHASE {_currentPhaseIndex + 1}: {_phaseNames[_currentPhaseIndex].ToUpper()} ═══");
        Console.WriteLine(_phaseDescriptions[_currentPhaseIndex]);
        Console.WriteLine();
        
        // Provide specific technical details for each phase
        switch (_currentPhaseIndex)
        {
            case 0: // Configuration
                Console.WriteLine("TECHNICAL DETAILS:");
                Console.WriteLine("• Immutable data structures (thread-safe)");
                Console.WriteLine("• No GPU interaction or file I/O");
                Console.WriteLine("• Builder pattern for complex setup");
                Console.WriteLine("• Validation at construction time");
                break;
                
            case 1: // Preprocessing
                Console.WriteLine("TECHNICAL DETAILS:");
                Console.WriteLine("• Asset loading and shader compilation");
                Console.WriteLine("• OpenGL capability validation");
                Console.WriteLine("• GPU resource allocation");
                Console.WriteLine("• One-time expensive operations");
                break;
                
            case 2: // Processing
                Console.WriteLine("TECHNICAL DETAILS:");
                Console.WriteLine("• Per-frame vertex submission");
                Console.WriteLine("• GPU state management");
                Console.WriteLine("• Draw call optimization");
                Console.WriteLine("• Minimal CPU overhead");
                break;
                
            case 3: // Post-Processing
                Console.WriteLine("TECHNICAL DETAILS:");
                Console.WriteLine("• Screen-space effect application");
                Console.WriteLine("• HDR tone mapping");
                Console.WriteLine("• Bloom and glow effects");
                Console.WriteLine("• Frame presentation");
                break;
        }
        Console.WriteLine();
    }

    private static void CycleShaderMode(IEngineFacade engine)
    {
        _currentShaderMode = _currentShaderMode switch
        {
            ShaderMode.Normal => ShaderMode.SoftGlow,
            ShaderMode.SoftGlow => ShaderMode.Bloom,
            ShaderMode.Bloom => ShaderMode.Normal,
            _ => ShaderMode.Normal
        };
        
        Console.WriteLine($"Shader mode changed to: {_currentShaderMode}");
        Console.WriteLine($"  Effect: {GetShaderModeDescription(_currentShaderMode)}");
        Console.WriteLine();
    }

    private static string GetShaderModeDescription(ShaderMode mode)
    {
        return mode switch
        {
            ShaderMode.Normal => "Standard rendering without post-processing",
            ShaderMode.SoftGlow => "Subtle luminance enhancement",
            ShaderMode.Bloom => "HDR bloom with dramatic light bleeding",
            _ => "Unknown shader mode"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GEOMETRIC RENDERING FOR PIPELINE DEMONSTRATION
    // ═══════════════════════════════════════════════════════════════════════════

    private static void DrawDemonstrationGeometry(IEngineFacade engine)
    {
        // Draw static triangle (left side) to show basic vertex processing
        engine.Renderer.SetColor(new Vector4D<float>(1f, 1f, 1f, 1f));
        
        // Transform vertices for left position - using geometry generator output directly
        var leftTriangleVertices = TransformVertices(_triangleVertices, new Vector2D<float>(-0.6f, 0f), 0f, 0.8f);
        engine.Renderer.UpdateVertices(leftTriangleVertices);
        engine.Renderer.Draw();
        
        // Transform vertices for right position with rotation
        var rightQuadVertices = TransformVertices(_quadVertices, new Vector2D<float>(0.6f, 0f), _rotationAngle, 0.8f);
        engine.Renderer.UpdateVertices(rightQuadVertices);
        engine.Renderer.Draw();
    }

    /// <summary>
    /// Transforms an array of FullVertex structures by applying translation, rotation, and scaling.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Demonstrates geometric transformations applied to pre-generated vertex data from
    /// GeometryGenerators. This approach separates geometry creation from transformation,
    /// enabling better performance and cleaner code organization.
    /// 
    /// TRANSFORMATION ORDER:
    /// 1. Scale: Multiply position by scale factor
    /// 2. Rotate: Apply 2D rotation matrix around origin
    /// 3. Translate: Add offset to final position
    /// 
    /// UV COORDINATE PRESERVATION:
    /// UV coordinates are NOT modified during transformation, preserving the original
    /// texture mapping established by the geometry generators. This ensures consistent
    /// texture application regardless of object transformations.
    /// </summary>
    /// <param name="vertices">Source vertices generated by GeometryGenerators</param>
    /// <param name="offset">Translation offset in world coordinates</param>
    /// <param name="rotation">Rotation angle in degrees (counter-clockwise)</param>
    /// <param name="scale">Uniform scale factor applied to all dimensions</param>
    /// <returns>New array of transformed vertices with preserved UV coordinates</returns>
    private static FullVertex[] TransformVertices(FullVertex[] vertices, Vector2D<float> offset, float rotation, float scale)
    {
        // Convert rotation from degrees to radians and calculate trigonometric values
        var radians = rotation * MathF.PI / 180f;
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        
        var result = new FullVertex[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            
            // ───────────────────────────────────────────────────────────────
            // GEOMETRIC TRANSFORMATIONS (UV COORDINATES PRESERVED)
            // ───────────────────────────────────────────────────────────────
            
            // Step 1: Apply scaling to local position
            var scaledX = vertex.Position.X * scale;
            var scaledY = vertex.Position.Y * scale;
            
            // Step 2: Apply rotation using 2D rotation matrix
            // [ cos(θ)  -sin(θ) ] [ x ]   [ x*cos(θ) - y*sin(θ) ]
            // [ sin(θ)   cos(θ) ] [ y ] = [ x*sin(θ) + y*cos(θ) ]
            var rotatedX = scaledX * cos - scaledY * sin;
            var rotatedY = scaledX * sin + scaledY * cos;
            
            // Step 3: Apply translation to get final world position
            var finalPosition = new Vector2D<float>(rotatedX + offset.X, rotatedY + offset.Y);
            
            // Create transformed vertex preserving original UV coordinates and color
            result[i] = new FullVertex(finalPosition, vertex.TexCoord, vertex.Color);
        }
        
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EDUCATIONAL UI OVERLAY SYSTEM
    // ═══════════════════════════════════════════════════════════════════════════

    private static void DrawPhaseInformationUI(IEngineFacade engine)
    {
        // This would typically use a UI system, but for this demo we'll use console output
        // In a real implementation, this would render text overlays showing:
        // - Current phase being highlighted
        // - Performance metrics if enabled
        // - Real-time pipeline statistics
        // - Interactive help text
        
        if (_showPerformanceMetrics)
        {
            // In a full implementation, this would display frame time, vertex count,
            // draw calls, etc. using on-screen text rendering
        }
    }
}