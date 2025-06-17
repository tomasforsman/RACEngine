// ═══════════════════════════════════════════════════════════════════════════════
// AUDIO VOLUME MANAGEMENT AND MIXING
// ═══════════════════════════════════════════════════════════════════════════════
//
// The AudioMixer class provides hierarchical volume control for different categories
// of audio content. This design pattern is commonly used in game engines to allow
// players to independently control music, sound effects, and master volume.
//
// AUDIO MIXING CONCEPTS:
// - Master Volume: Global multiplier affecting all audio output
// - Category Volumes: Independent controls for Music and SFX
// - Final Volume: Calculated as Master × Category × Individual volume
// - Logarithmic Scaling: Human perception of loudness is logarithmic, not linear
//
// DESIGN PATTERNS:
// - Singleton pattern: Single mixer instance manages all volume categories
// - Observer pattern: Volume changes can notify interested systems
// - Immutable operations: Volume calculations don't modify stored values

using System;

namespace Rac.Audio;

/// <summary>
/// Manages hierarchical volume control for different audio categories.
/// Provides master volume control and independent category volumes for music and sound effects.
/// </summary>
public sealed class AudioMixer
{
    // ───────────────────────────────────────────────────────────────────────────
    // VOLUME CATEGORIES AND CONSTANTS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Minimum allowed volume value (complete silence).</summary>
    public const float MinVolume = 0.0f;

    /// <summary>Maximum allowed volume value (full volume).</summary>
    public const float MaxVolume = 1.0f;

    /// <summary>Default volume for all categories.</summary>
    public const float DefaultVolume = 1.0f;

    // ───────────────────────────────────────────────────────────────────────────
    // PRIVATE VOLUME STORAGE
    // ───────────────────────────────────────────────────────────────────────────

    private float _masterVolume = DefaultVolume;
    private float _musicVolume = DefaultVolume;
    private float _sfxVolume = DefaultVolume;

    // ───────────────────────────────────────────────────────────────────────────
    // VOLUME PROPERTIES WITH VALIDATION
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the master volume affecting all audio output.
    /// Valid range is 0.0 (silence) to 1.0 (full volume).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside valid range.</exception>
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            ValidateVolumeRange(value, nameof(MasterVolume));
            _masterVolume = value;
            MasterVolumeChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Gets or sets the music category volume.
    /// Combined with master volume to determine final music playback volume.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside valid range.</exception>
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            ValidateVolumeRange(value, nameof(MusicVolume));
            _musicVolume = value;
            MusicVolumeChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Gets or sets the sound effects category volume.
    /// Combined with master volume to determine final SFX playback volume.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside valid range.</exception>
    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            ValidateVolumeRange(value, nameof(SfxVolume));
            _sfxVolume = value;
            SfxVolumeChanged?.Invoke(value);
        }
    }

    // ───────────────────────────────────────────────────────────────────────────
    // VOLUME CHANGE EVENTS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Fired when master volume changes. Parameter is the new volume value.</summary>
    public event Action<float>? MasterVolumeChanged;

    /// <summary>Fired when music volume changes. Parameter is the new volume value.</summary>
    public event Action<float>? MusicVolumeChanged;

    /// <summary>Fired when SFX volume changes. Parameter is the new volume value.</summary>
    public event Action<float>? SfxVolumeChanged;

    // ───────────────────────────────────────────────────────────────────────────
    // VOLUME CALCULATION METHODS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates the final volume for music playback.
    /// Combines master volume with music category volume and individual sound volume.
    /// </summary>
    /// <param name="individualVolume">The volume setting for the specific sound (0.0 to 1.0).</param>
    /// <returns>Final volume value for music playback.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when individualVolume is outside valid range.</exception>
    public float GetFinalMusicVolume(float individualVolume = DefaultVolume)
    {
        ValidateVolumeRange(individualVolume, nameof(individualVolume));
        return MasterVolume * MusicVolume * individualVolume;
    }

    /// <summary>
    /// Calculates the final volume for sound effects playback.
    /// Combines master volume with SFX category volume and individual sound volume.
    /// </summary>
    /// <param name="individualVolume">The volume setting for the specific sound (0.0 to 1.0).</param>
    /// <returns>Final volume value for SFX playback.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when individualVolume is outside valid range.</exception>
    public float GetFinalSfxVolume(float individualVolume = DefaultVolume)
    {
        ValidateVolumeRange(individualVolume, nameof(individualVolume));
        return MasterVolume * SfxVolume * individualVolume;
    }

    /// <summary>
    /// Calculates the final volume using master volume and individual sound volume only.
    /// Used for sounds that don't belong to music or SFX categories.
    /// </summary>
    /// <param name="individualVolume">The volume setting for the specific sound (0.0 to 1.0).</param>
    /// <returns>Final volume value for general playback.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when individualVolume is outside valid range.</exception>
    public float GetFinalVolume(float individualVolume = DefaultVolume)
    {
        ValidateVolumeRange(individualVolume, nameof(individualVolume));
        return MasterVolume * individualVolume;
    }

    // ───────────────────────────────────────────────────────────────────────────
    // UTILITY AND RESET METHODS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets all volume categories to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        MasterVolume = DefaultVolume;
        MusicVolume = DefaultVolume;
        SfxVolume = DefaultVolume;
    }

    /// <summary>
    /// Mutes all audio by setting master volume to zero.
    /// </summary>
    public void MuteAll()
    {
        MasterVolume = MinVolume;
    }

    /// <summary>
    /// Unmutes audio by restoring master volume to maximum.
    /// </summary>
    public void UnmuteAll()
    {
        MasterVolume = MaxVolume;
    }

    // ───────────────────────────────────────────────────────────────────────────
    // PRIVATE VALIDATION METHODS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that a volume value is within the acceptable range.
    /// </summary>
    /// <param name="volume">Volume value to validate.</param>
    /// <param name="parameterName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume is outside valid range.</exception>
    private static void ValidateVolumeRange(float volume, string parameterName)
    {
        if (float.IsNaN(volume) || volume < MinVolume || volume > MaxVolume)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                volume,
                $"Volume must be between {MinVolume} and {MaxVolume}");
        }
    }

    /// <summary>
    /// Returns a string representation of current mixer state for debugging.
    /// </summary>
    /// <returns>String containing all current volume levels.</returns>
    public override string ToString()
    {
        return $"AudioMixer[Master={MasterVolume:F2}, Music={MusicVolume:F2}, SFX={SfxVolume:F2}]";
    }
}
