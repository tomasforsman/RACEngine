// ════════════════════════════════════════════════════════════════════════════════
// EDUCATIONAL SHADER PROGRAM MANAGEMENT
// ════════════════════════════════════════════════════════════════════════════════
//
// This class demonstrates fundamental concepts in modern graphics programming:
//
// 1. GRAPHICS SHADER PIPELINE:
//    - Vertex Shader: Transforms 3D vertices to 2D screen coordinates
//    - Fragment Shader: Computes final pixel colors and effects
//    - Program Linking: Connects vertex output to fragment input
//    - GPU Execution: Massively parallel processing on graphics hardware
//
// 2. GLSL (OpenGL Shading Language):
//    - C-like syntax for GPU programming
//    - Vertex shaders process vertices independently
//    - Fragment shaders process pixels/fragments independently
//    - Built-in variables and functions for graphics operations
//
// 3. SHADER COMPILATION PROCESS:
//    - Source code → Tokenization → AST → GPU machine code
//    - Just-in-time compilation on target GPU hardware
//    - Driver optimizations specific to GPU architecture
//    - Error reporting for syntax and semantic issues
//
// 4. OPENGL PROGRAM OBJECTS:
//    - Container for linked vertex + fragment shader pair
//    - GPU state machine activation via glUseProgram()
//    - Uniform variables for CPU→GPU parameter passing
//    - Attribute bindings for vertex data input
//
// 5. RESOURCE LIFECYCLE MANAGEMENT:
//    - GPU memory allocation for compiled shader code
//    - Explicit cleanup required (no automatic garbage collection)
//    - RAII pattern for exception-safe resource management
//    - Deterministic destruction via IDisposable pattern
//
// 6. ERROR HANDLING AND VALIDATION:
//    - Compilation error detection and reporting
//    - Link-time error validation
//    - Runtime shader debugging techniques
//    - Cross-platform compatibility considerations
//
// ════════════════════════════════════════════════════════════════════════════════

using Silk.NET.OpenGL;

namespace Rac.Rendering.Shader;

