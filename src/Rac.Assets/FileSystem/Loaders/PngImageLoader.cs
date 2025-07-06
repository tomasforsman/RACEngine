// ═══════════════════════════════════════════════════════════════════════════════
// File: PngImageLoader.cs
// Description: Asset loader for PNG image files using SixLabors.ImageSharp
// Educational Focus: PNG format handling and image processing concepts
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - PNG file format characteristics and advantages
// - Image pixel data organization and memory layout
// - Color space conversion and pixel format standardization
// - Performance considerations for image loading and memory usage
//
// TECHNICAL IMPLEMENTATION:
// - SixLabors.ImageSharp for cross-platform PNG decoding
// - RGBA8 pixel format for consistent OpenGL compatibility
// - Memory-efficient loading with proper resource disposal
// - Comprehensive error handling for malformed PNG files
//
// PNG FORMAT EDUCATION:
// - PNG (Portable Network Graphics) is a lossless image format
// - Supports transparency through alpha channel
// - Widely supported and excellent for game sprites and UI
// - Larger file sizes than JPEG but better quality preservation
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rac.Assets.FileSystem.Loaders;

/// <summary>
/// Asset loader for PNG image files using SixLabors.ImageSharp library.
/// 
/// EDUCATIONAL PURPOSE:
/// This loader demonstrates professional PNG image handling in game engines:
/// - PNG format characteristics and why it's ideal for games
/// - Pixel data organization and memory management
/// - Color space conversion for consistent rendering
/// - Error handling for robust asset loading pipelines
/// 
/// PNG FORMAT ADVANTAGES FOR GAMES:
/// - Lossless compression preserves image quality
/// - Alpha channel support for transparency effects
/// - Wide compatibility across platforms and tools
/// - Efficient compression for sprites and UI elements
/// 
/// TECHNICAL IMPLEMENTATION DETAILS:
/// - Uses SixLabors.ImageSharp for cross-platform compatibility
/// - Converts all images to RGBA8 format for OpenGL consistency
/// - Handles various PNG sub-formats (indexed, grayscale, RGB, RGBA)
/// - Memory-efficient loading with proper disposal patterns
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - PNG decoding is CPU-intensive but happens once per asset
/// - RGBA conversion ensures consistent memory layout
/// - Loading time scales with image resolution
/// - Consider texture atlasing for many small images
/// </summary>
public class PngImageLoader : IAssetLoader<Texture>
{
    /// <inheritdoc/>
    public string Description => "PNG Image Loader using SixLabors.ImageSharp";

    /// <inheritdoc/>
    public IEnumerable<string> SupportedExtensions => new[] { ".png" };

    /// <summary>
    /// Determines if this loader can handle PNG files.
    /// Educational note: PNG files use the .png extension universally.
    /// </summary>
    /// <param name="extension">File extension to check</param>
    /// <returns>True if the extension is .png (case-insensitive)</returns>
    public bool CanLoad(string extension)
    {
        return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads a PNG image from the provided stream and converts it to a Texture.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// This method demonstrates the complete PNG loading pipeline:
    /// 1. PNG file format validation and decoding
    /// 2. Pixel format conversion to RGBA8 for consistency
    /// 3. Memory layout organization for OpenGL compatibility
    /// 4. Comprehensive error handling for production robustness
    /// 
    /// PIXEL FORMAT CONVERSION:
    /// - All PNG variants (RGB, RGBA, indexed, grayscale) are converted to RGBA8
    /// - RGBA8 = 4 bytes per pixel (Red, Green, Blue, Alpha)
    /// - Consistent format simplifies texture creation and shader usage
    /// - Alpha channel is set to 255 (opaque) for images without transparency
    /// 
    /// MEMORY ORGANIZATION:
    /// - Pixels are stored row-by-row, left-to-right
    /// - Each pixel: [R, G, B, A] as consecutive bytes
    /// - Total size: Width × Height × 4 bytes
    /// - Memory layout compatible with OpenGL GL_RGBA format
    /// </summary>
    /// <param name="stream">Stream containing PNG image data</param>
    /// <param name="path">File path for error reporting and debugging</param>
    /// <returns>Loaded texture with RGBA pixel data</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream or path is null</exception>
    /// <exception cref="FileFormatException">Thrown when PNG format is invalid or unsupported</exception>
    public Texture LoadFromStream(Stream stream, string path)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream), "Stream cannot be null");
        if (path == null)
            throw new ArgumentNullException(nameof(path), "Path cannot be null");

        try
        {
            // Load PNG image using ImageSharp
            // Educational note: ImageSharp automatically detects PNG format and handles
            // various PNG sub-types (indexed color, grayscale, RGB, RGBA)
            using var image = Image.Load<Rgba32>(stream);

            // Extract image dimensions
            // Educational note: These dimensions determine texture size and memory usage
            var width = image.Width;
            var height = image.Height;

            // Allocate pixel data array
            // Educational note: RGBA8 format = 4 bytes per pixel
            var pixelData = new byte[width * height * 4];

            // Copy pixel data from ImageSharp to our byte array
            // Educational note: This converts the image to a consistent RGBA format
            // regardless of the original PNG color type
            image.CopyPixelDataTo(pixelData);

            // Create and return the texture
            // Educational note: The texture encapsulates both pixel data and metadata
            return new Texture(pixelData, width, height, "RGBA", path);
        }
        catch (UnknownImageFormatException ex)
        {
            // Educational note: This exception occurs when the file is not a valid PNG
            // or contains unsupported PNG features
            throw new FileFormatException(
                $"File '{path}' is not a valid PNG image or contains unsupported features: {ex.Message}", 
                ex);
        }
        catch (InvalidImageContentException ex)
        {
            // Educational note: This exception occurs when the PNG file is corrupted
            // or contains invalid data structures
            throw new FileFormatException(
                $"PNG file '{path}' contains invalid or corrupted image data: {ex.Message}", 
                ex);
        }
        catch (Exception ex)
        {
            // Educational note: Catch-all for unexpected errors during PNG processing
            // Preserves the original exception for debugging
            throw new FileFormatException(
                $"Unexpected error loading PNG image from '{path}': {ex.Message}", 
                ex);
        }
    }
}