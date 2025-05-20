# RACEngine Unit Tests

This directory contains unit tests for the RACEngine project. The tests are organized according to the project structure, with each project having its own test project.

## Test Projects

- **Rac.ECS.Tests**: Tests for the Entity Component System
  - Tests for `World` class - entity creation, component management, querying
  - Tests for `Entity` struct - validation of record properties and behavior
  
- **Rac.Core.Tests**: Tests for the Core functionality
  - Tests for `ConfigManager` - configuration loading and settings
  
- **Rac.ProjectTools.Tests**: Tests for the Project Tools
  - Tests for component generation functionality

## Running the Tests

Tests can be run using the standard .NET test command:

```bash
dotnet test
```

Or for a specific test project:

```bash
dotnet test tests/Rac.ECS.Tests
```

## Test Design Principles

1. **Isolation**: Each test should be independent and not rely on the results of other tests.
2. **Clarity**: Test names clearly describe what they're testing and the expected behavior.
3. **Coverage**: Tests aim to cover critical path and edge cases.
4. **Simplicity**: Tests are kept as simple as possible while still thoroughly validating behavior.

## Future Test Extensions

Additional test projects should be created for:

- Rac.AI
- Rac.Animation
- Rac.Audio
- Rac.Assets
- Rac.Input
- Rac.Networking
- Rac.Rendering
- Rac.Scripting
- Rac.Physics