/// <summary>
/// Manages OpenGL shader program compilation, linking, and lifecycle.
///
/// SHADER PROGRAM ARCHITECTURE:
/// A shader program consists of multiple shader stages working together:
/// - Vertex Shader: Processes each vertex (position, normal, UV coordinates)
/// - Fragment Shader: Processes each pixel/fragment (color, lighting, effects)
/// - Optional: Geometry, Tessellation, Compute shaders (advanced features)
///
/// GPU EXECUTION MODEL:
/// - Vertex shaders run in parallel for all vertices in a draw call
/// - Rasterizer converts triangles to fragments (pixels) automatically
/// - Fragment shaders run in parallel for all covered pixels
/// - Results are written to framebuffer for display or further processing
///
/// PERFORMANCE CONSIDERATIONS:
/// - Shaders execute on hundreds/thousands of GPU cores simultaneously
/// - Branching (if/else) can reduce parallel efficiency
/// - Memory access patterns affect performance (texture cache, etc.)
/// - Compile-time optimizations by GPU driver are crucial
/// </summary>
public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private bool _disposed = false;

    /// <summary>
    /// Creates and links a complete shader program from vertex and fragment source.
    ///
    /// COMPILATION AND LINKING PIPELINE:
    /// 1. Compile vertex shader source to GPU bytecode
    /// 2. Compile fragment shader source to GPU bytecode
    /// 3. Create program object to contain both shaders
    /// 4. Link shaders together, resolving inter-stage connections
    /// 5. Validate program can execute on current GPU
    /// 6. Clean up intermediate shader objects (only program handle remains)
    ///
    /// LINKING PROCESS DETAILS:
    /// - Vertex shader outputs must match fragment shader inputs
    /// - Built-in variables (gl_Position, gl_FragColor) handled automatically
    /// - Custom variables declared with "out" in vertex, "in" in fragment
    /// - Unused variables are optimized away by driver
    /// - Link errors occur if interfaces don't match
    /// </summary>
    /// <param name="gl">OpenGL API context for all GPU operations</param>
    /// <param name="vertSrc">GLSL vertex shader source code</param>
    /// <param name="fragSrc">GLSL fragment shader source code</param>
    public ShaderProgram(GL gl, string vertSrc, string fragSrc)
    {
        _gl = gl;

        // ───────────────────────────────────────────────────────────────────────
        // INDIVIDUAL SHADER COMPILATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // VERTEX SHADER RESPONSIBILITIES:
        // - Transform vertex positions from model space to screen space
        // - Calculate per-vertex lighting (if using Gouraud shading)
        // - Pass texture coordinates and other data to fragment shader
        // - Set gl_Position built-in variable (required output)
        //
        // FRAGMENT SHADER RESPONSIBILITIES:
        // - Calculate final pixel color based on interpolated vertex data
        // - Apply textures, lighting, and visual effects
        // - Handle transparency and blending operations
        // - Set gl_FragColor or custom output variables

        uint vs = Compile(ShaderType.VertexShader, vertSrc);
        uint fs = Compile(ShaderType.FragmentShader, fragSrc);

        // ───────────────────────────────────────────────────────────────────────
        // PROGRAM OBJECT CREATION AND LINKING
        // ───────────────────────────────────────────────────────────────────────
        //
        // PROGRAM OBJECT PURPOSE:
        // - Container for multiple shader stages
        // - Manages inter-stage variable connections
        // - Provides interface for uniform variable access
        // - Can be activated for rendering via glUseProgram()

        Handle = _gl.CreateProgram();

        // Attach compiled shaders to program
        _gl.AttachShader(Handle, vs);
        _gl.AttachShader(Handle, fs);

        // ───────────────────────────────────────────────────────────────────────
        // LINKING PHASE
        // ───────────────────────────────────────────────────────────────────────
        //
        // LINKING PROCESS:
        // 1. Resolve vertex shader outputs → fragment shader inputs
        // 2. Optimize shader code for target GPU architecture
        // 3. Generate final GPU machine code
        // 4. Validate program meets OpenGL requirements
        // 5. Create symbol table for uniform/attribute access
        //
        // COMMON LINK ERRORS:
        // - Vertex output variable has no matching fragment input
        // - Fragment shader doesn't write to required output
        // - Too many uniform variables for GPU limits
        // - Incompatible variable types between stages

        _gl.LinkProgram(Handle);

        // ───────────────────────────────────────────────────────────────────────
        // LINKING STATUS VERIFICATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // ERROR DETECTION:
        // OpenGL uses query-based error reporting for asynchronous operations.
        // Linking may succeed/fail independently of API call success.
        // Must explicitly check link status to detect errors.

        _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            // ───────────────────────────────────────────────────────────────────
            // ERROR INFORMATION RETRIEVAL
            // ───────────────────────────────────────────────────────────────────
            //
            // LINKING ERROR LOG:
            // - Variable interface mismatches between stages
            // - Resource limit violations (uniforms, attributes)
            // - Platform-specific linking constraints
            // - GPU/driver-specific diagnostic information
            //
            // COMMON LINKING ERRORS:
            // - Vertex output variable has no matching fragment input
            // - Fragment shader doesn't write to required output
            // - Too many uniform variables for GPU limits
            // - Incompatible variable types between stages

            string infoLog = _gl.GetProgramInfoLog(Handle);

            // Clean up failed program and shader objects before throwing
            _gl.DeleteShader(vs);
            _gl.DeleteShader(fs);
            _gl.DeleteProgram(Handle);

            throw new InvalidOperationException($"Shader linking failed: {infoLog}");
        }

        // ───────────────────────────────────────────────────────────────────────
        // RESOURCE CLEANUP
        // ───────────────────────────────────────────────────────────────────────
        //
        // SHADER OBJECT LIFECYCLE:
        // After successful linking, individual shader objects are no longer needed.
        // Program object contains all necessary compiled code.
        // Deleting shader objects frees GPU memory and driver resources.
        //
        // IMPORTANT: This doesn't affect the linked program.
        // Similar to deleting .obj files after linking an executable.

        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
    }

    /// <summary>
    /// OpenGL handle for the compiled and linked shader program.
    ///
    /// HANDLE USAGE:
    /// - Pass to glUseProgram() to activate for rendering
    /// - Use with glGetUniformLocation() to access uniform variables
    /// - Required for all program-related OpenGL operations
    /// - Unique identifier within OpenGL context
    ///
    /// PROGRAM ACTIVATION:
    /// Only one shader program can be active at a time per OpenGL context.
    /// Switching programs has moderate cost; batch similar rendering operations.
    /// </summary>
    public uint Handle { get; }

    /// <summary>
    /// Releases GPU resources allocated for this shader program.
    ///
    /// RESOURCE MANAGEMENT IMPORTANCE:
    /// - GPU memory is limited and precious
    /// - Shader programs can be large (complex shaders = more GPU code)
    /// - Leaked programs can exhaust GPU memory over time
    /// - Driver may need to swap programs in/out of GPU cache
    ///
    /// DISPOSABLE PATTERN:
    /// Implements deterministic cleanup for unmanaged GPU resources.
    /// C# garbage collector cannot automatically clean up OpenGL objects.
    /// Use 'using' statements or explicit Dispose() calls for proper cleanup.
    /// </summary>
    public void Dispose()
    {
        // ───────────────────────────────────────────────────────────────────────
        // STANDARD IDISPOSABLE PATTERN IMPLEMENTATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // IDEMPOTENCY PROTECTION:
        // - Ensures Dispose() can be called multiple times safely
        // - Prevents duplicate resource deallocation attempts
        // - Follows .NET disposal pattern best practices
        // - Guards against OpenGL errors from invalid handle usage

        if (_disposed)
            return;

        // ───────────────────────────────────────────────────────────────────────
        // GPU RESOURCE DEALLOCATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // DELETION EFFECTS:
        // - Frees compiled shader bytecode from GPU memory
        // - Releases driver internal data structures
        // - Invalidates program handle (becomes unusable)
        // - May trigger GPU cache reorganization
        //
        // SAFETY NOTES:
        // - Program must not be currently active when deleted
        // - Any uniform locations become invalid after deletion

        _gl.DeleteProgram(Handle);
        _disposed = true;

        // Suppress finalizer since we've cleaned up resources explicitly
        GC.SuppressFinalize(this);

        // Note: Consider implementing finalizer for safety:
        // ~ShaderProgram() { Dispose(); }
        // However, finalizers have performance overhead and timing issues.
    }

    /// <summary>
    /// Compiles individual shader source code into GPU-executable form.
    ///
    /// COMPILATION PROCESS:
    /// 1. Parse GLSL source code into abstract syntax tree (AST)
    /// 2. Perform semantic analysis and type checking
    /// 3. Optimize shader code for target GPU architecture
    /// 4. Generate GPU-specific machine code or intermediate representation
    /// 5. Store compiled shader in GPU memory for linking
    ///
    /// GLSL LANGUAGE FEATURES:
    /// - Vector and matrix types (vec2, vec3, vec4, mat4, etc.)
    /// - Built-in functions (sin, cos, dot, cross, normalize, etc.)
    /// - Texture sampling functions (texture2D, textureCube, etc.)
    /// - Control flow (if/else, for loops, while loops)
    /// - User-defined functions and structures
    ///
    /// PERFORMANCE IMPLICATIONS:
    /// - Compilation happens at runtime on target GPU
    /// - First-time compilation can cause frame hitches
    /// - Modern drivers cache compiled shaders to disk
    /// - Complex shaders take longer to compile
    /// </summary>
    /// <param name="type">Shader stage type (vertex, fragment, geometry, etc.)</param>
    /// <param name="src">GLSL source code string</param>
    /// <returns>OpenGL shader object handle</returns>
    /// <exception cref="Exception">Thrown when compilation fails with error details</exception>
    private uint Compile(ShaderType type, string src)
    {
        // ───────────────────────────────────────────────────────────────────────
        // SHADER OBJECT CREATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // SHADER OBJECT PURPOSE:
        // - Container for source code and compiled bytecode
        // - Maintains compilation state and error information
        // - Can be attached to multiple program objects (reuse)
        // - Separate from program object (modular design)

        uint s = _gl.CreateShader(type);

        // ───────────────────────────────────────────────────────────────────────
        // SOURCE CODE ASSIGNMENT
        // ───────────────────────────────────────────────────────────────────────
        //
        // SOURCE HANDLING:
        // - OpenGL copies source string into internal storage
        // - Multiple source strings can be provided (for includes/modules)
        // - Source code is parsed during compilation, not here
        // - Original string can be discarded after this call

        _gl.ShaderSource(s, src);

        // ───────────────────────────────────────────────────────────────────────
        // COMPILATION EXECUTION
        // ───────────────────────────────────────────────────────────────────────
        //
        // COMPILATION PHASES:
        // 1. Lexical analysis: Source → tokens
        // 2. Syntax analysis: Tokens → AST
        // 3. Semantic analysis: Type checking, variable resolution
        // 4. Optimization: Dead code elimination, constant folding
        // 5. Code generation: AST → GPU machine code
        //
        // GPU-SPECIFIC OPTIMIZATIONS:
        // - Register allocation for shader variables
        // - Instruction scheduling for parallel execution
        // - Memory access pattern optimization
        // - Branch prediction and reduction

        _gl.CompileShader(s);

        // ───────────────────────────────────────────────────────────────────────
        // COMPILATION STATUS VERIFICATION
        // ───────────────────────────────────────────────────────────────────────
        //
        // ERROR DETECTION:
        // OpenGL uses query-based error reporting for asynchronous operations.
        // Compilation may succeed/fail independently of API call success.
        // Must explicitly check compilation status to detect errors.

        _gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
        if (ok == 0)
        {
            // ───────────────────────────────────────────────────────────────────
            // ERROR INFORMATION RETRIEVAL
            // ───────────────────────────────────────────────────────────────────
            //
            // COMPILATION ERROR LOG:
            // - Line numbers and column positions of errors
            // - Detailed error descriptions (syntax, type errors)
            // - Warning messages about potential issues
            // - GPU/driver-specific diagnostic information
            //
            // COMMON COMPILATION ERRORS:
            // - Syntax errors: Missing semicolons, unmatched braces
            // - Type errors: Incompatible variable assignments
            // - Undefined variables or functions
            // - Version directive issues (#version 330 core)
            // - GPU feature usage beyond capabilities

            string log = _gl.GetShaderInfoLog(s);

            // Clean up failed shader object before throwing
            _gl.DeleteShader(s);

            throw new Exception($"Shader compile error: {log}");
        }

        // Return handle to successfully compiled shader
        return s;

        // ───────────────────────────────────────────────────────────────────────
        // SHADER DEBUGGING TECHNIQUES
        // ───────────────────────────────────────────────────────────────────────
        //
        // DEBUGGING STRATEGIES:
        // 1. Use shader validation tools (RenderDoc, NVIDIA NSight)
        // 2. Output intermediate values as colors for visualization
        // 3. Simplify shaders by commenting out complex sections
        // 4. Check GLSL version compatibility (#version directive)
        // 5. Validate against different GPU vendors (NVIDIA, AMD, Intel)
        //
        // COMMON RUNTIME ISSUES:
        // - Precision differences between desktop/mobile GPUs
        // - Performance variations across GPU architectures
        // - Driver bug workarounds for specific hardware
        // - Memory bandwidth limitations on complex shaders
    }
}
