using Rac.ECS.Systems;
using Xunit;

namespace Rac.ECS.Tests.Systems;

public class RunAfterAttributeTests
{
    [Fact]
    public void Constructor_WithValidSystemType_SetsSystemType()
    {
        // Arrange
        var systemType = typeof(TestInputSystem);

        // Act
        var attribute = new RunAfterAttribute(systemType);

        // Assert
        Assert.Equal(systemType, attribute.SystemType);
    }

    [Fact]
    public void Constructor_WithNullSystemType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RunAfterAttribute(null!));
    }

    [Fact]
    public void Constructor_WithNonSystemType_ThrowsArgumentException()
    {
        // Arrange
        var nonSystemType = typeof(string);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RunAfterAttribute(nonSystemType));
        Assert.Contains("must implement ISystem interface", exception.Message);
        Assert.Contains("String", exception.Message);
    }

    [Fact]
    public void Constructor_WithSystemInterface_IsValid()
    {
        // Arrange
        var systemType = typeof(ISystem);

        // Act & Assert - Should not throw
        var attribute = new RunAfterAttribute(systemType);
        Assert.Equal(systemType, attribute.SystemType);
    }

    [Fact]
    public void Constructor_WithConcreteSystemType_IsValid()
    {
        // Arrange
        var systemType = typeof(TestInputSystem);

        // Act & Assert - Should not throw
        var attribute = new RunAfterAttribute(systemType);
        Assert.Equal(systemType, attribute.SystemType);
    }

    [Fact]
    public void AttributeUsage_AllowsMultipleInstances()
    {
        // This test verifies the AttributeUsage allows multiple instances
        // by checking that the attribute metadata is correctly configured
        
        // Arrange
        var attributeType = typeof(RunAfterAttribute);
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.True(attributeUsage.AllowMultiple);
        Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
        Assert.True(attributeUsage.Inherited);
    }

    [Fact]
    public void MultipleAttributes_OnSameClass_AreAllowed()
    {
        // Arrange
        var systemType = typeof(TestComplexSystem);

        // Act
        var attributes = systemType.GetCustomAttributes(typeof(RunAfterAttribute), false)
            .Cast<RunAfterAttribute>()
            .ToList();

        // Assert
        Assert.Equal(2, attributes.Count);
        var dependencyTypes = attributes.Select(a => a.SystemType).ToHashSet();
        Assert.Contains(typeof(TestInputSystem), dependencyTypes);
        Assert.Contains(typeof(TestMovementSystem), dependencyTypes);
    }

    [Fact]
    public void Attribute_OnInheritedClass_IsInherited()
    {
        // This test would require a derived class to test inheritance
        // For now, just verify the Inherited property is set correctly
        
        // Arrange
        var attributeType = typeof(RunAfterAttribute);
        var attributeUsage = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.True(attributeUsage.Inherited);
    }
}