---
title: "Rac.Core Project Documentation"
description: "Core engine functionality including mathematics, utilities, and cross-cutting concerns"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["core", "mathematics", "utilities", "foundation"]
---

# Rac.Core Project Documentation

## Overview

Rac.Core provides the foundational utilities and cross-cutting concerns for RACEngine. This module contains mathematical operations, data structures, extensions, and core abstractions that all other engine modules depend on.

## Prerequisites

- Understanding of C# generics and value types
- Basic knowledge of 3D mathematics concepts
- Familiarity with .NET extension methods
- [System Overview](../architecture/system-overview.md) for architectural context

## Project Structure

```
Rac.Core/
├── Math/
│   ├── Vector2D.cs          # 2D vector mathematics
│   ├── Vector3D.cs          # 3D vector mathematics
│   ├── Vector4D.cs          # 4D vector mathematics
│   ├── Matrix3x3.cs         # 3x3 matrix operations
│   ├── Matrix4x4.cs         # 4x4 matrix operations
│   ├── Quaternion.cs        # Rotation representation
│   ├── BoundingBox.cs       # Axis-aligned bounding boxes
│   ├── Plane.cs             # Geometric plane representation
│   └── MathHelper.cs        # Common mathematical utilities
├── Extensions/
│   ├── CollectionExtensions.cs    # Collection utility methods
│   ├── StringExtensions.cs        # String manipulation helpers
│   └── NumericExtensions.cs       # Numeric type extensions
├── DataStructures/
│   ├── SpatialHashGrid.cs         # Spatial partitioning
│   ├── ObjectPool.cs              # Object pooling system
│   ├── RingBuffer.cs              # Circular buffer implementation
│   └── LRUCache.cs                # Least Recently Used cache
└── Abstractions/
    ├── IUpdateable.cs             # Update pattern interface
    ├── IDisposableResource.cs     # Resource management
    └── ISerializable.cs           # Serialization interface
```

## Core Mathematics

### Vector Mathematics

The math system uses generic vectors for type safety and performance:

```csharp
/// <summary>
/// Generic 2D vector supporting multiple numeric types
/// Educational note: Generic implementation allows both float and double precision
/// Academic reference: "Mathematics for 3D Game Programming" (Eric Lengyel)
/// </summary>
public struct Vector2D<T> where T : struct, INumber<T>
{
    public T X { get; set; }
    public T Y { get; set; }
    
    /// <summary>
    /// Calculates the squared length of the vector
    /// Educational note: Squared length avoids expensive square root calculation
    /// Use when comparing distances: if (a.LengthSquared > b.LengthSquared)
    /// </summary>
    public T LengthSquared => X * X + Y * Y;
    
    /// <summary>
    /// Calculates the actual length of the vector
    /// Educational note: Only use when you need the actual distance value
    /// </summary>
    public T Length => T.Sqrt(LengthSquared);
    
    /// <summary>
    /// Returns a normalized version of the vector (length = 1)
    /// Educational note: Normalized vectors represent pure direction
    /// </summary>
    public Vector2D<T> Normalized()
    {
        var length = Length;
        if (length == T.Zero)
            return Zero;
        return new Vector2D<T>(X / length, Y / length);
    }
    
    /// <summary>
    /// Dot product operation
    /// Educational note: Dot product measures alignment between vectors
    /// Result > 0: vectors point in similar directions
    /// Result = 0: vectors are perpendicular
    /// Result < 0: vectors point in opposite directions
    /// </summary>
    public static T Dot(Vector2D<T> a, Vector2D<T> b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
    
    /// <summary>
    /// Linear interpolation between two vectors
    /// Educational note: Lerp is fundamental for smooth animation
    /// t = 0 returns 'from', t = 1 returns 'to'
    /// </summary>
    public static Vector2D<T> Lerp(Vector2D<T> from, Vector2D<T> to, T t)
    {
        return from + (to - from) * t;
    }
    
    // Common vector constants
    public static Vector2D<T> Zero => new(T.Zero, T.Zero);
    public static Vector2D<T> One => new(T.One, T.One);
    public static Vector2D<T> UnitX => new(T.One, T.Zero);
    public static Vector2D<T> UnitY => new(T.Zero, T.One);
}
```

### 3D Vector Operations

