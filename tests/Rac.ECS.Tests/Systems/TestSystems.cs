using Rac.ECS.Core;
using Rac.ECS.Systems;

namespace Rac.ECS.Tests.Systems;

/// <summary>
/// Test systems for verifying system lifecycle and dependency management.
/// These are simple test implementations used for validation purposes.
/// </summary>

public class TestInputSystem : ISystem
{
    public bool InitializeCalled { get; private set; }
    public bool ShutdownCalled { get; private set; }
    public int UpdateCallCount { get; private set; }
    public IWorld? ReceivedWorld { get; private set; }

    public void Initialize(IWorld world)
    {
        InitializeCalled = true;
        ReceivedWorld = world;
    }

    public void Update(float delta)
    {
        UpdateCallCount++;
    }

    public void Shutdown(IWorld world)
    {
        ShutdownCalled = true;
    }
}

[RunAfter(typeof(TestInputSystem))]
public class TestMovementSystem : ISystem
{
    public bool InitializeCalled { get; private set; }
    public bool ShutdownCalled { get; private set; }
    public int UpdateCallCount { get; private set; }
    public IWorld? ReceivedWorld { get; private set; }

    public void Initialize(IWorld world)
    {
        InitializeCalled = true;
        ReceivedWorld = world;
    }

    public void Update(float delta)
    {
        UpdateCallCount++;
    }

    public void Shutdown(IWorld world)
    {
        ShutdownCalled = true;
    }
}

[RunAfter(typeof(TestMovementSystem))]
public class TestRenderSystem : ISystem
{
    public bool InitializeCalled { get; private set; }
    public bool ShutdownCalled { get; private set; }
    public int UpdateCallCount { get; private set; }
    public IWorld? ReceivedWorld { get; private set; }

    public void Initialize(IWorld world)
    {
        InitializeCalled = true;
        ReceivedWorld = world;
    }

    public void Update(float delta)
    {
        UpdateCallCount++;
    }

    public void Shutdown(IWorld world)
    {
        ShutdownCalled = true;
    }
}

// System with multiple dependencies
[RunAfter(typeof(TestInputSystem))]
[RunAfter(typeof(TestMovementSystem))]
public class TestComplexSystem : ISystem
{
    public bool InitializeCalled { get; private set; }
    public bool ShutdownCalled { get; private set; }
    public int UpdateCallCount { get; private set; }

    public void Initialize(IWorld world)
    {
        InitializeCalled = true;
    }

    public void Update(float delta)
    {
        UpdateCallCount++;
    }

    public void Shutdown(IWorld world)
    {
        ShutdownCalled = true;
    }
}

// System with circular dependency for testing error handling
[RunAfter(typeof(TestCircularB))]
public class TestCircularA : ISystem
{
    public void Update(float delta) { }
}

[RunAfter(typeof(TestCircularA))]
public class TestCircularB : ISystem
{
    public void Update(float delta) { }
}

// Legacy system that only implements Update method
public class TestLegacySystem : ISystem
{
    public int UpdateCallCount { get; private set; }

    public void Update(float delta)
    {
        UpdateCallCount++;
    }

    // Note: No Initialize or Shutdown implementations - testing backward compatibility
}