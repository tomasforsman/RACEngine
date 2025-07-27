// ═══════════════════════════════════════════════════════════════════════════════
// OPENAL AUDIO SERVICE IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// OpenAL-based implementation of the IAudioService interface providing complete
// audio functionality for the RACEngine. Supports both simple audio playback
// and advanced 3D positional audio with comprehensive resource management.
//
// OPENAL CONCEPTS:
// - Device: Represents audio hardware (speakers, headphones)
// - Context: Audio processing context (similar to OpenGL context)
// - Buffers: Store audio data (PCM samples)
// - Sources: Playback instances with position, velocity, and gain
// - Listener: Represents the "ears" in 3D space
//
// AUDIO PIPELINE:
// 1. Load audio files into OpenAL buffers
// 2. Create sources for playback instances
// 3. Bind buffers to sources and configure properties
// 4. Play, pause, stop, and manage source states
// 5. Clean up resources on disposal
//
// THREAD SAFETY:
// - OpenAL contexts are thread-safe for most operations
// - Resource management uses concurrent collections
// - Disposal pattern ensures safe cleanup

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Silk.NET.OpenAL;

namespace Rac.Audio;

/// <summary>
/// OpenAL implementation of IAudioService providing comprehensive audio functionality.
/// Supports both simple audio playback and advanced 3D positional audio.
/// </summary>
public sealed class OpenALAudioService : IAudioService, IDisposable
{
    // ───────────────────────────────────────────────────────────────────────────
    // OPENAL CORE OBJECTS AND STATE
    // ───────────────────────────────────────────────────────────────────────────

    private readonly AL _al;
    private readonly ALContext _alc;
    private readonly unsafe Device* _device;
    private readonly unsafe Context* _context;
    private readonly AudioMixer _mixer;

    // ───────────────────────────────────────────────────────────────────────────
    // RESOURCE MANAGEMENT COLLECTIONS
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Thread-safe collection of all active sound instances.</summary>
    private readonly ConcurrentDictionary<int, Sound> _activeSounds;

    /// <summary>Thread-safe collection mapping file paths to OpenAL buffer IDs.</summary>
    private readonly ConcurrentDictionary<string, uint> _audioBuffers;

    /// <summary>Collection of all allocated OpenAL source IDs for cleanup.</summary>
    private readonly List<uint> _allocatedSources;

    // ───────────────────────────────────────────────────────────────────────────
    // DISPOSAL AND STATE TRACKING
    // ───────────────────────────────────────────────────────────────────────────

    private volatile bool _disposed;
    private int _nextSoundId = 1;

