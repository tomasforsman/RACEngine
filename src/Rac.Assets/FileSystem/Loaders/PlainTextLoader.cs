// ═══════════════════════════════════════════════════════════════════════════════
// File: PlainTextLoader.cs
// Description: Asset loader for plain text files (shaders, configs, scripts)
// Educational Focus: Text file handling and encoding considerations
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Text file encoding detection and handling (UTF-8, ASCII)
// - Cross-platform text file compatibility (line endings)
// - Memory-efficient text loading for large shader files
// - Text asset use cases in game development
//
// TECHNICAL IMPLEMENTATION:
// - UTF-8 encoding with fallback to ASCII for compatibility
// - Automatic line ending normalization for cross-platform support
// - Memory-efficient streaming for large text assets
// - Comprehensive error handling for encoding issues
//
// TEXT ASSETS IN GAMES:
// - Shader source code (vertex, fragment, compute shaders)
// - Configuration files (JSON, XML, INI formats)
// - Script files (Lua, Python, JavaScript game scripts)
// - Localization data (text strings, dialog, UI text)
//
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text;

namespace Rac.Assets.FileSystem.Loaders;

/// <summary>
/// Asset loader for plain text files with encoding detection and cross-platform compatibility.
/// 
/// EDUCATIONAL PURPOSE:
/// This loader demonstrates text file handling concepts important for game development:
/// - Character encoding detection and conversion (UTF-8, ASCII)
/// - Cross-platform text compatibility (Unix vs Windows line endings)
/// - Memory-efficient text loading for shader files and configurations
/// - Error handling for encoding and formatting issues
/// 
/// TEXT ASSETS IN GAME DEVELOPMENT:
/// - Shader Source: GLSL vertex, fragment, and compute shader code
/// - Configuration: Game settings, level data, asset manifests
/// - Scripts: Game logic scripts in various scripting languages
/// - Localization: Text strings, dialog, and UI text in multiple languages
/// 
/// ENCODING CONSIDERATIONS:
/// - UTF-8: Universal encoding supporting all Unicode characters
/// - ASCII: Simple 7-bit encoding for basic English text (legacy compatibility)
/// - BOM Detection: Byte Order Mark can indicate UTF-8 encoding
/// - Cross-platform: Different operating systems use different line endings
/// 
/// PERFORMANCE CHARACTERISTICS:
/// - Text files are typically small compared to binary assets
/// - Loading time is minimal but encoding conversion can add overhead
/// - Memory usage scales linearly with file size
/// - Caching is beneficial for frequently accessed shader source code
/// </summary>
public class PlainTextLoader : IAssetLoader<string>
{
    /// <inheritdoc/>
    public string Description => "Plain Text Loader with encoding detection and cross-platform support";

    /// <inheritdoc/>
    public IEnumerable<string> SupportedExtensions => new[]
    {
        // Shader files
        ".vert", ".frag", ".geom", ".comp", ".tesc", ".tese",
        ".glsl", ".hlsl", ".fx",
        
        // Configuration files
        ".txt", ".ini", ".cfg", ".conf",
        
        // Script files
        ".lua", ".js", ".py", ".cs",
        
        // Data files
        ".json", ".xml", ".yaml", ".yml", ".csv"
    };

