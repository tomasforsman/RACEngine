# Gemini Agent Instructions for RACEngine

This document provides instructions for the Gemini AI agent to effectively assist with development tasks in the RACEngine repository.

## About This Project

RACEngine is a modular, educational game engine built with C# and .NET. It is designed with a strict Entity-Component-System (ECS) architecture at its core and prioritizes clear, well-documented code for learning purposes.

## Technology Stack

- **Core Engine:** C# / .NET
- **Documentation:** DocFX
- **Build/Doc Scripts:** Node.js

## Key Directories

- `src/`: Contains all the C# source code for the engine modules.
- `tests/`: Contains the unit and integration tests for the engine.
- `docs/`: Contains the markdown files used as source for the documentation.
- `.docfx/`: Contains the configuration and generated site for DocFX.
- `samples/`: Contains sample projects demonstrating engine features.

## How to Build

The project is a standard .NET solution. To build the entire solution, use the `dotnet` CLI from the root directory:

```shell
dotnet build RACEngine.sln
```

## How to Run Tests

Tests are written using a .NET testing framework and can be run from the root of the repository:

```shell
dotnet test RACEngine.sln
```

## How to Work with Documentation

The documentation is generated using DocFX.

- To run a local server to preview the docs: `docfx .docfx/docfx.json --serve`
- To build the documentation site: `docfx build .docfx/docfx.json`
- **Important:** Before any pull request, ensure that any code changes are reflected in the documentation in the `docs/` directory. Incomplete documentation is treated like a failing test.

## Core Architecture & Coding Conventions

Adherence to the existing architecture is critical.

- **Educational Focus:** Code should be commented to explain algorithms and concepts. Performance is secondary to clarity.
- **ECS Architecture:**
    - **Components:** Must be `readonly record struct` implementing `IComponent`. They are for data only.
    - **Systems:** Must be stateless classes that operate on `World` data. They contain the logic.
- **Null Object Pattern:** Required for optional subsystems (`NullRenderer`, `NullAudioService`, etc.).
- **4-Phase Rendering Pipeline:** Rendering follows a strict `Configuration → Preprocessing → Processing → Post-processing` flow.
- **Namespaces:** Follow the `Rac.{ModuleName}` pattern.
- **Style:** Follow the C# style conventions in `docs/code-guides/code-style-guidelines.md`.

## Key Documentation to Reference

Before making changes, consult these files for context:

- **Overall Architecture:** `docs/architecture/system-overview.md`
- **Coding Standards:** `docs/code-guides/code-style-guidelines.md`
- **XML Comments Guide:** `docs/code-guides/csharp_xml_comments_guide.md`
- **ECS Patterns:** `docs/architecture/ecs-architecture.md`
- **Rendering Pipeline:** `docs/architecture/rendering-pipeline.md`
- **Module-Specific Info:** `docs/projects/Rac.{ModuleName}.md`
