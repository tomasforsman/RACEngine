# Glow Shaders Implementation Documentation

## Overview

This document describes the implementation of glow shaders in the RACEngine rendering system. The implementation adds visual glow effects for specific game objects while maintaining backward compatibility with existing rendering code.

## Design Decisions

### 1. Shader Mode Architecture

**Decision**: Add a `ShaderMode` enum and `SetShaderMode` method to the `IRenderer` interface.

**Rationale**: 
- Maintains clear separation between color setting and visual effect selection
- Allows for easy extension with additional shader effects in the future
- Keeps the existing rendering API intact while adding new functionality

**Alternative Considered**: Adding glow flags to `SetColor` method, but this would mix concerns and be less extensible.

### 2. Multiple Shader Programs

**Decision**: Create separate `ShaderProgram` instances for each shader mode (Normal, SoftGlow, Bloom).

**Rationale**:
- OpenGL best practice is to compile shaders once and switch between programs
- Avoids runtime shader compilation overhead
- Allows for different uniform locations per shader if needed in the future

### 3. Triangle-Based Glow Effects

**Challenge**: The rendering system uses triangle primitives, not points, which limits glow effect options.

**Solution**: Implemented color-space glow effects using:
- **SoftGlow**: Moderate brightness boost (1.3x) with slight desaturation for softer appearance
- **Bloom**: Strong brightness boost (2.2x) with enhanced color saturation and white shift

**Alternative Considered**: Modifying vertex shaders to pass position data for radial falloff calculations, but this would require more significant changes to the vertex pipeline.

### 4. Additive Blending

**Decision**: Enable additive blending (`SrcAlpha + One`) for glow shader modes.

**Rationale**:
- Creates authentic glow appearance by allowing colors to accumulate
- Standard technique for glow/bloom effects in game rendering
- Automatically disabled for normal rendering to maintain existing appearance

## Implementation Details

### Shader Effects

#### Normal Shader
```glsl
fragColor = uColor;
```
Standard flat color rendering, no modifications.

#### SoftGlow Shader
```glsl
vec3 glowColor = baseColor * 1.3;                    // Moderate brightness boost
float luminance = dot(glowColor, vec3(0.299, 0.587, 0.114));
glowColor = mix(glowColor, vec3(luminance), 0.1);    // Slight desaturation
```
Creates a subtle glow effect suitable for ambient lighting or magical effects.

#### Bloom Shader
```glsl
vec3 bloomColor = baseColor * 2.2;                   // Strong brightness boost
bloomColor = mix(vec3(luminance), bloomColor, 1.4);  // Enhanced saturation
bloomColor = mix(bloomColor, vec3(1.0), 0.1);        // White shift
```
Creates an intense bloom effect suitable for bright lights, explosions, or special objects.

### OpenGL State Management

- **Blending**: Enabled with additive blending for glow modes, disabled for normal mode
- **Shader Programs**: Switched via `glUseProgram` calls based on current mode
- **Frame Reset**: Shader mode automatically resets to Normal at the start of each frame

### Integration Points

#### BoidSample Integration
Red boids automatically use the Bloom shader for high-impact visual distinction:
```csharp
if (filterId == "Red")
{
    engine.Renderer.SetShaderMode(ShaderMode.Bloom);
}
else
{
    engine.Renderer.SetShaderMode(ShaderMode.Normal);
}
```

## Usage Examples

### Basic Glow Effect
```csharp
renderer.SetShaderMode(ShaderMode.SoftGlow);
renderer.SetColor(new Vector4D<float>(1f, 0f, 0f, 1f));
renderer.UpdateVertices(vertices);
renderer.Draw();
```

### Intense Bloom Effect
```csharp
renderer.SetShaderMode(ShaderMode.Bloom);
renderer.SetColor(new Vector4D<float>(1f, 0f, 0f, 1f));
renderer.UpdateVertices(vertices);
renderer.Draw();
```

### Mixed Rendering
```csharp
// Draw normal objects
renderer.SetShaderMode(ShaderMode.Normal);
DrawNormalObjects();

// Draw glowing objects  
renderer.SetShaderMode(ShaderMode.SoftGlow);
DrawMagicalObjects();

// Draw bright effects
renderer.SetShaderMode(ShaderMode.Bloom);
DrawExplosions();
```

## Future Enhancements

### Possible Improvements
1. **Vertex Position-Based Effects**: Pass vertex positions to fragment shader for true radial falloff
2. **Multiple Render Passes**: Implement proper bloom with blur passes for more realistic effects
3. **Configurable Parameters**: Add uniforms for glow intensity, radius, and color shifts
4. **Animation Support**: Add time-based uniforms for animated glow effects

### Performance Considerations
- Current implementation adds minimal overhead (shader program switching only)
- Additive blending may affect fill rate on mobile devices
- Consider depth sorting for optimal blending results

## Testing

The implementation has been verified to:
- ✅ Compile successfully with .NET 8.0
- ✅ Maintain backward compatibility with existing rendering code
- ✅ Provide distinct visual effects for each shader mode
- ✅ Properly manage OpenGL state transitions

## Files Modified

- `src/Rac.Rendering/Shader/ShaderMode.cs` - New shader mode enumeration
- `src/Rac.Rendering/IRenderer.cs` - Added SetShaderMode method
- `src/Rac.Rendering/OpenGLRenderer.cs` - Core glow shader implementation
- `samples/SampleGame/BoidSample.cs` - Integration with boid rendering