```csharp
/// <summary>
/// 3D vector with comprehensive geometric operations
/// Educational note: Essential for 3D graphics, physics, and spatial calculations
/// </summary>
public struct Vector3D<T> where T : struct, INumber<T>
{
    public T X { get; set; }
    public T Y { get; set; }
    public T Z { get; set; }
    
    /// <summary>
    /// Cross product operation - fundamental for 3D graphics
    /// Educational note: Cross product produces a vector perpendicular to both inputs
    /// Used for calculating surface normals, torque, and angular momentum
    /// Order matters: Cross(a, b) = -Cross(b, a)
    /// </summary>
    public static Vector3D<T> Cross(Vector3D<T> a, Vector3D<T> b)
    {
        return new Vector3D<T>(
            a.Y * b.Z - a.Z * b.Y,  // i component
            a.Z * b.X - a.X * b.Z,  // j component
            a.X * b.Y - a.Y * b.X   // k component
        );
    }
    
    /// <summary>
    /// Reflects a vector off a surface with given normal
    /// Educational note: Perfect reflection formula used in physics and graphics
    /// Formula: R = V - 2 * (V • N) * N
    /// Where V is incident vector, N is surface normal, R is reflected vector
    /// </summary>
    public static Vector3D<T> Reflect(Vector3D<T> incident, Vector3D<T> normal)
    {
        var dot = Dot(incident, normal);
        return incident - normal * (T.CreateChecked(2) * dot);
    }
    
    /// <summary>
    /// Projects vector 'a' onto vector 'b'
    /// Educational note: Vector projection finds the "shadow" of one vector on another
    /// Used in physics for force decomposition and graphics for lighting calculations
    /// </summary>
    public static Vector3D<T> Project(Vector3D<T> a, Vector3D<T> b)
    {
        var dotProduct = Dot(a, b);
        var bLengthSquared = b.LengthSquared;
        
        if (bLengthSquared == T.Zero)
            return Zero;
            
        return b * (dotProduct / bLengthSquared);
    }
    
    /// <summary>
    /// Spherical linear interpolation for smooth rotation
    /// Educational note: Slerp maintains constant angular velocity
    /// Better than Lerp for rotating objects or camera movement
    /// </summary>
    public static Vector3D<T> Slerp(Vector3D<T> from, Vector3D<T> to, T t)
    {
        var dot = Dot(from.Normalized(), to.Normalized());
        
        // If vectors are nearly parallel, use linear interpolation
        if (T.Abs(dot) > T.CreateChecked(0.9995))
            return Lerp(from, to, t).Normalized();
        
        var theta = T.Acos(T.Abs(dot));
        var sinTheta = T.Sin(theta);
        
        var a = T.Sin((T.One - t) * theta) / sinTheta;
        var b = T.Sin(t * theta) / sinTheta;
        
        return from * a + to * b;
    }
}
```

### Matrix Operations

```csharp
/// <summary>
/// 4x4 matrix for 3D transformations
/// Educational note: Matrices encode translation, rotation, and scale in a single structure
/// Homogeneous coordinates allow translation as matrix multiplication
/// </summary>
public struct Matrix4x4<T> where T : struct, INumber<T>
{
    private readonly T[,] _matrix;
    
    /// <summary>
    /// Creates a translation matrix
    /// Educational note: Translation matrix moves objects in 3D space
    /// Homogeneous coordinate system allows translation via matrix multiplication
    /// </summary>
    public static Matrix4x4<T> CreateTranslation(Vector3D<T> translation)
    {
        var result = Identity;
        result[0, 3] = translation.X;  // X translation
        result[1, 3] = translation.Y;  // Y translation
        result[2, 3] = translation.Z;  // Z translation
        return result;
    }
    
    /// <summary>
    /// Creates a rotation matrix around X axis
    /// Educational note: Rotation matrices preserve vector length and angles
    /// Right-hand rule: thumb points along axis, fingers curl in rotation direction
    /// </summary>
    public static Matrix4x4<T> CreateRotationX(T radians)
    {
        var cos = T.Cos(radians);
        var sin = T.Sin(radians);
        
        var result = Identity;
        result[1, 1] = cos;   result[1, 2] = -sin;
        result[2, 1] = sin;   result[2, 2] = cos;
        return result;
    }
    
    /// <summary>
    /// Creates a perspective projection matrix
    /// Educational note: Projects 3D world onto 2D screen with proper depth
    /// Field of view determines how "wide" the camera sees
    /// Near/far planes define the visible depth range
    /// </summary>
    public static Matrix4x4<T> CreatePerspective(T fieldOfView, T aspectRatio, T nearPlane, T farPlane)
    {
        var yScale = T.One / T.Tan(fieldOfView * T.CreateChecked(0.5));
        var xScale = yScale / aspectRatio;
        var depthRange = farPlane - nearPlane;
        
        var result = new Matrix4x4<T>();
        result[0, 0] = xScale;
        result[1, 1] = yScale;
        result[2, 2] = -(farPlane + nearPlane) / depthRange;
        result[2, 3] = -(T.CreateChecked(2) * farPlane * nearPlane) / depthRange;
        result[3, 2] = -T.One;
        
        return result;
    }
    
    /// <summary>
    /// Matrix multiplication - combines transformations
    /// Educational note: Order matters! Matrix multiplication is not commutative
    /// Transform order: Scale → Rotate → Translate (SRT)
    /// </summary>
    public static Matrix4x4<T> operator *(Matrix4x4<T> left, Matrix4x4<T> right)
    {
        var result = new Matrix4x4<T>();
        
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                T sum = T.Zero;
                for (int k = 0; k < 4; k++)
                {
                    sum += left[row, k] * right[k, col];
                }
                result[row, col] = sum;
            }
        }
        
        return result;
    }
}
```

