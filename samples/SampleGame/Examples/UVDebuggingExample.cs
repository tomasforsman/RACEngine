// ═══════════════════════════════════════════════════════════════════════════════
// UV DEBUGGING USAGE EXAMPLE
// ═══════════════════════════════════════════════════════════════════════════════
//
// This example demonstrates how to use the Visual Texture Coordinate Debugging
// feature to inspect and validate UV mapping in your geometry.
//
// USAGE SCENARIOS:
// 1. Debugging texture coordinate generation algorithms
// 2. Validating procedural UV mapping functions
// 3. Verifying UV coordinate ranges and distributions
// 4. Testing UV coordinate transformations and effects
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Rendering;
using Rac.Rendering.Shader;
using System;

namespace SampleGame.Examples;

/// <summary>
/// Demonstrates how to enable and use Visual Texture Coordinate Debugging
/// for diagnosing UV mapping issues in rendered geometry.
/// </summary>
public static class UVDebuggingExample
{
    /// <summary>
    /// Example of enabling UV debugging mode for mesh inspection.
    /// 
    /// DEBUGGING WORKFLOW:
    /// 1. Enable DebugUV shader mode
    /// 2. Render your geometry 
    /// 3. Inspect the color output:
    ///    - Red intensity = U coordinate value
    ///    - Green intensity = V coordinate value
    ///    - Expected patterns validate correct UV mapping
    /// 
    /// INTERPRETATION GUIDE:
    /// - Black (0,0) = Bottom-left corner of geometry (centered coord -0.5, -0.5)
    /// - Red (1,0) = Bottom-right corner of geometry (centered coord +0.5, -0.5)  
    /// - Green (0,1) = Top-left corner of geometry (centered coord -0.5, +0.5)
    /// - Yellow (1,1) = Top-right corner of geometry (centered coord +0.5, +0.5)
    /// 
    /// NOTE: RACEngine uses centered coordinates around (0,0) for procedural effects.
    /// The DebugUV shader automatically converts these to [0,1] range for visualization.
    /// </summary>
    /// <param name="renderer">Active renderer instance</param>
    public static void EnableUVDebugging(IRenderer renderer)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                  UV DEBUGGING MODE ENABLED");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("VISUALIZATION MAPPING:");
        Console.WriteLine("  U Coordinate → Red Channel");
        Console.WriteLine("  V Coordinate → Green Channel");
        Console.WriteLine();
        Console.WriteLine("EXPECTED COLOR PATTERNS:");
        Console.WriteLine("  (0,0) Bottom-Left  → Black   (centered coord -0.5, -0.5)");
        Console.WriteLine("  (1,0) Bottom-Right → Red     (centered coord +0.5, -0.5)");
        Console.WriteLine("  (0,1) Top-Left     → Green   (centered coord -0.5, +0.5)");
        Console.WriteLine("  (1,1) Top-Right    → Yellow  (centered coord +0.5, +0.5)");
        Console.WriteLine();
        Console.WriteLine("NOTE: RACEngine uses centered UV coordinates for procedural effects.");
        Console.WriteLine("The DebugUV shader converts these to [0,1] range for visualization.");
        Console.WriteLine();
        Console.WriteLine("Press Space to cycle back to normal rendering");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        
        // Enable DebugUV shader mode
        renderer.SetShaderMode(ShaderMode.DebugUV);
    }
    
    /// <summary>
    /// Example of programmatically cycling through shader modes including DebugUV.
    /// Useful for runtime switching between normal rendering and UV debugging.
    /// </summary>
    /// <param name="renderer">Active renderer instance</param>
    /// <param name="currentMode">Current shader mode</param>
    /// <returns>Next shader mode in the cycle</returns>
    public static ShaderMode CycleShaderModes(IRenderer renderer, ShaderMode currentMode)
    {
        var nextMode = currentMode switch
        {
            ShaderMode.Normal => ShaderMode.SoftGlow,
            ShaderMode.SoftGlow => ShaderMode.Bloom,
            ShaderMode.Bloom => ShaderMode.DebugUV,
            ShaderMode.DebugUV => ShaderMode.Normal,
            _ => ShaderMode.Normal
        };
        
        renderer.SetShaderMode(nextMode);
        
        Console.WriteLine($"Shader Mode: {nextMode}");
        if (nextMode == ShaderMode.DebugUV)
        {
            Console.WriteLine("  UV Debugging: U→Red, V→Green");
        }
        
        return nextMode;
    }
    
    /// <summary>
    /// Validates that DebugUV shader mode is available before attempting to use it.
    /// Essential for robust applications that may run on different platforms.
    /// </summary>
    /// <returns>True if DebugUV mode is available and ready to use</returns>
    public static bool ValidateUVDebuggingAvailability()
    {
        try
        {
            var isAvailable = ShaderLoader.IsShaderModeAvailable(ShaderMode.DebugUV);
            
            if (!isAvailable)
            {
                Console.WriteLine("Warning: DebugUV shader mode is not available.");
                Console.WriteLine("Ensure debuguv.frag exists in the shader directory.");
                return false;
            }
            
            Console.WriteLine("✓ DebugUV shader mode is available and ready to use.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating DebugUV availability: {ex.Message}");
            return false;
        }
    }
}