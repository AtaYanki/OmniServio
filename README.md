# OmniServio

A flexible and powerful dependency injection and service locator system for Unity. This package provides a robust dependency injection system with support for global, scene-specific, and hierarchical service resolution, automatic dependency injection via `[Inject]` attribute, runtime dependency injection with event bus, configurable exception handling, and comprehensive configuration management, making it easy to manage dependencies across your Unity project.

---

## 🚀 Features

* **Global & Scene Services** – Register services globally or per-scene with automatic lifecycle management
* **Hierarchical Resolution** – Services resolve through parent hierarchy, scene, and global locators automatically
* **Automatic Dependency Injection** – Use `[Inject]` attribute for automatic field and property injection via reflection
* **Runtime Dependency Injection** – Components automatically receive services when they become available at runtime
* **Configurable Error Handling** – Choose between warning mode or exception mode for injection errors
* **Configuration System** – Centralized settings via ScriptableObject config and Settings window
* **Update Integration** – Register services that need `Update()`, `FixedUpdate()`, or `LateUpdate()` callbacks
* **Lifecycle Management** – Automatic cleanup with `IDestroyable` interface support
* **Type-Safe** – Strongly-typed service registration and retrieval
* **Zero Configuration** – Works out of the box with minimal setup
* **Editor Integration** – Quick creation via Unity menu items and Settings window

---

## 📦 Installation

### Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click the **+** button → **Add package from git URL**
3. Enter the repository URL:

```
https://github.com/AtaYanki/OmniServio.git
```

4. Wait for Unity to download and import the package

**Requirements:**
- Unity 6000.0 or newer

---

## 🛠️ How It Works

OmniServio combines the Service Locator pattern with automatic dependency injection. It provides a centralized registry for dependencies, allowing components to request services without knowing their concrete implementations. This package implements three levels of service resolution:

1. **Parent Hierarchy** – Checks parent GameObjects for OmniServio components
2. **Scene Level** – Falls back to scene-specific OmniServio
3. **Global Level** – Finally checks the global OmniServio

This hierarchy ensures services are resolved from the most specific to the most general context.

---

## 📘 Quick Start

### 1. Create a Service Interface and Implementation

```csharp
public interface IAudioService
{
    void PlaySound(string soundName);
}

public class AudioService : IAudioService
{
    public void PlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}");
    }
}
```

### 2. Register Services

Create a bootstrapper to register your services. You can use either a Global or Scene OmniServio:

**Global OmniServio** (persists across scenes):

```csharp
using Omni.Servio;
using UnityEngine;

public class GlobalBootstrapper : OmniServioGlobalBootstrapper
{
    protected override void Bootstrap()
    {
        base.Bootstrap(); // Important: Call base to configure as global
        
        // Register services
        OmniServio
            .Register<IAudioService>(new AudioService())
            .Register<ISaveService>(new SaveService());
    }
}
```

**Scene OmniServio** (per-scene):

```csharp
using Omni.Servio;
using UnityEngine;

public class SceneBootstrapper : OmniServioSceneBootstrapper
{
    protected override void Bootstrap()
    {
        base.Bootstrap(); // Important: Call base to configure for scene
        
        // Register scene-specific services
        OmniServio.Register<IGameplayService>(new GameplayService());
    }
}
```

### 3. Retrieve Services

**Method 1: Manual Retrieval**

```csharp
using Omni.Servio;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IAudioService _audioService;
    
    void Start()
    {
        // Automatically resolves from hierarchy → scene → global
        OmniServio.For(this).Get(out _audioService);
    }
    
    void OnJump()
    {
        _audioService?.PlaySound("jump");
    }
}
```

**Method 2: Automatic Dependency Injection (Recommended)**

```csharp
using Omni.Servio;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Inject] private IAudioService _audioService;
    [Inject] private ISaveService _saveService;
    
    void Start()
    {
        // Dependencies are automatically injected before Start() is called
        // No need to add any components - injection happens automatically!
    }
    
    void OnJump()
    {
        _audioService?.PlaySound("jump");
    }
}
```

**Automatic injection is handled automatically!** When you add a `OmniServioSceneBootstrapper` or `OmniServioGlobalBootstrapper` to your scene, a `DependencyInjectionManager` is automatically created. This manager:

1. Runs after all bootstrappers complete (execution order 100)
2. Automatically injects dependencies into all MonoBehaviour components in the scene
3. Ensures services are registered before injection occurs

**No manual setup required!** Just:
1. Add a bootstrapper to your scene (Global or Scene)
2. Register your services in the bootstrapper
3. Mark fields/properties with `[Inject]` attribute
4. Dependencies are injected automatically!

---

## 🎯 Advanced Usage