### Quaternion Rotations

```csharp
/// <summary>
/// Quaternion for representing rotations without gimbal lock
/// Educational note: Quaternions provide smooth, efficient 3D rotations
/// Avoids gimbal lock problem inherent in Euler angles
/// Academic reference: "Quaternions and Rotation Sequences" (Jack Kuipers)
/// </summary>
public struct Quaternion<T> where T : struct, INumber<T>
{
    public T X { get; set; }  // i component
    public T Y { get; set; }  // j component
    public T Z { get; set; }  // k component
    public T W { get; set; }  // Real component
    
    /// <summary>
    /// Creates a quaternion from axis-angle representation
    /// Educational note: Axis-angle is intuitive for understanding rotations
    /// Axis defines rotation direction, angle defines amount of rotation
    /// </summary>
    public static Quaternion<T> FromAxisAngle(Vector3D<T> axis, T angle)
    {
        var halfAngle = angle * T.CreateChecked(0.5);
        var sin = T.Sin(halfAngle);
        var cos = T.Cos(halfAngle);
        
        var normalizedAxis = axis.Normalized();
        
        return new Quaternion<T>
        {
            X = normalizedAxis.X * sin,
            Y = normalizedAxis.Y * sin,
            Z = normalizedAxis.Z * sin,
            W = cos
        };
    }
    
    /// <summary>
    /// Spherical linear interpolation between quaternions
    /// Educational note: SLERP provides smooth rotation animation
    /// Maintains constant angular velocity between orientations
    /// Essential for character animation and camera movement
    /// </summary>
    public static Quaternion<T> Slerp(Quaternion<T> from, Quaternion<T> to, T t)
    {
        var dot = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;
        
        // If dot product is negative, take the shorter path
        if (dot < T.Zero)
        {
            to = new Quaternion<T> { X = -to.X, Y = -to.Y, Z = -to.Z, W = -to.W };
            dot = -dot;
        }
        
        T scale0, scale1;
        
        if (dot > T.CreateChecked(0.9995))
        {
            // Quaternions are very close, use linear interpolation
            scale0 = T.One - t;
            scale1 = t;
        }
        else
        {
            // Use spherical interpolation
            var theta = T.Acos(dot);
            var sinTheta = T.Sin(theta);
            
            scale0 = T.Sin((T.One - t) * theta) / sinTheta;
            scale1 = T.Sin(t * theta) / sinTheta;
        }
        
        return new Quaternion<T>
        {
            X = from.X * scale0 + to.X * scale1,
            Y = from.Y * scale0 + to.Y * scale1,
            Z = from.Z * scale0 + to.Z * scale1,
            W = from.W * scale0 + to.W * scale1
        };
    }
    
    /// <summary>
    /// Converts quaternion to rotation matrix
    /// Educational note: Matrices are needed for GPU vertex transformations
    /// Quaternions are better for interpolation, matrices for actual transformation
    /// </summary>
    public Matrix4x4<T> ToMatrix()
    {
        var xx = X * X;  var xy = X * Y;  var xz = X * Z;  var xw = X * W;
        var yy = Y * Y;  var yz = Y * Z;  var yw = Y * W;
        var zz = Z * Z;  var zw = Z * W;
        
        var result = Matrix4x4<T>.Identity;
        
        result[0, 0] = T.One - T.CreateChecked(2) * (yy + zz);
        result[0, 1] = T.CreateChecked(2) * (xy - zw);
        result[0, 2] = T.CreateChecked(2) * (xz + yw);
        
        result[1, 0] = T.CreateChecked(2) * (xy + zw);
        result[1, 1] = T.One - T.CreateChecked(2) * (xx + zz);
        result[1, 2] = T.CreateChecked(2) * (yz - xw);
        
        result[2, 0] = T.CreateChecked(2) * (xz - yw);
        result[2, 1] = T.CreateChecked(2) * (yz + xw);
        result[2, 2] = T.One - T.CreateChecked(2) * (xx + yy);
        
        return result;
    }
}
```

