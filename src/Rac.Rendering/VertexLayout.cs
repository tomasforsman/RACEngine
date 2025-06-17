// ═══════════════════════════════════════════════════════════════════════════════
// VERTEX LAYOUT SPECIFICATION SYSTEM
// ═══════════════════════════════════════════════════════════════════════════════
//
// This record defines the complete memory layout specification for vertex data
// structures used in the OpenGL rendering pipeline. It enables type-safe vertex
// attribute configuration with compile-time validation.
//
// GRAPHICS PROGRAMMING CONCEPTS:
// - Vertex attributes: Individual data components (position, color, texture, normals)
// - Memory stride: Total bytes per vertex for GPU buffer calculations
// - Layout specification: Describes how GPU interprets vertex buffer data
// - Type safety: Compile-time validation prevents runtime OpenGL errors
//
// USAGE PATTERN:
// 1. Define vertex structure with [StructLayout(LayoutKind.Sequential)]
// 2. Create VertexLayout describing the structure's memory layout
// 3. OpenGL renderer uses layout for glVertexAttribPointer configuration
// 4. GPU correctly interprets vertex buffer data based on layout
//
// ═══════════════════════════════════════════════════════════════════════════════

namespace Rac.Rendering;

/// <summary>
/// Complete vertex layout specification for a vertex type with GPU memory layout details.
/// 
/// TECHNICAL PURPOSE:
/// - Describes vertex attribute memory layout for OpenGL vertex array configuration
/// - Enables type-safe vertex buffer setup with compile-time validation
/// - Provides stride information for efficient GPU memory access patterns
/// - Supports multiple vertex formats through flexible attribute arrays
/// 
/// GRAPHICS PIPELINE INTEGRATION:
/// - Used by OpenGLRenderer for glVertexAttribPointer setup
/// - Ensures correct GPU interpretation of vertex buffer data
/// - Enables automatic vertex type detection and configuration
/// - Prevents common vertex attribute binding errors
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - Stride value enables GPU to efficiently skip between vertices
/// - Proper alignment reduces memory bandwidth requirements
/// - Attribute ordering affects cache efficiency during vertex processing
/// </summary>
/// <param name="Attributes">Array of vertex attribute specifications defining data layout</param>
/// <param name="Stride">Total size in bytes of a single vertex structure</param>
public record VertexLayout(
    VertexAttribute[] Attributes,
    int Stride
);