### Automatic Dependency Injection

OmniServio supports automatic dependency injection using the `[Inject]` attribute. This eliminates the need for manual service retrieval.

**Field Injection:**

```csharp
public class GameManager : MonoBehaviour
{
    [Inject] private IAudioService _audioService;
    [Inject] private ISaveService _saveService;
    
    // Fields are automatically injected before Start()
}
```

**Property Injection:**

```csharp
public class GameManager : MonoBehaviour
{
    [Inject] public IAudioService AudioService { get; private set; }
    [Inject] public ISaveService SaveService { get; private set; }
}
```

**Global Service Injection:**

Use `UseGlobal = true` to always inject from the global OmniServio:

```csharp
public class ConfigManager : MonoBehaviour
{
    [Inject(UseGlobal = true)] private IConfigService _configService;
    // Always uses OmniServio.Global, regardless of hierarchy
}
```

**Manual Injection:**

You can also manually trigger injection:

```csharp
using Omni.Servio;

public class CustomComponent : MonoBehaviour
{
    [Inject] private IAudioService _audioService;
    
    void Awake()
    {
        // Manually inject dependencies
        DependencyInjector.Inject(this);
    }
}
```

### Registering Updatable Services

Services can implement `IUpdatable`, `IFixedUpdatable`, or `ILateUpdatable` to receive Unity update callbacks:

```csharp
public class GameTimer : IUpdatable
{
    private float _time;
    
    public void Update(float deltaTime)
    {
        _time += deltaTime;
    }
    
    public float GetTime() => _time;
}

// Register as updatable
OmniServio.Global.RegisterUpdatable<GameTimer>(new GameTimer());
```

### Registering Destroyable Services

Services can implement `IDestroyable` to receive cleanup callbacks when the OmniServio is destroyed:

```csharp
public class NetworkManager : IDestroyable
{
    public void Destroy()
    {
        // Cleanup network connections
        Disconnect();
    }
}

// Register as destroyable
OmniServio.Global.RegisterDestroyable<NetworkManager>(new NetworkManager());
```

### Direct Global Access

For services that should always be global:

```csharp
// Register globally
OmniServio.Global.Register<IAudioService>(new AudioService());

// Retrieve globally
OmniServio.Global.Get<IAudioService>(out IAudioService audioService);
```

### Scene-Specific Services

```csharp
// Get the OmniServio for a specific scene
OmniServio sceneLocator = OmniServio.ForSceneOf(this);

// Register scene-specific service
sceneLocator.Register<IGameplayService>(new GameplayService());
```

### Chaining Registration

The fluent API allows chaining multiple registrations:

```csharp
OmniServio.Global
    .Register<IAudioService>(new AudioService())
    .Register<ISaveService>(new SaveService())
    .RegisterUpdatable<GameTimer>(new GameTimer())
    .RegisterDestroyable<NetworkManager>(new NetworkManager());
```

### Runtime Dependency Injection

For components that need to receive services registered at runtime, inherit from `RuntimeInjectable`:

```csharp
using Omni.Servio;
using UnityEngine;

public class MyComponent : RuntimeInjectable
{
    [Inject] private IAudioService _audioService;
    [Inject(UseGlobal = true)] private IConfigService _configService;

    protected override void OnServiceInjected(MemberInfo memberInfo, Type serviceType, object service)
    {
        // Called when a service is injected at runtime
        if (serviceType == typeof(IAudioService))
        {
            Debug.Log("Audio service is now available!");
        }
    }
}
```

When a service is registered at runtime:
```csharp
omniServio.Register(new AudioService());
```

Components inheriting from `RuntimeInjectable` automatically receive the service when it becomes available.

### Configuration System

OmniServio includes a comprehensive configuration system accessible via **OmniServio > Config > Settings**.

**Opening Settings:**
- Menu: `OmniServio > Config > Settings`
- Config is automatically created if it doesn't exist

**Configuration Options:**
- **Exception Handler Mode** - Choose Warning (default) or ThrowException mode
- **Bootstrap Scene** - Assign global bootstrap scene reference
- **Auto Load in Editor** - Automatically load bootstrap scene when entering Play mode
- **Load at Runtime** - Load bootstrap scene when game starts (for builds)

**Exception Handler Modes:**
- **Warning** - Logs warnings when injection fails, execution continues
- **ThrowException** - Throws exceptions when injection fails, stops execution

The configuration is stored as a ScriptableObject asset and can be placed in the Resources folder for runtime access.

### Exception Handler Configuration

You can configure exception handling globally via config or per-instance:

**Global Configuration (via Config):**
```csharp
// Set in OmniServioConfig asset via Settings window
// All DependencyInjectionManager instances use this by default
```

---

