# RACEngine Audio Integration Guide

This guide demonstrates how to use the RACEngine audio system in your game projects, with examples from the ShooterSample.

## Overview

RACEngine provides a comprehensive audio system through the `IAudioService` interface, supporting:
- Simple sound effects and music playback
- Advanced 3D positional audio
- Volume management with master/music/SFX categories
- OpenAL-based implementation with automatic fallback to NullAudioService

## Basic Usage

### Accessing the Audio Service

The audio service is available through the engine facade:

```csharp
// Access audio service from your game code
var audioService = engineFacade.Audio;

// Play a simple sound effect
audioService.PlaySound("path/to/sound.ogg");

// Play background music with looping
audioService.PlayMusic("path/to/music.ogg", loop: true);
```

### Volume Management

```csharp
// Set master volume (affects all audio)
audioService.SetMasterVolume(0.8f);

// Set category-specific volumes
audioService.SetMusicVolume(0.6f);  // Background music
audioService.SetSfxVolume(0.9f);    // Sound effects
```

## ShooterSample Integration Example

Here's how you can add audio to the ShooterSample:

### 1. Add Audio Files

First, add audio files to your project:

```
samples/SampleGame/
├── audio/
│   ├── shoot.ogg          # Shooting sound effect
│   ├── background.ogg     # Background music  
│   └── explosion.ogg      # Bullet impact sound
```

### 2. Initialize Audio in ShooterSample

Add audio initialization to the `Run` method:

```csharp
public static void Run(string[] args)
{
    // ... existing engine initialization ...

    // ═══════════════════════════════════════════════════════════════════════════
    // AUDIO SYSTEM SETUP
    // ═══════════════════════════════════════════════════════════════════════════
    
    // Configure audio volumes
    engineFacade.Audio.SetMasterVolume(0.8f);
    engineFacade.Audio.SetMusicVolume(0.4f);   // Keep music subtle
    engineFacade.Audio.SetSfxVolume(0.7f);     // Clear sound effects

    // Start background music
    engineFacade.Audio.PlayMusic("audio/background.ogg", loop: true);

    // ... existing event registration and engine start ...
}
```

### 3. Add Shooting Sound Effects

Modify the `SpawnBulletInCurrentDirection` method:

```csharp
private static void SpawnBulletInCurrentDirection()
{
    // ... existing bullet spawning logic ...

    // ─── Audio Feedback ─────────────────────────────────────────────────
    // Play shooting sound effect with slight volume and pitch variation
    // for more dynamic audio experience
    var volume = 0.6f + (Random.Shared.NextSingle() * 0.2f); // 0.6-0.8 range
    var pitch = 0.9f + (Random.Shared.NextSingle() * 0.2f);  // 0.9-1.1 range
    
    engineFacade!.Audio.PlaySound("audio/shoot.ogg", volume, pitch);
}
```

### 4. Add 3D Positional Audio Example

For advanced 3D audio effects, you can position sounds in space:

```csharp
private static void PlayPositionalExplosion(Vector2D<float> position)
{
    // Convert 2D position to 3D (Z=0 for 2D games)
    var soundId = engineFacade!.Audio.PlaySound3D(
        "audio/explosion.ogg", 
        position.X, position.Y, 0.0f,  // 3D position
        volume: 0.8f
    );
    
    // Update listener position to match player/camera
    engineFacade.Audio.SetListener(
        0.0f, 0.0f, 0.0f,        // Listener at origin
        0.0f, 0.0f, -1.0f        // Looking down negative Z-axis
    );
}
```

### 5. Add Audio Controls

Extend the input handling to include audio controls:

```csharp
private static void OnKeyPressed(Key key, KeyboardKeyState.KeyEvent keyEvent)
{
    if (keyEvent != KeyboardKeyState.KeyEvent.Pressed)
        return;

    switch (key)
    {
        // ... existing controls ...
        
        case Key.M: // Mute/unmute toggle
            ToggleMute();
            break;
            
        case Key.Plus:
        case Key.KeypadAdd:
            AdjustMasterVolume(0.1f);
            break;
            
        case Key.Minus:
        case Key.KeypadSubtract:
            AdjustMasterVolume(-0.1f);
            break;
    }
}

private static bool isMuted = false;
private static float volumeBeforeMute = 1.0f;

private static void ToggleMute()
{
    if (isMuted)
    {
        engineFacade!.Audio.SetMasterVolume(volumeBeforeMute);
        Console.WriteLine("🔊 Audio unmuted");
    }
    else
    {
        volumeBeforeMute = 1.0f; // Would need to track actual current volume
        engineFacade!.Audio.SetMasterVolume(0.0f);
        Console.WriteLine("🔇 Audio muted");
    }
    isMuted = !isMuted;
}

private static void AdjustMasterVolume(float delta)
{
    if (!isMuted)
    {
        var currentVolume = Math.Clamp(volumeBeforeMute + delta, 0.0f, 1.0f);
        engineFacade!.Audio.SetMasterVolume(currentVolume);
        volumeBeforeMute = currentVolume;
        Console.WriteLine($"🔊 Master volume: {currentVolume:P0}");
    }
}
```

### 6. Update Console Output

Add audio controls to the educational guidance:

```csharp
Console.WriteLine("🎮 CONTROLS:");
Console.WriteLine("   WASD/Arrow Keys: Rotate ship direction");
Console.WriteLine("   Space: Toggle auto-fire mode");
Console.WriteLine("   V: Cycle through visual effect modes");
Console.WriteLine("   M: Toggle audio mute");
Console.WriteLine("   +/-: Adjust master volume");
```

## Advanced Audio Features

### Custom Audio Service Implementation

You can provide your own audio service implementation for special requirements:

```csharp
// In your engine initialization
var customAudioService = new OpenALAudioService();
engineFacade = new EngineFacade(windowManager, inputService, configManager)
{
    // Note: Current implementation uses NullAudioService by default
    // For custom service, you'd need to modify the facade or use dependency injection
};
```

### Audio Resource Management

For games with many sounds, consider preloading audio resources:

```csharp
// Preload sounds during initialization
var soundIds = new Dictionary<string, int>();

engineFacade.LoadEvent += () =>
{
    // Preload common sounds (implementation would need audio resource caching)
    Console.WriteLine("🎵 Loading audio resources...");
    
    // Note: Current OpenALAudioService loads on-demand
    // You might want to add a preloading mechanism for production games
};
```

## Audio File Format Support

The OpenAL implementation currently supports audio files through the Silk.NET.OpenAL package. For production use, you'll want to add support for common formats:

- **OGG Vorbis** (recommended) - Good compression, royalty-free
- **WAV** - Uncompressed, immediate playback
- **MP3** - Widely supported, but may require licensing

## Performance Considerations

1. **File Loading**: Audio files are loaded on-demand. Consider preloading for frequently used sounds.
2. **3D Audio**: Positional audio has additional CPU overhead but provides immersive experience.
3. **Simultaneous Sounds**: OpenAL can handle multiple concurrent sounds efficiently.
4. **Memory Usage**: Large audio files consume memory. Use compressed formats for music.

## Troubleshooting

### No Audio Output

1. Check if OpenAL-compatible audio device is available
2. Verify audio files exist and are in supported format  
3. Check volume levels (master, category, individual)
4. Look for NullAudioService fallback messages in debug output

### Audio Initialization Fails

The engine gracefully falls back to NullAudioService if OpenAL initialization fails:

```
[DEBUG] Warning: NullAudioService is being used - no audio will be played.
```

This ensures your game continues to work even without audio capabilities.

## Next Steps

1. **Experiment** with the ShooterSample audio integration
2. **Add your own audio files** and effects
3. **Try 3D positional audio** for immersive experiences
4. **Implement audio-driven gameplay** mechanics
5. **Optimize audio loading** for your specific game requirements

The RACEngine audio system provides a solid foundation for everything from simple 2D games to complex 3D audio experiences. Start simple and gradually add more sophisticated audio features as your game develops.