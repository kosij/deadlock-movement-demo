# Deadlock Movement System
> This project aims to replace the standard Facepunch PlayerController with a from-scratch C# implementation to mimic the momentum-based mechanics found in *Deadlock*.

## Current Status: Implementing Core Mechanics
- [x] CharacterController Integration
- [x] Camera-relative WishDir Projection
- [x] Air Strafe Physics
- [x] Dash Mechanics
- [x] Double-Jumping
- [x] Dash-Jump
- [x] Wall Bouncing
- [x] Sliding on slopes (min. slide-speed variation)
- [x] Edge Boosting
- [ ] Mantling
- [ ] (Bonus) Mantle Sliding
- [ ] (Environmental) Ziplines
- [ ] (Environmental) Vents
- [ ] Stamina System



## Technical Goal
> To implement a momentum-conservative state machine that handles stamina-gated movement actions while maintaining the classic Source "snappiness" through custom WishDir projection.

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


## Calibration Backlog
> Physics values to measure and match against the Deadlock reference (+ other small features/tweaks).
- [x] input-based directional impulse on double jumps

## Movement Glossary

Terms used throughout this codebase and devlog which might not be obvious.

- **Edge Boost**: (while airborne) Sliding along a wall and jumping at the corner edge. The diagonal wall normal at the corner adds a forward component to the kick, boosting the player's momentum.
- **Wall Coyote Time**: A short window after leaving a wall surface where the wall jump input is still accepted.