## Performance-Optimized Data Structures

### Spatial Hash Grid

```csharp
/// <summary>
/// Spatial hash grid for efficient spatial queries
/// Educational note: Reduces collision detection from O(n²) to approximately O(n)
/// Divides space into uniform grid cells for fast neighbor lookups
/// Academic reference: "Real-Time Collision Detection" (Christer Ericson)
/// </summary>
public class SpatialHashGrid<T>
{
    private readonly Dictionary<int, List<T>> _grid = new();
    private readonly float _cellSize;
    private readonly Func<T, Vector2D<float>> _getPosition;
    
    public SpatialHashGrid(float cellSize, Func<T, Vector2D<float>> getPosition)
    {
        _cellSize = cellSize;
        _getPosition = getPosition;
    }
    
    /// <summary>
    /// Inserts an object into the spatial grid
    /// Educational note: Hash function maps 2D coordinates to 1D grid cell index
    /// </summary>
    public void Insert(T item)
    {
        var position = _getPosition(item);
        var cellIndex = GetCellIndex(position);
        
        if (!_grid.TryGetValue(cellIndex, out var cell))
        {
            cell = new List<T>();
            _grid[cellIndex] = cell;
        }
        
        cell.Add(item);
    }
    
    /// <summary>
    /// Queries objects within a circular region
    /// Educational note: Only checks relevant grid cells, not all objects
    /// Massive performance improvement for large numbers of objects
    /// </summary>
    public IEnumerable<T> Query(Vector2D<float> center, float radius)
    {
        var radiusSquared = radius * radius;
        var cellRadius = (int)Math.Ceiling(radius / _cellSize);
        
        var centerCell = GetCellCoords(center);
        
        for (int x = centerCell.X - cellRadius; x <= centerCell.X + cellRadius; x++)
        {
            for (int y = centerCell.Y - cellRadius; y <= centerCell.Y + cellRadius; y++)
            {
                var cellIndex = HashCoords(x, y);
                
                if (_grid.TryGetValue(cellIndex, out var cell))
                {
                    foreach (var item in cell)
                    {
                        var itemPos = _getPosition(item);
                        var distanceSquared = (itemPos - center).LengthSquared;
                        
                        if (distanceSquared <= radiusSquared)
                            yield return item;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Hash function for converting 2D grid coordinates to 1D index
    /// Educational note: Good hash function minimizes collisions
    /// Uses large prime numbers to distribute values evenly
    /// </summary>
    private int HashCoords(int x, int y)
    {
        return x * 73856093 ^ y * 19349663;
    }
    
    private int GetCellIndex(Vector2D<float> position)
    {
        var coords = GetCellCoords(position);
        return HashCoords(coords.X, coords.Y);
    }
    
    private (int X, int Y) GetCellCoords(Vector2D<float> position)
    {
        return ((int)(position.X / _cellSize), (int)(position.Y / _cellSize));
    }
}
```

### Object Pool

