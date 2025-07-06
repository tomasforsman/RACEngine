// ═══════════════════════════════════════════════════════════════════════════════
// File: WavAudioLoader.cs
// Description: Asset loader for WAV audio files using manual RIFF/WAVE parsing
// Educational Focus: WAV format structure and digital audio concepts
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - WAV file format structure (RIFF header, format chunk, data chunk)
// - PCM audio representation and digital audio fundamentals
// - Binary file parsing techniques and endianness considerations
// - Audio quality vs. file size trade-offs in game development
//
// TECHNICAL IMPLEMENTATION:
// - Manual RIFF/WAVE parsing for educational transparency
// - Support for common PCM formats (8-bit, 16-bit mono/stereo)
// - Comprehensive validation and error reporting
// - Memory-efficient loading with proper resource management
//
// WAV FORMAT EDUCATION:
// - WAV uses RIFF (Resource Interchange File Format) container
// - Contains uncompressed PCM audio data
// - Larger files than compressed formats but no quality loss
// - Ideal for short sound effects in games (immediate playback)
//
// ═══════════════════════════════════════════════════════════════════════════════

using Rac.Assets.Types;
using System.Text;

namespace Rac.Assets.FileSystem.Loaders;

/// <summary>
/// Asset loader for WAV audio files with educational RIFF/WAVE format parsing.
/// 
/// EDUCATIONAL PURPOSE:
/// This loader demonstrates WAV format handling and digital audio concepts:
/// - RIFF/WAVE file structure and binary parsing techniques
/// - PCM audio representation and format validation
/// - Digital audio parameters (sample rate, bit depth, channels)
/// - Memory management for audio data in game engines
/// 
/// WAV FORMAT ADVANTAGES FOR GAMES:
/// - Uncompressed PCM audio for immediate playback (no decode overhead)
/// - Universal compatibility across platforms and audio libraries
/// - Simple format structure ideal for educational purposes
/// - Predictable memory usage and loading performance
/// 
/// DIGITAL AUDIO FUNDAMENTALS:
/// - Sample Rate: Audio samples per second (22050, 44100, 48000 Hz)
/// - Bit Depth: Bits per sample (8, 16, 24, 32) - affects quality and size
/// - Channels: Mono (1), Stereo (2), or multi-channel configurations
/// - PCM: Pulse Code Modulation - linear representation of audio amplitude
/// 
/// TECHNICAL IMPLEMENTATION:
/// - Manual RIFF parsing for educational transparency and control
/// - Validation of WAV format compliance and error reporting
/// - Support for common game audio formats (16-bit stereo at various sample rates)
/// - Memory-efficient loading with comprehensive error handling
/// </summary>
public class WavAudioLoader : IAssetLoader<AudioClip>
{
    /// <inheritdoc/>
    public string Description => "WAV Audio Loader with educational RIFF/WAVE parsing";

    /// <inheritdoc/>
    public IEnumerable<string> SupportedExtensions => new[] { ".wav", ".wave" };

