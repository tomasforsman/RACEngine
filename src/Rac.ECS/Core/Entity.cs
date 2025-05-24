namespace Rac.ECS.Core;

public readonly record struct Entity(int Id, bool IsAlive = true);