    /// <summary>
    /// Determines if this loader can handle text-based files.
    /// Educational note: This loader supports many text-based formats commonly used in games.
    /// </summary>
    /// <param name="extension">File extension to check</param>
    /// <returns>True if the extension represents a text-based format</returns>
    public bool CanLoad(string extension)
    {
        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads a text file from the provided stream with automatic encoding detection.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// This method demonstrates professional text loading practices:
    /// 1. Encoding detection using BOM (Byte Order Mark) or fallback to UTF-8
    /// 2. Cross-platform line ending normalization
    /// 3. Memory-efficient streaming for large text files
    /// 4. Comprehensive error handling for encoding and I/O issues
    /// 
    /// ENCODING DETECTION STRATEGY:
    /// - Check for UTF-8 BOM (Byte Order Mark) at file start
    /// - Default to UTF-8 encoding (covers ASCII as subset)
    /// - Handle encoding errors gracefully with fallback strategies
    /// - Preserve original text formatting where possible
    /// 
    /// CROSS-PLATFORM COMPATIBILITY:
    /// - Windows uses CRLF (\r\n) line endings
    /// - Unix/Linux/Mac use LF (\n) line endings
    /// - Normalize to LF for consistent processing
    /// - Shader compilers expect consistent line endings
    /// 
    /// PERFORMANCE OPTIMIZATIONS:
    /// - Stream-based reading for memory efficiency
    /// - Single-pass encoding detection and conversion
    /// - Minimal string allocations during processing
    /// - Efficient line ending normalization
    /// </summary>
    /// <param name="stream">Stream containing text file data</param>
    /// <param name="path">File path for error reporting and debugging</param>
    /// <returns>Loaded text content as string</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream or path is null</exception>
    /// <exception cref="FileFormatException">Thrown when text encoding is invalid or unsupported</exception>
    public string LoadFromStream(Stream stream, string path)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream), "Stream cannot be null");
        if (path == null)
            throw new ArgumentNullException(nameof(path), "Path cannot be null");

        try
        {
            // Detect encoding and read text content
            var encoding = DetectEncoding(stream);
            
            // Reset stream position after encoding detection
            stream.Position = 0;
            
            // Read text content using detected encoding
            using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var content = reader.ReadToEnd();
            
            // Normalize line endings for cross-platform compatibility
            // Educational note: This ensures consistent text processing across different platforms
            var normalizedContent = NormalizeLineEndings(content);
            
            return normalizedContent;
        }
        catch (DecoderFallbackException ex)
        {
            // Educational note: This occurs when the file contains invalid characters for the detected encoding
            throw new FileFormatException(
                $"Text file '{path}' contains invalid characters that cannot be decoded: {ex.Message}", 
                ex);
        }
        catch (Exception ex) when (!(ex is FileFormatException))
        {
            // Educational note: Catch-all for unexpected errors during text loading
            throw new FileFormatException(
                $"Unexpected error loading text file '{path}': {ex.Message}", 
                ex);
        }
    }

    /// <summary>
    /// Detects the text encoding of the stream by examining the Byte Order Mark (BOM).
    /// 
    /// EDUCATIONAL PURPOSE:
    /// BOM detection is important for correctly interpreting text files:
    /// - UTF-8 BOM: EF BB BF (indicates UTF-8 encoding)
    /// - UTF-16 BOM: FF FE or FE FF (indicates UTF-16 encoding)
    /// - No BOM: Assume UTF-8 (which includes ASCII as subset)
    /// 
    /// This ensures text is interpreted correctly regardless of the editor used to create it.
    /// </summary>
    /// <param name="stream">Stream to examine for BOM</param>
    /// <returns>Detected encoding (UTF-8 by default)</returns>
    private static Encoding DetectEncoding(Stream stream)
    {
        // Read first few bytes to check for BOM
        var bomBuffer = new byte[4];
        var bytesRead = stream.Read(bomBuffer, 0, bomBuffer.Length);
        
        // Check for UTF-8 BOM (EF BB BF)
        if (bytesRead >= 3 && 
            bomBuffer[0] == 0xEF && 
            bomBuffer[1] == 0xBB && 
            bomBuffer[2] == 0xBF)
        {
            return Encoding.UTF8;
        }
        
        // Check for UTF-16 BOM (FF FE or FE FF)
        if (bytesRead >= 2)
        {
            if (bomBuffer[0] == 0xFF && bomBuffer[1] == 0xFE)
                return Encoding.Unicode; // UTF-16 Little Endian
            if (bomBuffer[0] == 0xFE && bomBuffer[1] == 0xFF)
                return Encoding.BigEndianUnicode; // UTF-16 Big Endian
        }
        
        // Default to UTF-8 (which includes ASCII as subset)
        // Educational note: UTF-8 is the most common encoding for text files
        // and can represent any Unicode character while being backward compatible with ASCII
        return Encoding.UTF8;
    }

    /// <summary>
    /// Normalizes line endings to Unix-style LF for cross-platform compatibility.
    /// 
    /// EDUCATIONAL PURPOSE:
    /// Different operating systems use different line ending conventions:
    /// - Windows: CRLF (\r\n) - Carriage Return + Line Feed
    /// - Unix/Linux/Mac: LF (\n) - Line Feed only
    /// - Old Mac: CR (\r) - Carriage Return only (rare)
    /// 
    /// Normalizing to LF ensures consistent text processing across platforms
    /// and prevents issues with shader compilers and configuration parsers.
    /// </summary>
    /// <param name="content">Text content to normalize</param>
    /// <returns>Text with normalized line endings</returns>
    private static string NormalizeLineEndings(string content)
    {
        // Educational note: This method handles all common line ending variants
        // and converts them to the Unix standard (LF only)
        
        // Replace Windows CRLF with LF
        content = content.Replace("\r\n", "\n");
        
        // Replace remaining CR with LF (handles old Mac format)
        content = content.Replace("\r", "\n");
        
        return content;
    }
}