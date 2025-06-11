namespace Rac.Rendering;

/// <summary>
/// Complete vertex layout specification for a vertex type
/// </summary>
public record VertexLayout(
    VertexAttribute[] Attributes,
    int Stride
);