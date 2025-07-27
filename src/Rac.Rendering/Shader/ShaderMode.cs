namespace Rac.Rendering.Shader;

/// <summary>
/// Defines the available shader rendering modes for different visual effects.
/// </summary>
public enum ShaderMode
{
    /// <summary>Standard flat color rendering.</summary>
    Normal,
    
    /// <summary>Basic textured rendering with texture sampling and color blending.</summary>
    Textured,
    
    /// <summary>Soft edge glow effect with radial gradient alpha.</summary>
    SoftGlow,
    
    /// <summary>Advanced bloom effect with enhanced glow radius.</summary>
    Bloom,
    
    /// <summary>Visual debugging mode that displays texture coordinates as colors (U→Red, V→Green).</summary>
    DebugUV
}