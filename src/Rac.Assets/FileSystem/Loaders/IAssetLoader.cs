// ═══════════════════════════════════════════════════════════════════════════════
// File: IAssetLoader.cs
// Description: Generic interface for loading specific asset types from streams
// Educational Focus: Plugin architecture and file format handling
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Plugin architecture patterns for extensible systems
// - Stream-based I/O for efficient file handling
// - File format detection and validation strategies
// - Error handling patterns for robust file parsing
//
// ARCHITECTURAL BENEFITS:
// - Modular design allows adding new asset types without changing core code
// - Stream-based loading enables efficient memory usage
// - Extension-based format detection provides simple configuration
// - Generic type system ensures type safety
//
// PLUGIN PATTERN IMPLEMENTATION:
// - Each asset type has a dedicated loader implementation
// - Loaders are registered with the asset service
// - Format detection through file extensions
// - Swappable implementations for different libraries or optimizations
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace Rac.Assets.FileSystem.Loaders;

/// <summary>
/// Generic interface for loading specific asset types from file streams.
/// 
/// EDUCATIONAL PURPOSE:
/// This interface demonstrates the plugin architecture pattern commonly used in game engines:
/// - Modular design allows new asset types without modifying existing code
/// - Stream-based I/O provides efficient memory usage and flexibility
/// - Generic type system ensures compile-time type safety
/// - Extension-based format detection simplifies configuration
/// 
/// PLUGIN ARCHITECTURE BENEFITS:
/// - Extensibility: New asset types can be added by implementing this interface
/// - Maintainability: Each loader is self-contained and independently testable
/// - Performance: Specialized loaders can optimize for specific file formats
/// - Flexibility: Multiple loaders can support the same asset type (fallbacks)
/// 
/// IMPLEMENTATION PATTERN:
/// 1. Check CanLoad() to verify the loader supports the file extension
/// 2. Call LoadFromStream() with an open file stream
/// 3. Loader parses the file format and returns the appropriate asset type
/// 4. Error handling through exceptions with descriptive messages
/// 
/// SUPPORTED SCENARIOS:
/// - Single format loaders (PngImageLoader for PNG files only)
/// - Multi-format loaders (ImageLoader supporting PNG, JPEG, BMP)
/// - Fallback loaders (DefaultTextLoader for any text-based format)
/// - Optimized loaders (FastPngLoader with specialized PNG optimizations)
/// </summary>
/// <typeparam name="T">The type of asset this loader produces</typeparam>
public interface IAssetLoader<T> where T : class
{
    /// <summary>
    /// Determines if this loader can handle files with the specified extension.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// File extension checking is a simple but effective format detection method:
    /// - Fast comparison without opening files
    /// - Consistent with user expectations
    /// - Allows multiple loaders per extension (priority-based selection)
    /// - Simple configuration through string comparisons
    /// 
    /// DESIGN CONSIDERATIONS:
    /// - Extensions should include the dot (e.g., ".png", not "png")
    /// - Comparison should be case-insensitive for cross-platform compatibility
    /// - Multiple extensions can be supported by a single loader
    /// - Magic number validation can be added in LoadFromStream for security
    /// </summary>
    /// <param name="extension">File extension including the dot (e.g., ".png", ".wav")</param>
    /// <returns>True if this loader can handle the specified extension</returns>
    /// <example>
    /// <code>
    /// // Example implementation for PNG loader
    /// public bool CanLoad(string extension)
    /// {
    ///     return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase);
    /// }
    /// 
    /// // Example implementation for multi-format image loader
    /// public bool CanLoad(string extension)
    /// {
    ///     var supportedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };
    ///     return supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    /// }
    /// </code>
    /// </example>
    bool CanLoad(string extension);

    /// <summary>
    /// Loads an asset from the provided stream.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Stream-based loading demonstrates several important concepts:
    /// - Efficient I/O through streaming rather than loading entire files into memory
    /// - Separation of file access from format parsing
    /// - Enables loading from various sources (files, networks, embedded resources)
    /// - Proper resource management through using statements
    /// 
    /// IMPLEMENTATION GUIDELINES:
    /// - Do not dispose the stream - the caller manages its lifetime
    /// - Reset stream position if you need to read multiple times
    /// - Validate file format early to provide clear error messages
    /// - Use binary readers for binary formats, text readers for text formats
    /// - Handle malformed files gracefully with descriptive exceptions
    /// 
    /// ERROR HANDLING STRATEGY:
    /// - FileFormatException: Invalid or corrupted file format
    /// - NotSupportedException: Valid format but unsupported features
    /// - ArgumentException: Invalid parameters or file path
    /// - IOException: File system errors (permissions, disk full, etc.)
    /// </summary>
    /// <param name="stream">Open stream containing the asset data</param>
    /// <param name="path">File path for debugging and error reporting</param>
    /// <returns>Loaded asset instance</returns>
    /// <exception cref="FileFormatException">Thrown when the file format is invalid or corrupted</exception>
    /// <exception cref="NotSupportedException">Thrown when the format is valid but contains unsupported features</exception>
    /// <exception cref="ArgumentNullException">Thrown when stream or path is null</exception>
    /// <exception cref="IOException">Thrown when stream reading fails</exception>
    /// <example>
    /// <code>
    /// // Example implementation for PNG loader
    /// public Texture LoadFromStream(Stream stream, string path)
    /// {
    ///     if (stream == null) throw new ArgumentNullException(nameof(stream));
    ///     if (path == null) throw new ArgumentNullException(nameof(path));
    ///     
    ///     try
    ///     {
    ///         using var image = Image.Load&lt;Rgba32&gt;(stream);
    ///         var pixels = new byte[image.Width * image.Height * 4];
    ///         image.CopyPixelDataTo(pixels);
    ///         
    ///         return new Texture(pixels, image.Width, image.Height, "RGBA", path);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         throw new FileFormatException($"Failed to load PNG image from '{path}': {ex.Message}", ex);
    ///     }
    /// }
    /// </code>
    /// </example>
    T LoadFromStream(Stream stream, string path);

    /// <summary>
    /// Gets a human-readable description of this loader.
    /// Educational note: Useful for debugging, logging, and asset pipeline tools.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the supported file extensions for this loader.
    /// Educational note: Enables automatic discovery and configuration of available loaders.
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }
}

/// <summary>
/// Exception thrown when an asset file format is invalid or corrupted.
/// 
/// EDUCATIONAL PURPOSE:
/// Custom exceptions provide specific error handling for asset loading scenarios:
/// - Clear distinction between format errors and other I/O errors
/// - Preserves original exception context through inner exceptions
/// - Enables specific error handling in loading code
/// - Improves debugging experience with descriptive messages
/// </summary>
public class FileFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the FileFormatException class.
    /// </summary>
    public FileFormatException() : base("Invalid file format") { }

    /// <summary>
    /// Initializes a new instance of the FileFormatException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public FileFormatException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the FileFormatException class with a specified error message 
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public FileFormatException(string message, Exception innerException) : base(message, innerException) { }
}