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
using Rac.Rendering.Pipeline;
using Rac.Rendering.Shader;
using Silk.NET.Input;
using Silk.NET.Maths;
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
    // SAMPLE GEOMETRY FOR PIPELINE DEMONSTRATION
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // Simple geometric shapes that clearly demonstrate each pipeline phase:
    // - Triangle: Basic vertex processing
    // - Rotating quad: Transformation and animation
    // - Color gradient: Shader and uniform management
    
    private static readonly float[] _triangleVertices = new float[]
    {
        // Position        Color
         0.0f,  0.5f,     1.0f, 0.2f, 0.2f, 1.0f,  // Top vertex (red)
        -0.5f, -0.5f,     0.2f, 1.0f, 0.2f, 1.0f,  // Bottom left (green)
         0.5f, -0.5f,     0.2f, 0.2f, 1.0f, 1.0f   // Bottom right (blue)
    };
    
    private static readonly float[] _quadVertices = new float[]
    {
        // Triangle 1
        -0.3f, -0.3f,     0.8f, 0.6f, 0.2f, 1.0f,  // Bottom left
         0.3f, -0.3f,     0.2f, 0.8f, 0.6f, 1.0f,  // Bottom right
         0.3f,  0.3f,     0.6f, 0.2f, 0.8f, 1.0f,  // Top right
        
        // Triangle 2
        -0.3f, -0.3f,     0.8f, 0.6f, 0.2f, 1.0f,  // Bottom left
         0.3f,  0.3f,     0.6f, 0.2f, 0.8f, 1.0f,  // Top right
        -0.3f,  0.3f,     0.8f, 0.8f, 0.2f, 1.0f   // Top left
    };

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
        
        // Transform vertices for left position
        var leftTriangleVertices = TransformVertices(_triangleVertices, new Vector2D<float>(-0.6f, 0f), 0f, 0.8f);
        engine.Renderer.UpdateVertices(leftTriangleVertices);
        engine.Renderer.Draw();
        
        // Draw rotating quad (right side) to show animation and transformation
        var rightQuadVertices = TransformVertices(_quadVertices, new Vector2D<float>(0.6f, 0f), _rotationAngle, 0.8f);
        engine.Renderer.UpdateVertices(rightQuadVertices);
        engine.Renderer.Draw();
    }

    private static float[] TransformVertices(float[] vertices, Vector2D<float> offset, float rotation, float scale)
    {
        var transformed = new float[vertices.Length];
        var cos = MathF.Cos(rotation * MathF.PI / 180f);
        var sin = MathF.Sin(rotation * MathF.PI / 180f);
        
        for (int i = 0; i < vertices.Length; i += 6) // 6 floats per vertex (x, y, r, g, b, a)
        {
            // Get original position
            var x = vertices[i] * scale;
            var y = vertices[i + 1] * scale;
            
            // Apply rotation
            var rotX = x * cos - y * sin;
            var rotY = x * sin + y * cos;
            
            // Apply translation and store
            transformed[i] = rotX + offset.X;
            transformed[i + 1] = rotY + offset.Y;
            
            // Copy color values unchanged
            transformed[i + 2] = vertices[i + 2]; // R
            transformed[i + 3] = vertices[i + 3]; // G
            transformed[i + 4] = vertices[i + 4]; // B
            transformed[i + 5] = vertices[i + 5]; // A
        }
        
        return transformed;
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