using Rac.ECS.Core;
using Xunit;

namespace Rac.ECS.Tests.Core;

public class EntityTests
{
	[Fact]
	public void Entity_Constructor_SetsIdAndIsAlive()
	{
		// Arrange & Act
		var entity = new Entity(42);

		// Assert
		Assert.Equal(42, entity.Id);
		Assert.True(entity.IsAlive);
	}

	[Fact]
	public void Entity_EqualsAndHashCode_WorksCorrectly()
	{
		// Arrange
		var entity1 = new Entity(42);
		var entity2 = new Entity(42);
		var entity3 = new Entity(99);
		var entity4 = new Entity(42, false);

		// Assert - Equal Id and IsAlive
		Assert.Equal(entity1, entity2);
		Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());

		// Assert - Different Id
		Assert.NotEqual(entity1, entity3);
		Assert.NotEqual(entity1.GetHashCode(), entity3.GetHashCode());

		// Assert - Same Id but different IsAlive
		Assert.NotEqual(entity1, entity4);
		Assert.NotEqual(entity1.GetHashCode(), entity4.GetHashCode());
	}

	[Fact]
	public void Entity_DefaultIsAlive_IsTrue()
	{
		// Arrange & Act
		var entity = new Entity(42);

		// Assert
		Assert.True(entity.IsAlive);
	}
}