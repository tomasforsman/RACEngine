using System;

namespace Rac.Audio;

/// <summary>
/// Null Object pattern implementation of IAudioService.
/// Provides safe no-op audio functionality for testing and fallback scenarios.
/// </summary>
public class NullAudioService : IAudioService
{
#if DEBUG
    private static bool _warningShown = false;
    
    private static void ShowWarningOnce()
    {
        if (!_warningShown)
        {
            _warningShown = true;
            Console.WriteLine("[DEBUG] Warning: NullAudioService is being used - no audio will be played.");
        }
    }
#endif

    // Simple audio methods
    public void PlaySound(string soundPath)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // No-op: sound is not played
    }
    
    public void PlayMusic(string musicPath, bool loop = true)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // No-op: music is not played
    }
    
    public void StopAll()
    {
        // No-op: nothing to stop
    }
    
    public void SetMasterVolume(float volume)
    {
        // No-op: no volume to set
    }
    
    // Advanced audio methods
    public int PlaySound(string soundPath, float volume, float pitch = 1.0f, bool loop = false)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // Return dummy audio ID
        return -1;
    }
    
    public int PlaySound3D(string soundPath, float x, float y, float z, float volume = 1.0f)
    {
#if DEBUG
        ShowWarningOnce();
#endif
        // Return dummy audio ID
        return -1;
    }
    
    public void StopSound(int audioId)
    {
        // No-op: no sounds to stop
    }
    
    public void PauseSound(int audioId, bool paused)
    {
        // No-op: no sounds to pause
    }
    
    public void SetListener(float x, float y, float z, float forwardX, float forwardY, float forwardZ)
    {
        // No-op: no listener to set
    }
    
    public void UpdateSoundPosition(int audioId, float x, float y, float z)
    {
        // No-op: no sounds to update
    }
    
    public void SetSfxVolume(float volume)
    {
        // No-op: no volume to set
    }
    
    public void SetMusicVolume(float volume)
    {
        // No-op: no volume to set
    }
}