    /// <summary>
    /// Determines if this loader can handle WAV audio files.
    /// Educational note: WAV files typically use .wav extension, sometimes .wave.
    /// </summary>
    /// <param name="extension">File extension to check</param>
    /// <returns>True if the extension is .wav or .wave (case-insensitive)</returns>
    public bool CanLoad(string extension)
    {
        return string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extension, ".wave", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads a WAV audio file from the provided stream and converts it to an AudioClip.
    /// 
    /// EDUCATIONAL IMPLEMENTATION:
    /// This method demonstrates complete WAV file parsing:
    /// 1. RIFF header validation (file format identification)
    /// 2. WAVE format chunk parsing (audio parameters)
    /// 3. Data chunk location and PCM audio extraction
    /// 4. Audio metadata calculation and validation
    /// 
    /// WAV FILE STRUCTURE (RIFF format):
    /// [RIFF Header] - File type and size information
    /// [Format Chunk] - Audio parameters (sample rate, channels, bit depth)
    /// [Data Chunk] - Raw PCM audio samples
    /// [Optional Chunks] - Additional metadata (ignored for basic playback)
    /// 
    /// PCM AUDIO REPRESENTATION:
    /// - Each audio sample represents amplitude at a specific time
    /// - 16-bit samples: -32768 to +32767 range (most common)
    /// - Stereo interleaving: [L, R, L, R, ...] sample pairs
    /// - Little-endian byte order in WAV format (LSB first)
    /// </summary>
    /// <param name="stream">Stream containing WAV audio data</param>
    /// <param name="path">File path for error reporting and debugging</param>
    /// <returns>Loaded audio clip with PCM audio data</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream or path is null</exception>
    /// <exception cref="FileFormatException">Thrown when WAV format is invalid or unsupported</exception>
    public AudioClip LoadFromStream(Stream stream, string path)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream), "Stream cannot be null");
        if (path == null)
            throw new ArgumentNullException(nameof(path), "Path cannot be null");

        try
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            
            // Parse RIFF header
            var riffHeader = ParseRiffHeader(reader, path);
            
            // Parse format chunk to get audio parameters
            var formatInfo = ParseFormatChunk(reader, path);
            
            // Find and parse data chunk
            var audioData = ParseDataChunk(reader, path, formatInfo);
            
            // Create and return AudioClip
            return new AudioClip(
                audioData, 
                formatInfo.SampleRate, 
                formatInfo.Channels, 
                formatInfo.BitsPerSample, 
                "WAV", 
                path);
        }
        catch (EndOfStreamException ex)
        {
            throw new FileFormatException($"WAV file '{path}' is truncated or corrupted: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is FileFormatException))
        {
            throw new FileFormatException($"Unexpected error loading WAV file '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses the RIFF header to validate WAV file format.
    /// Educational note: RIFF is a container format, WAVE identifies it as audio.
    /// </summary>
    private static RiffHeader ParseRiffHeader(BinaryReader reader, string path)
    {
        // Read RIFF signature (4 bytes: "RIFF")
        var riffSignature = reader.ReadBytes(4);
        if (Encoding.ASCII.GetString(riffSignature) != "RIFF")
        {
            throw new FileFormatException($"File '{path}' is not a valid RIFF file. Expected 'RIFF' signature.");
        }

        // Read file size (4 bytes, little-endian)
        // Educational note: This is the total file size minus 8 bytes (RIFF header)
        var fileSize = reader.ReadUInt32();

        // Read WAVE signature (4 bytes: "WAVE")
        var waveSignature = reader.ReadBytes(4);
        if (Encoding.ASCII.GetString(waveSignature) != "WAVE")
        {
            throw new FileFormatException($"File '{path}' is not a valid WAV file. Expected 'WAVE' format.");
        }

        return new RiffHeader { FileSize = fileSize };
    }

    /// <summary>
    /// Parses the format chunk to extract audio parameters.
    /// Educational note: Format chunk contains all information needed for audio playback.
    /// </summary>
    private static WavFormatInfo ParseFormatChunk(BinaryReader reader, string path)
    {
        // Find format chunk
        var formatChunk = FindChunk(reader, "fmt ", path);
        
        // Read audio format (2 bytes) - should be 1 for PCM
        var audioFormat = reader.ReadUInt16();
        if (audioFormat != 1)
        {
            throw new FileFormatException($"WAV file '{path}' uses unsupported audio format {audioFormat}. Only PCM (format 1) is supported.");
        }

        // Read audio parameters
        var channels = reader.ReadUInt16();
        var sampleRate = reader.ReadUInt32();
        var byteRate = reader.ReadUInt32(); // bytes per second
        var blockAlign = reader.ReadUInt16(); // bytes per sample frame
        var bitsPerSample = reader.ReadUInt16();

        // Validate parameters
        ValidateAudioParameters(channels, sampleRate, bitsPerSample, path);

        // Skip any remaining format chunk data
        var remainingFormatBytes = formatChunk.Size - 16;
        if (remainingFormatBytes > 0)
        {
            reader.ReadBytes((int)remainingFormatBytes);
        }

        return new WavFormatInfo
        {
            Channels = channels,
            SampleRate = (int)sampleRate,
            BitsPerSample = bitsPerSample,
            ByteRate = byteRate,
            BlockAlign = blockAlign
        };
    }

    /// <summary>
    /// Finds and parses the data chunk containing PCM audio samples.
    /// Educational note: Data chunk contains the raw audio samples for playback.
    /// </summary>
    private static byte[] ParseDataChunk(BinaryReader reader, string path, WavFormatInfo formatInfo)
    {
        var dataChunk = FindChunk(reader, "data", path);
        
        // Read PCM audio data
        var audioData = reader.ReadBytes((int)dataChunk.Size);
        if (audioData.Length != dataChunk.Size)
        {
            throw new FileFormatException($"WAV file '{path}' data chunk is truncated. Expected {dataChunk.Size} bytes, got {audioData.Length}.");
        }

        return audioData;
    }

    /// <summary>
    /// Finds a specific chunk in the WAV file by signature.
    /// Educational note: WAV files can contain multiple chunks; we search for specific ones.
    /// </summary>
    private static ChunkInfo FindChunk(BinaryReader reader, string chunkSignature, string path)
    {
        while (true)
        {
            // Read chunk signature (4 bytes)
            var signature = reader.ReadBytes(4);
            if (signature.Length < 4)
            {
                throw new FileFormatException($"WAV file '{path}' ended unexpectedly while searching for '{chunkSignature}' chunk.");
            }

            var signatureString = Encoding.ASCII.GetString(signature);
            var chunkSize = reader.ReadUInt32();

            if (signatureString == chunkSignature)
            {
                return new ChunkInfo { Signature = signatureString, Size = chunkSize };
            }

            // Skip this chunk and continue searching
            reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
        }
    }

    /// <summary>
    /// Validates audio parameters for game engine compatibility.
    /// Educational note: Games typically use specific audio formats for optimal performance.
    /// </summary>
    private static void ValidateAudioParameters(ushort channels, uint sampleRate, ushort bitsPerSample, string path)
    {
        if (channels == 0 || channels > 8)
        {
            throw new FileFormatException($"WAV file '{path}' has unsupported channel count: {channels}. Supported range: 1-8 channels.");
        }

        if (sampleRate < 8000 || sampleRate > 192000)
        {
            throw new FileFormatException($"WAV file '{path}' has unsupported sample rate: {sampleRate} Hz. Supported range: 8000-192000 Hz.");
        }

        if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
        {
            throw new FileFormatException($"WAV file '{path}' has unsupported bit depth: {bitsPerSample} bits. Supported: 8, 16, 24, 32 bits.");
        }
    }

    /// <summary>Helper structure for RIFF header information.</summary>
    private struct RiffHeader
    {
        public uint FileSize;
    }

    /// <summary>Helper structure for WAV format information.</summary>
    private struct WavFormatInfo
    {
        public int Channels;
        public int SampleRate;
        public ushort BitsPerSample;
        public uint ByteRate;
        public ushort BlockAlign;
    }

    /// <summary>Helper structure for chunk information.</summary>
    private struct ChunkInfo
    {
        public string Signature;
        public uint Size;
    }
}