## 📂 Package Structure

```
Runtime/
  ├── Core/
  │   ├── OmniServio.cs                    # Main OmniServio component
  │   ├── ServiceManager.cs                # Core service registry
  │   ├── OmniServioConfig.cs              # Configuration ScriptableObject
  │   └── BootstrapSceneLoader.cs          # Runtime bootstrap scene loader
  ├── DependencyInjection/
  │   ├── InjectAttribute.cs               # [Inject] attribute for DI
  │   ├── DependencyInjector.cs            # Reflection-based injection system
  │   ├── DependencyInjectionManager.cs    # Automatic injection manager (execution order 100)
  │   ├── RuntimeInjectable.cs             # Base class for runtime injection
  │   ├── ServiceRegistrationEventBus.cs   # Event bus for service registration
  │   ├── IInjectionExceptionHandler.cs    # Exception handler interface
  │   ├── ThrowExceptionHandler.cs         # Exception handler (strict mode)
  │   └── WarningExceptionHandler.cs       # Exception handler (warning mode)
  ├── Bootstrapping/
  │   ├── Bootstrapper.cs                  # Base bootstrapper classes (execution order -100)
  │   ├── OmniServioGlobalBootstrapper.cs  # Global bootstrapper
  │   └── OmniServioSceneBootstrapper.cs   # Scene bootstrapper
  ├── Lifecycle/
  │   ├── UpdateManager.cs                 # Update callback management
  │   └── DestroyManager.cs                # Lifecycle cleanup management
  └── Extensions/
      └── OmniServioExtensions.cs          # Utility extensions

Editor/
  ├── OmniServioMenuItems.cs              # Unity menu integration
  ├── OmniServioSettingsWindow.cs          # Settings window
  ├── OmniServioConfigEditor.cs            # Config asset editor
  └── SceneBootstrapper.cs                 # Bootstrap scene loader (editor)

Samples/
  ├── Scripts/
  │   ├── SampleServices.cs                # Example service interfaces/implementations
  │   ├── SampleBootstrapper.cs            # Example bootstrapper
  │   └── SampleConsumer.cs                # Example service consumer
```

---

## ⚙️ API Reference

### OmniServio

**Static Properties:**
- `static OmniServio Global { get; }` – Access the global OmniServio instance

**Static Methods:**
- `static OmniServio For(MonoBehaviour monoBehaviour)` – Get OmniServio for a MonoBehaviour (hierarchy → scene → global)
- `static OmniServio ForSceneOf(MonoBehaviour monoBehaviour)` – Get OmniServio for a specific scene

**Instance Methods:**
- `OmniServio Register<T>(T service)` – Register a service by type
- `OmniServio Register(Type type, object service)` – Register a service with explicit type
- `OmniServio RegisterUpdatable<T>(T service)` – Register a service that implements `IUpdatable`
- `OmniServio RegisterFixedUpdatable<T>(T service)` – Register a service that implements `IFixedUpdatable`
- `OmniServio RegisterLateUpdatable<T>(T service)` – Register a service that implements `ILateUpdatable`
- `OmniServio RegisterDestroyable<T>(T service)` – Register a service that implements `IDestroyable`
- `OmniServio Get<T>(out T service)` – Retrieve a service (throws if not found)
- `bool TryGet<T>(out T service)` – Try to get a service (returns false if not found)

### Dependency Injection

- `[Inject]` – Attribute to mark fields and properties for automatic injection
- `[Inject(UseGlobal = true)]` – Always inject from global OmniServio
- `DependencyInjector.Inject(MonoBehaviour component, OmniServio omniServio = null)` – Manually inject dependencies
- `DependencyInjector.SetExceptionHandler(IInjectionExceptionHandler handler)` – Set custom exception handler
- `DependencyInjectionManager` – Automatically created by bootstrappers to handle injection (execution order 100)
- `RuntimeInjectable` – Base class for components needing runtime dependency injection
- `ServiceRegistrationEventBus` – Event bus for service registration notifications

### Configuration

- `OmniServioConfig` – ScriptableObject configuration asset
- `OmniServioConfig.Instance` – Singleton access to config
- `OmniServioSettingsWindow` – Editor window for configuration (OmniServio > Config > Settings)
- `IInjectionExceptionHandler` – Interface for custom exception handlers
- `ThrowExceptionHandler` – Exception handler that throws on errors
- `WarningExceptionHandler` – Exception handler that logs warnings (default)

### Interfaces

- `IUpdatable` – Implement to receive `Update()` callbacks
- `IFixedUpdatable` – Implement to receive `FixedUpdate()` callbacks
- `ILateUpdatable` – Implement to receive `LateUpdate()` callbacks
- `IDestroyable` – Implement to receive cleanup callbacks on OmniServio destruction

