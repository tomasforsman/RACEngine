// ═══════════════════════════════════════════════════════════════════════════════
// OPENGL VERTEX ATTRIBUTE SPECIFICATION
// ═══════════════════════════════════════════════════════════════════════════════
//
// This record encapsulates a single vertex attribute specification for OpenGL
// vertex array configuration. It provides type-safe parameter passing to
// glVertexAttribPointer with clear semantic meaning.
//
// OPENGL GRAPHICS CONCEPTS:
// - Vertex attributes: Individual data components passed to vertex shaders
// - Attribute index: Location binding in shader (layout location = X)
// - Data type specification: Tells GPU how to interpret raw memory bytes
// - Normalization: Automatic conversion from integer to float range [0,1] or [-1,1]
// - Memory offset: Byte position within vertex structure for data access
//
// COMMON ATTRIBUTE EXAMPLES:
// - Position: Index=0, Size=2/3, Type=Float, Normalized=false (world coordinates)
// - Color: Index=1, Size=4, Type=Float, Normalized=false (RGBA values)
// - TexCoord: Index=2, Size=2, Type=Float, Normalized=false (UV mapping)
// - Normal: Index=3, Size=3, Type=Float, Normalized=true (lighting vectors)
//
// ═══════════════════════════════════════════════════════════════════════════════

using Silk.NET.OpenGL;

namespace Rac.Rendering;

/// <summary>
/// Defines a single vertex attribute specification for OpenGL vertex array configuration.
/// 
/// TECHNICAL PURPOSE:
/// - Encapsulates parameters for glVertexAttribPointer OpenGL function
/// - Enables type-safe vertex attribute specification with compile-time validation
/// - Provides clear semantic meaning for vertex buffer layout configuration
/// - Supports automatic vertex array object (VAO) setup in rendering pipeline
/// 
/// GRAPHICS PIPELINE INTEGRATION:
/// - Maps directly to OpenGL vertex attribute configuration
/// - Used by vertex layout system for automatic GPU state setup
/// - Enables flexible vertex formats without hardcoded attribute bindings
/// - Supports both simple and complex vertex structures
/// 
/// PARAMETER SPECIFICATIONS:
/// - Index: Shader attribute location (layout location = X in GLSL)
/// - Size: Number of components (2 for Vec2, 3 for Vec3, 4 for Vec4/Color)
/// - Type: OpenGL data type (Float, UnsignedByte, etc.)
/// - Normalized: Auto-convert integers to normalized float range
/// - Offset: Byte position within vertex structure for this attribute
/// </summary>
/// <param name="Index">Vertex attribute location index for shader binding</param>
/// <param name="Size">Number of components per attribute (1-4)</param>
/// <param name="Type">OpenGL data type for attribute interpretation</param>
/// <param name="Normalized">Whether to normalize integer values to [0,1] or [-1,1] range</param>
/// <param name="Offset">Byte offset within vertex structure where this attribute begins</param>
public record VertexAttribute(
    uint Index,
    int Size,
    VertexAttribPointerType Type,
    bool Normalized,
    int Offset
);