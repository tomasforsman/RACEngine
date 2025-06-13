namespace Rac.Audio;

/// <summary>
/// Audio service interface providing both simple and advanced audio functionality.
/// </summary>
public interface IAudioService
{
    // Simple audio methods
    /// <summary>Play a sound effect once.</summary>
    void PlaySound(string soundPath);
    
    /// <summary>Play background music with looping.</summary>
    void PlayMusic(string musicPath, bool loop = true);
    
    /// <summary>Stop all audio playback.</summary>
    void StopAll();
    
    /// <summary>Set master volume (0.0 to 1.0).</summary>
    void SetMasterVolume(float volume);
    
    // Advanced audio methods
    /// <summary>Play sound with advanced parameters.</summary>
    int PlaySound(string soundPath, float volume, float pitch = 1.0f, bool loop = false);
    
    /// <summary>Play 3D positioned sound.</summary>
    int PlaySound3D(string soundPath, float x, float y, float z, float volume = 1.0f);
    
    /// <summary>Stop specific audio instance.</summary>
    void StopSound(int audioId);
    
    /// <summary>Pause/resume specific audio instance.</summary>
    void PauseSound(int audioId, bool paused);
    
    /// <summary>Update 3D listener position and orientation.</summary>
    void SetListener(float x, float y, float z, float forwardX, float forwardY, float forwardZ);
    
    /// <summary>Update 3D sound position.</summary>
    void UpdateSoundPosition(int audioId, float x, float y, float z);
    
    /// <summary>Set sound effect volume category.</summary>
    void SetSfxVolume(float volume);
    
    /// <summary>Set music volume category.</summary>
    void SetMusicVolume(float volume);
}