```csharp
/// <summary>
/// Generic object pool for reducing garbage collection pressure
/// Educational note: Object pooling reuses objects instead of creating new ones
/// Critical for performance in real-time applications like games
/// Reduces GC pressure and allocation overhead
/// </summary>
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _resetAction;
    private readonly int _maxSize;
    
    public ObjectPool(int maxSize = 100, Func<T>? objectGenerator = null, Action<T>? resetAction = null)
    {
        _maxSize = maxSize;
        _objectGenerator = objectGenerator ?? (() => new T());
        _resetAction = resetAction ?? (_ => { });
    }
    
    /// <summary>
    /// Gets an object from the pool, creating if necessary
    /// Educational note: Amortizes allocation cost across many Get/Return cycles
    /// </summary>
    public T Get()
    {
        if (_objects.TryDequeue(out var item))
        {
            return item;
        }
        
        return _objectGenerator();
    }
    
    /// <summary>
    /// Returns an object to the pool for reuse
    /// Educational note: Reset action clears object state for next use
    /// </summary>
    public void Return(T item)
    {
        if (_objects.Count < _maxSize)
        {
            _resetAction(item);
            _objects.Enqueue(item);
        }
        // If pool is full, let object be garbage collected
    }
    
    /// <summary>
    /// Pre-fills the pool with objects
    /// Educational note: Pre-warming eliminates allocation spikes during gameplay
    /// </summary>
    public void PreFill(int count)
    {
        for (int i = 0; i < count && i < _maxSize; i++)
        {
            _objects.Enqueue(_objectGenerator());
        }
    }
}
```

## Extension Methods

### Collection Extensions

```csharp
/// <summary>
/// Extension methods for improved collection manipulation
/// Educational note: Extension methods add functionality without modifying original types
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Shuffles a list using Fisher-Yates algorithm
    /// Educational note: Unbiased shuffle algorithm with O(n) complexity
    /// Each permutation has equal probability of occurring
    /// </summary>
    public static void Shuffle<T>(this IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
    /// <summary>
    /// Removes items matching a predicate and returns count removed
    /// Educational note: More efficient than multiple Remove() calls
    /// Single pass through collection with efficient removal
    /// </summary>
    public static int RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
    {
        int writeIndex = 0;
        int removedCount = 0;
        
        for (int readIndex = 0; readIndex < list.Count; readIndex++)
        {
            if (predicate(list[readIndex]))
            {
                removedCount++;
            }
            else
            {
                if (writeIndex != readIndex)
                    list[writeIndex] = list[readIndex];
                writeIndex++;
            }
        }
        
        // Remove extra elements from end
        for (int i = list.Count - 1; i >= writeIndex; i--)
        {
            list.RemoveAt(i);
        }
        
        return removedCount;
    }
    
    /// <summary>
    /// Finds the element with the minimum value using a selector
    /// Educational note: Single pass algorithm more efficient than sorting
    /// </summary>
    public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector) 
        where TKey : IComparable<TKey>
    {
        using var enumerator = source.GetEnumerator();
        
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Sequence contains no elements");
        
        var min = enumerator.Current;
        var minKey = selector(min);
        
        while (enumerator.MoveNext())
        {
            var key = selector(enumerator.Current);
            if (key.CompareTo(minKey) < 0)
            {
                min = enumerator.Current;
                minKey = key;
            }
        }
        
        return min;
    }
}
```

## Math Helper Functions

```csharp
/// <summary>
/// Mathematical utility functions commonly used in game development
/// Educational note: These functions handle edge cases and provide consistent behavior
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps a value between minimum and maximum bounds
    /// Educational note: Essential for preventing values from going out of valid ranges
    /// Used extensively in graphics, physics, and user interface code
    /// </summary>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }
    
    /// <summary>
    /// Linear interpolation between two values
    /// Educational note: Fundamental operation for animation and gradual changes
    /// t = 0 returns 'from', t = 1 returns 'to', values between interpolate smoothly
    /// </summary>
    public static float Lerp(float from, float to, float t)
    {
        return from + (to - from) * t;
    }
    
    /// <summary>
    /// Inverse linear interpolation - finds t given from, to, and current value
    /// Educational note: Opposite of Lerp, useful for calculating progress percentages
    /// </summary>
    public static float InverseLerp(float from, float to, float value)
    {
        if (Math.Abs(to - from) < float.Epsilon)
            return 0.0f;
        return (value - from) / (to - from);
    }
    
    /// <summary>
    /// Smooth step function for eased interpolation
    /// Educational note: Provides smooth acceleration and deceleration
    /// Formula: 3t² - 2t³ where t is clamped to [0,1]
    /// Derivative is 0 at both ends, creating smooth transitions
    /// </summary>
    public static float SmoothStep(float from, float to, float t)
    {
        t = Clamp(t, 0.0f, 1.0f);
        t = t * t * (3.0f - 2.0f * t);
        return Lerp(from, to, t);
    }
    
    /// <summary>
    /// Converts degrees to radians
    /// Educational note: Most mathematical functions expect radians
    /// Formula: radians = degrees * π / 180
    /// </summary>
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180.0f);
    }
    
    /// <summary>
    /// Wraps an angle to the range [0, 2π)
    /// Educational note: Prevents angle accumulation issues in rotation calculations
    /// </summary>
    public static float WrapAngle(float angle)
    {
        while (angle >= 2.0f * MathF.PI)
            angle -= 2.0f * MathF.PI;
        while (angle < 0.0f)
            angle += 2.0f * MathF.PI;
        return angle;
    }
    
    /// <summary>
    /// Calculates the shortest angular distance between two angles
    /// Educational note: Handles angle wrapping for smooth rotation interpolation
    /// </summary>
    public static float DeltaAngle(float from, float to)
    {
        var delta = to - from;
        
        while (delta > MathF.PI)
            delta -= 2.0f * MathF.PI;
        while (delta < -MathF.PI)
            delta += 2.0f * MathF.PI;
            
        return delta;
    }
}
```

