// ═══════════════════════════════════════════════════════════════════════════════
// File: AudioClip.cs
// Description: Asset type representing loaded audio data with metadata
// Educational Focus: Audio data representation and digital audio concepts
// ═══════════════════════════════════════════════════════════════════════════════
//
// EDUCATIONAL CONTENT:
// - Digital audio fundamentals (PCM, sample rate, bit depth)
// - Audio format considerations for game development
// - Memory vs. quality trade-offs in audio assets
// - Audio data organization for efficient playback
//
// TECHNICAL DETAILS:
// - Raw PCM audio data stored for direct audio API usage
// - Immutable design for thread-safe audio playback
// - Sample rate and channel information for proper playback
// - Duration calculation for audio timing systems
//
// AUDIO PIPELINE INTEGRATION:
// - Data format compatible with OpenAL and other audio APIs
// - Sample information for proper audio device configuration
// - Efficient data access for real-time audio streaming
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace Rac.Assets.Types;

/// <summary>
/// Represents a loaded audio clip with PCM audio data and metadata.
/// 
/// EDUCATIONAL PURPOSE:
/// This class demonstrates how game engines handle audio assets, including:
/// - PCM (Pulse Code Modulation) audio data representation
/// - Sample rate and channel configuration management
/// - Memory-efficient audio storage with proper disposal
/// - Duration calculation for audio timing systems
/// 
/// DIGITAL AUDIO FUNDAMENTALS:
/// - Sample Rate: Number of audio samples per second (e.g., 44100 Hz)
/// - Channels: Mono (1), Stereo (2), or Surround configurations
/// - Bit Depth: Bits per sample (8, 16, 24, 32) - affects quality and size
/// - PCM Format: Uncompressed linear audio data for direct playback
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Audio data can be large (44100 samples/sec × 2 bytes × 2 channels = 176KB/sec)
/// - Streaming vs. loading entire clips affects memory usage
/// - Consider audio compression for music, keep SFX uncompressed
/// - Disposal prevents memory leaks in audio-heavy applications
/// </summary>
public sealed class AudioClip : IDisposable
{
    /// <summary>
    /// Gets the raw PCM audio data.
    /// Educational note: PCM (Pulse Code Modulation) represents audio as linear samples.
    /// This is the format most audio APIs expect for direct playback.
    /// </summary>
    public byte[] AudioData { get; }

    /// <summary>
    /// Gets the sample rate in Hz (samples per second).
    /// Common values: 22050 (low quality), 44100 (CD quality), 48000 (professional).
    /// Educational note: Higher sample rates provide better quality but use more memory.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the number of audio channels.
    /// 1 = Mono, 2 = Stereo, 6 = 5.1 Surround, etc.
    /// Educational note: Stereo doubles memory usage but provides spatial audio.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the bits per sample (bit depth).
    /// Common values: 8, 16 (most common), 24, 32.
    /// Educational note: Higher bit depth provides better dynamic range and quality.
    /// </summary>
    public int BitsPerSample { get; }

    /// <summary>
    /// Gets the duration of the audio clip in seconds.
    /// Calculated from data length, sample rate, channels, and bit depth.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Gets the source file path this audio clip was loaded from.
    /// Useful for debugging and asset pipeline optimization.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Gets the audio format description (e.g., "WAV", "OGG").
    /// </summary>
    public string Format { get; }

    private bool _disposed;

    /// <summary>
    /// Creates a new audio clip instance with the specified audio data and metadata.
    /// </summary>
    /// <param name="audioData">Raw PCM audio data</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="channels">Number of audio channels</param>
    /// <param name="bitsPerSample">Bits per audio sample</param>
    /// <param name="format">Audio format description</param>
    /// <param name="sourcePath">Source file path for debugging</param>
    /// <exception cref="ArgumentNullException">Thrown when audioData or format is null</exception>
    /// <exception cref="ArgumentException">Thrown when audio parameters are invalid</exception>
    public AudioClip(byte[] audioData, int sampleRate, int channels, int bitsPerSample, string format, string sourcePath)
    {
        AudioData = audioData ?? throw new ArgumentNullException(nameof(audioData));
        SampleRate = sampleRate > 0 ? sampleRate : throw new ArgumentException("Sample rate must be positive", nameof(sampleRate));
        Channels = channels > 0 ? channels : throw new ArgumentException("Channels must be positive", nameof(channels));
        BitsPerSample = bitsPerSample > 0 ? bitsPerSample : throw new ArgumentException("Bits per sample must be positive", nameof(bitsPerSample));
        Format = format ?? throw new ArgumentException("Format cannot be null", nameof(format));
        SourcePath = sourcePath ?? "";

        // Calculate duration from audio data parameters
        // Formula: Duration = DataLength / (SampleRate × Channels × BytesPerSample)
        var bytesPerSample = bitsPerSample / 8;
        var totalSamples = audioData.Length / (channels * bytesPerSample);
        Duration = (float)totalSamples / sampleRate;
    }

    /// <summary>
    /// Releases the audio clip resources.
    /// Educational note: Proper disposal is critical to prevent memory leaks in audio systems.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // In a full implementation, this might also release audio buffer handles
            // For now, we rely on garbage collection for the byte array
            _disposed = true;
        }
    }

    /// <summary>
    /// Gets the total memory size of this audio clip in bytes.
    /// Educational note: Useful for audio memory profiling and optimization.
    /// </summary>
    public int MemorySize => AudioData.Length;

    /// <summary>
    /// Gets the number of audio samples in this clip.
    /// Educational note: Total samples = Data length / (Channels × Bytes per sample)
    /// </summary>
    public int SampleCount => AudioData.Length / (Channels * (BitsPerSample / 8));

    /// <summary>
    /// Gets a string representation of this audio clip for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"AudioClip({Duration:F2}s, {SampleRate}Hz, {Channels}ch, {BitsPerSample}bit, {MemorySize / 1024}KB, {SourcePath})";
    }
}