// ═══════════════════════════════════════════════════════════════════════════════
// OPENGL SHADER PROGRAM MANAGEMENT
// ═══════════════════════════════════════════════════════════════════════════════
//
// This class manages the complete lifecycle of OpenGL shader programs, from source
// code compilation through GPU resource cleanup. It demonstrates proper OpenGL
// resource management and provides educational insight into the shader pipeline.
//
// GRAPHICS PROGRAMMING CONCEPTS:
// - Shader compilation: Converting GLSL source code to GPU machine code
// - Program linking: Combining vertex and fragment shaders into executable program
// - Resource lifecycle: Proper creation, usage, and cleanup of GPU resources
// - Error handling: Robust error reporting for shader development workflow
//
// OPENGL SHADER PIPELINE:
// 1. Create individual shaders (vertex, fragment)
// 2. Compile source code to GPU bytecode
// 3. Create shader program object
// 4. Attach compiled shaders to program
// 5. Link program (resolve inter-shader communication)
// 6. Delete individual shader objects (program retains compiled code)
// 7. Use program for rendering operations
//
// EDUCATIONAL VALUE:
// - Demonstrates modern OpenGL best practices
// - Shows proper resource management patterns
// - Illustrates graphics pipeline shader stages
// - Provides foundation for advanced shader techniques
//
// ═══════════════════════════════════════════════════════════════════════════════

using Silk.NET.OpenGL;

namespace Rac.Rendering.Shader;

/// <summary>
/// Manages OpenGL shader program lifecycle with proper resource management and error handling.
/// 
/// TECHNICAL PURPOSE:
/// - Compiles GLSL vertex and fragment shader source code to GPU executable
/// - Links individual shaders into complete rendering program
/// - Provides safe resource lifecycle management with deterministic cleanup
/// - Offers comprehensive error reporting for shader development workflow
/// 
/// GRAPHICS PIPELINE INTEGRATION:
/// - Represents complete shader program ready for GPU execution
/// - Handles vertex transformation and fragment coloring stages
/// - Enables custom rendering effects through GLSL programming
/// - Supports uniforms, attributes, and varying variables
/// 
/// RESOURCE MANAGEMENT:
/// - Implements IDisposable for deterministic GPU resource cleanup
/// - Automatically deletes temporary shader objects after linking
/// - Provides robust error handling during compilation and linking
/// - Ensures no GPU memory leaks through proper disposal pattern
/// </summary>
public class ShaderProgram : IDisposable
{
    // ═══════════════════════════════════════════════════════════════════════════
    // OPENGL RESOURCE HANDLES AND STATE
    // ═══════════════════════════════════════════════════════════════════════════
    
    private readonly GL _gl;
    private bool _disposed;

    /// <summary>OpenGL program handle for GPU resource identification</summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// Creates and links a complete shader program from vertex and fragment source code.
    /// 
    /// CONSTRUCTION PROCESS:
    /// 1. Compile vertex shader from GLSL source
    /// 2. Compile fragment shader from GLSL source  
    /// 3. Create OpenGL program object
    /// 4. Attach both compiled shaders
    /// 5. Link program (resolve variable bindings)
    /// 6. Validate linking success
    /// 7. Clean up individual shader objects
    /// 
    /// ERROR HANDLING:
    /// - Compilation errors include shader type and detailed info log
    /// - Linking errors provide program info log for debugging
    /// - All temporary resources cleaned up even on failure
    /// - Exceptions provide actionable error information
    /// </summary>
    /// <param name="gl">OpenGL context for resource creation</param>
    /// <param name="vertSrc">GLSL vertex shader source code</param>
    /// <param name="fragSrc">GLSL fragment shader source code</param>
    /// <exception cref="InvalidOperationException">Thrown on compilation or linking failure</exception>
    public ShaderProgram(GL gl, string vertSrc, string fragSrc)
    {
        _gl = gl;

        uint vs = 0;
        uint fs = 0;

        try
        {
            vs = Compile(ShaderType.VertexShader, vertSrc);
            fs = Compile(ShaderType.FragmentShader, fragSrc);

            Handle = _gl.CreateProgram();
            
            _gl.AttachShader(Handle, vs);
            _gl.AttachShader(Handle, fs);
            _gl.LinkProgram(Handle);

            _gl.GetProgram(Handle, GLEnum.LinkStatus, out var success);
            if (success == 0)
            {
                string infoLog = _gl.GetProgramInfoLog(Handle);
                throw new InvalidOperationException($"Shader linking failed: {infoLog}");
            }
        }
        finally
        {
            if (vs != 0) _gl.DeleteShader(vs);
            if (fs != 0) _gl.DeleteShader(fs);
        }
    }

    /// <summary>
    /// Activates this shader program for subsequent rendering operations.
    /// 
    /// GRAPHICS PIPELINE EFFECT:
    /// - Sets this program as the active shader for all draw calls
    /// - GPU will use this program's vertex and fragment stages
    /// - Remains active until another program is bound or 0 is used
    /// - Required before setting uniforms or issuing draw commands
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    // ───────────────────────────────────────────────────────────────────────────
    // SHADER COMPILATION IMPLEMENTATION
    // ───────────────────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Compiles individual shader source code to GPU executable format.
    /// 
    /// COMPILATION PROCESS:
    /// 1. Create shader object of specified type
    /// 2. Upload GLSL source code to GPU
    /// 3. Trigger compilation to GPU bytecode
    /// 4. Check compilation status and retrieve errors
    /// 5. Return compiled shader handle or throw exception
    /// 
    /// ERROR REPORTING:
    /// - Includes shader type (vertex/fragment) in error messages
    /// - Provides complete compiler info log for debugging
    /// - Automatically cleans up failed shader objects
    /// </summary>
    /// <param name="type">Shader type (vertex, fragment, etc.)</param>
    /// <param name="src">GLSL source code to compile</param>
    /// <returns>OpenGL shader handle for compiled shader</returns>
    /// <exception cref="InvalidOperationException">Thrown on compilation failure with info log</exception>
    private uint Compile(ShaderType type, string src)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, src);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, GLEnum.CompileStatus, out var success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            _gl.DeleteShader(shader);
            throw new InvalidOperationException($"Shader compilation failed ({type}): {infoLog}");
        }

        return shader;
    }

    /// <summary>
    /// Properly disposes OpenGL shader program resources.
    /// 
    /// RESOURCE CLEANUP:
    /// - Deletes GPU program object and frees VRAM
    /// - Marks object as disposed to prevent double-deletion
    /// - Suppresses finalizer since cleanup is complete
    /// - Follows standard .NET disposal pattern
    /// 
    /// GRAPHICS RESOURCE MANAGEMENT:
    /// - Essential for preventing GPU memory leaks
    /// - Should be called when program is no longer needed
    /// - Automatically called by using statements
    /// - Safe to call multiple times
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteProgram(Handle);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}