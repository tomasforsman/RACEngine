// ═══════════════════════════════════════════════════════════════════════════════
// File: Texture.cs
// Description: Asset type representing loaded image textures with metadata
// Educational Focus: Image data representation and memory management
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Image data storage patterns in game engines
// - Pixel format considerations (RGBA8, RGB24, etc.)
// - Memory layout and texture data organization
// - Performance implications of different image formats
//
// TECHNICAL DETAILS:
// - Raw pixel data stored as byte array for OpenGL compatibility
// - Immutable design prevents accidental modification after loading
// - Metadata preserved for debugging and optimization decisions
// - Disposal pattern for proper memory management
//
// GRAPHICS PIPELINE INTEGRATION:
// - Texture data format compatible with OpenGL texture creation
// - Width/height information for proper texture binding
// - Pixel format specification for shader sampling
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace Rac.Assets.Types;

/// <summary>
/// Represents a loaded image texture with pixel data and metadata.
/// 
/// EDUCATIONAL PURPOSE:
/// This class demonstrates how game engines handle image assets, including:
/// - Raw pixel data management for graphics APIs
/// - Immutable asset design for thread safety
/// - Memory-efficient storage with proper disposal
/// - Metadata preservation for debugging and optimization
/// 
/// TECHNICAL IMPLEMENTATION:
/// - Pixel data stored as byte array in RGBA format (4 bytes per pixel)
/// - Width and Height specify image dimensions in pixels
/// - Format indicates pixel arrangement (typically RGBA for compatibility)
/// - Immutable design prevents modification after creation
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Large textures consume significant memory (Width × Height × 4 bytes)
/// - Disposal is critical to prevent memory leaks
/// - Consider texture compression for production assets
/// </summary>
public sealed class Texture : IDisposable
{
    /// <summary>
    /// Gets the raw pixel data in RGBA format.
    /// Each pixel is represented by 4 bytes: Red, Green, Blue, Alpha.
    /// </summary>
    public byte[] PixelData { get; }

    /// <summary>
    /// Gets the width of the texture in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the texture in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the pixel format specification.
    /// Educational note: Different formats affect memory usage and compatibility.
    /// RGBA provides maximum compatibility but uses more memory than RGB.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Gets the source file path this texture was loaded from.
    /// Useful for debugging and asset pipeline optimization.
    /// </summary>
    public string SourcePath { get; }

    private bool _disposed;

    /// <summary>
    /// Creates a new texture instance with the specified pixel data and metadata.
    /// </summary>
    /// <param name="pixelData">Raw pixel data in the specified format</param>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="format">Pixel format (e.g., "RGBA", "RGB")</param>
    /// <param name="sourcePath">Source file path for debugging</param>
    /// <exception cref="ArgumentNullException">Thrown when pixelData or format is null</exception>
    /// <exception cref="ArgumentException">Thrown when dimensions are invalid</exception>
    public Texture(byte[] pixelData, int width, int height, string format, string sourcePath)
    {
        PixelData = pixelData ?? throw new ArgumentNullException(nameof(pixelData));
        Width = width > 0 ? width : throw new ArgumentException("Width must be positive", nameof(width));
        Height = height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height));
        Format = format ?? throw new ArgumentException("Format cannot be null", nameof(format));
        SourcePath = sourcePath ?? "";
    }

    /// <summary>
    /// Releases the texture resources.
    /// Educational note: Proper disposal is critical in game engines to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // In a full implementation, this might also release GPU texture handles
            // For now, we rely on garbage collection for the byte array
            _disposed = true;
        }
    }

    /// <summary>
    /// Gets the total memory size of this texture in bytes.
    /// Educational note: Useful for memory profiling and optimization.
    /// </summary>
    public int MemorySize => PixelData.Length;

    /// <summary>
    /// Gets a string representation of this texture for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"Texture({Width}x{Height}, {Format}, {MemorySize / 1024}KB, {SourcePath})";
    }
}