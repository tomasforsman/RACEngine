// ═══════════════════════════════════════════════════════════════════════════════
// CAMERA SYSTEM TESTS
// ═══════════════════════════════════════════════════════════════════════════════
//
// Comprehensive test suite for the dual-camera system ensuring proper
// matrix generation, coordinate transformations, and rendering integration.
//
// TEST COVERAGE:
// - Camera matrix computation and caching
// - World-to-screen coordinate transformations
// - Screen-to-world coordinate transformations
// - Viewport resize handling
// - Camera property changes (position, zoom, rotation)
// - UI camera 1:1 pixel mapping
// - Game camera world transformations
//
// MATHEMATICAL VALIDATION:
// - Matrix multiplication order (Projection × View)
// - Orthographic projection correctness
// - Coordinate system transformations
// - Inverse matrix calculations

using Silk.NET.Maths;
using Xunit;
using Rac.Rendering.Camera;

namespace Rac.Rendering.Tests;

/// <summary>
/// Tests for camera system functionality including matrix generation and coordinate transformations.
/// </summary>
public class CameraSystemTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // GAME CAMERA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GameCamera_DefaultState_ShouldHaveIdentityTransformation()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);

        // Default camera should be at origin with 1.0 zoom and 0 rotation
        Assert.Equal(Vector2D<float>.Zero, camera.Position);
        Assert.Equal(0f, camera.Rotation);
        Assert.Equal(1f, camera.Zoom);
    }

    [Fact]
    public void GameCamera_PositionChange_ShouldUpdateMatrices()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);
        var originalMatrix = camera.CombinedMatrix;

        camera.Position = new Vector2D<float>(1f, 1f);
        
        // Matrix should be different after position change
        Assert.NotEqual(originalMatrix, camera.CombinedMatrix);
    }

    [Fact]
    public void GameCamera_ZoomChange_ShouldUpdateMatrices()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);
        var originalMatrix = camera.CombinedMatrix;

        camera.Zoom = 2f;
        
        // Matrix should be different after zoom change
        Assert.NotEqual(originalMatrix, camera.CombinedMatrix);
    }

    [Fact]
    public void GameCamera_RotationChange_ShouldUpdateMatrices()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);
        var originalMatrix = camera.CombinedMatrix;

        camera.Rotation = MathF.PI / 4f; // 45 degrees
        
        // Matrix should be different after rotation change
        Assert.NotEqual(originalMatrix, camera.CombinedMatrix);
    }

    [Fact]
    public void GameCamera_WorldToScreenToWorld_ShouldBeSymmetric()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);

        var originalWorldPos = new Vector2D<float>(1f, 0.5f);
        var screenPos = camera.WorldToScreen(originalWorldPos, 800, 600);
        var backToWorldPos = camera.ScreenToWorld(screenPos, 800, 600);

        // Should get back to original position (within floating point precision)
        Assert.True(Math.Abs(originalWorldPos.X - backToWorldPos.X) < 0.001f);
        Assert.True(Math.Abs(originalWorldPos.Y - backToWorldPos.Y) < 0.001f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UI CAMERA TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void UICamera_ViewMatrix_ShouldBeIdentity()
    {
        var camera = new UICamera();
        camera.UpdateMatrices(800, 600);

        // UI camera should have identity view matrix
        Assert.Equal(Matrix4X4<float>.Identity, camera.ViewMatrix);
    }

    [Fact]
    public void UICamera_CenterPosition_ShouldMapToScreenCenter()
    {
        var camera = new UICamera();
        camera.UpdateMatrices(800, 600);

        var worldCenter = Vector2D<float>.Zero; // UI world center
        var screenPos = camera.WorldToScreen(worldCenter, 800, 600);

        // UI world center should map to screen center
        Assert.Equal(400f, screenPos.X, 1f); // Allow 1 pixel tolerance
        Assert.Equal(300f, screenPos.Y, 1f); // Allow 1 pixel tolerance
    }

    [Fact]
    public void UICamera_ScreenToUIWorldToScreen_ShouldBeSymmetric()
    {
        var camera = new UICamera();
        camera.UpdateMatrices(800, 600);

        var originalScreenPos = new Vector2D<float>(200f, 150f);
        var uiWorldPos = camera.ScreenToWorld(originalScreenPos, 800, 600);
        var backToScreenPos = camera.WorldToScreen(uiWorldPos, 800, 600);

        // Should get back to original screen position (within floating point precision)
        Assert.True(Math.Abs(originalScreenPos.X - backToScreenPos.X) < 0.001f);
        Assert.True(Math.Abs(originalScreenPos.Y - backToScreenPos.Y) < 0.001f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CAMERA MANAGER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CameraManager_ShouldProvideGameAndUICameras()
    {
        var manager = new CameraManager();

        Assert.NotNull(manager.GameCamera);
        Assert.NotNull(manager.UICamera);
        Assert.IsType<GameCamera>(manager.GameCamera);
        Assert.IsType<UICamera>(manager.UICamera);
    }

    [Fact]
    public void CameraManager_UpdateViewport_ShouldUpdateBothCameras()
    {
        var manager = new CameraManager();
        
        // This should not throw and should update both cameras
        manager.UpdateViewport(1024, 768);
        
        // Verify cameras can generate matrices after viewport update
        Assert.NotEqual(default(Matrix4X4<float>), manager.GameCamera.CombinedMatrix);
        Assert.NotEqual(default(Matrix4X4<float>), manager.UICamera.CombinedMatrix);
    }

    [Fact]
    public void CameraManager_CoordinateTransformations_ShouldDelegateToCorrectCamera()
    {
        var manager = new CameraManager();
        manager.UpdateViewport(800, 600);

        var screenPos = new Vector2D<float>(400f, 300f); // Screen center

        // Game world and UI world transformations should give different results
        var gameWorldPos = manager.ScreenToGameWorld(screenPos, 800, 600);
        var uiWorldPos = manager.ScreenToUIWorld(screenPos, 800, 600);

        // UI world center should be (0, 0)
        Assert.True(Math.Abs(uiWorldPos.X) < 0.001f);
        Assert.True(Math.Abs(uiWorldPos.Y) < 0.001f);

        // Game world center depends on camera settings but should be valid
        Assert.True(float.IsFinite(gameWorldPos.X));
        Assert.True(float.IsFinite(gameWorldPos.Y));
    }

    [Fact]
    public void CameraManager_InvalidViewport_ShouldThrowException()
    {
        var manager = new CameraManager();

        Assert.Throws<ArgumentException>(() => manager.UpdateViewport(0, 600));
        Assert.Throws<ArgumentException>(() => manager.UpdateViewport(800, 0));
        Assert.Throws<ArgumentException>(() => manager.UpdateViewport(-800, 600));
        Assert.Throws<ArgumentException>(() => manager.UpdateViewport(800, -600));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MATRIX COMPUTATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GameCamera_CombinedMatrix_ShouldBeProjectionTimesView()
    {
        var camera = new GameCamera();
        camera.UpdateMatrices(800, 600);

        // CombinedMatrix should equal Projection * View
        var expectedCombined = camera.ViewMatrix * camera.ProjectionMatrix;
        
        // Compare each element (matrices should be equal)
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var expected = GetMatrixElement(expectedCombined, i, j);
                var actual = GetMatrixElement(camera.CombinedMatrix, i, j);
                Assert.True(Math.Abs(expected - actual) < 0.001f, 
                    $"Matrix element [{i},{j}] mismatch: expected {expected}, got {actual}");
            }
        }
    }

    /// <summary>
    /// Helper method to get matrix element by row and column indices.
    /// </summary>
    private static float GetMatrixElement(Matrix4X4<float> matrix, int row, int col)
    {
        return (row, col) switch
        {
            (0, 0) => matrix.M11, (0, 1) => matrix.M12, (0, 2) => matrix.M13, (0, 3) => matrix.M14,
            (1, 0) => matrix.M21, (1, 1) => matrix.M22, (1, 2) => matrix.M23, (1, 3) => matrix.M24,
            (2, 0) => matrix.M31, (2, 1) => matrix.M32, (2, 2) => matrix.M33, (2, 3) => matrix.M34,
            (3, 0) => matrix.M41, (3, 1) => matrix.M42, (3, 2) => matrix.M43, (3, 3) => matrix.M44,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}