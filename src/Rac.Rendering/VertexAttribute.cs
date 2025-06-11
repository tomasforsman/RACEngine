using Silk.NET.OpenGL;

namespace Rac.Rendering;

/// <summary>
/// Defines a single vertex attribute specification
/// </summary>
public record VertexAttribute(
    uint Index,
    int Size,
    VertexAttribPointerType Type,
    bool Normalized,
    int Offset
);