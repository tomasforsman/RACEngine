// ═══════════════════════════════════════════════════════════════════════════════
// AUDIO RESOURCE REPRESENTATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// The Sound class represents an individual audio resource within the RACEngine audio
// system. It encapsulates both the audio data and metadata necessary for playback,
// 3D positioning, and resource management.
//
// DESIGN PATTERNS:
// - Resource wrapper: Encapsulates OpenAL buffer and source management
// - Immutable data structure: Ensures thread-safe sharing between systems
// - Component pattern: Can be used as audio component in ECS architecture
//
// AUDIO CONCEPTS:
// - Buffer: Contains the actual audio data (PCM samples)
// - Source: Represents a playback instance with position, velocity, and gain
// - Format: Defines audio characteristics (mono/stereo, sample rate, bit depth)

using System;

namespace Rac.Audio;

/// <summary>
/// Represents an audio resource with OpenAL buffer and source management.
/// Provides both simple playback and advanced 3D audio positioning capabilities.
/// </summary>
public sealed class Sound : IDisposable
{
    // ───────────────────────────────────────────────────────────────────────────
    // CORE AUDIO PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Gets the unique identifier for this sound instance.</summary>
    public int Id { get; }

    /// <summary>Gets the file path of the audio resource.</summary>
    public string FilePath { get; }

    /// <summary>Gets the OpenAL buffer ID containing the audio data.</summary>
    public uint BufferId { get; }

    /// <summary>Gets the OpenAL source ID for playback control.</summary>
    public uint SourceId { get; }

    /// <summary>Gets whether this sound is configured for 3D positioning.</summary>
    public bool Is3D { get; }

    /// <summary>Gets whether this sound is currently disposed.</summary>
    public bool IsDisposed { get; private set; }

    // ───────────────────────────────────────────────────────────────────────────
    // AUDIO PLAYBACK STATE
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the playback volume (0.0 to 1.0).</summary>
    public float Volume { get; set; } = 1.0f;

    /// <summary>Gets or sets the pitch multiplier (0.5 to 2.0 typical range).</summary>
    public float Pitch { get; set; } = 1.0f;

    /// <summary>Gets or sets whether the sound should loop continuously.</summary>
    public bool IsLooping { get; set; }

    // ───────────────────────────────────────────────────────────────────────────
    // 3D SPATIAL AUDIO PROPERTIES
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the X position in 3D space.</summary>
    public float PositionX { get; set; }

    /// <summary>Gets or sets the Y position in 3D space.</summary>
    public float PositionY { get; set; }

    /// <summary>Gets or sets the Z position in 3D space.</summary>
    public float PositionZ { get; set; }

    // ───────────────────────────────────────────────────────────────────────────
    // CONSTRUCTION AND INITIALIZATION
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new Sound instance with OpenAL buffer and source IDs.
    /// </summary>
    /// <param name="id">Unique identifier for this sound instance.</param>
    /// <param name="filePath">Path to the audio file resource.</param>
    /// <param name="bufferId">OpenAL buffer ID containing audio data.</param>
    /// <param name="sourceId">OpenAL source ID for playback control.</param>
    /// <param name="is3D">Whether this sound supports 3D positioning.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when id is negative.</exception>
    public Sound(int id, string filePath, uint bufferId, uint sourceId, bool is3D = false)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "Sound ID cannot be negative");
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        Id = id;
        FilePath = filePath;
        BufferId = bufferId;
        SourceId = sourceId;
        Is3D = is3D;
    }

    // ───────────────────────────────────────────────────────────────────────────
    // SPATIAL AUDIO METHODS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the 3D position of this sound in world space.
    /// </summary>
    /// <param name="x">X coordinate in world space.</param>
    /// <param name="y">Y coordinate in world space.</param>
    /// <param name="z">Z coordinate in world space.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the sound is disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when sound is not configured for 3D.</exception>
    public void SetPosition(float x, float y, float z)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Sound));
        if (!Is3D)
            throw new InvalidOperationException("Cannot set position on non-3D sound");

        PositionX = x;
        PositionY = y;
        PositionZ = z;
    }

    // ───────────────────────────────────────────────────────────────────────────
    // RESOURCE MANAGEMENT AND DISPOSAL
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Releases all OpenAL resources associated with this sound.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns a string representation of this sound for debugging purposes.
    /// </summary>
    /// <returns>A string containing sound ID, file path, and 3D status.</returns>
    public override string ToString()
    {
        return $"Sound[Id={Id}, Path={FilePath}, 3D={Is3D}, Disposed={IsDisposed}]";
    }
}
