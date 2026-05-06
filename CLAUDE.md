# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**BlackDeath** is a turn-based combat RPG built in Unity 6000.3.10f1 (C#). The project has two development workspaces under `Assets/Workpaces/`: **Jaakko** (primary architecture, ~3300 lines) and **Tatu** (experimental/alternative implementations).

## Build & Run

This is a Unity project — open it in the Unity Editor (Unity 6000.3.10f1). There are no standalone build scripts or test runners. Use Unity's built-in Play Mode to test. The main scene is `Assets/Scenes/GameplayScene.unity`.

Command-line build (if needed):
```
Unity -projectPath . -buildTarget StandaloneWindows64 -executeMethod BuildScript.Build
```

## Architecture

### Entry Point & Dependency Injection

`Game.cs` is a singleton that bootstraps `GameManager`, which owns a custom DI `Container` (`Assets/Workpaces/Jaakko/Scripts/Game/Container.cs`). All 13 managers are registered and resolved via reflection-based constructor injection — no external DI frameworks. Access managers at runtime via `Game.Instance.GameManager.Get<T>()` or the static `Services` locator.

Managers inherit `ManagerBase` and follow this lifecycle: `Init()` → `OnManagersInitialized()` → `Update()` → `Dispose()`.

### Actor & Component System

`Actor` is the base character class with a GUID identity. It composes behaviors via `IActorComponent` instances (e.g., `CombatActor`, `HealthComponent`, `AnimatorComponent`, `CameraTarget`). Components are discovered via reflection and share the lifecycle: `Init()` → `OnActorComponentsInitialized()` → `SaveData()`/`LoadData()` → `Dispose()`. Actor teams: `Player`, `Enemy`, `Neutral`. Control types: `User`, `AI`.

### Combat System

`CombatManager` owns several `CombatSystemBase` subsystems:

- **TurnSystem** — determines turn order
- **ActionSystem** — requests and executes actions via `IActionProvider` (Player or AI)
- **ReactionSystem** — handles parry/dodge prompts via `IReactionProvider`
- **DamageSystem** — applies damage after animation resolves
- **TransitionSystem** — drives animation state transitions
- **CombatStatSystem** — tracks battle statistics

Turn flow: `CombatManager.NextTurn()` → `TurnSystem` picks actor → `ActionSystem` requests action from provider → `CombatAction.Resolve()` → `TransitionSystem` plays animation → `DamageSystem` applies result → next turn or `EndCombat()`.

Actions are `ScriptableObject` assets under `Assets/Resources/Actions/`. The command pattern (`ICombatCommand`, `CombatCommandDispatcher`, `CombatCommandProcessor`) handles dispatch.

### Game States

Three top-level states: `GameState.None` (exploration), `GameState.Combat`, `GameState.Dialogue`. State drives which managers are active and how input is routed.

### Input & UI

`InputManager` routes input to the active `IInputSource` (`PlayerInputSource` or `AIInputSource`) and `IUIInputReceiver`. `UIManager` manages a UI component stack; active components receive input. UI components: `CombatUI`, `DialogueUI`, `MainMenuUI`.

### Event Communication

Two parallel event systems coexist:
- **Static events** on `GameEvents` and `CombatEvents` (Action delegates) — used for manager-to-manager communication.
- **EventBus** — generic queued events (less commonly used).
- **Services** (`Assets/Workpaces/Jaakko/Scripts/Services/Services.cs`) — static service locator as a secondary access path.

### Other Systems

- **Camera**: `CameraManager` + `ICameraMode` strategy (`CombatCameraMode`, `FollowActorCameraMode`). Presets configured via `CameraPresetsConfig` ScriptableObject; editor tool at `Camera/Editor/CameraPresetEditorWindow.cs`.
- **Audio**: `AudioManager` + pluggable `IAudioModule` (e.g., `CombatAudioModule`).
- **Movement**: `MovementController` + `IMovementModule` with intent/impulse/environment sub-modules.
- **Dialogue**: `DialogueManager` + `DialogueNode` ScriptableObjects under `Assets/Resources/DialogueNodes/`. Flag-based conditional branching.
- **Save**: `SaveManager` — components opt in via `SaveData()`/`LoadData()`.
- **Pool**: `PoolManager` — object pooling for effects/projectiles.

## Conventions

- **Private fields**: `m_camelCase`
- **Interfaces**: `IPascalCase`
- **Events/callbacks**: `OnPascalCase`
- Code is organized with `#region` blocks (Init/Lifecycle, Properties, Methods, Events)
- Generic accessors (`Get<T>()`, `Resolve<T>()`) are the primary way to retrieve typed dependencies
- Null checks with early returns are preferred over nested conditionals
- ScriptableObjects are used for all data/config (actions, camera presets, dialogue nodes, audio clips)

## Workspace Notes

- **Jaakko** workspace = production code; PRs and features should go here unless explicitly experimental.
- **Tatu** workspace = personal experiments; `BattleManager.cs` and related files are a parallel battle system prototype, not wired to the main game loop.
- The `ThirdParty/MasterMagicFX` folder contains a purchased asset for VFX — do not modify it.
