# Deadlock Movement System
> This project aims to replace the standard Facepunch PlayerController with a from-scratch C# implementation to mimic the momentum-based mechanics found in *Deadlock*.

## Demo
> Gameplay reel and playable build coming after I add animations and VFX.

## Controls
| Action | Key |
|---|---|
| Move | WASD |
| Camera | Mouse |
| Jump / Double Jump | Space |
| Dash | Left Shift |
| Crouch / Slide | C |
| Mantle | Hold Space near a ledge |

## Current Status: Presentation Pass

**Core Mechanics** (complete)
- [x] CharacterController Integration
- [x] Camera-relative WishDir Projection
- [x] Air Strafe Physics
- [x] Dash Mechanics
- [x] Double-Jumping
- [x] Dash-Jump
- [x] Wall Bouncing
- [x] Sliding on slopes (min. slide-speed variation)
- [x] Edge Boosting
- [x] Mantling
- [x] (Bonus) Mantle Sliding
- [x] Stamina System

**Presentation** (in progress)
- [ ] Camera polish (FOV push on dash, shake on mantle/landing/wall impact)
- [ ] Animations (dash, slide, wall slide, wall jump, mantle)
- [ ] VFX (landing puffs, dash streaks, wall impact particles)
- [ ] Demo arena / movement course

**Deferred**
- [ ] (Bonus) High / Low Mantle
- [ ] (Environmental) Ziplines
- [ ] (Environmental) Vents


## Technical Goal
> To implement a momentum-conservative state machine that handles stamina-gated movement actions while maintaining the classic Source "snappiness" through custom WishDir projection.

## Architecture

`Movement.cs` acts as the state manager and global property container. Each fixed update it calls `CurrentState.Update()`, which returns either `null` (stay) or a new state instance (transition). All velocity math lives inside the states.

| File | Role |
|---|---|
| `Movement.cs` | State machine manager. Holds all tunable properties, calls `CharacterController.Move()` |
| `BaseState.cs` | Abstract base. Provides `GetWishDir()` and the `Enter()` / `Update()` / `Exit()` lifecycle |
| `StaminaComponent.cs` | Standalone stamina resource with `TryConsume()`. Referenced by states and HUD independently |
| `PlayerCamera.cs` | Third-person tracking with sphere-cast wall collision |
| `Hud.razor` | Telemetry overlay and stamina pip UI |

**States:** `GroundedState` - `AirborneState` - `DashState` - `SlideState` - `CrouchState` - `WallSlideState` - `MantleState`

**Start reading:** `Movement.cs` -> `BaseState.cs` -> `AirborneState.cs`

## What I Built vs What the Engine Provides

s&box's `CharacterController` handles collision detection, ground detection, and velocity application (`Move()`). Everything else is custom:

| Custom | Engine (`CharacterController`) |
|---|---|
| All velocity math (friction, acceleration, air strafing) | Collision detection and resolution |
| State machine and every transition condition | `IsOnGround` detection |
| WishDir camera-relative projection | `Move()` to apply the final velocity |
| Wall detection via 8-ray starburst | |
| Dash, wall jump, edge boost, coyote timer | |
| Slope-aware slide physics | |
| Double jump with directional impulse | |
| Mantle detection (5-ray forward sweep + downward ledge scan) | |
| Stamina resource system (`StaminaComponent`) with time-based regen | |


## Calibration Backlog
> Physics values to measure and match against the Deadlock reference (+ other small features/tweaks).
- [x] input-based directional impulse on double jumps
- [x] mantle exit impulse: **177 u/s** (standard) / **512 u/s** (mantle slide)
- [ ] slope sliding needs refactoring; higher initial impulse and a speed cap I think

## Movement Glossary

Terms used throughout this codebase and devlog which might not be obvious.

- **Edge Boost**: (while airborne) Sliding along a wall and jumping at the corner edge. The diagonal wall normal at the corner adds a forward component to the kick, boosting the player's momentum.
- **Wall Coyote Time**: A short window after leaving a wall surface where the wall jump input is still accepted.
- **Mantle Slide**: Triggered on mantle exit by holding crouch and directing input toward the wall. Injects an explicit 512 u/s forward impulse into SlideState rather than the standard 177 u/s exit boost.