## Performance Considerations

### Memory Layout Optimization

```csharp
/// <summary>
/// Demonstrates memory-efficient struct design
/// Educational note: Struct layout affects cache performance
/// Sequential layout improves memory access patterns
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CacheOptimizedComponent
{
    // Group frequently accessed fields together
    public float X, Y, Z;          // Position (12 bytes)
    public float VelX, VelY, VelZ; // Velocity (12 bytes)
    public int EntityId;           // ID (4 bytes)
    public byte Flags;             // State flags (1 byte)
    
    // Total: 29 bytes - compiler may add padding to 32 bytes
    
    // Avoid: mixing small and large types randomly
    // Bad: float, byte, float, byte (causes padding)
    // Good: float, float, byte, byte (minimal padding)
}
```

### Allocation Reduction Patterns

```csharp
/// <summary>
/// Demonstrates zero-allocation iteration patterns
/// Educational note: Avoiding allocations reduces garbage collection pressure
/// </summary>
public static class AllocationOptimization
{
    /// <summary>
    /// Processes a list without creating intermediate collections
    /// Educational note: Streaming operations avoid temporary allocations
    /// </summary>
    public static void ProcessItemsEfficiently<T>(IEnumerable<T> items, Func<T, bool> filter, Action<T> processor)
    {
        // Bad: items.Where(filter).ToList().ForEach(processor);
        // Creates temporary list and delegates
        
        // Good: Single pass with no allocations
        foreach (var item in items)
        {
            if (filter(item))
                processor(item);
        }
    }
    
    /// <summary>
    /// Reuses StringBuilder to avoid string allocations
    /// Educational note: StringBuilder reuse dramatically reduces memory pressure
    /// </summary>
    private static readonly StringBuilder _sharedStringBuilder = new StringBuilder(1024);
    
    public static string FormatMessage(string template, params object[] args)
    {
        lock (_sharedStringBuilder)
        {
            _sharedStringBuilder.Clear();
            _sharedStringBuilder.AppendFormat(template, args);
            return _sharedStringBuilder.ToString();
        }
    }
}
```

## Integration with Other Modules

### ECS Integration

```csharp
/// <summary>
/// Transform component using Rac.Core math types
/// Educational note: Core math types provide the foundation for all engine components
/// </summary>
public readonly record struct TransformComponent(
    Vector3D<float> Position,
    Quaternion<float> Rotation,
    Vector3D<float> Scale
) : IComponent
{
    /// <summary>
    /// Creates the transformation matrix for this transform
    /// Educational note: SRT order (Scale, Rotate, Translate) is standard
    /// </summary>
    public Matrix4x4<float> ToMatrix()
    {
        var scaleMatrix = Matrix4x4<float>.CreateScale(Scale);
        var rotationMatrix = Rotation.ToMatrix();
        var translationMatrix = Matrix4x4<float>.CreateTranslation(Position);
        
        return scaleMatrix * rotationMatrix * translationMatrix;
    }
}
```

## See Also

- [System Overview](../architecture/system-overview.md) - Overall engine architecture
- [ECS Architecture](../architecture/ecs-architecture.md) - How Core integrates with ECS
- [Mathematics Reference](../educational-material/mathematics-reference.md) - Mathematical concepts explained
- [Performance Considerations](../architecture/performance-considerations.md) - Optimization strategies

## Changelog

- 2025-06-26: Comprehensive Rac.Core documentation with mathematical foundations, data structures, and performance optimization patterns