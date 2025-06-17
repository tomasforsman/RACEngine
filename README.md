# RACEngine

**RACEngine** is a modular, modern game engine built with .NET 8 and C#. It serves as a learning platform and a foundation for developing custom games without relying on existing game engines. The project emphasizes clarity, separation of concerns, and practical elegance.

---

## 🚀 Features

* **Entity Component System (ECS):** A clean, idiomatic ECS implementation featuring `Entity`, `IComponent`, `World`, `ISystem`, and `SystemScheduler`.
* **Modular Architecture:** Each subsystem (e.g., AI, Animation, Audio, Assets) resides in its own project/folder, promoting scalability and maintainability.
* **Sample Game:** Includes a sample game demonstrating the engine's capabilities with components like `BoidSystem`, `PositionComponent`, and `VelocityComponent`.([GitHub][1])

---

## 📁 Project Structure

```
RACEngine/
├── src/
│   ├── Rac.ECS/            # Core ECS module
│   ├── Rac.AI/             # AI subsystem
│   ├── Rac.Animation/      # Animation subsystem
│   ├── Rac.Audio/          # Audio subsystem
│   ├── Rac.Assets/         # Asset management
│   └── Rac.Core/           # Core utilities and extensions
└── samples/
    └── SampleGame/         # Sample game using RACEngine
```

---

## 🛠️ Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### System Requirements

For optimal performance and full audiovisual effects support:

* **Graphics:** OpenGL 3.3 or higher
* **Audio:** OpenAL-compatible audio device (automatically handled by Silk.NET.OpenAL)
* **Required OpenGL Extensions:**
  - GL_ARB_framebuffer_object (for post-processing effects)
  - GL_ARB_texture_float (for HDR rendering)

**Note:** The engine will automatically detect your graphics and audio capabilities and gracefully fall back to basic rendering/no audio if requirements aren't met. Update your graphics drivers or use a newer graphics card for full visual effects support. Audio will use the NullAudioService as fallback if OpenAL initialization fails.

### Building the Engine

1. **Clone the repository:**

   ```bash
   git clone https://github.com/tomasforsman/RACEngine.git
   cd RACEngine
   ```

2. **Navigate to the sample game:**

   ```bash
   cd samples/SampleGame
   ```

3. **Build and run the sample game:**

   ```bash
   dotnet run
   ```

---

## 🧪 Sample Game

The `SampleGame` project demonstrates the usage of RACEngine's ECS and other subsystems. It includes:

* **Boid Simulation:** Showcases flocking behavior using `BoidSystem`, `PositionComponent`, and `VelocityComponent`.
* **Obstacle Avoidance:** Implements basic obstacle components to influence boid movement.
* **Species Differentiation:** Utilizes `BoidSpeciesComponent` and `MultiSpeciesBoidSettingsComponent` for varied behaviors.

---

## 📚 Documentation

Comprehensive documentation is forthcoming. In the meantime, refer to the source code and the `SampleGame` project for insights into the engine's usage and capabilities.

---

## 🤝 Contributing

Contributions are welcome! If you're interested in contributing:

1. **Fork the repository.**
2. **Create a new branch:**

   ```bash
   git checkout -b feature/YourFeature
   ```
3. **Commit your changes:**

   ```bash
   git commit -m 'Add YourFeature'
   ```
4. **Push to the branch:**

   ```bash
   git push origin feature/YourFeature
   ```
5. **Open a pull request.**

Please ensure that your code adheres to the project's coding standards and includes appropriate tests.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
---

## 📬 Contact

For questions, suggestions, or feedback:

* **Author:** Tomas Forsman
* **GitHub:** [@tomasforsman](https://github.com/tomasforsman)
* **Email:** [tomas@forsman.dev](mailto:tomas@forsman.dev)