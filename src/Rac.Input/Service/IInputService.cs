using Rac.Input.State;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Rac.Input.Service;

/// <summary>
/// Defines the contract for input services providing comprehensive keyboard, mouse, and gamepad input handling.
/// Implements both polling-based and event-driven input patterns for maximum flexibility and performance.
/// </summary>
/// <remarks>
/// The IInputService interface follows the service interface pattern used throughout RACEngine,
/// enabling dependency injection, testing, and architectural consistency. It provides multiple
/// access patterns for different input handling needs:
/// 
/// **Input Handling Patterns:**
/// - **Polling-based**: Check input state each frame (KeyboardKeyState properties)
/// - **Event-driven**: Respond to input events as they occur (OnLeftClick, OnKeyEvent)
/// - **Hybrid**: Combine both approaches for optimal user experience
/// 
/// **Educational Design Principles:**
/// - **Real-time Input**: Direct polling for responsive gameplay mechanics
/// - **Event System**: UI interactions and one-time input handling
/// - **State Management**: Comprehensive key state tracking (pressed, held, released)
/// - **Cross-platform**: Abstraction over platform-specific input systems
/// 
/// **Input Architecture Benefits:**
/// - **Service Interface Pattern**: Enables dependency injection and testing
/// - **Null Object Implementation**: Safe fallback for headless scenarios
/// - **Event Aggregation**: Centralized input event management
/// - **Performance Optimization**: Efficient polling and event dispatching
/// 
/// **Frame-Rate Independence:**
/// - Update method normalizes input timing across different frame rates
/// - Delta time parameter enables time-based input processing
/// - Event system provides immediate response regardless of frame timing
/// - State polling ensures consistent input availability every frame
/// 
/// **Common Usage Patterns:**
/// - Game Movement: Polling for smooth, responsive character control
/// - UI Interaction: Events for buttons, menus, and interface elements
/// - Input Combos: State tracking for complex input sequences
/// - Accessibility: Multiple input methods for inclusive design
/// </remarks>
/// <example>
/// <code>
/// // Polling-based movement input
/// if (inputService.KeyboardKeyKeyState.IsKeyDown(Key.W))
/// {
///     player.MoveForward(deltaTime);
/// }
/// 
/// // Event-driven UI interaction
/// inputService.OnLeftClick += (position) => {
///     var clickedUI = uiSystem.GetElementAt(position);
///     clickedUI?.OnClick();
/// };
/// 
/// // Hybrid approach for combo detection
/// inputService.OnKeyEvent += (key, eventType) => {
///     comboSystem.ProcessInput(key, eventType);
/// };
/// </code>
/// </example>
public interface IInputService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // REAL-TIME POLLING INTERFACE (FRAME-BASED INPUT)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current keyboard state for real-time polling of key press status.
    /// Updated every frame to provide immediate access to current key states.
    /// </summary>
    /// <value>
    /// KeyboardKeyState instance containing the current status of all keyboard keys.
    /// Use for smooth, responsive input that needs to be checked every frame.
    /// </value>
    /// <remarks>
    /// Real-time polling is essential for:
    /// - Character movement and navigation
    /// - Camera controls and view manipulation
    /// - Continuous actions (holding to charge, sustained fire)
    /// - Time-sensitive gameplay mechanics
    /// 
    /// Educational Note: Polling-based input provides frame-rate dependent responsiveness.
    /// Each frame, the game loop checks key states and responds immediately, ensuring
    /// smooth character movement and responsive controls critical for gameplay feel.
    /// 
    /// Performance Consideration: Polling is efficient as it only checks keys that
    /// the game cares about, avoiding unnecessary processing of unused inputs.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Smooth character movement
    /// var movement = Vector2D&lt;float&gt;.Zero;
    /// if (inputService.KeyboardKeyKeyState.IsKeyDown(Key.W)) movement.Y += 1;
    /// if (inputService.KeyboardKeyKeyState.IsKeyDown(Key.S)) movement.Y -= 1;
    /// if (inputService.KeyboardKeyKeyState.IsKeyDown(Key.A)) movement.X -= 1;
    /// if (inputService.KeyboardKeyKeyState.IsKeyDown(Key.D)) movement.X += 1;
    /// 
    /// player.Move(movement * speed * deltaTime);
    /// </code>
    /// </example>
    KeyboardKeyState KeyboardKeyKeyState { get; }

    /// <summary>
    /// Gets the current keyboard event state for detecting key press transitions and timing.
    /// Provides detailed information about key press, release, and hold duration.
    /// </summary>
    /// <value>
    /// KeyboardKeyState instance containing event-specific key information.
    /// Use for detecting state changes and timing-based input mechanics.
    /// </value>
    /// <remarks>
    /// Key event state enables:
    /// - Single-frame key press detection
    /// - Key release timing and duration
    /// - Input sequence recording and playback
    /// - Advanced input pattern recognition
    /// 
    /// Educational Note: Event state differs from polling state by providing
    /// transition information. While polling tells you "what is pressed now",
    /// events tell you "what changed since last frame" and "how long was it held".
    /// </remarks>
    /// <example>
    /// <code>
    /// // Jump on key press (not hold)
    /// if (inputService.KeyEvent.WasKeyJustPressed(Key.Space))
    /// {
    ///     player.Jump();
    /// }
    /// 
    /// // Charge attack based on hold duration
    /// if (inputService.KeyEvent.WasKeyJustReleased(Key.X))
    /// {
    ///     var holdDuration = inputService.KeyEvent.GetKeyHoldDuration(Key.X);
    ///     player.ChargedAttack(holdDuration);
    /// }
    /// </code>
    /// </example>
    KeyboardKeyState KeyEvent { get; }

    // ═══════════════════════════════════════════════════════════════════════════
    // SERVICE LIFECYCLE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes the input service with the target window for input capture and event binding.
    /// Sets up platform-specific input handling and establishes event connections.
    /// </summary>
    /// <param name="window">
    /// Target window for input capture. Must be a valid, initialized window with active input context.
    /// The window provides the surface for mouse interactions and keyboard focus.
    /// </param>
    /// <remarks>
    /// Initialization process includes:
    /// - Platform-specific input system setup (Windows, Linux, macOS)
    /// - Window event binding for mouse and keyboard events
    /// - Input device enumeration and configuration
    /// - Initial state establishment and validation
    /// 
    /// Educational Note: Modern input systems require explicit window binding
    /// to capture input events. Unlike older systems with global input capture,
    /// current approaches respect window focus and provide better security.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when window is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when input system initialization fails.</exception>
    /// <example>
    /// <code>
    /// // Initialize input service with game window
    /// var window = WindowBuilder.Configure(windowManager)
    ///     .WithTitle("Game")
    ///     .WithSize(1280, 720)
    ///     .Create();
    /// 
    /// inputService.Initialize(window);
    /// </code>
    /// </example>
    void Initialize(IWindow window);

    /// <summary>
    /// Updates input state and processes input events for the current frame.
    /// Should be called once per frame before input-dependent game logic.
    /// </summary>
    /// <param name="delta">
    /// Time elapsed since the last update in seconds. Used for time-based input processing
    /// such as key hold duration calculation and input sequence timing.
    /// </param>
    /// <remarks>
    /// Update process includes:
    /// - Polling current input device states
    /// - Processing queued input events
    /// - Updating key hold durations and timing
    /// - Triggering registered event callbacks
    /// - Preparing state for next frame's polling
    /// 
    /// Educational Note: Input processing typically occurs early in the frame
    /// update cycle, before game logic and rendering. This ensures that all
    /// systems have access to the most current input information.
    /// 
    /// Frame Timing: The delta parameter enables time-independent input processing.
    /// Key hold durations and input sequences can be measured in real time
    /// rather than frame counts, providing consistent behavior across frame rates.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standard game loop integration
    /// while (gameRunning)
    /// {
    ///     var deltaTime = timer.GetDeltaTime();
    ///     
    ///     // Update input first
    ///     inputService.Update(deltaTime);
    ///     
    ///     // Process input-dependent game logic
    ///     gameLogic.Update(deltaTime);
    ///     
    ///     // Render frame
    ///     renderer.Render();
    /// }
    /// </code>
    /// </example>
    void Update(double delta);

    /// <summary>
    /// Shuts down the input service and releases all associated resources.
    /// Should be called during application shutdown to ensure proper cleanup.
    /// </summary>
    /// <remarks>
    /// Shutdown process includes:
    /// - Disconnecting from input devices
    /// - Releasing window event bindings
    /// - Clearing internal state and caches
    /// - Disposing of platform-specific resources
    /// 
    /// Educational Note: Proper resource cleanup is essential in input systems
    /// as they often hold references to system resources and event handlers.
    /// Failing to clean up can cause memory leaks and system instability.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Application shutdown sequence
    /// try
    /// {
    ///     inputService.Shutdown();
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError(ex, "InputService", "Shutdown");
    /// }
    /// </code>
    /// </example>
    void Shutdown();

    // ═══════════════════════════════════════════════════════════════════════════
    // EVENT-DRIVEN INTERFACE (IMMEDIATE INPUT RESPONSE)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Occurs when the left mouse button is pressed, providing the cursor position in screen pixels.
    /// Ideal for UI interactions, object selection, and point-and-click mechanics.
    /// </summary>
    /// <remarks>
    /// Mouse click events are essential for:
    /// - UI button interactions and menu navigation
    /// - Object selection and manipulation
    /// - Point-and-click gameplay mechanics
    /// - Context menu activation and tool selection
    /// 
    /// Educational Note: Click events provide immediate response to user interaction,
    /// which is crucial for UI responsiveness. The screen pixel coordinates can be
    /// transformed to world coordinates for in-game object interaction.
    /// 
    /// Coordinate System: Position is provided in screen space (pixels) with origin
    /// typically at top-left corner. Convert to world space using camera matrices
    /// for gameplay object interaction.
    /// </remarks>
    /// <example>
    /// <code>
    /// // UI interaction
    /// inputService.OnLeftClick += (position) => {
    ///     var button = uiSystem.GetButtonAt(position);
    ///     button?.OnClick();
    /// };
    /// 
    /// // World object interaction
    /// inputService.OnLeftClick += (screenPos) => {
    ///     var worldPos = camera.ScreenToWorld(screenPos);
    ///     var gameObject = world.GetObjectAt(worldPos);
    ///     gameObject?.OnInteract();
    /// };
    /// </code>
    /// </example>
    event Action<Vector2D<float>>? OnLeftClick;

    /// <summary>
    /// Occurs when the mouse wheel is scrolled, providing the scroll delta amount.
    /// Useful for camera zoom, menu scrolling, and value adjustment interfaces.
    /// </summary>
    /// <remarks>
    /// Mouse scroll events enable:
    /// - Camera zoom and view adjustment
    /// - Scrollable UI elements (lists, menus, text)
    /// - Value increment/decrement controls
    /// - Weapon or tool selection cycling
    /// 
    /// Educational Note: Scroll delta represents the amount of wheel movement,
    /// typically in standardized units. Positive values usually indicate scrolling
    /// "up" or "away from user", while negative values indicate "down" or "toward user".
    /// 
    /// Sensitivity Consideration: Different devices and platforms may have varying
    /// scroll sensitivities. Consider implementing user-configurable scroll scaling
    /// for optimal user experience across different hardware.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Camera zoom control
    /// inputService.OnMouseScroll += (delta) => {
    ///     camera.Zoom += delta * zoomSensitivity;
    ///     camera.Zoom = Math.Clamp(camera.Zoom, minZoom, maxZoom);
    /// };
    /// 
    /// // Menu scrolling
    /// inputService.OnMouseScroll += (delta) => {
    ///     scrollableMenu.ScrollBy(delta * scrollSpeed);
    /// };
    /// </code>
    /// </example>
    event Action<float>? OnMouseScroll;

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED KEYBOARD INTERFACE (DETAILED KEY HANDLING)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Occurs when any keyboard key is pressed down, providing the specific key and event details.
    /// Ideal for single-action inputs, hotkeys, and keyboard shortcuts.
    /// </summary>
    /// <remarks>
    /// Key press events are perfect for:
    /// - Single-action commands (jump, attack, interact)
    /// - Menu navigation and selection
    /// - Hotkey activation and shortcuts
    /// - Text input and character entry
    /// 
    /// Educational Note: Press events fire once per key press, regardless of how
    /// long the key is held. This differs from polling, which returns true for
    /// every frame while the key is down. Use press events for discrete actions.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Hotkey system
    /// inputService.PressedKey += (key, eventType) => {
    ///     switch (key)
    ///     {
    ///         case Key.F1: helpSystem.ShowHelp(); break;
    ///         case Key.Escape: gameMenu.Toggle(); break;
    ///         case Key.Tab: inventory.Toggle(); break;
    ///     }
    /// };
    /// </code>
    /// </example>
    event Action<Key, KeyboardKeyState.KeyEvent>? PressedKey;

    /// <summary>
    /// Occurs on any keyboard key state change, including press, release, and repeat events.
    /// Provides comprehensive key event information for advanced input processing.
    /// </summary>
    /// <remarks>
    /// General key events capture all keyboard activity:
    /// - Key press, release, and repeat events
    /// - Modifier key combinations (Ctrl, Alt, Shift)
    /// - Special key handling (function keys, numpad)
    /// - Input method editor (IME) events for international text
    /// 
    /// Educational Note: This is the most comprehensive keyboard event, capturing
    /// all key state changes. Use this for complex input systems that need to
    /// track modifier keys, key combinations, or implement custom input handling.
    /// 
    /// Event Types: KeyEvent provides detailed information about the type of
    /// key event (press, release, repeat) and timing information for advanced
    /// input processing and combo detection systems.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Combo detection system
    /// inputService.OnKeyEvent += (key, eventType) => {
    ///     comboDetector.ProcessKeyEvent(key, eventType);
    ///     
    ///     if (comboDetector.DetectedCombo(out var combo))
    ///     {
    ///         player.ExecuteSpecialMove(combo);
    ///     }
    /// };
    /// 
    /// // Modifier key handling
    /// inputService.OnKeyEvent += (key, eventType) => {
    ///     if (eventType == KeyEvent.Pressed && 
    ///         modifierTracker.IsHeld(Key.ControlLeft))
    ///     {
    ///         // Handle Ctrl+Key combinations
    ///         hotkeys.ExecuteShortcut(key);
    ///     }
    /// };
    /// </code>
    /// </example>
    event Action<Key, KeyboardKeyState.KeyEvent>? OnKeyEvent;
}