### Bootstrappers

- `OmniServioGlobalBootstrapper` – Base class for global service registration
- `OmniServioSceneBootstrapper` – Base class for scene-specific service registration

---

## 🎨 Best Practices

### Service Registration

1. **Register in Bootstrappers** – Always register services in bootstrapper classes, not in individual components
2. **Use Interfaces** – Register services by interface, not concrete types, for better decoupling
3. **Global vs Scene** – Use global services for cross-scene dependencies (audio, save system), scene services for gameplay-specific logic

### Service Resolution

1. **Use `OmniServio.For(this)`** – This automatically resolves through the hierarchy
2. **Prefer Dependency Injection** – Use `[Inject]` attribute instead of manual retrieval when possible
3. **Cache References** – Store service references in fields rather than retrieving repeatedly
4. **Null Checks** – Always check if services are null before use

### Dependency Injection

1. **Automatic Injection** – Injection happens automatically when bootstrappers are present - no manual setup needed!
2. **Execution Order** – Bootstrappers run first (order -100), then DependencyInjectionManager runs (order 100)
3. **Private Fields** – Use private fields with `[Inject]` for encapsulation
4. **Properties** – Use properties with `[Inject]` if you need public access
5. **Global Services** – Use `[Inject(UseGlobal = true)]` for services that should always come from global
6. **Manual Injection** – Use `DependencyInjector.Inject()` for runtime injection scenarios

### Lifecycle Management

1. **Implement IDestroyable** – For services that need cleanup (network connections, file handles)
2. **Use Update Interfaces** – Only implement update interfaces if your service truly needs Unity callbacks
3. **Avoid Circular Dependencies** – Design services to avoid depending on each other in cycles

---

## 🧪 Included Samples

The package includes sample scripts demonstrating:

- Service interface and implementation patterns
- Global and scene bootstrapper setup
- Service consumption in MonoBehaviour components
- Automatic dependency injection with `[Inject]` attribute
- Update callback integration
- Lifecycle management

**To use samples:**
1. Check the `Samples/` folder in the package
2. Copy sample scripts to your project
3. Create GameObjects with bootstrapper components
4. Add `AutoInjectComponent` to test automatic injection
5. Test service registration and retrieval

---

## 📝 Changelog

### Version 2.0.0
- **BREAKING CHANGE**: Renamed from ServiceLocator to OmniServio
- Added automatic dependency injection with `[Inject]` attribute
- Added `DependencyInjector` class for reflection-based injection
- Added `AutoInjectComponent` for automatic injection on Awake
- Improved namespace organization

See [CHANGELOG.md](CHANGELOG.md) for full version history.

---

## 🔧 Troubleshooting

**Issue:** Service not found exception
- Ensure the service is registered before it's requested
- Check that you're using the correct OmniServio (global vs scene)
- Verify the service type matches exactly (including generic parameters)

**Issue:** Dependencies not injected
- Ensure a bootstrapper (`OmniServioSceneBootstrapper` or `OmniServioGlobalBootstrapper`) exists in the scene
- Check that `DependencyInjectionManager` was created (should happen automatically)
- Verify services are registered in the bootstrapper before injection occurs
- Verify fields/properties are marked with `[Inject]` attribute
- Check console for injection warnings
- Ensure execution order: Bootstrappers (-100) → DependencyInjectionManager (100)

**Issue:** Services not persisting across scenes
- Use `OmniServioGlobalBootstrapper` for cross-scene services
- Ensure `dontDestroyOnLoad` is set to `true` on the global bootstrapper

**Issue:** Update callbacks not firing
- Verify the service implements `IUpdatable`, `IFixedUpdatable`, or `ILateUpdatable`
- Ensure the service is registered using `RegisterUpdatable`, `RegisterFixedUpdatable`, or `RegisterLateUpdatable`

**Issue:** Multiple OmniServios in scene
- Only one OmniServio per scene is recommended
- Use the hierarchy system if you need nested service resolution

---

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.

---

## 🙌 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes with clear commit messages
4. Submit a pull request

**Areas for contribution:**
- Additional sample scenes
- Performance improvements
- Bug fixes
- Documentation improvements

---

## 💬 Support

**Found a bug or have a feature request?**
- Open an issue on [GitHub](https://github.com/AtaYanki/OmniServio/issues)
- Provide Unity version, error logs, and reproduction steps

**Need help integrating?**
- Check the included sample scripts
- Review the Quick Start guide above
- Open a discussion on GitHub

---

## 🏷️ Keywords

Unity, Dependency Injection, Service Locator, IoC, Design Patterns, Architecture, Services, Global Services, Scene Services, Reflection, Automatic Injection