    // ───────────────────────────────────────────────────────────────────────────
    // INITIALIZATION AND SETUP
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes the OpenAL audio service with default audio device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when OpenAL initialization fails.</exception>
    public unsafe OpenALAudioService()
    {
        try
        {
            // Initialize OpenAL context
            _al = AL.GetApi();
            _alc = ALContext.GetApi();

            // Open the default audio device
            _device = _alc.OpenDevice(string.Empty);
            if (_device == null)
            {
                Console.WriteLine("OpenAL Error: Failed to open OpenAL audio device.");
                throw new InvalidOperationException("Failed to open OpenAL audio device");
            }

            // Create audio context
            _context = _alc.CreateContext(_device, null);
            if (_context == null)
            {
                Console.WriteLine("OpenAL Error: Failed to create OpenAL context.");
                throw new InvalidOperationException("Failed to create OpenAL context");
            }

            // Make context current
            _alc.MakeContextCurrent(_context);
            Console.WriteLine("OpenAL: Context made current.");

            // Initialize collections
            _activeSounds = new ConcurrentDictionary<int, Sound>();
            _audioBuffers = new ConcurrentDictionary<string, uint>();
            _allocatedSources = new List<uint>();
            _mixer = new AudioMixer();

            // Set up default listener orientation (looking down negative Z-axis)
            SetListener(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenAL Initialization Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw new InvalidOperationException("Failed to initialize OpenAL audio service", ex);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SIMPLE AUDIO METHODS (IAudioService Implementation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Plays a sound effect once with default settings.
    /// </summary>
    /// <param name="soundPath">Path to the audio file to play.</param>
    /// <exception cref="ArgumentException">Thrown when soundPath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when audio file doesn't exist.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void PlaySound(string soundPath)
    {
        ValidateNotDisposed();
        if (string.IsNullOrWhiteSpace(soundPath))
            throw new ArgumentException("Sound path cannot be null or empty", nameof(soundPath));

        PlaySound(soundPath, _mixer.GetFinalSfxVolume(), 1.0f, false);
    }

    /// <summary>
    /// Plays background music with optional looping.
    /// </summary>
    /// <param name="musicPath">Path to the music file to play.</param>
    /// <param name="loop">Whether the music should loop continuously.</param>
    /// <exception cref="ArgumentException">Thrown when musicPath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when audio file doesn't exist.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void PlayMusic(string musicPath, bool loop = true)
    {
        ValidateNotDisposed();
        if (string.IsNullOrWhiteSpace(musicPath))
            throw new ArgumentException("Music path cannot be null or empty", nameof(musicPath));

        PlaySound(musicPath, _mixer.GetFinalMusicVolume(), 1.0f, loop);
    }

    /// <summary>
    /// Stops all currently playing audio.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void StopAll()
    {
        ValidateNotDisposed();

        foreach (var sound in _activeSounds.Values)
        {
            try
            {
                _al.SourceStop(sound.SourceId);
            }
            catch (Exception ex)
            {
                // Log error but continue stopping other sounds
                Console.WriteLine($"Warning: Failed to stop sound {sound.Id}: {ex.Message}");
            }
        }

        _activeSounds.Clear();
    }

    /// <summary>
    /// Sets the master volume affecting all audio output.
    /// </summary>
    /// <param name="volume">Master volume from 0.0 (silence) to 1.0 (full volume).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume is outside valid range.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void SetMasterVolume(float volume)
    {
        ValidateNotDisposed();
        _mixer.MasterVolume = volume;
        
        // Update all active sounds with new master volume
        UpdateAllSoundVolumes();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED AUDIO METHODS (IAudioService Implementation)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Plays a sound with advanced parameters and returns a unique audio ID.
    /// </summary>
    /// <param name="soundPath">Path to the audio file to play.</param>
    /// <param name="volume">Volume for this specific sound (0.0 to 1.0).</param>
    /// <param name="pitch">Pitch multiplier (0.5 to 2.0 typical range).</param>
    /// <param name="loop">Whether the sound should loop continuously.</param>
    /// <returns>Unique audio ID for controlling this sound instance.</returns>
    /// <exception cref="ArgumentException">Thrown when soundPath is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume or pitch is outside valid range.</exception>
    /// <exception cref="FileNotFoundException">Thrown when audio file doesn't exist.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public int PlaySound(string soundPath, float volume, float pitch = 1.0f, bool loop = false)
    {
        ValidateNotDisposed();
        ValidatePlaySoundParameters(soundPath, volume, pitch);

        try
        {
            // Load or get existing buffer
            var bufferId = LoadAudioBuffer(soundPath);

            // Create new source
            var sourceId = CreateSource();

            // Generate unique sound ID
            var soundId = Interlocked.Increment(ref _nextSoundId);

            // Create sound instance
            var sound = new Sound(soundId, soundPath, bufferId, sourceId, false)
            {
                Volume = volume,
                Pitch = pitch,
                IsLooping = loop
            };

            // Configure OpenAL source
            ConfigureSource(sound);

            // Start playback
            _al.SourcePlay(sourceId);

            // Store active sound
            _activeSounds[soundId] = sound;

            return soundId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to play sound '{soundPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Plays a 3D positioned sound with spatial audio.
    /// </summary>
    /// <param name="soundPath">Path to the audio file to play.</param>
    /// <param name="x">X position in 3D space.</param>
    /// <param name="y">Y position in 3D space.</param>
    /// <param name="z">Z position in 3D space.</param>
    /// <param name="volume">Volume for this specific sound (0.0 to 1.0).</param>
    /// <returns>Unique audio ID for controlling this sound instance.</returns>
    /// <exception cref="ArgumentException">Thrown when soundPath is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume is outside valid range.</exception>
    /// <exception cref="FileNotFoundException">Thrown when audio file doesn't exist.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public int PlaySound3D(string soundPath, float x, float y, float z, float volume = 1.0f)
    {
        ValidateNotDisposed();
        ValidatePlaySoundParameters(soundPath, volume, 1.0f);

        try
        {
            // Load or get existing buffer
            var bufferId = LoadAudioBuffer(soundPath);

            // Create new source
            var sourceId = CreateSource();

            // Generate unique sound ID
            var soundId = Interlocked.Increment(ref _nextSoundId);

            // Create 3D sound instance
            var sound = new Sound(soundId, soundPath, bufferId, sourceId, true)
            {
                Volume = volume,
                PositionX = x,
                PositionY = y,
                PositionZ = z
            };

            // Configure OpenAL source for 3D
            ConfigureSource(sound);

            // Start playback
            _al.SourcePlay(sourceId);

            // Store active sound
            _activeSounds[soundId] = sound;

            return soundId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to play 3D sound '{soundPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Stops a specific audio instance by its ID.
    /// </summary>
    /// <param name="audioId">The unique audio ID returned by PlaySound methods.</param>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void StopSound(int audioId)
    {
        ValidateNotDisposed();

        if (_activeSounds.TryRemove(audioId, out var sound))
        {
            try
            {
                _al.SourceStop(sound.SourceId);
                sound.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to stop sound {audioId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Pauses or resumes a specific audio instance.
    /// </summary>
    /// <param name="audioId">The unique audio ID returned by PlaySound methods.</param>
    /// <param name="paused">True to pause, false to resume.</param>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void PauseSound(int audioId, bool paused)
    {
        ValidateNotDisposed();

        if (_activeSounds.TryGetValue(audioId, out var sound))
        {
            try
            {
                if (paused)
                    _al.SourcePause(sound.SourceId);
                else
                    _al.SourcePlay(sound.SourceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to {(paused ? "pause" : "resume")} sound {audioId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Updates the 3D listener position and orientation.
    /// </summary>
    /// <param name="x">Listener X position.</param>
    /// <param name="y">Listener Y position.</param>
    /// <param name="z">Listener Z position.</param>
    /// <param name="forwardX">Forward direction X component.</param>
    /// <param name="forwardY">Forward direction Y component.</param>
    /// <param name="forwardZ">Forward direction Z component.</param>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public unsafe void SetListener(float x, float y, float z, float forwardX, float forwardY, float forwardZ)
    {
        ValidateNotDisposed();

        try
        {
            // Set listener position
            _al.SetListenerProperty(ListenerVector3.Position, x, y, z);

            // Set listener orientation (forward and up vectors)
            // Up vector is assumed to be (0, 1, 0) for standard Y-up coordinate system
            var orientation = stackalloc float[6] { forwardX, forwardY, forwardZ, 0.0f, 1.0f, 0.0f };
            _al.SetListenerProperty(ListenerFloatArray.Orientation, orientation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to set listener properties: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the 3D position of a specific sound.
    /// </summary>
    /// <param name="audioId">The unique audio ID returned by PlaySound methods.</param>
    /// <param name="x">New X position in 3D space.</param>
    /// <param name="y">New Y position in 3D space.</param>
    /// <param name="z">New Z position in 3D space.</param>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void UpdateSoundPosition(int audioId, float x, float y, float z)
    {
        ValidateNotDisposed();

        if (_activeSounds.TryGetValue(audioId, out var sound) && sound.Is3D)
        {
            try
            {
                sound.SetPosition(x, y, z);
                _al.SetSourceProperty(sound.SourceId, SourceVector3.Position, x, y, z);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to update position for sound {audioId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sets the volume for the sound effects category.
    /// </summary>
    /// <param name="volume">SFX category volume from 0.0 to 1.0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume is outside valid range.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void SetSfxVolume(float volume)
    {
        ValidateNotDisposed();
        _mixer.SfxVolume = volume;
        UpdateAllSoundVolumes();
    }

    /// <summary>
    /// Sets the volume for the music category.
    /// </summary>
    /// <param name="volume">Music category volume from 0.0 to 1.0.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when volume is outside valid range.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    public void SetMusicVolume(float volume)
    {
        ValidateNotDisposed();
        _mixer.MusicVolume = volume;
        UpdateAllSoundVolumes();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE IMPLEMENTATION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads an audio file into an OpenAL buffer, caching for reuse.
    /// </summary>
    private uint LoadAudioBuffer(string filePath)
    {
        // Return cached buffer if already loaded
        if (_audioBuffers.TryGetValue(filePath, out var existingBufferId))
            return existingBufferId;

        // Load and decode WAV data
        var (data, sampleRate, format) = LoadWav(filePath);

        // Generate buffer and upload data
        var bufferId = _al.GenBuffer();
        _al.BufferData(bufferId, format, data, sampleRate);

        _audioBuffers[filePath] = bufferId;
        return bufferId;
    }

    /// <summary>
    /// Loads and decodes a WAV file.
    /// </summary>
    /// <returns>Tuple of audio data (byte array), sample rate (int), and OpenAL format (BufferFormat).</returns>
    /// <exception cref="FormatException">Thrown if the WAV file is invalid or unsupported.</exception>
    private (byte[] Data, int SampleRate, BufferFormat Format) LoadWav(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // RIFF header
        var riffId = new string(reader.ReadChars(4));
        if (riffId != "RIFF") throw new FormatException("Not a WAV file (missing RIFF header)");
        reader.ReadInt32(); // File size

        var waveId = new string(reader.ReadChars(4));
        if (waveId != "WAVE") throw new FormatException("Not a WAV file (missing WAVE header)");

        // FMT sub-chunk
        var fmtId = new string(reader.ReadChars(4));
        if (fmtId != "fmt ") throw new FormatException("Not a WAV file (missing fmt sub-chunk)");
        var fmtSize = reader.ReadInt32(); // Format chunk size
        if (fmtSize < 16) throw new FormatException("WAV fmt chunk too small");

        var audioFormat = reader.ReadInt16(); // Audio format (1 for PCM)
        if (audioFormat != 1) throw new FormatException("Only PCM WAV files are supported");

        var channels = reader.ReadInt16();
        var sampleRate = reader.ReadInt32();
        reader.ReadInt32(); // Byte rate
        reader.ReadInt16(); // Block align
        var bitsPerSample = reader.ReadInt16();

        // Skip any extra format bytes
        if (fmtSize > 16)
        {
            reader.ReadBytes(fmtSize - 16);
        }

        // DATA sub-chunk
        var dataId = new string(reader.ReadChars(4));
        while (dataId != "data")
        {
            // Skip unknown chunks
            var chunkSize = reader.ReadInt32();
            reader.ReadBytes(chunkSize);
            dataId = new string(reader.ReadChars(4));
        }

        var dataSize = reader.ReadInt32();
        var data = reader.ReadBytes(dataSize);

        BufferFormat format;
        if (bitsPerSample == 16)
        {
            format = channels == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;
        }
        else if (bitsPerSample == 8)
        {
            format = channels == 1 ? BufferFormat.Mono8 : BufferFormat.Stereo8;
        }
        else
        {
            throw new FormatException($"Unsupported bits per sample: {bitsPerSample}");
        }

        return (data, sampleRate, format);
    }

    /// <summary>
    /// Creates a new OpenAL source for audio playback.
    /// </summary>
    private uint CreateSource()
    {
        var sourceId = _al.GenSource();
        _allocatedSources.Add(sourceId);
        return sourceId;
    }

    /// <summary>
    /// Configures an OpenAL source with properties from a Sound instance.
    /// </summary>
    private void ConfigureSource(Sound sound)
    {
        // Bind buffer to source
        _al.SetSourceProperty(sound.SourceId, SourceInteger.Buffer, (int)sound.BufferId);

        // Set basic properties
        _al.SetSourceProperty(sound.SourceId, SourceFloat.Gain, sound.Volume);
        _al.SetSourceProperty(sound.SourceId, SourceFloat.Pitch, sound.Pitch);
        _al.SetSourceProperty(sound.SourceId, SourceBoolean.Looping, sound.IsLooping);

        // Configure for 3D or 2D playback
        if (sound.Is3D)
        {
            _al.SetSourceProperty(sound.SourceId, SourceVector3.Position, 
                sound.PositionX, sound.PositionY, sound.PositionZ);
            _al.SetSourceProperty(sound.SourceId, SourceBoolean.SourceRelative, false);
        }
        else
        {
            // Non-3D sounds are relative to listener
            _al.SetSourceProperty(sound.SourceId, SourceBoolean.SourceRelative, true);
        }
    }

    /// <summary>
    /// Updates volume for all active sounds based on current mixer settings.
    /// </summary>
    private void UpdateAllSoundVolumes()
    {
        foreach (var sound in _activeSounds.Values)
        {
            try
            {
                var finalVolume = _mixer.GetFinalVolume(sound.Volume);
                _al.SetSourceProperty(sound.SourceId, SourceFloat.Gain, finalVolume);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to update volume for sound {sound.Id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Validates parameters for PlaySound methods.
    /// </summary>
    private static void ValidatePlaySoundParameters(string soundPath, float volume, float pitch)
    {
        if (string.IsNullOrWhiteSpace(soundPath))
            throw new ArgumentException("Sound path cannot be null or empty", nameof(soundPath));
        
        if (volume < 0.0f || volume > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "Volume must be between 0.0 and 1.0");
        
        if (pitch <= 0.0f)
            throw new ArgumentOutOfRangeException(nameof(pitch), pitch, "Pitch must be greater than 0.0");
    }

    /// <summary>
    /// Validates that the service has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when service is disposed.</exception>
    private void ValidateNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenALAudioService));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // RESOURCE DISPOSAL AND CLEANUP
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Releases all OpenAL resources and cleans up the audio service.
    /// </summary>
    public unsafe void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // Stop all active sounds
            StopAll();

            // Clean up all sources
            foreach (var sourceId in _allocatedSources)
            {
                try
                {
                    _al.DeleteSource(sourceId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete source {sourceId}: {ex.Message}");
                }
            }

            // Clean up all buffers
            foreach (var bufferId in _audioBuffers.Values)
            {
                try
                {
                    _al.DeleteBuffer(bufferId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete buffer {bufferId}: {ex.Message}");
                }
            }

            // Clean up OpenAL context and device
            _alc.MakeContextCurrent(null);
            
            if (_context != null)
                _alc.DestroyContext(_context);
            
            if (_device != null)
                _alc.CloseDevice(_device);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error during OpenAL cleanup: {ex.Message}");
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
