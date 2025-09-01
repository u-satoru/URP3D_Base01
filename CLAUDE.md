# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Unity Project Configuration

Unity Version: **6000.0.42f1** (Unity 6)
Project Type: 3D Game Framework with Event-Driven Architecture

## Unity-Specific Commands

### Building and Testing
```bash
# Unity uses the Editor for builds - no CLI build commands
# Open project in Unity Hub or Unity Editor to build

# For automated builds, use Unity's batch mode (requires Unity installation):
# Windows: "C:\Program Files\Unity\Hub\Editor\6000.0.42f1\Editor\Unity.exe" -batchmode -projectPath "D:\workspace\Unity6_Base" -buildWindows64Player "Build\game.exe"
# Mac: /Applications/Unity/Hub/Editor/6000.0.42f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /path/to/project -buildOSXUniversalPlayer "Build/game.app"
```

### Project Setup
1. Open Unity Hub and add this project folder
2. Ensure Unity 6000.0.42f1 is installed
3. Open project - Unity will generate Library/ folder and .meta files
4. Install required packages via Package Manager if not auto-installed

## Assembly Architecture

The project uses Unity Assembly Definitions for modular code organization:

### Assembly Dependencies
- **Unity6.Core** → No dependencies (foundation layer)
- **Unity6.Player** → Unity6.Core, Unity6.Camera, Unity.InputSystem
- **Unity6.Systems** → Unity6.Core
- **Unity6.Camera** → Unity6.Core, Unity.Cinemachine (循環参照を解消済み)
- **Unity6.Optimization** → Unity6.Core

### Key Directories
- `Assets/` - Main project assets directory
  - `Assembly_Definitions/` - Unity assembly definition files (.asmdef)
  - `Core/` - Core systems
    - `Data/` - Game data structures
    - `Events/` - Event system implementation
    - `Player/` - Player state definitions
  - `Player/` - Player implementation
    - `States/` - Player state machine states
  - `Systems/` - Game management systems
  - `Camera/` - Camera integration (Cinemachine)
  - `Materials/` - Material assets
  - `Prefabs/` - Reusable GameObject prefabs
  - `Scenes/` - Unity scene files
  - `ScriptableObjects/Events/` - Runtime event assets (created in Unity Editor)

## Core Architecture Patterns

### Event System Implementation
The entire codebase uses ScriptableObject events to eliminate direct references between components:

1. **Event Types**:
   - `GameEvent` - Basic events without parameters
   - `GenericGameEvent<T>` - Typed events with data payloads
   - Events support caching last value for late subscribers
   - Priority-based listener ordering via cached sorted lists

2. **Listener Registration**:
   - Listeners use HashSet for O(1) registration/unregistration
   - Must unregister in OnDisable to prevent memory leaks
   - Supports both MonoBehaviour and interface-based listeners

3. **Event Flow**:
   ```
   Component A → Raises Event (ScriptableObject) → All Listeners Respond
   ```

### State Machine Architecture
Both PlayerStateMachine and GameManager use event-driven state transitions:
- State change requests come via events, not direct method calls
- States defined as enums in Core namespace
- Each state transition fires a state changed event for UI/systems to respond

### Component Communication Rules
1. **Never use GetComponent or FindObjectOfType** - use events instead
2. **All dependencies injected via SerializeField** - configured in Inspector
3. **No singleton patterns** - GameManager uses events for global communication (※CinemachineIntegrationは例外的にシングルトンを使用)
4. **Prefab-based workflow** - components configured on prefabs, not in code

## Critical Implementation Rules

### Adding New Features
1. **Event Creation**: Create ScriptableObject events in `Core/Events/` first
2. **Assembly Placement**: Add code to correct assembly based on dependencies
3. **Namespace**: Follow exact namespace convention (Unity6.Core.*, Unity6.Player.*, etc.)
4. **State Machines**: Use event-driven state pattern for any stateful behavior

### Modifying Existing Code
1. **Preserve Event Channels**: Never remove or rename existing SerializeField events
2. **Maintain Decoupling**: Never add direct component references or singletons
3. **Assembly Dependencies**: Only reference assemblies already in the .asmdef
4. **Memory Safety**: Always unregister listeners in OnDisable

### Unity Inspector Configuration
- All configuration happens via SerializeField in Inspector
- Event connections made by dragging ScriptableObject assets
- Never hardcode asset paths or use Resources.Load
- Prefabs store all component configurations

## Event System Usage Examples

### Creating a New Event Type
```csharp
// 1. Define the event class
[CreateAssetMenu(fileName = "NewHealthEvent", menuName = "Unity6/Events/Health Event")]
public class HealthEvent : GenericGameEvent<float> { }

// 2. Create listener interface
public interface IHealthEventListener : IGameEventListener<float> { }

// 3. Use in component
[SerializeField] private HealthEvent onHealthChanged;
void TakeDamage(float damage) {
    health -= damage;
    onHealthChanged?.Raise(health);
}
```

### Listening to Events
```csharp
// Via Interface
public class HealthBar : MonoBehaviour, IHealthEventListener {
    [SerializeField] private HealthEvent healthEvent;
    
    void OnEnable() => healthEvent.RegisterListener(this);
    void OnDisable() => healthEvent.UnregisterListener(this);
    
    public void OnEventRaised(float newHealth) {
        // Update UI
    }
}
```

## Package Dependencies
Required packages defined in `Packages/manifest.json`:
- InputSystem 1.7.0 - New Input System for player controls
- Cinemachine 3.1.0 - Camera system integration
- Addressables 2.0.8 - Asset management
- UI Toolkit 2.0.0 - UI system
- TextMeshPro 3.0.9 - Text rendering

## Recent Changes and Fixes
- **アセンブリ循環参照の解消**: Unity6.Camera から Unity6.Player への参照を削除
- **名前空間の修正**: 存在しない Unity6.Camera.Events への参照を削除
- **イベントリスナーの型安全性向上**: GenericGameEventListener の適切な型引数を設定
- **カプセル化の改善**: publicフィールドをprotectedに変更し、プロパティでアクセス